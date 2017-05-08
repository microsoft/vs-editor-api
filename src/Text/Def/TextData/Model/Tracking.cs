using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.VisualStudio.Text
{
    public static class Tracking
    {
        /// <summary>
        /// Track a position forward in time using forward fidelity.
        /// </summary>
        public static int TrackPositionForwardInTime(PointTrackingMode trackingMode,
                                                     int currentPosition,
                                                     ITextVersion currentVersion,
                                                     ITextVersion targetVersion)
        {
            if (trackingMode < PointTrackingMode.Positive || trackingMode > PointTrackingMode.Negative)
            {
                throw new ArgumentOutOfRangeException("trackingMode");
            }
            if (currentVersion == null)
            {
                throw new ArgumentNullException("currentVersion");
            }
            if (targetVersion == null)
            {
                throw new ArgumentNullException("targetVersion");
            }
            if (targetVersion.TextBuffer != currentVersion.TextBuffer)
            {
                throw new ArgumentException("currentVersion and targetVersion must be from the same ITextBuffer");
            }
            if (targetVersion.VersionNumber < currentVersion.VersionNumber)
            {
                throw new ArgumentOutOfRangeException("targetVersion");
            }
            if (currentPosition < 0 || currentPosition > currentVersion.Length)
            {
                throw new ArgumentOutOfRangeException("currentPosition");
            }

            // track forward in time
            while (currentVersion != targetVersion)
            {
                int changeCount = currentVersion.Changes.Count;

                // perform binary search over the old text (deleted) ranges
                int lo = 0;
                int hi = changeCount - 1;
                while (lo <= hi)
                {
                    int mid = (lo + hi) / 2;
                    ITextChange textChange = currentVersion.Changes[mid];
                    if (currentPosition < textChange.OldPosition)
                    {
                        hi = mid - 1;
                    }
                    else if (currentPosition > textChange.OldEnd)
                    {
                        lo = mid + 1;
                    }
                    else
                    {
                        // currentPosition lies within or on the edge of 
                        // text deleted by the change
                        if (IsOpaque(textChange))
                        {
                            int offset = currentPosition - textChange.OldPosition;

                            if (offset > 0)
                            {
                                if ((offset >= textChange.OldLength) || (offset >= textChange.NewLength))
                                {
                                    offset = textChange.NewLength;
                                }
                                else if (ShouldOffsetEndpointOfChange(textChange, offset, isForwardTracking: true))
                                {
                                    // Shift offset to the front of the \r\n
                                    --offset;
                                }
                            }

                            currentPosition = textChange.NewPosition + offset;
                        }
                        else
                        {
                            if (trackingMode == PointTrackingMode.Positive)
                            {
                                currentPosition = textChange.NewEnd;
                            }
                            else
                            {
                                currentPosition = textChange.NewPosition;
                            }
                        }
                        break;
                    }
                }

                if (hi < lo)
                {
                    // currentPosition is outside all changes.
                    Debug.Assert(hi == lo - 1);
                    if (lo > 0)
                    {
                        // currentPosition is to the right of Changes[hi]
                        ITextChange textChange = currentVersion.Changes[hi];
                        currentPosition += (textChange.NewEnd - textChange.OldEnd);
                    }
                }

                currentVersion = currentVersion.Next;
            }

            Debug.Assert(currentPosition >= 0 && currentPosition <= currentVersion.Length);
            return currentPosition;
        }

        /// <summary>
        /// Returns true if endpoint of a change lands in between a \r\n sequence and so needs to be offsetted.
        /// </summary>
        private static bool ShouldOffsetEndpointOfChange(ITextChange textChange, int offset, bool isForwardTracking)
        {
            var textChange3 = textChange as ITextChange3;
            if (isForwardTracking)
            {
                if (textChange3 == null)
                {
                    Debug.Fail("ITextChange implementation unexpectedly doesn't implement ITextChange3.");
                    return (textChange.NewText[offset] == '\n') && (textChange.NewText[offset - 1] == '\r') && 
                        // Don't let the translated point land in-between a \r\n (unless it started there)
                        ((textChange.OldText[offset] != '\n') || (textChange.OldText[offset - 1] != '\r'));
                }

                // We want to avoid a situation where, when translating a point across an opaque change, 
                // we have it land in the middle of a \r\n (this can break the elision buffer if the point in 
                // question is the endpoint of an elided span).
                // This test basically says that if a point lands between a \r\n in the new snapshot but didn’t 
                // start between a \r\n in the old snapshot, we will offset it by one to avoid the problem.
                return (textChange3.GetNewTextAt(offset) == '\n') && (textChange3.GetNewTextAt(offset - 1) == '\r') &&
                        // Don't let the translated point land in-between a \r\n (unless it started there)
                        ((textChange3.GetOldTextAt(offset) != '\n') || (textChange3.GetOldTextAt(offset - 1) != '\r'));
            }
            else
            {
                // Backward in time tracking

                if (textChange3 == null)
                {
                    Debug.Fail("ITextChange implementation unexpectedly doesn't implement ITextChange3.");
                    return (textChange.OldText[offset] == '\n') && (textChange.OldText[offset - 1] == '\r') && 
                        // Don't let the translated point land in-between a \r\n (unless it started there)
                        ((textChange.NewText[offset] != '\n') || (textChange.NewText[offset - 1] != '\r'));
                }

                return (textChange3.GetOldTextAt(offset) == '\n') && (textChange3.GetOldTextAt(offset - 1) == '\r') &&
                        // Don't let the translated point land in-between a \r\n (unless it started there)
                        ((textChange3.GetNewTextAt(offset) != '\n') || (textChange3.GetNewTextAt(offset - 1) != '\r'));
            }
        }

        /// <summary>
        /// Track a position backward in time using forward fidelity.
        /// </summary>
        public static int TrackPositionBackwardInTime(PointTrackingMode trackingMode,
                                                      int currentPosition,
                                                      ITextVersion currentVersion,
                                                      ITextVersion targetVersion)
        {
            if (trackingMode < PointTrackingMode.Positive || trackingMode > PointTrackingMode.Negative)
            {
                throw new ArgumentOutOfRangeException("trackingMode");
            }
            if (currentVersion == null)
            {
                throw new ArgumentNullException("currentVersion");
            }
            if (targetVersion == null)
            {
                throw new ArgumentNullException("targetVersion");
            }
            if (targetVersion.TextBuffer != currentVersion.TextBuffer)
            {
                throw new ArgumentException("currentVersion and targetVersion must be from the same ITextBuffer");
            }
            if (targetVersion.VersionNumber > currentVersion.VersionNumber)
            {
                throw new ArgumentOutOfRangeException("targetVersion");
            }
            if (currentPosition < 0 || currentPosition > currentVersion.Length)
            {
                throw new ArgumentOutOfRangeException("currentPosition");
            }

            // track backwards in time
            IList<ITextChange>[] textChangesStack = new IList<ITextChange>[currentVersion.VersionNumber - targetVersion.VersionNumber];
            int top = 0;
            {
                ITextVersion roverVersion = targetVersion;
                while (roverVersion != currentVersion)
                {
                    textChangesStack[top++] = roverVersion.Changes;
                    roverVersion = roverVersion.Next;
                }
            }

            while (top > 0)
            {
                IList<ITextChange> textChanges = textChangesStack[--top];

                // perform binary search over the old text (deleted) ranges
                int lo = 0;
                int hi = textChanges.Count - 1;
                while (lo <= hi)
                {
                    int mid = (lo + hi) / 2;
                    ITextChange textChange = textChanges[mid];
                    if (currentPosition < textChange.NewPosition)
                    {
                        hi = mid - 1;
                    }
                    else if (currentPosition > textChange.NewEnd)
                    {
                        lo = mid + 1;
                    }
                    else
                    {
                        // currentPosition lies within or on the edge of 
                        // text deleted by the change
                        if (IsOpaque(textChange))
                        {
                            int offset = currentPosition - textChange.NewPosition;

                            if (offset > 0)
                            {
                                if ((offset >= textChange.OldLength) || (offset >= textChange.NewLength))
                                {
                                    offset = textChange.OldLength;
                                }
                                else if (ShouldOffsetEndpointOfChange(textChange, offset, isForwardTracking: false))
                                {
                                    // Shift offset to the front of the \r\n
                                    --offset;
                                }
                            }

                            currentPosition = textChange.OldPosition + offset;
                        }
                        else
                        {
                            if (trackingMode == PointTrackingMode.Positive)
                            {
                                currentPosition = textChange.OldEnd;
                            }
                            else
                            {
                                currentPosition = textChange.OldPosition;
                            }
                        }
                        break;
                    }
                }

                if (hi < lo)
                {
                    // currentPosition is outside all changes.
                    Debug.Assert(hi == lo - 1);
                    if (lo > 0)
                    {
                        // currentPosition is to the right of Changes[hi]
                        ITextChange textChange = textChanges[hi];
                        currentPosition += (textChange.OldEnd - textChange.NewEnd);
                    }
                }
            }

            return currentPosition;
        }

        /// <summary>
        /// Track a span forward in time using forward fidelity.
        /// </summary>
        public static Span TrackSpanForwardInTime(SpanTrackingMode trackingMode, Span span, ITextVersion currentVersion, ITextVersion targetVersion)
        {
            if (trackingMode < SpanTrackingMode.EdgeExclusive || trackingMode > SpanTrackingMode.Custom)
            {
                throw new ArgumentOutOfRangeException("trackingMode");
            }
            if (currentVersion == null)
            {
                throw new ArgumentNullException("currentVersion");
            }
            if (targetVersion == null)
            {
                throw new ArgumentNullException("targetVersion");
            }
            if (targetVersion.TextBuffer != currentVersion.TextBuffer)
            {
                throw new ArgumentException("currentVersion and targetVersion must be from the same ITextBuffer");
            }
            if (span.End > currentVersion.Length)
            {
                throw new ArgumentOutOfRangeException("span");
            }
            if (targetVersion.VersionNumber < currentVersion.VersionNumber)
            {
                throw new ArgumentOutOfRangeException("targetVersion");
            }

            int resultStart =
                TrackPositionForwardInTime
                    ((trackingMode == SpanTrackingMode.EdgeExclusive || trackingMode == SpanTrackingMode.EdgePositive)
                          ? PointTrackingMode.Positive
                          : PointTrackingMode.Negative, span.Start, currentVersion, targetVersion);

            int resultEnd =
                TrackPositionForwardInTime
                    ((trackingMode == SpanTrackingMode.EdgeExclusive || trackingMode == SpanTrackingMode.EdgeNegative)
                          ? PointTrackingMode.Negative
                          : PointTrackingMode.Positive, span.End, currentVersion, targetVersion);

            return Span.FromBounds(resultStart, System.Math.Max(resultStart, resultEnd));
        }

        /// <summary>
        /// Track a span backward in time using forward fidelity.
        /// </summary>
        public static Span TrackSpanBackwardInTime(SpanTrackingMode trackingMode, Span span, ITextVersion currentVersion, ITextVersion targetVersion)
        {
            if (trackingMode < SpanTrackingMode.EdgeExclusive || trackingMode > SpanTrackingMode.Custom)
            {
                throw new ArgumentOutOfRangeException("trackingMode");
            }
            if (currentVersion == null)
            {
                throw new ArgumentNullException("currentVersion");
            }
            if (targetVersion == null)
            {
                throw new ArgumentNullException("targetVersion");
            }
            if (targetVersion.TextBuffer != currentVersion.TextBuffer)
            {
                throw new ArgumentException("currentVersion and targetVersion must be from the same ITextBuffer");
            }
            if (span.End > currentVersion.Length)
            {
                throw new ArgumentOutOfRangeException("span");
            }
            if (targetVersion.VersionNumber > currentVersion.VersionNumber)
            {
                throw new ArgumentOutOfRangeException("targetVersion");
            }

            int resultStart =
                TrackPositionBackwardInTime
                    ((trackingMode == SpanTrackingMode.EdgeExclusive || trackingMode == SpanTrackingMode.EdgePositive)
                          ? PointTrackingMode.Positive
                          : PointTrackingMode.Negative,
                     span.Start, currentVersion, targetVersion);

            int resultEnd =
                TrackPositionBackwardInTime
                    ((trackingMode == SpanTrackingMode.EdgeExclusive || trackingMode == SpanTrackingMode.EdgeNegative)
                          ? PointTrackingMode.Negative
                          : PointTrackingMode.Positive,
                      span.End, currentVersion, targetVersion);

            return Span.FromBounds(resultStart, System.Math.Max(resultStart, resultEnd));
        }

        private static bool IsOpaque(ITextChange textChange)
        {
            ITextChange2 tc2 = textChange as ITextChange2;
            return tc2 != null && tc2.IsOpaque;
        }
    }
}