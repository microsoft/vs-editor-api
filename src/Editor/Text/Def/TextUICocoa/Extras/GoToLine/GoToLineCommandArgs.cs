//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License. See License.txt in the project root for license information.
//

using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Editor.Commanding;

namespace Microsoft.VisualStudio.Text.Extras.GoToLine
{
    public class GoToLineCommandArgs : EditorCommandArgs
    {
        public int? LineNumber { get; private set; }
        public int? ColumnNumber { get; private set; }

        public GoToLineCommandArgs(
            ITextView textView,
            ITextBuffer subjectBuffer)
            : this(textView, subjectBuffer, null, null)
        {
        }

        public GoToLineCommandArgs(
            ITextView textView,
            ITextBuffer subjectBuffer,
            int? lineNumber,
            int? columnNumber)
            : base(textView, subjectBuffer)
        {
            LineNumber = lineNumber;
            ColumnNumber = columnNumber;
        }
    }
}