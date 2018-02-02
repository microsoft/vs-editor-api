using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Language.Intellisense;
using System.Collections.Immutable;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.PatternMatching;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Core.Imaging;
using System;

#if NET46
using System.ComponentModel.Composition;
#else
using System.Composition;
using Microsoft.VisualStudio.Text.Editor;
#endif

namespace Microsoft.VisualStudio.Language.Intellisense.Implementation
{
    [Export(typeof(IAsyncCompletionService))]
    [Name("Default completion service")]
    [ContentType("text")]
    public class DefaultCompletionService : IAsyncCompletionService
    {
        [Import]
        public IPatternMatcherFactory PatternMatcherFactory { get; set; }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        async Task<CompletionList> IAsyncCompletionService.UpdateCompletionListAsync(IEnumerable<CompletionItem> originalList, CompletionTrigger trigger, ITextSnapshot snapshot, ITrackingSpan applicableSpan, ImmutableArray<CompletionFilterWithState> filters)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            // Filter by text
            var filterText = applicableSpan.GetText(snapshot);
            if (string.IsNullOrWhiteSpace(filterText))
            {
                // There is no text filtering. Just apply user filters, sort alphabetically and return.
                var listFiltered = originalList;
                if (filters.Any(n => n.IsSelected))
                {
                    listFiltered = originalList.Where(n => ShouldBeInCompletionList(n, filters));
                }
                var listSorted = listFiltered.OrderBy(n => n.SortText);
                var listHighlighted = listSorted.Select(n => new CompletionItemWithHighlight(n));
                return new CompletionList(listHighlighted, 0, filters);
            }

            // Pattern matcher not only filters, but also provides a way to order the results by their match quality.
            // The relevant CompletionItem is match.Item1, its PatternMatch is match.Item2
            var patternMatcher = PatternMatcherFactory.CreatePatternMatcher(
                filterText,
                new PatternMatcherCreationOptions(System.Globalization.CultureInfo.CurrentCulture, PatternMatcherCreationFlags.IncludeMatchedSpans));

            var matches = originalList
                // Perform pattern matching
                .Select(completionItem => (completionItem, patternMatcher.TryMatch(completionItem.FilterText)))
                // Pick only items that were matched, unless length of filter text is 1
                .Where(n => (filterText.Length == 1 || n.Item2.HasValue));

            // See which filters might be enabled based on the typed code
            var textFilteredFilters = matches.SelectMany(n => n.Item1.Filters).Distinct();

            // When no items are available for a given filter, it becomes unavailable
            var updatedFilters = ImmutableArray.CreateRange(filters.Select(n => n.WithAvailability(textFilteredFilters.Contains(n.Filter))));

            // Filter by user-selected filters. The value on availableFiltersWithSelectionState conveys whether the filter is selected.
            var filterFilteredList = matches;
            if (filters.Any(n => n.IsSelected))
            {
                filterFilteredList = matches.Where(n => ShouldBeInCompletionList(n.Item1, filters));
            }

            // Order the list alphabetically and select the best match
            var sortedList = filterFilteredList.OrderBy(n => n.Item1.SortText);
            var bestMatch = filterFilteredList.OrderByDescending(n => n.Item2.HasValue).ThenBy(n => n.Item2).FirstOrDefault();
            var listWithHighlights = sortedList.Select(n => n.Item2.HasValue ? new CompletionItemWithHighlight(n.Item1, n.Item2.Value.MatchedSpans) : new CompletionItemWithHighlight(n.Item1)).ToImmutableArray();

            int selectedItemIndex = 0;
            for (int i = 0; i < listWithHighlights.Length; i++)
            {
                if (listWithHighlights[i].CompletionItem == bestMatch.Item1)
                {
                    selectedItemIndex = i;
                    break;
                }
            }

            return new CompletionList(listWithHighlights, selectedItemIndex, updatedFilters);
        }

        #region Filtering

        public static bool ShouldBeInCompletionList(
            CompletionItem item,
            ImmutableArray<CompletionFilterWithState> filtersWithState)
        {
            foreach (var filterWithState in filtersWithState.Where(n => n.IsSelected))
            {
                if (item.Filters.Any(n => n == filterWithState.Filter))
                {
                    return true;
                }
            }
            return false;
        }

        #endregion
    }

#if DEBUG
    [Export(typeof(IAsyncCompletionItemSource))]
    [Name("Debug completion item source")]
    [Order(After = "default")]
    [ContentType("any")]
    public class DebugCompletionItemSource : IAsyncCompletionItemSource
    {
        private static readonly ImageId Icon1 = new ImageId(new Guid("{ae27a6b0-e345-4288-96df-5eaf394ee369}"), 666);
        private static readonly CompletionFilter Filter1 = new CompletionFilter("Diagnostic", "d", Icon1);
        private static readonly ImageId Icon2 = new ImageId(new Guid("{ae27a6b0-e345-4288-96df-5eaf394ee369}"), 2852);
        private static readonly CompletionFilter Filter2 = new CompletionFilter("Snippets", "s", Icon2);
        private static readonly ImageId Icon3 = new ImageId(new Guid("{ae27a6b0-e345-4288-96df-5eaf394ee369}"), 473);
        private static readonly CompletionFilter Filter3 = new CompletionFilter("Class", "c", Icon3);
        private static readonly ImmutableArray<CompletionFilter> FilterCollection1 = ImmutableArray.Create(Filter1);
        private static readonly ImmutableArray<CompletionFilter> FilterCollection2 = ImmutableArray.Create(Filter2);
        private static readonly ImmutableArray<CompletionFilter> FilterCollection3 = ImmutableArray.Create(Filter3);
        private static readonly ImmutableArray<string> commitCharacters = ImmutableArray.Create(" ", ";", "\t", ".", "<", "(", "[");

        void IAsyncCompletionItemSource.CustomCommit(Text.Editor.ITextView view, ITextBuffer buffer, CompletionItem item, ITrackingSpan applicableSpan, string commitCharacter)
        {
            throw new System.NotImplementedException();
        }

        async Task<CompletionContext> IAsyncCompletionItemSource.GetCompletionContextAsync(CompletionTrigger trigger, SnapshotPoint triggerLocation)
        {
            var charBeforeCaret = triggerLocation.Subtract(1).GetChar();
            SnapshotSpan applicableSpan;
            if (commitCharacters.Contains(charBeforeCaret.ToString()))
            {
                // skip this character. the applicable span starts later
                applicableSpan = new SnapshotSpan(triggerLocation, 0);
            }
            else
            {
                // include this character. the applicable span starts here
                applicableSpan = new SnapshotSpan(triggerLocation - 1, 1);
            }
            return await Task.FromResult(new CompletionContext(
                ImmutableArray.Create(
                    new CompletionItem("SampleItem<>", "SampleItem", "SampleItem<>", "SampleItem", this, FilterCollection1, false, Icon1),
                    new CompletionItem("AnotherItem🐱‍👤", "AnotherItem", "AnotherItem", "AnotherItem", this, FilterCollection1, false, Icon1),
                    new CompletionItem("Aaaaa", "Aaaaa", "Aaaaa", "Aaaaa", this, FilterCollection2, false, Icon2),
                    new CompletionItem("Bbbbb", "Bbbbb", "Bbbbb", "Bbbbb", this, FilterCollection2, false, Icon2),
                    new CompletionItem("Ccccc", "Ccccc", "Ccccc", "Ccccc", this, FilterCollection2, false, Icon2),
                    new CompletionItem("Ddddd", "Ddddd", "Ddddd", "Ddddd", this, FilterCollection2, false, Icon2),
                    new CompletionItem("Eeee", "Eeee", "Eeee", "Eeee", this, FilterCollection2, false, Icon2),
                    new CompletionItem("Ffffff", "Ffffff", "Ffffff", "Ffffff", this, FilterCollection2, false, Icon2),
                    new CompletionItem("Ggggggg", "Ggggggg", "Ggggggg", "Ggggggg", this, FilterCollection2, false, Icon2),
                    new CompletionItem("Hhhhh", "Hhhhh", "Hhhhh", "Hhhhh", this, FilterCollection2, false, Icon2),
                    new CompletionItem("Iiiii", "Iiiii", "Iiiii", "Iiiii", this, FilterCollection2, false, Icon2),
                    new CompletionItem("Jjjjj", "Jjjjj", "Jjjjj", "Jjjjj", this, FilterCollection3, false, Icon3),
                    new CompletionItem("kkkkk", "kkkkk", "kkkkk", "kkkkk", this, FilterCollection3, false, Icon3),
                    new CompletionItem("llllol", "llllol", "llllol", "llllol", this, FilterCollection3, false, Icon3),
                    new CompletionItem("mmmmm", "mmmmm", "mmmmm", "mmmmm", this, FilterCollection3, false, Icon3),
                    new CompletionItem("nnNnnn", "nnNnnn", "nnNnnn", "nnNnnn", this, FilterCollection3, false, Icon3),
                    new CompletionItem("oOoOOO", "oOoOOO", "oOoOOO", "oOoOOO", this, FilterCollection3, false, Icon3)
                ), applicableSpan));//, true, true, "Suggestion mode description!"));
        }

        async Task<object> IAsyncCompletionItemSource.GetDescriptionAsync(CompletionItem item)
        {
            return await Task.FromResult("This is a tooltip for " + item.DisplayText);
        }

        ImmutableArray<string> IAsyncCompletionItemSource.GetPotentialCommitCharacters() => commitCharacters;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        async Task IAsyncCompletionItemSource.HandleViewClosedAsync(Text.Editor.ITextView view)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            return;
        }

        bool IAsyncCompletionItemSource.ShouldCommitCompletion(string typedChar, SnapshotPoint location)
        {
            return true;
        }

        bool IAsyncCompletionItemSource.ShouldTriggerCompletion(string typedChar, SnapshotPoint location)
        {
            return true;
        }
    }

    [Export(typeof(IAsyncCompletionItemSource))]
    [Name("Debug HTML completion item source")]
    [Order(After = "default")]
    [ContentType("RazorCSharp")]
    public class DebugHtmlCompletionItemSource : IAsyncCompletionItemSource
    {
        void IAsyncCompletionItemSource.CustomCommit(Text.Editor.ITextView view, ITextBuffer buffer, CompletionItem item, ITrackingSpan applicableSpan, string commitCharacter)
        {
            throw new System.NotImplementedException();
        }

        async Task<CompletionContext> IAsyncCompletionItemSource.GetCompletionContextAsync(CompletionTrigger trigger, SnapshotPoint triggerLocation)
        {
            return await Task.FromResult(new CompletionContext(ImmutableArray.Create(new CompletionItem("html", this), new CompletionItem("head", this), new CompletionItem("body", this), new CompletionItem("header", this)), new SnapshotSpan(triggerLocation, 0)));
        }

        async Task<object> IAsyncCompletionItemSource.GetDescriptionAsync(CompletionItem item)
        {
            return await Task.FromResult(item.DisplayText);
        }

        ImmutableArray<string> IAsyncCompletionItemSource.GetPotentialCommitCharacters()
        {
            return ImmutableArray.Create(" ", ">", "=", "\t");
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        async Task IAsyncCompletionItemSource.HandleViewClosedAsync(Text.Editor.ITextView view)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            return;
        }

        bool IAsyncCompletionItemSource.ShouldCommitCompletion(string typedChar, SnapshotPoint location)
        {
            return true;
        }

        bool IAsyncCompletionItemSource.ShouldTriggerCompletion(string typedChar, SnapshotPoint location)
        {
            return true;
        }
    }
#endif
}
