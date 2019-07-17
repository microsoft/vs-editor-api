//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License. See License.txt in the project root for license information.
//

using System;
using System.Linq;

using AppKit;
using CoreGraphics;

using Microsoft.VisualStudio.Text.Formatting;

namespace Microsoft.VisualStudio.Text.Editor
{
    public static class ICocoaTextViewExtensions
    {
        public static CGPoint GetViewRelativeMousePosition(this ICocoaTextView textView, NSEvent theEvent)
            => textView.VisualElement.ConvertPointFromView(theEvent.LocationInWindow, null);

        public static void MoveCaretToPosition(this ICocoaTextView textView, NSEvent theEvent)
        {
            var mousePosition = GetViewRelativeMousePosition(textView, theEvent);

            mousePosition.X += (nfloat)textView.ViewportLeft;
            mousePosition.Y += (nfloat)textView.ViewportTop;

            // Set the caret at the closest mouse location (if there is no selection surrounding the clicked location)
            ITextViewLine textViewLine = textView.TextViewLines.GetTextViewLineContainingYCoordinate(mousePosition.Y);
            if (textViewLine != null)
            {
                VirtualSnapshotPoint clickedBufferPosition = textViewLine.GetInsertionBufferPositionFromXCoordinate(mousePosition.X);

                // What we're trying to determine is: if the caret were to be placed by this
                // click, would it still be inside the current selection?

                bool insideSelection;

                // If neither virtual space is enabled nor the selection mode is box, check the non-virtual buffer point
                if (!(textView.Options.GetOptionValue(DefaultTextViewOptions.UseVirtualSpaceId) || (textView.Selection.Mode == TextSelectionMode.Box)))
                {
                    SnapshotPoint nonVirtualPoint = clickedBufferPosition.Position;
                    insideSelection = textView.Selection.SelectedSpans.Any(selectionSpan => selectionSpan.Contains(nonVirtualPoint));
                }
                else
                {
                    insideSelection = textView.Selection.VirtualSelectedSpans.Any(selectionSpan => selectionSpan.Contains(clickedBufferPosition));
                }

                // If this is outside of the selection, move the caret.
                if (!insideSelection)
                {
                    textView.Caret.MoveTo(textViewLine, mousePosition.X);
                    textView.Selection.Clear();
                    textView.Caret.EnsureVisible();
                }
            }
            else
            {
                textView.Caret.MoveTo(new SnapshotPoint(textView.TextSnapshot, textView.TextSnapshot.Length));
            }
        }
    }
}
