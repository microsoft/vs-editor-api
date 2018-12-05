using System.Collections.Immutable;
using Microsoft.VisualStudio.Text;

namespace Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion.Data
{
    /// <summary>
    /// Contains data of <see cref="IAsyncCompletionSession"/> valid at a specific, instantaneous moment pertinent to current computation.
    /// This data is passed to <see cref="IAsyncCompletionItemManager"/> to filter the list and select appropriate item.
    /// </summary>
    public class AsyncCompletionSessionDataSnapshot
    {
        /// <summary>
        /// Set of <see cref="CompletionItem"/>s to filter and sort, originally returned from <see cref="IAsyncCompletionItemManager.SortCompletionListAsync"/>.
        /// </summary>
        public ImmutableArray<CompletionItem> InitialSortedList { get; }

        /// <summary>
        /// The <see cref="ITextSnapshot"/> applicable for this computation. The snapshot comes from the view's data buffer.
        /// </summary>
        public ITextSnapshot Snapshot { get; }

        /// <summary>
        /// The <see cref="CompletionTrigger"/> that caused this update.
        /// </summary>
        public CompletionTrigger Trigger { get; }

        /// <summary>
        /// Filters, their availability and selection state.
        /// </summary>
        public ImmutableArray<CompletionFilterWithState> SelectedFilters { get; }

        /// <summary>
        /// Inidicates whether the session is using soft selection
        /// </summary>
        public bool IsSoftSelected { get; }

        /// <summary>
        /// Inidicates whether the session displays a suggestion item.
        /// </summary>
        public bool DisplaySuggestionItem { get; }

        /// <summary>
        /// Constructs <see cref="AsyncCompletionSessionDataSnapshot"/>
        /// </summary>
        /// <param name="initialSortedList">Set of <see cref="CompletionItem"/>s to filter and sort, originally returned from <see cref="IAsyncCompletionItemManager.SortCompletionListAsync"/></param>
        /// <param name="snapshot">The <see cref="ITextSnapshot"/> applicable for this computation. The snapshot comes from the view's data buffer</param>
        /// <param name="trigger">The <see cref="CompletionTrigger"/> that caused this update</param>
        /// <param name="selectedFilters">Filters, their availability and selection state</param>
        /// <param name="isSoftSelected">Inidicates whether the session is using soft selection</param>
        /// <param name="displaySuggestionItem">Inidicates whether the session has a suggestion item</param>
        public AsyncCompletionSessionDataSnapshot(
            ImmutableArray<CompletionItem> initialSortedList,
            ITextSnapshot snapshot,
            CompletionTrigger trigger,
            ImmutableArray<CompletionFilterWithState> selectedFilters,
            bool isSoftSelected,
            bool displaySuggestionItem
        )
        {
            InitialSortedList = initialSortedList;
            Snapshot = snapshot;
            Trigger = trigger;
            SelectedFilters = selectedFilters;
            IsSoftSelected = isSoftSelected;
            DisplaySuggestionItem = displaySuggestionItem;
        }
    }
}
