using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;

namespace Microsoft.VisualStudio.Language.Intellisense.Implementation
{
    sealed class CompletionModel
    {
        /// <summary>
        /// All items, as provided by completion item sources.
        /// </summary>
        public readonly ImmutableArray<CompletionItem> InitialItems;

        /// <summary>
        /// Sorted array of all items, as provided by the completion service.
        /// </summary>
        public readonly ImmutableArray<CompletionItem> SortedItems;

        /// <summary>
        /// Span pertinent to this completion model.
        /// </summary>
        public readonly ITrackingSpan ApplicableSpan;

        /// <summary>
        /// Snapshot pertinent to this completion model.
        /// </summary>
        public readonly ITextSnapshot Snapshot;

        /// <summary>
        /// Stores the initial reason this session was triggererd.
        /// </summary>
        public readonly CompletionTriggerReason InitialTriggerReason;

        /// <summary>
        /// Filters involved in this completion model, including their availability and selection state.
        /// </summary>
        public readonly ImmutableArray<CompletionFilterWithState> Filters;

        /// <summary>
        /// Items to be displayed in the UI.
        /// </summary>
        public readonly ImmutableArray<CompletionItemWithHighlight> PresentedItems;

        /// <summary>
        /// Index of item to select. Use -1 to select nothing, when suggestion mode item should be selected.
        /// </summary>
        public readonly int SelectedIndex;

        /// <summary>
        /// Whether selection should be displayed as soft selection.
        /// </summary>
        public readonly bool UseSoftSelection;

        /// <summary>
        /// Whether suggestion mode item should be visible.
        /// </summary>
        public readonly bool DisplaySuggestionMode;

        /// <summary>
        /// Whether suggestion mode item should be selected.
        /// </summary>
        public readonly bool SelectSuggestionMode;

        /// <summary>
        /// <see cref="CompletionItem"/> which contains user-entered text.
        /// Used to display and commit the suggestion mode item
        /// </summary>
        public readonly CompletionItem SuggestionModeItem;

        /// <summary>
        /// Text to display in place of suggestion mode when filtered text is empty.
        /// </summary>
        public readonly string SuggestionModeDescription;

        /// <summary>
        /// <see cref="CompletionItem"/> which overrides regular unique item selection.
        /// When this is null, the single item from <see cref="PresentedItems"/> is used as unique item.
        /// </summary>
        public readonly CompletionItem UniqueItem;

        /// <summary>
        /// Constructor for the initial model
        /// </summary>
        public CompletionModel(ImmutableArray<CompletionItem> initialItems, ImmutableArray<CompletionItem> sortedItems,
            ITrackingSpan applicableSpan, CompletionTriggerReason initialTriggerReason, ITextSnapshot snapshot,
            ImmutableArray<CompletionFilterWithState> filters, bool useSoftSelection, bool useSuggestionMode, string suggestionModeDescription, CompletionItem suggestionModeItem)
        {
            InitialItems = initialItems;
            SortedItems = sortedItems;
            ApplicableSpan = applicableSpan;
            InitialTriggerReason = initialTriggerReason;
            Snapshot = snapshot;
            Filters = filters;
            SelectedIndex = 0;
            UseSoftSelection = useSoftSelection;
            DisplaySuggestionMode = useSuggestionMode;
            SelectSuggestionMode = useSuggestionMode;
            SuggestionModeDescription = suggestionModeDescription;
            SuggestionModeItem = suggestionModeItem;
            UniqueItem = null;
        }

        /// <summary>
        /// Private constructor for the With* methods
        /// </summary>
        private CompletionModel(ImmutableArray<CompletionItem> initialItems, ImmutableArray<CompletionItem> sortedItems, ITrackingSpan applicableSpan, CompletionTriggerReason initialTriggerReason,
            ITextSnapshot snapshot, ImmutableArray<CompletionFilterWithState> filters, ImmutableArray<CompletionItemWithHighlight> presentedItems, bool useSoftSelection, bool useSuggestionMode,
            string suggestionModeDescription, int selectedIndex, bool selectSuggestionMode, CompletionItem suggestionModeItem, CompletionItem uniqueItem)
        {
            InitialItems = initialItems;
            SortedItems = sortedItems;
            ApplicableSpan = applicableSpan;
            InitialTriggerReason = initialTriggerReason;
            Snapshot = snapshot;
            Filters = filters;
            PresentedItems = presentedItems;
            SelectedIndex = selectedIndex;
            UseSoftSelection = useSoftSelection;
            DisplaySuggestionMode = useSuggestionMode;
            SelectSuggestionMode = selectSuggestionMode;
            SuggestionModeDescription = suggestionModeDescription;
            SuggestionModeItem = suggestionModeItem;
        }

        public CompletionModel WithPresentedItems(ImmutableArray<CompletionItemWithHighlight> newPresentedItems, int newSelectedIndex)
        {
            return new CompletionModel(
                initialItems: InitialItems,
                sortedItems: SortedItems,
                applicableSpan: ApplicableSpan,
                initialTriggerReason: InitialTriggerReason,
                snapshot: Snapshot,
                filters: Filters,
                presentedItems: newPresentedItems, // Updated
                useSoftSelection: UseSoftSelection,
                useSuggestionMode: DisplaySuggestionMode,
                suggestionModeDescription: SuggestionModeDescription,
                selectedIndex: newSelectedIndex, // Updated
                selectSuggestionMode: SelectSuggestionMode,
                suggestionModeItem: SuggestionModeItem,
                uniqueItem: UniqueItem
            );
        }

        public CompletionModel WithSnapshot(ITextSnapshot newSnapshot)
        {
            return new CompletionModel(
                initialItems: InitialItems,
                sortedItems: SortedItems,
                applicableSpan: ApplicableSpan,
                initialTriggerReason: InitialTriggerReason,
                snapshot: newSnapshot, // Updated
                filters: Filters,
                presentedItems: PresentedItems,
                useSoftSelection: UseSoftSelection,
                useSuggestionMode: DisplaySuggestionMode,
                suggestionModeDescription: SuggestionModeDescription,
                selectedIndex: SelectedIndex,
                selectSuggestionMode: SelectSuggestionMode,
                suggestionModeItem: SuggestionModeItem,
                uniqueItem: UniqueItem
            );
        }

        public CompletionModel WithFilters(ImmutableArray<CompletionFilterWithState> newFilters)
        {
            return new CompletionModel(
                initialItems: InitialItems,
                sortedItems: SortedItems,
                applicableSpan: ApplicableSpan,
                initialTriggerReason: InitialTriggerReason,
                snapshot: Snapshot,
                filters: newFilters, // Updated
                presentedItems: PresentedItems,
                useSoftSelection: UseSoftSelection,
                useSuggestionMode: DisplaySuggestionMode,
                suggestionModeDescription: SuggestionModeDescription,
                selectedIndex: SelectedIndex,
                selectSuggestionMode: SelectSuggestionMode,
                suggestionModeItem: SuggestionModeItem,
                uniqueItem: UniqueItem
            );
        }

        public CompletionModel WithSelectedIndex(int newIndex)
        {
            return new CompletionModel(
                initialItems: InitialItems,
                sortedItems: SortedItems,
                applicableSpan: ApplicableSpan,
                initialTriggerReason: InitialTriggerReason,
                snapshot: Snapshot,
                filters: Filters,
                presentedItems: PresentedItems,
                useSoftSelection: false, // Explicit selection and soft selection are mutually exclusive
                useSuggestionMode: DisplaySuggestionMode,
                suggestionModeDescription: SuggestionModeDescription,
                selectedIndex: newIndex, // Updated
                selectSuggestionMode: false, // Explicit selection of regular item
                suggestionModeItem: SuggestionModeItem,
                uniqueItem: UniqueItem
            );
        }

        public CompletionModel WithSuggestionItemSelected()
        {
            return new CompletionModel(
                initialItems: InitialItems,
                sortedItems: SortedItems,
                applicableSpan: ApplicableSpan,
                initialTriggerReason: InitialTriggerReason,
                snapshot: Snapshot,
                filters: Filters,
                presentedItems: PresentedItems,
                useSoftSelection: false, // Explicit selection and soft selection are mutually exclusive
                useSuggestionMode: DisplaySuggestionMode,
                suggestionModeDescription: SuggestionModeDescription,
                selectedIndex: -1, // Deselect regular item
                selectSuggestionMode: true, // Explicit selection of suggestion item
                suggestionModeItem: SuggestionModeItem,
                uniqueItem: UniqueItem
            );
        }

        public CompletionModel WithSuggestionModeActive(bool newUseSuggestionMode)
        {
            return new CompletionModel(
                initialItems: InitialItems,
                sortedItems: SortedItems,
                applicableSpan: ApplicableSpan,
                initialTriggerReason: InitialTriggerReason,
                snapshot: Snapshot,
                filters: Filters,
                presentedItems: PresentedItems,
                useSoftSelection: UseSoftSelection | newUseSuggestionMode, // Enabling suggestion mode also enables soft selection
                useSuggestionMode: newUseSuggestionMode, // Updated
                suggestionModeDescription: SuggestionModeDescription,
                selectedIndex: SelectedIndex,
                selectSuggestionMode: SelectSuggestionMode,
                suggestionModeItem: SuggestionModeItem,
                uniqueItem: UniqueItem
            );
        }

        /// <summary>
        /// </summary>
        /// <param name="newSuggestionModeItem">It is ok to pass in null when there is no suggestion. UI will display SuggestsionModeDescription instead.</param>
        internal CompletionModel WithSuggestionModeItem(CompletionItem newSuggestionModeItem)
        {
            return new CompletionModel(
                initialItems: InitialItems,
                sortedItems: SortedItems,
                applicableSpan: ApplicableSpan,
                initialTriggerReason: InitialTriggerReason,
                snapshot: Snapshot,
                filters: Filters,
                presentedItems: PresentedItems,
                useSoftSelection: UseSoftSelection,
                useSuggestionMode: DisplaySuggestionMode,
                suggestionModeDescription: SuggestionModeDescription,
                selectedIndex: SelectedIndex,
                selectSuggestionMode: SelectSuggestionMode,
                suggestionModeItem: newSuggestionModeItem,
                uniqueItem: UniqueItem
            );
        }

        /// <summary>
        /// </summary>
        /// <param name="newUniqueItem">Overrides typical unique item selection.
        /// Pass in null to use regular behavior: treating single <see cref="PresentedItems"/> item as the unique item.</param>
        internal CompletionModel WithUniqueItem(CompletionItem newUniqueItem)
        {
            return new CompletionModel(
                initialItems: InitialItems,
                sortedItems: SortedItems,
                applicableSpan: ApplicableSpan,
                initialTriggerReason: InitialTriggerReason,
                snapshot: Snapshot,
                filters: Filters,
                presentedItems: PresentedItems,
                useSoftSelection: UseSoftSelection,
                useSuggestionMode: DisplaySuggestionMode,
                suggestionModeDescription: SuggestionModeDescription,
                selectedIndex: SelectedIndex,
                selectSuggestionMode: SelectSuggestionMode,
                suggestionModeItem: SuggestionModeItem,
                uniqueItem: newUniqueItem
            );
        }

        internal CompletionModel WithSoftSelection(bool newSoftSelection)
        {
            return new CompletionModel(
                initialItems: InitialItems,
                sortedItems: SortedItems,
                applicableSpan: ApplicableSpan,
                initialTriggerReason: InitialTriggerReason,
                snapshot: Snapshot,
                filters: Filters,
                presentedItems: PresentedItems,
                useSoftSelection: newSoftSelection, // Updated
                useSuggestionMode: DisplaySuggestionMode,
                suggestionModeDescription: SuggestionModeDescription,
                selectedIndex: SelectedIndex,
                selectSuggestionMode: SelectSuggestionMode,
                suggestionModeItem: SuggestionModeItem,
                uniqueItem: UniqueItem
            );
        }
    }

    sealed class ModelComputation<TModel>
    {
        Task<TModel> _lastTask = Task.FromResult(default(TModel));
        private Task _notifyUITask = Task.CompletedTask;
        private readonly TaskScheduler _computationTaskScheduler;
        private readonly CancellationToken _token;
        private CancellationTokenSource _uiCancellation;
        internal TModel RecentModel { get; private set; } = default(TModel);

        public ModelComputation(TaskScheduler computationTaskScheduler, CancellationToken token)
        {
            _computationTaskScheduler = computationTaskScheduler;
            _token = token;
            _uiCancellation = new CancellationTokenSource();
        }

        /// <summary>
        /// Schedules work to be done on the background,
        /// potentially preempted by another piece of work scheduled in the future.
        /// </summary>
        public void Enqueue(Func<TModel, CancellationToken, Task<TModel>> transformation)
        {
            Enqueue(transformation, null);
        }

        /// <summary>
        /// Schedules work to be done on the background,
        /// potentially preempted by another piece of work scheduled in the future,
        /// followed by a single piece of work that will execute once all background work is completed.
        /// </summary>
        public void Enqueue(Func<TModel, CancellationToken, Task<TModel>> transformation, Func<TModel, Task> updateUI)
        {
            // This method is based on Roslyn's ModelComputation.ChainTaskAndNotifyControllerWhenFinished
            var nextTask = _lastTask.ContinueWith(t => transformation(t.Result, _token), _computationTaskScheduler).Unwrap();
            _lastTask = nextTask;

            // If the _notifyUITask is canceled, refresh it
            if (_notifyUITask.IsCanceled || _uiCancellation.IsCancellationRequested)
            {
                _notifyUITask = Task.CompletedTask;
                _uiCancellation = new CancellationTokenSource();
            }

            _notifyUITask = Task.Factory.ContinueWhenAll(
                new[] { _notifyUITask, nextTask },
                async existingTasks =>
                {
                    if (existingTasks.All(t => t.Status == TaskStatus.RanToCompletion))
                    {
                        OnModelUpdated(nextTask.Result);
                        if (updateUI != null && nextTask == _lastTask)
                        {
                            await updateUI(nextTask.Result);
                        }
                    }
                },
                _uiCancellation.Token
            );
        }

        private void OnModelUpdated(TModel result)
        {
            RecentModel = result;
        }

        /// <summary>
        /// Blocks, waiting for all background work to finish.
        /// Ignores the last piece of work a.k.a. "updateUI"
        /// </summary>
        public TModel WaitAndGetResult()
        {
            _uiCancellation.Cancel();
            _lastTask.Wait();
            return _lastTask.Result;
        }
    }
}
