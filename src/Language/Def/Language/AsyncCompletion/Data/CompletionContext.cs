using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.VisualStudio.Text;

namespace Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion.Data
{
    /// <summary>
    /// This type is used to transfer data from <see cref="IAsyncCompletionSource"/>
    /// to <see cref="IAsyncCompletionBroker"/> and further to <see cref="IAsyncCompletionItemManager"/>
    /// </summary>
    [DebuggerDisplay("{Items.Length} items")]
    public sealed class CompletionContext
    {
        /// <summary>
        /// Empty completion context, when <see cref="IAsyncCompletionSource"/> offers no items pertinent to given location.
        /// </summary>
        public static CompletionContext Empty { get; } = new CompletionContext(ImmutableArray<CompletionItem>.Empty);

        /// <summary>
        /// Set of completion items available at a location
        /// </summary>
        public ImmutableArray<CompletionItem> Items { get; }

        /// <summary>
        /// Recommends the initial selection method for the completion list.
        /// When <see cref="SuggestionItemOptions"/> is defined, "soft selection" will be used without a need to set this property.
        /// </summary>
        public InitialSelectionHint SelectionHint { get; }

        /// <summary>
        /// When defined, uses suggestion mode with options specified in this object.
        /// When null, this context does not activate the suggestion mode.
        /// Suggestion mode puts selection in "soft selection" mode without need to set <see cref="SelectionHint"/>
        /// </summary>
        public SuggestionItemOptions SuggestionItemOptions { get; }

        /// <summary>
        /// Constructs <see cref="CompletionContext"/> with specified <see cref="CompletionItem"/>s,
        /// with recommendation to not use suggestion mode and to use use regular selection.
        /// </summary>
        /// <param name="items">Available completion items. If none are available, use <code>CompletionContext.Default</code></param>
        public CompletionContext(ImmutableArray<CompletionItem> items)
            : this(items, suggestionItemOptions: null, selectionHint: InitialSelectionHint.RegularSelection)
        {
        }

        /// <summary>
        /// Constructs <see cref="CompletionContext"/> with specified <see cref="CompletionItem"/>s,
        /// with recommendation to use suggestion mode and to use regular selection.
        /// </summary>
        /// <param name="items">Available completion items</param>
        /// <param name="suggestionItemOptions">Suggestion item options, or null to not use suggestion mode. Default is <code>null</code></param>
        public CompletionContext(
            ImmutableArray<CompletionItem> items,
            SuggestionItemOptions suggestionItemOptions)
        : this(items, suggestionItemOptions, InitialSelectionHint.RegularSelection)
        {
        }

        /// <summary>
        /// Constructs <see cref="CompletionContext"/> with specified <see cref="CompletionItem"/>s,
        /// with recommendation to use suggestion mode item and to use a specific selection mode.
        /// </summary>
        /// <param name="items">Available completion items</param>
        /// <param name="suggestionItemOptions">Suggestion mode options, or null to not use suggestion mode. Default is <code>null</code></param>
        /// <param name="selectionHint">Recommended selection mode. Suggestion mode automatically sets soft selection Default is <code>InitialSelectionHint.RegularSelection</code></param>
        public CompletionContext(
            ImmutableArray<CompletionItem> items,
            SuggestionItemOptions suggestionItemOptions,
            InitialSelectionHint selectionHint)
        {
            if (items.IsDefault)
                throw new ArgumentException("Array must be initialized", nameof(items));
            Items = items;
            SelectionHint = selectionHint;
            SuggestionItemOptions = suggestionItemOptions;
        }
    }
}
