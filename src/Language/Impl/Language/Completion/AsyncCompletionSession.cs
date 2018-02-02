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
using Microsoft.VisualStudio.Text.Utilities;
using Microsoft.VisualStudio.Threading;

namespace Microsoft.VisualStudio.Language.Intellisense.Implementation
{
    /// <summary>
    /// Holds a state of the session
    /// and a reference to the UI element
    /// </summary>
    internal class AsyncCompletionSession : IAsyncCompletionSession
    {
        // Available data and services
        // TODO: consider storing MappingPoint instead of SnapshotPoint
        private readonly IDictionary<IAsyncCompletionItemSource, SnapshotPoint> _completionProviders;
        private readonly IAsyncCompletionService _completionService;
        private readonly JoinableTaskFactory _jtf;
        private readonly ICompletionUIProvider _uiFactory;
        private readonly IAsyncCompletionBroker _broker;
        private readonly ITextView _view;
        private readonly TelemetryData _telemetryData;

        // Presentation:
        ICompletionUI _gui; // Must be accessed from GUI thread
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

        public AsyncCompletionSession(JoinableTaskFactory jtf, ICompletionUIProvider uiFactory, IDictionary<IAsyncCompletionItemSource, SnapshotPoint> providers, IAsyncCompletionService service, AsyncCompletionBroker broker, ITextView view)
        {
            _jtf = jtf;
            _uiFactory = uiFactory;
            _broker = broker;
            _completionProviders = providers;
            _completionService = service;
            _view = view;
            _telemetryData = new TelemetryData(broker);
            PageStepSize = uiFactory?.ResultsPerPage ?? 1;
            _view.Caret.PositionChanged += OnCaretPositionChanged;
        }

        public void Commit(string edit)
        {
            var lastModel = _computation.WaitAndGetResult();

            if (lastModel.UseSuggestionMode && !String.IsNullOrEmpty(edit))
                return; // In suggestion mode, allow only explicit commits (click, tab, e.g. not tied to a text change)
            else if (lastModel.SelectSuggestionMode && lastModel.SuggestionIsEmpty)
                return; // In suggestion mode, don't commit empty suggestion (suggestion item temporarily shows description of suggestion mode)
            else if (lastModel.SelectSuggestionMode && !lastModel.SuggestionIsEmpty)
                Commit(edit, lastModel.SuggestionModeItem.CompletionItem);
            else if (!lastModel.PresentedItems.Any())
                return; // There is nothing to commit
            else
                Commit(edit, lastModel.PresentedItems.ElementAt(lastModel.SelectedIndex).CompletionItem);
        }

        public void Commit(string edit, CompletionItem itemToCommit)
        {
            var lastModel = _computation.WaitAndGetResult();

            // TODO: ensure we are on the UI thread

            _telemetryData.UiStopwatch.Restart();
            if (itemToCommit.CustomCommit)
            {
                // Pass appropriate buffer to the item's provider
                var buffer = _completionProviders[itemToCommit.Source].Snapshot.TextBuffer;
                itemToCommit.Source.CustomCommit(_view, buffer, itemToCommit, lastModel.ApplicableSpan, edit);
            }
            else
            {
                var buffer = _view.TextBuffer;
                var bufferEdit = buffer.CreateEdit();
                bufferEdit.Replace(lastModel.ApplicableSpan.GetSpan(buffer.CurrentSnapshot), itemToCommit.InsertText + edit);
                bufferEdit.Apply();
            }
            _telemetryData.UiStopwatch.Stop();
            _telemetryData.RecordCommitAndSend(_telemetryData.UiStopwatch.ElapsedMilliseconds, itemToCommit, edit);
            _committed = true;
        }

        void IAsyncCompletionSession.DismissAndHide()
        {
            // TODO: protect from race conditions when we get two Dismiss requests
            _isDismissed = true;
            _view.Caret.PositionChanged -= OnCaretPositionChanged;
            _computationCancellation.Cancel();

            if (!_committed)
                _telemetryData.RecordDismissedAndSend();

            if (_gui == null)
                return; // nothing to hide

            // TODO: ensure we are on the UI thread
            _gui.FiltersChanged -= OnFiltersChanged;
            _gui.CompletionItemCommitted -= OnItemCommitted;
            _gui.Hide();
            _gui.Dispose();
        }

        public void OpenOrUpdate(ITextView view, ITrackingSpan trackedEdit, CompletionTrigger trigger, SnapshotPoint triggerLocation)
        {
            if (_computation == null)
            {
                _computation = new ModelComputation<CompletionModel>(PrioritizedTaskScheduler.AboveNormalInstance, _computationCancellation.Token);
                _computation.Enqueue((model, token) => GetInitialModel(view, trackedEdit, trigger, triggerLocation, token));
            }

            var taskId = Interlocked.Increment(ref _lastFilteringTaskId);
            _computation.Enqueue((model, token) => UpdateSnapshot(model, trigger, triggerLocation, token, taskId), UpdateUI);
        }

        public void SelectDown()
        {
            _computation.Enqueue((model, token) => UpdateSelectedItem(model, +1, token), UpdateUI);
        }

        public void SelectPageDown()
        {
            _computation.Enqueue((model, token) => UpdateSelectedItem(model, +PageStepSize, token), UpdateUI);
        }

        public void SelectUp()
        {
            _computation.Enqueue((model, token) => UpdateSelectedItem(model, -1, token), UpdateUI);
        }

        public void SelectPageUp()
        {
            _computation.Enqueue((model, token) => UpdateSelectedItem(model, -PageStepSize, token), UpdateUI);
        }

        private void OnFiltersChanged(object sender, CompletionFilterChangedEventArgs e)
        {
            var taskId = Interlocked.Increment(ref _lastFilteringTaskId);
            _computation.Enqueue((model, token) => UpdateFilters(model, e.Filters, token, taskId), UpdateUI);
        }

        private void OnItemCommitted(object sender, CompletionItemCommittedEventArgs e)
        {
            Commit(String.Empty, e.Item);
            _broker.Dismiss(_view);
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
            _computation.Enqueue((model, token) => HandleCaretPositionChanged(model, e.NewPosition), UpdateUI);
        }

        private async Task UpdateUI(CompletionModel model)
        {
            if (_uiFactory == null) return;
            await _jtf.SwitchToMainThreadAsync();

            if (_isDismissed)
                return;

            _telemetryData.UiStopwatch.Restart();
            if (_gui == null)
            {
                _gui = _uiFactory.GetUI(_view);
                _gui.Open(new CompletionPresentation(model.PresentedItems, model.Filters, model.ApplicableSpan, model.UseSoftSelection, model.UseSuggestionMode, model.SelectSuggestionMode, model.SelectedIndex, model.SuggestionModeItem));
                _gui.FiltersChanged += OnFiltersChanged;
                _gui.CompletionItemCommitted += OnItemCommitted;
            }
            else
            {
                _gui.Update(new CompletionPresentation(model.PresentedItems, model.Filters, model.ApplicableSpan, model.UseSoftSelection, model.UseSuggestionMode, model.SelectSuggestionMode, model.SelectedIndex, model.SuggestionModeItem));
            }
            _telemetryData.UiStopwatch.Stop();
            _telemetryData.RecordRendering(_telemetryData.UiStopwatch.ElapsedMilliseconds);

            await TaskScheduler.Default;
        }

        /// <summary>
        /// Creates a new model and populates it with initial data
        /// </summary>
        private async Task<CompletionModel> GetInitialModel(ITextView view, ITrackingSpan trackedEdit, CompletionTrigger trigger, SnapshotPoint triggerLocation, CancellationToken token)
        {
            _telemetryData.ComputationStopwatch.Restart();
            // Map the trigger location to respective view for each completion provider
            var nestedResults = await Task.WhenAll(_completionProviders.Select(p => p.Key.GetCompletionContextAsync(trigger, p.Value)));
            var originalCompletionItems = nestedResults.SelectMany(n => n.Items).ToImmutableArray();

            var availableFilters = nestedResults
                .SelectMany(n => n.Items)
                .SelectMany(n => n.Filters)
                .Distinct()
                .Select(n => new CompletionFilterWithState(n, true))
                .ToImmutableArray();

            // Note: do not use the tracked edit from the editor. Exclusively rely on data from the completion providers
            var spans = nestedResults
                .Select(n => n.ApplicableToSpan)
                .Select(s => view.BufferGraph.CreateMappingSpan(s, SpanTrackingMode.EdgeNegative))
                .Select(s => new SnapshotSpan(
                    s.Start.GetPoint(triggerLocation.Snapshot, PositionAffinity.Predecessor).Value,
                    s.End.GetPoint(triggerLocation.Snapshot, PositionAffinity.Predecessor).Value));

            var extent = new SnapshotSpan(
                spans.Min(n => n.Start),
                spans.Max(n => n.End));
            var applicableSpan = triggerLocation.Snapshot.CreateTrackingSpan(extent, SpanTrackingMode.EdgeInclusive);

            var useSoftSelection = nestedResults.Any(n => n.UseSoftSelection);
            var useSuggestionMode = nestedResults.Any(n => n.UseSuggestionMode);
            var suggestionModeDescription = nestedResults.FirstOrDefault(n => !String.IsNullOrEmpty(n.SuggestionModeDescription)).SuggestionModeDescription ?? String.Empty;

            _telemetryData.ComputationStopwatch.Stop();
            _telemetryData.RecordInitialModel(_telemetryData.ComputationStopwatch.ElapsedMilliseconds, _completionProviders.Keys, originalCompletionItems.Length, _completionService);

            return new CompletionModel(originalCompletionItems, applicableSpan, triggerLocation.Snapshot, availableFilters, useSoftSelection, useSuggestionMode, suggestionModeDescription);
        }

        /// <summary>
        /// User has moved the caret. Ensure that the caret is still within the applicable span. If not, dismiss the session.
        /// </summary>
        /// <returns></returns>
        private async Task<CompletionModel> HandleCaretPositionChanged(CompletionModel model, CaretPosition caretPosition)
        {
            if (!(model.ApplicableSpan.GetSpan(caretPosition.VirtualBufferPosition.Position.Snapshot).IntersectsWith(new SnapshotSpan(caretPosition.VirtualBufferPosition.Position, 0))))
            {
                await _jtf.SwitchToMainThreadAsync();
                _broker.Dismiss(_view);
            }
            return model;
        }

        /// <summary>
        /// User has typed
        /// </summary>
        private async Task<CompletionModel> UpdateSnapshot(CompletionModel model, CompletionTrigger trigger, SnapshotPoint triggerLocation, CancellationToken token, int thisId)
        {
            // Filtering got preempted, so store the most recent snapshot for the next time we filter
            if (token.IsCancellationRequested || thisId != _lastFilteringTaskId)
                return model.WithSnapshot(triggerLocation.Snapshot);

            // Dismiss if we are outside of the applicable span
            // TODO: dismiss if applicable span was reduced to zero AND moved again (e.g. first backspace keeps completion open, second backspace closes it)
            if (triggerLocation < model.ApplicableSpan.GetStartPoint(triggerLocation.Snapshot) || triggerLocation > model.ApplicableSpan.GetEndPoint(triggerLocation.Snapshot))
            {
                await _jtf.SwitchToMainThreadAsync();
                _broker.Dismiss(_view); // We need to dismiss through the broker, so that it updates its state.
                return model;
            }

            _telemetryData.ComputationStopwatch.Restart();

            var filteredCompletion = await _completionService.UpdateCompletionListAsync(
                model.AllItems,
                trigger,
                triggerLocation.Snapshot,
                model.ApplicableSpan,
                model.Filters);

            if (model.UseSuggestionMode)
            {
                var filterText = model.ApplicableSpan.GetText(triggerLocation.Snapshot);
                CompletionItemWithHighlight suggestionModeItem;
                if (String.IsNullOrWhiteSpace(filterText))
                {
                    suggestionModeItem = new CompletionItemWithHighlight(new CompletionItem(model.SuggestionModeDescription, null), ImmutableArray<Span>.Empty);
                    model = model.WithEmptySuggestionItem(suggestionModeItem);
                }
                else
                {
                    suggestionModeItem = new CompletionItemWithHighlight(new CompletionItem(filterText, null), ImmutableArray.Create<Span>(new Span(0, filterText.Length)));
                    model = model.WithSuggestionItem(suggestionModeItem);
                }
            }

            _telemetryData.ComputationStopwatch.Stop();
            _telemetryData.RecordProcessing(_telemetryData.ComputationStopwatch.ElapsedMilliseconds, filteredCompletion.Items.Count());

            // TODO: if filtered completion has no items to display,
            // reuse previous filtered completion, but with soft selection
            return model.WithSnapshot(triggerLocation.Snapshot).WithPresentedItems(filteredCompletion.Items, filteredCompletion.SelectedItemIndex);
        }

        /// <summary>
        /// User has changed a filter
        /// </summary>
        private async Task<CompletionModel> UpdateFilters(CompletionModel model, ImmutableArray<CompletionFilterWithState> filters, CancellationToken token, int thisId)
        {
            _telemetryData.RecordChangingFilters();

            // Filtering got preempted, so store the most updated filters for the next time we filter
            if (token.IsCancellationRequested || thisId != _lastFilteringTaskId)
                return model.WithFilters(filters);

            var filteredCompletion = await _completionService.UpdateCompletionListAsync(
                model.AllItems,
                new CompletionTrigger(CompletionTriggerReason.FilterChange),
                model.Snapshot,
                model.ApplicableSpan,
                filters);

            return model.WithFilters(filters).WithPresentedItems(filteredCompletion.Items, filteredCompletion.SelectedItemIndex);
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        /// <summary>
        /// User is scrolling the list
        /// </summary>
        private async Task<CompletionModel> UpdateSelectedItem(CompletionModel model, int offset, CancellationToken token)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            _telemetryData.RecordScrolling();

            if (!model.PresentedItems.Any())
            {
                // No-op if there are no items
                if (model.UseSuggestionMode)
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
                    if (model.UseSuggestionMode)
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
                    if (model.UseSuggestionMode)
                        return model.WithSuggestionItemSelected();
                    else
                        return model.WithSelectedIndex(lastIndex);
                }
                var newIndex = currentIndex + offset;
                return model.WithSelectedIndex(Math.Max(newIndex, FirstIndex));
            }
        }

        /// <summary>
        /// This method creates a mapping point from the top-buffer trigger location
        /// then iterates through completion item sources pertinent to this session.
        /// It maps triggerLocation to each source's buffer
        /// Ensures that we work only with sources applicable to current location
        /// and returns whether any source would like to commit completion
        /// </summary>
        public bool ShouldCommit(ITextView view, string edit, SnapshotPoint triggerLocation)
        {
            var mappingPoint = view.BufferGraph.CreateMappingPoint(triggerLocation, PointTrackingMode.Negative);
            return _completionProviders
                .Select(p => (p, mappingPoint.GetPoint(p.Value.Snapshot.TextBuffer, PositionAffinity.Predecessor)))
                .Where(n => n.Item2.HasValue)
                .Any(n => n.Item1.Key.ShouldCommitCompletion(edit, n.Item2.Value));
            // Remove previous line and uncomment the following lines to get the specific IAsyncCompletionItemSource that wanted to commit
            /*
            .Where(n => n.Item1.Key.ShouldCommitCompletion(edit, n.Item2.Value))
            .Select(n => n.Item1.Key)
            .FirstOrDefault();
            */
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
                InitialItemSources = String.Join(" ", sources.Select(n => _broker.GetItemSourceName(n)));
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

            internal void RecordCommitAndSend(long commitDuration, CompletionItem committedItem, string edit)
            {
                _logger?.PostEvent(TelemetryEventType.Operation,
                    TelemetryData.CommitEvent,
                    TelemetryResult.Success,
                    (InitialItemSourcesProperty, InitialItemSources ?? string.Empty),
                    (InitialItemCountProperty, InitialItemCount),
                    (InitialModelDurationProperty, InitialModelDuration),
                    (CompletionServiceProperty, CompletionService),
                    (TotalProcessingDurationProperty, TotalProcessingDuration),
                    (TotalProcessingCountProperty, TotalProcessingCount),
                    (InitialRenderingDurationProperty, InitialRenderingDuration),
                    (TotalRenderingDurationProperty, TotalRenderingDuration),
                    (TotalRenderingCountProperty, TotalRenderingCount),
                    (UserEverScrolledProperty, UserEverScrolled),
                    (UserEverSetFiltersProperty, UserEverSetFilters),
                    (UserLastScrolledProperty, UserLastScrolled),
                    (LastItemCountProperty, LastItemCount),
                    (CustomCommitProperty, committedItem.CustomCommit),
                    (CommittedItemSourceProperty, GetItemSourceName(committedItem.Source)),
                    (CommitDurationProperty, commitDuration),
                    (CommitTriggerProperty, edit ?? string.Empty)
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
                    (CompletionServiceProperty, CompletionService),
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
