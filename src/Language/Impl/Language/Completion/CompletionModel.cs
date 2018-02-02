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
    class CompletionModel
    {
        /// <summary>
        /// All items, as provided by completion item sources.
        /// </summary>
        public readonly ImmutableArray<CompletionItem> AllItems;

        /// <summary>
        /// Span pertinent to this completion model.
        /// </summary>
        public readonly ITrackingSpan ApplicableSpan;

        /// <summary>
        /// Snapshot pertinent to this completion model.
        /// </summary>
        public readonly ITextSnapshot Snapshot;

        /// <summary>
        /// Filters involved in this completion model, including their availability and selection state.
        /// </summary>
        public readonly ImmutableArray<CompletionFilterWithState> Filters;

        /// <summary>
        /// Items to be displayed in the UI.
        /// </summary>
        public readonly IEnumerable<CompletionItemWithHighlight> PresentedItems;

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
        public readonly bool UseSuggestionMode;

        /// <summary>
        /// Whether suggestion mode item should be selected.
        /// </summary>
        public readonly bool SelectSuggestionMode;

        /// <summary>
        /// Text to display in place of suggestion mode when filtered text is empty.
        /// </summary>
        public readonly string SuggestionModeDescription;

        /// <summary>
        /// Item to display as suggestion mode item
        /// </summary>
        public readonly CompletionItemWithHighlight SuggestionModeItem;

        /// <summary>
        /// When true, committing the suggestion mode item is a no-op.
        /// </summary>
        public readonly bool SuggestionIsEmpty;

        /// <summary>
        /// Constructor for the initial model
        /// </summary>
        public CompletionModel(ImmutableArray<CompletionItem> originalItems, ITrackingSpan applicableSpan, ITextSnapshot snapshot, ImmutableArray<CompletionFilterWithState> filters, bool useSoftSelection, bool useSuggestionMode, string suggestionModeDescription)
        {
            AllItems = originalItems;
            ApplicableSpan = applicableSpan;
            Snapshot = snapshot;
            Filters = filters;
            SelectedIndex = 0;
            UseSoftSelection = useSoftSelection;
            UseSuggestionMode = useSuggestionMode;
            SelectSuggestionMode = useSuggestionMode;
            SuggestionModeDescription = suggestionModeDescription;
            SuggestionModeItem = CompletionItemWithHighlight.Empty;
        }

        /// <summary>
        /// Private constructor for the With* methods
        /// </summary>
        private CompletionModel(ImmutableArray<CompletionItem> originalItems, ITrackingSpan applicableSpan, ITextSnapshot snapshot, ImmutableArray<CompletionFilterWithState> filters, IEnumerable<CompletionItemWithHighlight> presentedItems, bool useSoftSelection, bool useSuggestionMode, string suggestionModeDescription, int selectedIndex, bool selectSuggestionMode, CompletionItemWithHighlight suggestionModeItem, bool suggestionIsEmpty)
        {
            AllItems = originalItems;
            ApplicableSpan = applicableSpan;
            Snapshot = snapshot;
            Filters = filters;
            PresentedItems = presentedItems;
            SelectedIndex = selectedIndex;
            UseSoftSelection = useSoftSelection;
            UseSuggestionMode = useSuggestionMode;
            SelectSuggestionMode = selectSuggestionMode;
            SuggestionModeDescription = suggestionModeDescription;
            SuggestionModeItem = suggestionModeItem;
            SuggestionIsEmpty = suggestionIsEmpty;
        }

        public CompletionModel WithPresentedItems(IEnumerable<CompletionItemWithHighlight> newPresentedItems, int newSelectedIndex)
        {
            return new CompletionModel(
                originalItems: AllItems,
                applicableSpan: ApplicableSpan,
                snapshot: Snapshot,
                filters: Filters,
                presentedItems: newPresentedItems, // Updated
                useSoftSelection: UseSoftSelection,
                useSuggestionMode: UseSuggestionMode,
                suggestionModeDescription: SuggestionModeDescription,
                selectedIndex: newSelectedIndex, // Updated
                selectSuggestionMode: SelectSuggestionMode,
                suggestionModeItem: SuggestionModeItem,
                suggestionIsEmpty: SuggestionIsEmpty
            );
        }

        public CompletionModel WithSnapshot(ITextSnapshot newSnapshot)
        {
            return new CompletionModel(
                originalItems: AllItems,
                applicableSpan: ApplicableSpan,
                snapshot: newSnapshot, // Updated
                filters: Filters,
                presentedItems: PresentedItems,
                useSoftSelection: UseSoftSelection,
                useSuggestionMode: UseSuggestionMode,
                suggestionModeDescription: SuggestionModeDescription,
                selectedIndex: SelectedIndex,
                selectSuggestionMode: SelectSuggestionMode,
                suggestionModeItem: SuggestionModeItem,
                suggestionIsEmpty: SuggestionIsEmpty
            );
        }

        public CompletionModel WithFilters(ImmutableArray<CompletionFilterWithState> newFilters)
        {
            return new CompletionModel(
                originalItems: AllItems,
                applicableSpan: ApplicableSpan,
                snapshot: Snapshot,
                filters: newFilters, // Updated
                presentedItems: PresentedItems,
                useSoftSelection: UseSoftSelection,
                useSuggestionMode: UseSuggestionMode,
                suggestionModeDescription: SuggestionModeDescription,
                selectedIndex: SelectedIndex,
                selectSuggestionMode: SelectSuggestionMode,
                suggestionModeItem: SuggestionModeItem,
                suggestionIsEmpty: SuggestionIsEmpty
            );
        }

        public CompletionModel WithSelectedIndex(int newIndex)
        {
            return new CompletionModel(
                originalItems: AllItems,
                applicableSpan: ApplicableSpan,
                snapshot: Snapshot,
                filters: Filters,
                presentedItems: PresentedItems,
                useSoftSelection: false, // Explicit selection and soft selection are mutually exclusive
                useSuggestionMode: UseSuggestionMode,
                suggestionModeDescription: SuggestionModeDescription,
                selectedIndex: newIndex, // Updated
                selectSuggestionMode: false, // Explicit selection of regular item
                suggestionModeItem: SuggestionModeItem,
                suggestionIsEmpty: SuggestionIsEmpty
            );
        }

        public CompletionModel WithSuggestionItemSelected()
        {
            return new CompletionModel(
                originalItems: AllItems,
                applicableSpan: ApplicableSpan,
                snapshot: Snapshot,
                filters: Filters,
                presentedItems: PresentedItems,
                useSoftSelection: false, // Explicit selection and soft selection are mutually exclusive
                useSuggestionMode: UseSuggestionMode,
                suggestionModeDescription: SuggestionModeDescription,
                selectedIndex: -1, // Deselect regular item
                selectSuggestionMode: true, // Explicit selection of suggestion item
                suggestionModeItem: SuggestionModeItem,
                suggestionIsEmpty: SuggestionIsEmpty
            );
        }

        internal CompletionModel WithSuggestionItem(CompletionItemWithHighlight newSuggestionModeItem)
        {
            return new CompletionModel(
                originalItems: AllItems,
                applicableSpan: ApplicableSpan,
                snapshot: Snapshot,
                filters: Filters,
                presentedItems: PresentedItems,
                useSoftSelection: UseSoftSelection,
                useSuggestionMode: UseSuggestionMode,
                suggestionModeDescription: SuggestionModeDescription,
                selectedIndex: SelectedIndex,
                selectSuggestionMode: SelectSuggestionMode,
                suggestionModeItem: newSuggestionModeItem, // Updated
                suggestionIsEmpty: false // This method guarantees that the suggestion is not empty
            );
        }

        internal CompletionModel WithEmptySuggestionItem(CompletionItemWithHighlight newSuggestionModeItem)
        {
            return new CompletionModel(
                originalItems: AllItems,
                applicableSpan: ApplicableSpan,
                snapshot: Snapshot,
                filters: Filters,
                presentedItems: PresentedItems,
                useSoftSelection: UseSoftSelection,
                useSuggestionMode: UseSuggestionMode,
                suggestionModeDescription: SuggestionModeDescription,
                selectedIndex: SelectedIndex,
                selectSuggestionMode: SelectSuggestionMode,
                suggestionModeItem: newSuggestionModeItem, // Updated
                suggestionIsEmpty: true // This method guarantees that the suggestion is empty
            );
        }
    }

    class ModelComputation<TModel>
    {
        Task<TModel> _lastTask = Task.FromResult(default(TModel));
        private Task _notifyUITask = Task.CompletedTask;
        private readonly TaskScheduler _computationTaskScheduler;
        private readonly CancellationToken _token;
        private readonly CancellationTokenSource _uiCancellation;

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

            _notifyUITask = Task.Factory.ContinueWhenAll(
                new[] { _notifyUITask, nextTask },
                async existingTasks =>
                {
                    if (existingTasks.All(t => t.Status == TaskStatus.RanToCompletion))
                    {
                        if (nextTask == _lastTask && updateUI != null)
                        {
                            await updateUI(nextTask.Result);
                        }
                    }
                },
                _uiCancellation.Token
            );
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
