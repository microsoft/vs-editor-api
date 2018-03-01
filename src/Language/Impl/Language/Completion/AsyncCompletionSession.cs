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
    internal class AsyncCompletionSession : IAsyncCompletionSession, ICompletionComputationCallbackHandler<CompletionModel>
    {
        // Available data and services
        private readonly IDictionary<IAsyncCompletionItemSource, SnapshotSpan> _completionSources;
        private readonly IAsyncCompletionService _completionService;
        private readonly SnapshotSpan _initialApplicableSpan;
        private readonly JoinableTaskFactory _jtf;
        private readonly ICompletionPresenterProvider _presenterProvider;
        private readonly AsyncCompletionBroker _broker;
        private readonly ITextView _view;
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

        // Telemetry:

        private readonly CompletionSessionTelemetry _telemetry;

        /// <summary>
        /// Tracks time spent on the worker thread - getting data, filtering and sorting. Used for telemetry.
        /// </summary>
        internal Stopwatch ComputationStopwatch { get; } = new Stopwatch();

        /// <summary>
        /// Tracks time spent on the UI thread - either rendering or committing. Used for telemetry.
        /// </summary>
        internal Stopwatch UiStopwatch { get; } = new Stopwatch();

        // When set, UI will no longer be updated
        private bool _isDismissed;

        public event EventHandler<CompletionItemEventArgs> ItemCommitted;
        public event EventHandler Dismissed;
        public event EventHandler<CompletionItemsWithHighlightEventArgs> ItemsUpdated;

        public ITextView TextView => _view;

        public bool IsDismissed => _isDismissed;

        public AsyncCompletionSession(SnapshotSpan applicableSpan, JoinableTaskFactory jtf, ICompletionPresenterProvider presenterProvider,
            IDictionary<IAsyncCompletionItemSource, SnapshotSpan> completionSources,
            IAsyncCompletionService completionService, AsyncCompletionBroker broker, ITextView view, CompletionTelemetryHost telemetryHost)
        {
            _initialApplicableSpan = applicableSpan;
            _jtf = jtf;
            _presenterProvider = presenterProvider;
            _broker = broker;
            _completionSources = completionSources;
            _completionService = completionService;
            _view = view;
            _textStructureNavigator = broker.TextStructureNavigatorSelectorService?.GetTextStructureNavigator(view.TextBuffer);
            _guardedOperations = broker.GuardedOperations;
            _telemetry = new CompletionSessionTelemetry(telemetryHost, completionService, presenterProvider);
            PageStepSize = presenterProvider?.ResultsPerPage ?? 1;
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
        CustomCommitBehavior IAsyncCompletionSession.Commit(CancellationToken token, char typedChar)
        {
            var lastModel = _computation.WaitAndGetResult();

            if (lastModel.UseSoftSelection && !typedChar.Equals(default(char)))
            {
                // In soft selection mode, user commits explicitly (click, tab, e.g. not tied to a text change). Otherwise, we dismiss the session
                ((IAsyncCompletionSession)this).Dismiss();
                return CustomCommitBehavior.None;
            }
            else if (lastModel.SelectSuggestionMode && string.IsNullOrWhiteSpace(lastModel.SuggestionModeItem?.InsertText))
            {
                // In suggestion mode, don't commit empty suggestion (suggestion item temporarily shows description of suggestion mode)
                return CustomCommitBehavior.None;
            }
            else if (lastModel.SelectSuggestionMode)
            {
                // Commit the suggestion mode item
                return Commit(typedChar, lastModel.SuggestionModeItem, token);
            }
            else if (!lastModel.PresentedItems.Any())
            {
                // There is nothing to commit
                Dismiss();
                return CustomCommitBehavior.None;
            }
            else
            {
                // Regular commit
                return Commit(typedChar, lastModel.PresentedItems[lastModel.SelectedIndex].CompletionItem, token);
            }
        }

        private CustomCommitBehavior Commit(char typedChar, CompletionItem itemToCommit, CancellationToken token)
        {
            CustomCommitBehavior result = CustomCommitBehavior.None;
            var lastModel = _computation.WaitAndGetResult();

            if (!_jtf.Context.IsOnMainThread)
                throw new InvalidOperationException($"{nameof(IAsyncCompletionSession)}.{nameof(IAsyncCompletionSession.Commit)} must be callled from UI thread.");

            UiStopwatch.Restart();
            if (itemToCommit.UseCustomCommit)
            {
                // Pass appropriate buffer to the item's provider
                var buffer = _completionSources[itemToCommit.Source].Snapshot.TextBuffer;
                result = _guardedOperations.CallExtensionPoint(
                    errorSource: itemToCommit.Source,
                    call: () => itemToCommit.Source.CustomCommit(_view, buffer, itemToCommit, lastModel.ApplicableSpan, typedChar, token),
                    valueOnThrow: CustomCommitBehavior.None);
            }
            else
            {
                InsertIntoBuffer(_view, lastModel, itemToCommit.InsertText, typedChar);
            }
            UiStopwatch.Stop();
            _telemetry.RecordCommitted(UiStopwatch.ElapsedMilliseconds, itemToCommit);
            _guardedOperations.RaiseEvent(this, ItemCommitted, new CompletionItemEventArgs(itemToCommit));
            Dismiss();
            return result;
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

        public void Dismiss()
        {
            if (_isDismissed)
                return;

            _isDismissed = true;
            _broker.DismissSession(this);
            _guardedOperations.RaiseEvent(this, Dismissed);
            _view.Caret.PositionChanged -= OnCaretPositionChanged;
            _computationCancellation.Cancel();
            _computation = null;

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
                _computation = new ModelComputation<CompletionModel>(PrioritizedTaskScheduler.AboveNormalInstance, _computationCancellation.Token, _guardedOperations, this);
                _computation.Enqueue((model, token) => GetInitialModel(view, trigger, triggerLocation, token), updateUi: false);
            }

            var taskId = Interlocked.Increment(ref _lastFilteringTaskId);
            _computation.Enqueue((model, token) => UpdateSnapshot(model, trigger, FromCompletionTriggerReason(trigger.Reason), triggerLocation, token, taskId), updateUi: true);
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
            _computation.Enqueue((model, token) => ToggleCompletionModeInner(model, token), updateUi: true);
        }

        internal void SelectDown()
        {
            _computation.Enqueue((model, token) => UpdateSelectedItem(model, +1, token), updateUi: true);
        }

        internal void SelectPageDown()
        {
            _computation.Enqueue((model, token) => UpdateSelectedItem(model, +PageStepSize, token), updateUi: true);
        }

        internal void SelectUp()
        {
            _computation.Enqueue((model, token) => UpdateSelectedItem(model, -1, token), updateUi: true);
        }

        internal void SelectPageUp()
        {
            _computation.Enqueue((model, token) => UpdateSelectedItem(model, -PageStepSize, token), updateUi: true);
        }

        #endregion

        private void OnFiltersChanged(object sender, CompletionFilterChangedEventArgs args)
        {
            var taskId = Interlocked.Increment(ref _lastFilteringTaskId);
            _computation.Enqueue((model, token) => UpdateFilters(model, args.Filters, token, taskId), updateUi: true);
        }

        private void OnCommitRequested(object sender, CompletionItemEventArgs args)
        {
            Commit(default(char), args.Item, default(CancellationToken));
        }

        private void OnItemSelected(object sender, CompletionItemSelectedEventArgs args)
        {
            // Note 1: Use this only to react to selection changes initiated by user's mouse\touch operation in the UI, since they cancel the soft selection
            // Note 2: we are not enqueuing a call to update the UI, since this would put us in infinite loop, and the UI is already updated
            _computation.Enqueue((model, token) => UpdateSelectedItem(model, args.SelectedItem, args.SuggestionModeSelected, token), updateUi: false);
        }

        private void OnGuiClosed(object sender, CompletionClosedEventArgs args)
        {
            Dismiss();
        }

        bool IAsyncCompletionSession.ShouldCommit(ITextView view, char typeChar, SnapshotPoint triggerLocation)
        {
            foreach (var contentType in CompletionUtilities.GetBuffersForPoint(view, triggerLocation).Select(b => b.ContentType))
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
            _computation.Enqueue((model, token) => HandleCaretPositionChanged(model, e.NewPosition), updateUi: true);
        }

        async Task ICompletionComputationCallbackHandler<CompletionModel>.UpdateUi(CompletionModel model)
        {
            if (_presenterProvider == null) return;
            await _jtf.SwitchToMainThreadAsync();
            UpdateUiInner(model);
            await TaskScheduler.Default;
        }

        /// <summary>
        /// Opens or updates the UI. Must be called on UI thread.
        /// </summary>
        /// <param name="model"></param>
        private void UpdateUiInner(CompletionModel model)
        {
            if (_isDismissed)
                return;
            // TODO: Consider building CompletionPresentationViewModel in BG and passing it here

            UiStopwatch.Restart();
            if (_gui == null)
            {
                _gui = _guardedOperations.CallExtensionPoint(errorSource: _presenterProvider, call: () => _presenterProvider.GetOrCreate(_view), valueOnThrow: null);
                if (_gui != null)
                {
                    _guardedOperations.CallExtensionPoint(
                        errorSource: _gui,
                        call: () =>
                        {
                            _gui = _presenterProvider.GetOrCreate(_view);
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
            UiStopwatch.Stop();
            _telemetry.RecordRendering(UiStopwatch.ElapsedMilliseconds);
        }

        /// <summary>
        /// Creates a new model and populates it with initial data
        /// </summary>
        private async Task<CompletionModel> GetInitialModel(ITextView view, CompletionTrigger trigger, SnapshotPoint triggerLocation, CancellationToken token)
        {
            // Map the trigger location to respective view for each completion provider
            var nestedResults = await Task.WhenAll(
                _completionSources.Select(
                    p => _guardedOperations.CallExtensionPointAsync<CompletionContext>(
                        errorSource: p.Key,
                        asyncCall: () => p.Key.GetCompletionContextAsync(trigger, p.Value, token),
                        valueOnThrow: null
                    )));
            var initialCompletionItems = nestedResults.Where(n => n != null && !n.Items.IsDefaultOrEmpty).SelectMany(n => n.Items).ToImmutableArray();

            // Do not continue with empty session
            if (initialCompletionItems.IsEmpty)
            {
                ((IAsyncCompletionSession)this).Dismiss();
                return default(CompletionModel);
            }

            var availableFilters = initialCompletionItems
                .SelectMany(n => n.Filters)
                .Distinct()
                .Select(n => new CompletionFilterWithState(n, true))
                .ToImmutableArray();

            var applicableSpan = triggerLocation.Snapshot.CreateTrackingSpan(_initialApplicableSpan, SpanTrackingMode.EdgeInclusive);

            var useSoftSelection = nestedResults.Any(n => n != null && n.UseSoftSelection);
            var useSuggestionMode = nestedResults.Any(n => n != null && n.UseSuggestionMode);
            var suggestionModeDescription = nestedResults.FirstOrDefault(n => !string.IsNullOrEmpty(n?.SuggestionModeDescription))?.SuggestionModeDescription ?? string.Empty;

#if DEBUG
            Debug.WriteLine("Completion session got data.");
            Debug.WriteLine("Sources: " + String.Join(", ", _completionSources.Select(n => n.Key.GetType())));
            Debug.WriteLine("Service: " + _completionService.GetType());
            Debug.WriteLine("Filters: " + String.Join(", ", availableFilters.Select(n => n.Filter.DisplayText)));
            Debug.WriteLine("Span: " + _initialApplicableSpan.GetText());
#endif

            ComputationStopwatch.Restart();
            var sortedList = await _guardedOperations.CallExtensionPointAsync(
                errorSource: _completionService,
                asyncCall: () => _completionService.SortCompletionListAsync(initialCompletionItems, trigger.Reason, triggerLocation.Snapshot, applicableSpan, _view, token),
                valueOnThrow: initialCompletionItems);
            ComputationStopwatch.Stop();
            _telemetry.RecordProcessing(ComputationStopwatch.ElapsedMilliseconds, initialCompletionItems.Length);
            _telemetry.RecordKeystroke();

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
            // Always record keystrokes, even if filtering is preempted
            _telemetry.RecordKeystroke();

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

            ComputationStopwatch.Restart();

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

            ComputationStopwatch.Stop();
            _telemetry.RecordProcessing(ComputationStopwatch.ElapsedMilliseconds, returnedItems.Length);

            if (filteredCompletion.SelectionMode == CompletionItemSelection.SoftSelected)
                model = model.WithSoftSelection(true);
            else if (filteredCompletion.SelectionMode == CompletionItemSelection.Selected)
                model = model.WithSoftSelection(false);

            // Prepare the suggestionModeItem if we ever change the mode
            var enteredText = model.ApplicableSpan.GetText(triggerLocation.Snapshot);
            var suggestionModeItem = new CompletionItem(enteredText, SuggestionModeCompletionItemSource.Instance);

            _guardedOperations.RaiseEvent(this, ItemsUpdated, new CompletionItemsWithHighlightEventArgs(returnedItems));

            return model.WithSnapshotAndItems(triggerLocation.Snapshot, returnedItems, filteredCompletion.SelectedItemIndex, filteredCompletion.UniqueItem, suggestionModeItem);
        }

        /// <summary>
        /// Reacts to user toggling a filter
        /// </summary>
        /// <param name="newFilters">Filters with updated Selected state, as indicated by the user.</param>
        private async Task<CompletionModel> UpdateFilters(CompletionModel model, ImmutableArray<CompletionFilterWithState> newFilters, CancellationToken token, int thisId)
        {
            _telemetry.RecordChangingFilters();
            _telemetry.RecordKeystroke();

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
        /// Reacts to user scrolling the list using keyboard
        /// </summary>
        private async Task<CompletionModel> UpdateSelectedItem(CompletionModel model, int offset, CancellationToken token)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            _telemetry.RecordScrolling();
            _telemetry.RecordKeystroke();

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
            _telemetry.RecordScrolling();
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
            // TODO: Consider returning array of CompletionItemWithHighlight so that we don't do linq here
            return _computation.RecentModel.PresentedItems.Select(n => n.CompletionItem).ToImmutableArray();
        }

        public CompletionItem GetSelectedItem(CancellationToken token)
        {
            var model = _computation.RecentModel;
            if (model.SelectSuggestionMode)
                return model.SuggestionModeItem;
            else
                return model.PresentedItems[model.SelectedIndex].CompletionItem;
        }
    }
}
