using System.Collections.Immutable;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion.Data;

namespace Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion.Implementation
{
    internal struct CompletionSourceConnectionResult
    {
        internal bool SourceUsesSuggestionMode { get; set; }
        internal SuggestionItemOptions RequestedSuggestionItemOptions { get; set; }
        internal InitialSelectionHint InitialSelectionHint { get; set; }
        internal ImmutableArray<CompletionItem> InitialCompletionItems { get; set; }
        internal bool IsCanceled { get; set; }

        internal CompletionSourceConnectionResult(bool sourceUsesSuggestionMode,
            SuggestionItemOptions requestedSuggestionItemOptions,
            InitialSelectionHint initialSelectionHint,
            ImmutableArray<CompletionItem> initialCompletionItems,
            bool isCanceled = false)
        {
            SourceUsesSuggestionMode = sourceUsesSuggestionMode;
            RequestedSuggestionItemOptions = requestedSuggestionItemOptions;
            InitialSelectionHint = initialSelectionHint;
            InitialCompletionItems = initialCompletionItems;
            IsCanceled = isCanceled;
        }

        internal static CompletionSourceConnectionResult Canceled
            => new CompletionSourceConnectionResult(default, default, default, default, isCanceled: true);
    }
}
