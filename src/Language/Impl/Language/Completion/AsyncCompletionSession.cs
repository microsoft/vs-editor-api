using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Text.Utilities;
using Microsoft.VisualStudio.Threading;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.Language.Intellisense.Implementation
{
    /// <summary>
    /// Holds a state of the session
    /// and a reference to the UI element
    /// </summary>
    internal class AsyncCompletionSession : IAsyncCompletionSession
    {
        // Available data and services
        private readonly IDictionary<IAsyncCompletionItemSource, SnapshotPoint> _completionSources;
        private readonly IAsyncCompletionService _completionService;
        private readonly JoinableTaskFactory _jtf;
        private readonly ICompletionPresenterProvider _uiFactory;
        private readonly AsyncCompletionBroker _broker;
        private readonly ITextView _view;
        private readonly TelemetryData _telemetryData;
        private readonly ITextStructureNavigator _textStructureNavigator;
        private readonly IGuardedOperations _guardedOperations;

        // Presentation:
        ICompletionPresenter _gui; // Must be accessed from GUI thread
        const int FirstIndex = 0;
        readonly int PageStepSize;

        // Computation state machine
        private ModelComputation<CompletionModel> _computation;
        private readonly CancellationTokenSource _computationCancellation = new CancellationTokenSource();
        int _lastFilteringTaskId;

        // When set, UI will no longer be updated
        private bool _isDismissed;
        // When set, we won't send dismissed telemetry
        private bool _committed;

        public event EventHandler<CompletionItemEventArgs> ItemCommitted;
        public event EventHandler Dismissed;
        public event EventHandler<CompletionItemsWithHighlightEventArgs> ItemsUpdated;

        public ITextView TextView => _view;

        public AsyncCompletionSession(JoinableTaskFactory jtf, ICompletionPresenterProvider uiFactory,
            IDictionary<IAsyncCompletionItemSource, SnapshotPoint> completionSources,
            IAsyncCompletionService completionService, AsyncCompletionBroker broker, ITextView view)
        {
            _jtf = jtf;
            _uiFactory = uiFactory;
            _broker = broker;
            _completionSources = completionSources;
            _completionService = completionService;
            _view = view;
            _textStructureNavigator = broker.TextStructureNavigatorSelectorService?.GetTextStructureNavigator(view.TextBuffer);
            _guardedOperations = broker.GuardedOperations;
            _telemetryData = new TelemetryData(broker);
            PageStepSize = uiFactory?.ResultsPerPage ?? 1;
            _view.Caret.PositionChanged += OnCaretPositionChanged;
        }

        bool IAsyncCompletionSession.CommitIfUnique(CancellationToken token)
        {
            // Note that this will deadlock if OpenOrUpdate wasn't called ahead of time.
            var lastModel = _computation.WaitAndGetResult();
            if (lastModel.UniqueItem != null)
            {
                Commit(default(char), lastModel.UniqueItem, token);
                return true;
            }
            else if (!lastModel.PresentedItems.IsDefaultOrEmpty && lastModel.PresentedItems.Length == 1)
            {
                Commit(default(char), lastModel.PresentedItems[0].CompletionItem, token);
                return true;
            }
            else
            {
                // Show the UI, because waitAndGetResult canceled showing the UI.
                UpdateUiInner(lastModel); // We are on the UI thread, so we may call UpdateUiInner
                return false;
            }
        }

        /// <summary>
        /// Entry point for committing. Decides whether to commit or not. The inner commit method calls Dispose to stop this session and hide the UI.
        /// </summary>
        /// <param name="token">Command handler infrastructure provides a token that we should pass to the language service's custom commit method.</param>
        /// <param name="typedChar">It is default(char) when commit was requested by an explcit command (e.g. hitting Tab, Enter or clicking)
        /// and it is not default(char) when commit happens as a result of typing a commit character.</param>
        void IAsyncCompletionSession.Commit(CancellationToken token, char typedChar)
        {
            var lastModel = _computation.WaitAndGetResult();

            if (lastModel.UseSoftSelection && !typedChar.Equals(default(char)))
                ((IAsyncCompletionSession)this).Dismiss(); // In soft selection mode, user commits explicitly (click, tab, e.g. not tied to a text change). Otherwise, we dismiss the session
            else if (lastModel.SelectSuggestionMode && string.IsNullOrWhiteSpace(lastModel.SuggestionModeItem?.InsertText))
                return; // In suggestion mode, don't commit empty suggestion (suggestion item temporarily shows description of suggestion mode)
            else if (lastModel.SelectSuggestionMode)
                Commit(typedChar, lastModel.SuggestionModeItem, token);
            else if (!lastModel.PresentedItems.Any())
                return; // There is nothing to commit
            else
                Commit(typedChar, lastModel.PresentedItems[lastModel.SelectedIndex].CompletionItem, token);
        }

        private void Commit(char typedChar, CompletionItem itemToCommit, CancellationToken token)
        {
            var lastModel = _computation.WaitAndGetResult();

            if (!_jtf.Context.IsOnMainThread)
                throw new InvalidOperationException($"{nameof(IAsyncCompletionSession)}.{nameof(IAsyncCompletionSession.Commit)} must be callled from UI thread.");

            _telemetryData.UiStopwatch.Restart();
            if (itemToCommit.UseCustomCommit)
            {
                // Pass appropriate buffer to the item's provider
                var buffer = _completionSources[itemToCommit.Source].Snapshot.TextBuffer;
                _guardedOperations.CallExtensionPoint(
                    errorSource: itemToCommit.Source,
                    call: () => itemToCommit.Source.CustomCommit(_view, buffer, itemToCommit, lastModel.ApplicableSpan, typedChar, token));
            }
            else
            {
                InsertIntoBuffer(_view, lastModel, itemToCommit.InsertText, typedChar);
            }
            _telemetryData.UiStopwatch.Stop();
            _telemetryData.RecordCommitAndSend(_telemetryData.UiStopwatch.ElapsedMilliseconds, itemToCommit, typedChar);
            _committed = true;
            _guardedOperations.RaiseEvent(this, ItemCommitted, new CompletionItemEventArgs(itemToCommit));
            ((IAsyncCompletionSession)this).Dismiss();
        }

        private void InsertIntoBuffer(ITextView view, CompletionModel model, string insertText, char typeChar)
        {
            var buffer = view.TextBuffer;
            var bufferEdit = buffer.CreateEdit();

            // ApplicableSpan already contains the typeChar and brace completion. Replacing this span will cause us to lose this data.
            // The command handler who invoked this code needs to re-play the type char command, such that we get these changes back.
            bufferEdit.Replace(model.ApplicableSpan.GetSpan(buffer.CurrentSnapshot), insertText);
            bufferEdit.Apply();
        }

        void IAsyncCompletionSession.Dismiss()
        {
            if (_isDismissed)
                return;

            _isDismissed = true;
            _broker.DismissSession(this);
            _guardedOperations.RaiseEvent(this, Dismissed);
            _view.Caret.PositionChanged -= OnCaretPositionChanged;
            _computationCancellation.Cancel();
            _computation = null;

            if (!_committed)
                _telemetryData.RecordDismissedAndSend();

            if (_gui != null)
            {
                var copyOfGui = _gui;
                _guardedOperations.CallExtensionPointAsync(
                    errorSource: _gui,
                    asyncAction: async () =>
                    {
                        await _jtf.SwitchToMainThreadAsync();
                        copyOfGui.FiltersChanged -= OnFiltersChanged;
                        copyOfGui.CommitRequested -= OnCommitRequested;
                        copyOfGui.CompletionItemSelected -= OnItemSelected;
                        copyOfGui.CompletionClosed -= OnGuiClosed;
                        copyOfGui.Close();
                    });
                _gui = null;
            }
        }

        void IAsyncCompletionSession.OpenOrUpdate(ITextView view, CompletionTrigger trigger, SnapshotPoint triggerLocation)
        {
            if (_computation == null)
            {
                _computation = new ModelComputation<CompletionModel>(PrioritizedTaskScheduler.AboveNormalInstance, _computationCancellation.Token);
                _computation.Enqueue((model, token) => GetInitialModel(view, trigger, triggerLocation, token));
            }

            var taskId = Interlocked.Increment(ref _lastFilteringTaskId);
            _computation.Enqueue((model, token) => UpdateSnapshot(model, trigger, FromCompletionTriggerReason(trigger.Reason), triggerLocation, token, taskId), UpdateUi);
        }

        internal void InvokeAndCommitIfUnique(ITextView view, CompletionTrigger trigger, SnapshotPoint triggerLocation, CancellationToken token)
        {
            ((IAsyncCompletionSession)this).OpenOrUpdate(view, trigger, triggerLocation);
            if (((IAsyncCompletionSession)this).CommitIfUnique(token))
            {
                ((IAsyncCompletionSession)this).Dismiss();
            }
        }

        private static CompletionFilterReason FromCompletionTriggerReason(CompletionTriggerReason reason)
        {
            switch (reason)
            {
                case CompletionTriggerReason.Invoke:
                case CompletionTriggerReason.InvokeAndCommitIfUnique:
                    return CompletionFilterReason.Initial;
                case CompletionTriggerReason.Insertion:
                    return CompletionFilterReason.Insertion;
                case CompletionTriggerReason.Deletion:
                    return CompletionFilterReason.Deletion;
                default:
                    throw new ArgumentOutOfRangeException(nameof(reason));
            }
        }

        #region Internal methods accessed by the command handlers

        internal void ToggleSuggestionMode()
        {
            _computation.Enqueue((model, token) => ToggleCompletionModeInner(model, token), UpdateUi);
        }

        internal void SelectDown()
        {
            _computation.Enqueue((model, token) => UpdateSelectedItem(model, +1, token), UpdateUi);
        }

        internal void SelectPageDown()
        {
            _computation.Enqueue((model, token) => UpdateSelectedItem(model, +PageStepSize, token), UpdateUi);
        }

        internal void SelectUp()
        {
            _computation.Enqueue((model, token) => UpdateSelectedItem(model, -1, token), UpdateUi);
        }

        internal void SelectPageUp()
        {
            _computation.Enqueue((model, token) => UpdateSelectedItem(model, -PageStepSize, token), UpdateUi);
        }

        #endregion

        private void OnFiltersChanged(object sender, CompletionFilterChangedEventArgs args)
        {
            var taskId = Interlocked.Increment(ref _lastFilteringTaskId);
            _computation.Enqueue((model, token) => UpdateFilters(model, args.Filters, token, taskId), UpdateUi);
        }

        private void OnCommitRequested(object sender, CompletionItemEventArgs args)
        {
            Commit(default(char), args.Item, default(CancellationToken));
        }

        private void OnItemSelected(object sender, CompletionItemSelectedEventArgs args)
        {
            // Note 1: Use this only to react to selection changes initiated by user's mouse\touch operation in the UI, since they cancel the soft selection
            // Note 2: we are not enqueuing a call to update the UI, since this would put us in infinite loop, and the UI is already updated
            _computation.Enqueue((model, token) => UpdateSelectedItem(model, args.SelectedItem, args.SuggestionModeSelected, token)); 
        }

        private void OnGuiClosed(object sender, CompletionClosedEventArgs args)
        {
            ((IAsyncCompletionSession)this).Dismiss();
        }

        bool IAsyncCompletionSession.ShouldCommit(ITextView view, char typeChar, SnapshotPoint triggerLocation)
        {
            foreach (var contentType in CompletionUtilities.GetBuffersForTriggerPoint(view, triggerLocation).Select(b => b.ContentType))
            {
                if (_broker.TryGetKnownCommitCharacters(contentType, view, out var commitChars))
                {
                    if (commitChars.Contains(typeChar))
                    {
                        if (ShouldCommit(view, typeChar, triggerLocation))
                            return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Monitors when user scrolled outside of the completion area.
        /// Note that this event is not raised during regular typing
        /// Also note that typing stretches the completion area
        /// </summary>
        private void OnCaretPositionChanged(object sender, CaretPositionChangedEventArgs e)
        {
            // http://source.roslyn.io/#Microsoft.CodeAnalysis.EditorFeatures/Implementation/IntelliSense/Completion/Controller_CaretPositionChanged.cs,40
            // enqueue a task to see if the caret is still within the broundary
            _computation.Enqueue((model, token) => HandleCaretPositionChanged(model, e.NewPosition), UpdateUi);
        }

        private async Task UpdateUi(CompletionModel model)
        {
            if (_uiFactory == null) return;
            await _jtf.SwitchToMainThreadAsync();
            UpdateUiInner(model);
            await TaskScheduler.Default;
        }

        private void UpdateUiInner(CompletionModel model)
        {
            if (_isDismissed)
                return;

            _telemetryData.UiStopwatch.Restart();
            if (_gui == null)
            {
                _gui = _guardedOperations.CallExtensionPoint(errorSource: _uiFactory, call: () => _uiFactory.GetOrCreate(_view), valueOnThrow: null);
                if (_gui != null)
                {
                    _guardedOperations.CallExtensionPoint(
                        errorSource: _gui,
                        call: () =>
                        {
                            _gui = _uiFactory.GetOrCreate(_view);
                            _gui.Open(new CompletionPresentationViewModel(model.PresentedItems, model.Filters, model.ApplicableSpan, model.UseSoftSelection,
                                model.DisplaySuggestionMode, model.SelectSuggestionMode, model.SelectedIndex, model.SuggestionModeItem, model.SuggestionModeDescription));
                            _gui.FiltersChanged += OnFiltersChanged;
                            _gui.CommitRequested += OnCommitRequested;
                            _gui.CompletionItemSelected += OnItemSelected;
                            _gui.CompletionClosed += OnGuiClosed;
                        });
                }
            }
            else
            {
                _guardedOperations.CallExtensionPoint(
                    errorSource: _gui,
                    call: () => _gui.Update(new CompletionPresentationViewModel(model.PresentedItems, model.Filters, model.ApplicableSpan, model.UseSoftSelection,
                        model.DisplaySuggestionMode, model.SelectSuggestionMode, model.SelectedIndex, model.SuggestionModeItem, model.SuggestionModeDescription)));
            }
            _telemetryData.UiStopwatch.Stop();
            _telemetryData.RecordRendering(_telemetryData.UiStopwatch.ElapsedMilliseconds);
        }

        /// <summary>
        /// Creates a new model and populates it with initial data
        /// </summary>
        private async Task<CompletionModel> GetInitialModel(ITextView view, CompletionTrigger trigger, SnapshotPoint triggerLocation, CancellationToken token)
        {
            _telemetryData.ComputationStopwatch.Restart();
            // Map the trigger location to respective view for each completion provider
            var nestedResults = await Task.WhenAll(
                _completionSources.Select(
                    p => _guardedOperations.CallExtensionPointAsync<CompletionContext>(
                        errorSource: p.Key,
                        asyncCall: () => p.Key.GetCompletionContextAsync(trigger, p.Value, token),
                        valueOnThrow: null
                    )));
            var initialCompletionItems = nestedResults.Where(n => n != null && !n.Items.IsDefaultOrEmpty).SelectMany(n => n.Items).ToImmutableArray();

            var availableFilters = initialCompletionItems
                .SelectMany(n => n.Filters)
                .Distinct()
                .Select(n => new CompletionFilterWithState(n, true))
                .ToImmutableArray();

            // Note: buffer graph is not thread safe. Amadeusz to discuss this and GetExtentOfWord with David Pugh.
            var spans = nestedResults
                .Select(n => n.ApplicableToSpan)
                .Select(s => view.BufferGraph.CreateMappingSpan(s, SpanTrackingMode.EdgeNegative))
                .Select(s => new SnapshotSpan(
                    s.Start.GetPoint(triggerLocation.Snapshot, PositionAffinity.Predecessor).Value,
                    s.End.GetPoint(triggerLocation.Snapshot, PositionAffinity.Predecessor).Value));

            //var extentFromStructureNavigator = _textStructureNavigator?.GetExtentOfWord(triggerLocation - 1).Span;
            var extent = new SnapshotSpan(
                spans.Min(n => n.Start),
                spans.Max(n => n.End));
            var applicableSpan = triggerLocation.Snapshot.CreateTrackingSpan(extent, SpanTrackingMode.EdgeInclusive);

            var useSoftSelection = nestedResults.Any(n => n.UseSoftSelection);
            var useSuggestionMode = nestedResults.Any(n => n.UseSuggestionMode);
            var suggestionModeDescription = nestedResults.FirstOrDefault(n => !string.IsNullOrEmpty(n.SuggestionModeDescription))?.SuggestionModeDescription ?? string.Empty;

            _telemetryData.ComputationStopwatch.Stop();
            _telemetryData.RecordInitialModel(_telemetryData.ComputationStopwatch.ElapsedMilliseconds, _completionSources.Keys, initialCompletionItems.Length, _completionService);

            _telemetryData.ComputationStopwatch.Restart();
            var sortedList = await _guardedOperations.CallExtensionPointAsync(
                errorSource: _completionService,
                asyncCall: () => _completionService.SortCompletionListAsync(initialCompletionItems, trigger.Reason, triggerLocation.Snapshot, applicableSpan, _view, token),
                valueOnThrow: initialCompletionItems);
            _telemetryData.ComputationStopwatch.Stop();
            _telemetryData.RecordProcessing(_telemetryData.ComputationStopwatch.ElapsedMilliseconds, initialCompletionItems.Length);

            // Do not continue with empty session
            if (initialCompletionItems.IsEmpty)
                ((IAsyncCompletionSession)this).Dismiss();

            return new CompletionModel(initialCompletionItems, sortedList, applicableSpan, trigger.Reason, triggerLocation.Snapshot,
                availableFilters, useSoftSelection, useSuggestionMode, suggestionModeDescription, suggestionModeItem: null);
        }

        /// <summary>
        /// User has moved the caret. Ensure that the caret is still within the applicable span. If not, dismiss the session.
        /// </summary>
        /// <returns></returns>
        private Task<CompletionModel> HandleCaretPositionChanged(CompletionModel model, CaretPosition caretPosition)
        {
            if (!(model.ApplicableSpan.GetSpan(caretPosition.VirtualBufferPosition.Position.Snapshot).IntersectsWith(new SnapshotSpan(caretPosition.VirtualBufferPosition.Position, 0))))
            {
                ((IAsyncCompletionSession)this).Dismiss();
            }
            return Task.FromResult(model);
        }

        /// <summary>
        /// User has moved the caret. Ensure that the caret is still within the applicable span. If not, dismiss the session.
        /// </summary>
        /// <returns></returns>
        private Task<CompletionModel> ToggleCompletionModeInner(CompletionModel model, CancellationToken token)
        {
            return Task.FromResult(model.WithSuggestionModeActive(!model.DisplaySuggestionMode));
        }

        /// <summary>
        /// User has typed
        /// </summary>
        private async Task<CompletionModel> UpdateSnapshot(CompletionModel model, CompletionTrigger trigger, CompletionFilterReason filterReason, SnapshotPoint triggerLocation, CancellationToken token, int thisId)
        {
            // Filtering got preempted, so store the most recent snapshot for the next time we filter
            if (token.IsCancellationRequested || thisId != _lastFilteringTaskId)
                return model.WithSnapshot(triggerLocation.Snapshot);

            // Dismiss if we are outside of the applicable span
            // TODO: dismiss if applicable span was reduced to zero AND moved again (e.g. first backspace keeps completion open, second backspace closes it)
            if (triggerLocation < model.ApplicableSpan.GetStartPoint(triggerLocation.Snapshot) || triggerLocation > model.ApplicableSpan.GetEndPoint(triggerLocation.Snapshot))
            {
                ((IAsyncCompletionSession)this).Dismiss();
                return model;
            }

            _telemetryData.ComputationStopwatch.Restart();

            var filteredCompletion = await _guardedOperations.CallExtensionPointAsync(
                errorSource: _completionService,
                asyncCall: () => _completionService.UpdateCompletionListAsync(
                    model.InitialItems,
                    model.InitialTriggerReason,
                    filterReason,
                    triggerLocation.Snapshot,
                    model.ApplicableSpan,
                    model.Filters,
                    _view,
                    token),
                valueOnThrow: null);

            // Handle error cases by logging the issue and dismissing the session.
            if (filteredCompletion == null)
            {
                ((IAsyncCompletionSession)this).Dismiss();
                return model;
            }

            // Prevent null references when service returns default(ImmutableArray)
            ImmutableArray<CompletionItemWithHighlight> returnedItems = filteredCompletion.Items.IsDefault
                ? ImmutableArray<CompletionItemWithHighlight>.Empty
                : filteredCompletion.Items;

            _telemetryData.ComputationStopwatch.Stop();
            _telemetryData.RecordProcessing(_telemetryData.ComputationStopwatch.ElapsedMilliseconds, returnedItems.Length);

            if (filteredCompletion.SelectionMode == CompletionItemSelection.SoftSelected)
                model = model.WithSoftSelection(true);
            else if (filteredCompletion.SelectionMode == CompletionItemSelection.Selected)
                model = model.WithSoftSelection(false);

            // Prepare the suggestionModeItem if we ever change the mode
            var enteredText = model.ApplicableSpan.GetText(triggerLocation.Snapshot);
            var suggestionModeItem = new CompletionItem(enteredText, SuggestionModeCompletionItemSource.Instance);

            _guardedOperations.RaiseEvent(this, ItemsUpdated, new CompletionItemsWithHighlightEventArgs(returnedItems));

            // TODO: combine this chain into a single method:
            return model.WithSnapshot(triggerLocation.Snapshot).WithUniqueItem(filteredCompletion.UniqueItem)
                .WithSuggestionModeItem(suggestionModeItem).WithPresentedItems(returnedItems, filteredCompletion.SelectedItemIndex);
        }

        /// <summary>
        /// Reacts to user toggling a filter
        /// </summary>
        /// <param name="newFilters">Filters with updated Selected state, as indicated by the user.</param>
        private async Task<CompletionModel> UpdateFilters(CompletionModel model, ImmutableArray<CompletionFilterWithState> newFilters, CancellationToken token, int thisId)
        {
            _telemetryData.RecordChangingFilters();

            // Filtering got preempted, so store the most updated filters for the next time we filter
            if (token.IsCancellationRequested || thisId != _lastFilteringTaskId)
                return model.WithFilters(newFilters);

            var filteredCompletion = await _guardedOperations.CallExtensionPointAsync(
                errorSource: _completionService,
                asyncCall: () => _completionService.UpdateCompletionListAsync(
                    model.InitialItems,
                    model.InitialTriggerReason,
                    CompletionFilterReason.FilterChange,
                    model.Snapshot,
                    model.ApplicableSpan,
                    newFilters,
                    _view,
                    token),
                valueOnThrow: null);

            // Handle error cases by logging the issue and discarding the request to filter
            if (filteredCompletion == null)
                return model;
            if (filteredCompletion.Filters.Length != newFilters.Length)
            {
                _guardedOperations.HandleException(
                    errorSource: _completionService,
                    e: new InvalidOperationException("Completion service returned incorrect set of filters."));
                return model;
            }

            return model.WithFilters(filteredCompletion.Filters).WithPresentedItems(filteredCompletion.Items, filteredCompletion.SelectedItemIndex);
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        /// <summary>
        /// Reacts to user scrolling the list
        /// </summary>
        private async Task<CompletionModel> UpdateSelectedItem(CompletionModel model, int offset, CancellationToken token)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            _telemetryData.RecordScrolling();

            if (!model.PresentedItems.Any())
            {
                // No-op if there are no items
                if (model.DisplaySuggestionMode)
                {
                    // Unless there is a suggestion mode item. Select it.
                    return model.WithSuggestionItemSelected();
                }
                return model;
            }

            var lastIndex = model.PresentedItems.Count() - 1;
            var currentIndex = model.SelectSuggestionMode ? -1 : model.SelectedIndex;

            if (offset > 0) // Scrolling down. Stop at last index then go to first index.
            {
                if (currentIndex == lastIndex)
                {
                    if (model.DisplaySuggestionMode)
                        return model.WithSuggestionItemSelected();
                    else
                        return model.WithSelectedIndex(FirstIndex);
                }
                var newIndex = currentIndex + offset;
                return model.WithSelectedIndex(Math.Min(newIndex, lastIndex));
            }
            else // Scrolling up. Stop at first index then go to last index.
            {
                if (currentIndex < FirstIndex)
                {
                    // Suggestion mode item is selected. Go to the last item.
                    return model.WithSelectedIndex(lastIndex);
                }
                else if (currentIndex == FirstIndex)
                {
                    // The first item is selected. If there is a suggestion, select it.
                    if (model.DisplaySuggestionMode)
                        return model.WithSuggestionItemSelected();
                    else
                        return model.WithSelectedIndex(lastIndex);
                }
                var newIndex = currentIndex + offset;
                return model.WithSelectedIndex(Math.Max(newIndex, FirstIndex));
            }
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        /// <summary>
        /// Reacts to user selecting a specific item in the list
        /// </summary>
        private async Task<CompletionModel> UpdateSelectedItem(CompletionModel model, CompletionItem selectedItem, bool suggestionModeSelected, CancellationToken token)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            if (suggestionModeSelected)
            {
                return model.WithSuggestionItemSelected();
            }
            else
            {
                for (int i = 0; i < model.PresentedItems.Length; i++)
                {
                    if (model.PresentedItems[i].CompletionItem == selectedItem)
                    {
                        return model.WithSelectedIndex(i);
                    }
                }
                // This item is not in the model
                return model;
            }
        }

        /// <summary>
        /// This method creates a mapping point from the top-buffer trigger location
        /// then iterates through completion item sources pertinent to this session.
        /// It maps triggerLocation to each source's buffer
        /// Ensures that we work only with sources applicable to current location
        /// and returns whether any source would like to commit completion
        /// </summary>
        public bool ShouldCommit(ITextView view, char typeChar, SnapshotPoint triggerLocation)
        {
            var mappingPoint = view.BufferGraph.CreateMappingPoint(triggerLocation, PointTrackingMode.Negative);
            return _completionSources
                .Select(p => (p, mappingPoint.GetPoint(p.Value.Snapshot.TextBuffer, PositionAffinity.Predecessor)))
                .Where(n => n.Item2.HasValue)
                .Any(n => _guardedOperations.CallExtensionPoint(
                    errorSource: n.Item1.Key,
                    call: () => n.Item1.Key.ShouldCommitCompletion(typeChar, n.Item2.Value),
                    valueOnThrow: false));
        }

        public ImmutableArray<CompletionItem> GetVisibleItems(CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public CompletionItem GetSelectedItem(CancellationToken token)
        {
            throw new NotImplementedException();
        }

        private class TelemetryData
        {
            // Constants
            internal const string CommitEvent = "VS/Editor/Completion/Committed";
            internal const string DismissedEvent = "VS/Editor/Completion/Dismissed";

            // Collected data
            internal string InitialItemSources { get; private set; }
            internal int InitialItemCount { get; private set; }
            internal long InitialModelDuration { get; private set; }

            internal string CompletionService { get; private set; }
            internal long TotalProcessingDuration { get; private set; }
            internal int TotalProcessingCount { get; private set; }

            internal long InitialRenderingDuration { get; private set; }
            internal long TotalRenderingDuration { get; private set; }
            internal int TotalRenderingCount { get; private set; }

            internal bool UserEverScrolled { get; private set; }
            internal bool UserEverSetFilters { get; private set; }
            internal bool UserLastScrolled { get; private set; }
            internal int LastItemCount { get; private set; }

            // Property names
            internal const string InitialItemSourcesProperty = "Property.InitialData.Sources";
            internal const string InitialItemCountProperty = "Property.ItemCount.Initial";
            internal const string InitialModelDurationProperty = "Property.InitialData.Duration";
            internal const string CompletionServiceProperty = "Property.Processing.Service";
            internal const string TotalProcessingDurationProperty = "Property.Processing.TotalDuration";
            internal const string TotalProcessingCountProperty = "Property.Processing.TotalCount";
            internal const string InitialRenderingDurationProperty = "Property.Rendering.InitialDuration";
            internal const string TotalRenderingDurationProperty = "Property.Rendering.TotalDuration";
            internal const string TotalRenderingCountProperty = "Property.Rendering.TotalCount";
            internal const string UserEverScrolledProperty = "Property.UserScrolled.Session";
            internal const string UserEverSetFiltersProperty = "Property.UserSetFilters.Session";
            internal const string UserLastScrolledProperty = "Property.UserScrolled.Last";
            internal const string LastItemCountProperty = "Property.ItemCount.Final";
            internal const string CustomCommitProperty = "Property.Commit.CustomCommit";
            internal const string CommittedItemSourceProperty = "Property.Commit.ItemSource";
            internal const string CommitDurationProperty = "Property.Commit.Duration";
            internal const string CommitTriggerProperty = "Property.Commit.Trigger";

            /// <summary>
            /// Tracks time spent on the worker thread - getting data, filtering and sorting
            /// </summary>
            internal Stopwatch ComputationStopwatch { get; }

            /// <summary>
            /// Tracks time spent on the UI thread - either rendering or committing
            /// </summary>
            internal Stopwatch UiStopwatch { get; }

            private readonly AsyncCompletionBroker _broker;
            private readonly ILoggingServiceInternal _logger;

            public TelemetryData(AsyncCompletionBroker broker)
            {
                ComputationStopwatch = new Stopwatch();
                UiStopwatch = new Stopwatch();
                _broker = broker;
                _logger = broker.Logger;
            }

            internal string GetItemSourceName(IAsyncCompletionItemSource source) => _broker.GetItemSourceName(source);
            internal string GetCompletionServiceName(IAsyncCompletionService service) => _broker.GetCompletionServiceName(service);

            internal void RecordInitialModel(long processingTime, ICollection<IAsyncCompletionItemSource> sources, int initialItemCount, IAsyncCompletionService service)
            {
                InitialItemCount = initialItemCount;
                InitialItemSources = string.Join(" ", sources.Select(n => _broker.GetItemSourceName(n)));
                InitialModelDuration = processingTime;
                CompletionService = _broker.GetCompletionServiceName(service); // Service does not participate in getting the initial model, but this is a good place to get this data
            }

            internal void RecordProcessing(long processingTime, int itemCount)
            {
                UserLastScrolled = false;
                TotalProcessingCount++;
                TotalProcessingDuration += processingTime;
                LastItemCount = itemCount;
            }

            internal void RecordRendering(long processingTime)
            {
                if (TotalRenderingCount == 0)
                    InitialRenderingDuration = processingTime;
                TotalRenderingCount++;
                TotalRenderingDuration += processingTime;
            }

            internal void RecordScrolling()
            {
                UserEverScrolled = true;
                UserLastScrolled = true;
            }

            internal void RecordChangingFilters()
            {
                UserEverSetFilters = true;
            }

            internal void RecordCommitAndSend(long commitDuration, CompletionItem committedItem, char typeChar)
            {
                _logger?.PostEvent(TelemetryEventType.Operation,
                    TelemetryData.CommitEvent,
                    TelemetryResult.Success,
                    (InitialItemSourcesProperty, InitialItemSources ?? string.Empty),
                    (InitialItemCountProperty, InitialItemCount),
                    (InitialModelDurationProperty, InitialModelDuration),
                    (CompletionServiceProperty, CompletionService ?? string.Empty),
                    (TotalProcessingDurationProperty, TotalProcessingDuration),
                    (TotalProcessingCountProperty, TotalProcessingCount),
                    (InitialRenderingDurationProperty, InitialRenderingDuration),
                    (TotalRenderingDurationProperty, TotalRenderingDuration),
                    (TotalRenderingCountProperty, TotalRenderingCount),
                    (UserEverScrolledProperty, UserEverScrolled),
                    (UserEverSetFiltersProperty, UserEverSetFilters),
                    (UserLastScrolledProperty, UserLastScrolled),
                    (LastItemCountProperty, LastItemCount),
                    (CustomCommitProperty, committedItem.UseCustomCommit),
                    (CommittedItemSourceProperty, GetItemSourceName(committedItem.Source) ?? string.Empty),
                    (CommitDurationProperty, commitDuration),
                    (CommitTriggerProperty, typeChar)
                );
            }

            internal void RecordDismissedAndSend()
            {
                _logger?.PostEvent(TelemetryEventType.Operation,
                    TelemetryData.DismissedEvent,
                    TelemetryResult.Success,
                    (InitialItemSourcesProperty, InitialItemSources ?? string.Empty),
                    (InitialItemCountProperty, InitialItemCount),
                    (InitialModelDurationProperty, InitialModelDuration),
                    (CompletionServiceProperty, CompletionService ?? string.Empty),
                    (LastItemCountProperty, LastItemCount),
                    (TotalProcessingDurationProperty, TotalProcessingDuration),
                    (TotalProcessingCountProperty, TotalProcessingCount),
                    (InitialRenderingDurationProperty, InitialRenderingDuration),
                    (TotalRenderingDurationProperty, TotalRenderingDuration),
                    (TotalRenderingCountProperty, TotalRenderingCount),
                    (UserEverScrolledProperty, UserEverScrolled),
                    (UserEverSetFiltersProperty, UserEverSetFilters),
                    (UserLastScrolledProperty, UserLastScrolled)
                );
            }
        }
    }
}
