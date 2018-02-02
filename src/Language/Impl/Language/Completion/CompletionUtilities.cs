using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.Language.Intellisense.Implementation
{
    internal static class CompletionUtilities
    {

        internal static IEnumerable<ITextBuffer> GetBuffersForTriggerPoint(ITextView view, SnapshotPoint point)
        {
            // We are looking at the buffer to the left of the caret.
            return view.BufferGraph.GetTextBuffers(n =>
                view.BufferGraph.MapDownToBuffer(point, PointTrackingMode.Negative, n, PositionAffinity.Predecessor) != null);
        }

        internal static IDictionary<IAsyncCompletionItemSource, SnapshotPoint> GetCompletionSourcesWithMappedLocations(ITextView view, SnapshotPoint originalPoint, Func<IContentType, ImmutableArray<IAsyncCompletionItemSource>> getCompletionItemSources)
        {
            // This method is created based on EditorCommandHandlerService.GetOrderedBuffersAndCommandHandlers

            // This method creates a collection of IAsyncCompletionItemSource, SnapshotPoint pairs
            // that where the SnapshotPoint is originalPoint translated to the buffer pertinent to the IAsyncCompletionItemSource
            // The collection is a dictionary such that each completion item source appears only once.

            // A general idea is that command handlers matching more specifically content type of buffers higher in the buffer
            // graph should be executed before those matching buffers lower in the graph or less specific content types.

            // So for example in a projection scenario (projection buffer containing C# buffer), 3 command handlers
            // matching "projection", "CSharp" and "any" content types will be ordered like this:
            // 1. command handler matching "projection" content type is executed on the projection buffer
            // 2. command handler matching "CSharp" content type is executed on the C# buffer
            // 3. command handler matching "any" content type is executed on the projection buffer

            // The ordering algorithm is as follows:
            // 1. Create an ordered list of all affected buffers in the buffer graph (TODO: this should be an extensibility point)
            //    by mapping caret position down and up the buffer graph. In a typical projection scenario
            //    (projection buffer containing C# buffer) that will produce (projection buffer, C# buffer) sequence.
            // 2. Create an ordered list of all content types in those buffers from most to least specific while
            //    resolving ties (such as between "projection" and "text" content types) per buffer order. Again, in
            //    a projection scenario that will result in "projection, CSharp, text, any" sequence.
            // 3. Now for each content type in the list and for each buffer in the buffer list find all command handlers
            //    matching that content type (exactly) and pair them with buffers. That will result in aforementioned
            //    list of (buffer, handler) pairs.

            var sortedContentTypes = new SortedSet<IContentType>(ContentTypeComparer.Instance);
            var result = new Dictionary<IAsyncCompletionItemSource, SnapshotPoint>();

            var mappedPoints = GetPointsOnAvailableBuffers(view, originalPoint);
            foreach (var mappedPoint in mappedPoints)
            {
                AddContentTypeHierarchy(sortedContentTypes, mappedPoint.Snapshot.ContentType);
            }

            foreach (var contentType in sortedContentTypes)
            {
                foreach (var mappedPoint in mappedPoints)
                {
                    if (mappedPoint.Snapshot.ContentType.IsOfType(contentType.TypeName))
                    {
                        foreach (var source in getCompletionItemSources(contentType))
                        {
                            if (!result.ContainsKey(source))
                                result.Add(source, mappedPoint);
                        }
                    }
                }
            }
            return result;
        }

        private static IEnumerable<SnapshotPoint> GetPointsOnAvailableBuffers(ITextView view, SnapshotPoint point)
        {
            var mappingPoint = view.BufferGraph.CreateMappingPoint(point, PointTrackingMode.Negative);
            var buffers = view.BufferGraph.GetTextBuffers(b => mappingPoint.GetPoint(b, PositionAffinity.Predecessor) != null);
            var pointsInBuffers = buffers.Select(b => mappingPoint.GetPoint(b, PositionAffinity.Predecessor).Value);
            return pointsInBuffers;
        }

        private static void AddContentTypeHierarchy(SortedSet<IContentType> sortedContentTypes, IContentType contentType)
        {
            sortedContentTypes.Add(contentType);

            foreach (var baseContentType in contentType.BaseTypes)
            {
                AddContentTypeHierarchy(sortedContentTypes, baseContentType);
            }
        }

        /// <summary>
        /// Custom comparer for sorting content types
        /// </summary>
        private class ContentTypeComparer : IComparer<IContentType>
        {
            public static ContentTypeComparer Instance = new ContentTypeComparer();

            public int Compare(IContentType x, IContentType y)
            {
                if (x == y)
                {
                    return 0;
                }

                // If x is of type y (e.g. "code" is of type "text") that means it's more specific and so
                // consider it less than y (to order higher than y).
                if (x.IsOfType(y.TypeName))
                {
                    return -1;
                }

                return 1;
            }
        }
    }
}
