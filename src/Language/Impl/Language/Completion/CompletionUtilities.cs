using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Utilities;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.Language.Intellisense.Implementation
{
    internal class MetadataUtilities<T> where T : class
    {
        /// <summary>
        /// This method creates a collection of (T, SnapshotPoint) pairs where the SnapshotPoint is the originalPoint
        /// translated to the buffer whose Content Type best matches Content Type associated with T
        /// Each instance of T appears only once in the returned collection.
        /// </summary>
        /// <param name="textView"></param>
        /// <param name="originalPoint"></param>
        /// <param name="imports"></param>
        /// <returns></returns>
        internal static IEnumerable<(ITextBuffer buffer, SnapshotPoint point, Lazy<T, IOrderableContentTypeMetadata> import)> GetOrderedBuffersAndImports(ITextView textView, SnapshotPoint location, Func<IContentType, ITextViewRoleSet, IReadOnlyList<Lazy<T, IOrderableContentTypeMetadata>>> getImports, IComparer<IEnumerable<string>> contentTypeComparer)
        {
            // This method is created based on EditorCommandHandlerService.GetOrderedBuffersAndCommandHandlers

            // A general idea is that imports matching more specifically content type of buffers higher in the buffer
            // graph should be executed before those matching buffers lower in the graph or less specific content types.

            // So for example in a projection scenario (projection buffer containing C# buffer), 3 command handlers
            // matching "projection", "CSharp" and "any" content types will be ordered like this:
            // 1. command handler matching "projection" content type is executed on the projection buffer
            // 2. command handler matching "CSharp" content type is executed on the C# buffer
            // 3. command handler matching "any" content type is executed on the projection buffer

            // The ordering algorithm is as follows:
            // 1. Create an ordered list of all affected buffers in the buffer graph
            //    by mapping caret position down and up the buffer graph. In a typical projection scenario
            //    (projection buffer containing C# buffer) that will produce (projection buffer, C# buffer) sequence.
            // 2. For each affected buffer get or create a bucket of matching command handlers,
            //    ordered by [Order] and content type specificity.
            // 3. Pick best command handler in all buckets in terms of content type specificity (e.g.
            //    if one command handler can handle "text" content type, but another can
            //    handle "CSharp" content type, we pick the latter one:
            // 3. Start with top command handler in first non empty bucket.
            // 4. Compare it with top command handlers in all other buckets in terms of content type specificity.
            // 5. yield return current handler or better one if found, pop it from its bucket
            // 6. Repeat starting with #3 utill all buckets are empty.
            //    In the projection scenario that will result in the following
            //    list of (buffer, handler) pairs: (projection buffer, projection handler), (C# buffer, C# handler),
            //    (projection buffer, any handler).

            var mappedPointsEnumeration = GetPointsOnAvailableBuffers(textView, location);
            if (!mappedPointsEnumeration.Any())
                yield break;

            var buffers = mappedPointsEnumeration.Select(n => n.Snapshot.TextBuffer).ToImmutableArray();
            var mappedPoints = mappedPointsEnumeration.ToImmutableArray();

            // An array of per-buffer buckets, each containing cached list of matching imports,
            // ordered by [Order] and content type specificity
            var importBuckets = new ImportBucket<T>[buffers.Length];
            for (int i = 0; i < buffers.Length; i++)
            {
                importBuckets[i] = new ImportBucket<T>(getImports(buffers[i].ContentType, textView.Roles));
            }

            while (true)
            {
                Lazy<T, IOrderableContentTypeMetadata> currentImport = null;
                int currentImportIndex = 0;

                for (int i = 0; i < importBuckets.Length; i++)
                {
                    if (!importBuckets[i].IsEmpty)
                    {
                        currentImport = importBuckets[i].Peek();
                        currentImportIndex = i;
                        break;
                    }
                }

                if (currentImport == null)
                {
                    // All buckets are empty, all done
                    break;
                }

                // Check if any other bucket has a better import (i.e. can handle more specific content type).
                var foundBetterHandler = false;
                for (int i = 0; i < buffers.Length; i++)
                {
                    // Search in other buckets only
                    if (i != currentImportIndex)
                    {
                        if (!importBuckets[i].IsEmpty)
                        {
                            var import = importBuckets[i].Peek();
                            // Can this handler handle content type more specific than top handler in firstNonEmptyBucket?
                            if (contentTypeComparer.Compare(import.Metadata.ContentTypes, currentImport.Metadata.ContentTypes) < 0)
                            {
                                foundBetterHandler = true;
                                importBuckets[i].Pop();
                                yield return (buffers[i], mappedPoints[i], import);
                                break;
                            }
                        }
                    }
                }

                if (!foundBetterHandler)
                {
                    yield return (buffers[currentImportIndex], mappedPoints[currentImportIndex], currentImport);
                    importBuckets[currentImportIndex].Pop();
                }
            }
        }

        /// <summary>
        /// A simpler method that returns all imports with declared content type that matches content type of subject buffers available at the given location
        /// </summary>
        /// <param name="textView"></param>
        /// <param name="location"></param>
        /// <param name="getImports"></param>
        /// <param name="contentTypeComparer"></param>
        /// <returns></returns>
        internal static IEnumerable<(ITextBuffer buffer, SnapshotPoint point, Lazy<T, IOrderableContentTypeMetadata> import)> GetBuffersAndImports(ITextView textView, SnapshotPoint location, Func<IContentType, ITextViewRoleSet, IReadOnlyList<Lazy<T, IOrderableContentTypeMetadata>>> getImports)
        {
            var mappedPointsEnumeration = GetPointsOnAvailableBuffers(textView, location);
            if (!mappedPointsEnumeration.Any())
                yield break;

            var buffers = mappedPointsEnumeration.Select(n => n.Snapshot.TextBuffer).ToImmutableArray();
            var mappedPoints = mappedPointsEnumeration.ToImmutableArray();

            // An array of per-buffer buckets, each containing cached list of matching imports,
            // ordered by [Order] and content type specificity
            var importBuckets = new ImportBucket<T>[buffers.Length];
            for (int i = 0; i < buffers.Length; i++)
            {
                foreach (var import in getImports(buffers[i].ContentType, textView.Roles))
                    yield return (buffers[i], mappedPoints[i], import);
            }
        }

        private static IEnumerable<SnapshotPoint> GetPointsOnAvailableBuffers(ITextView textView, SnapshotPoint location)
        {
            var mappingPoint = textView.BufferGraph.CreateMappingPoint(location, PointTrackingMode.Negative);
            var buffers = textView.BufferGraph.GetTextBuffers(b => mappingPoint.GetPoint(b, PositionAffinity.Predecessor) != null);
            var pointsInBuffers = buffers.Select(b => mappingPoint.GetPoint(b, PositionAffinity.Predecessor).Value);
            return pointsInBuffers;
        }
    }

    internal static class CompletionUtilities
    {
        /// <summary>
        /// Maps given point to buffers that contain this point. Requires UI thread.
        /// </summary>
        /// <param name="textView"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        internal static IEnumerable<ITextBuffer> GetBuffersForPoint(ITextView textView, SnapshotPoint point)
        {
            // We are looking at the buffer to the left of the caret.
            return textView.BufferGraph.GetTextBuffers(n =>
                textView.BufferGraph.MapDownToBuffer(point, PointTrackingMode.Negative, n, PositionAffinity.Predecessor) != null);
        }
    }
}
