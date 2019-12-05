//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License. See License.txt in the project root for license information.
//

namespace Microsoft.VisualStudio.Text.Editor.Commanding.Commands
{
    public sealed class FindCommandArgs : EditorCommandArgs
    {
        public FindCommandArgs(ITextView textView, ITextBuffer subjectBuffer) : base(textView, subjectBuffer)
        {
        }
    }

    public sealed class FindNextCommandArgs : EditorCommandArgs
    {
        public FindNextCommandArgs(ITextView textView, ITextBuffer subjectBuffer) : base(textView, subjectBuffer)
        {
        }
    }

    public sealed class FindNextLikeSelectionCommandArgs : EditorCommandArgs
    {
        public FindNextLikeSelectionCommandArgs(ITextView textView, ITextBuffer subjectBuffer) : base(textView, subjectBuffer)
        {
        }
    }

    public sealed class FindPreviousCommandArgs : EditorCommandArgs
    {
        public FindPreviousCommandArgs(ITextView textView, ITextBuffer subjectBuffer) : base(textView, subjectBuffer)
        {
        }
    }

    public sealed class FindPreviousLikeSelectionCommandArgs : EditorCommandArgs
    {
        public FindPreviousLikeSelectionCommandArgs(ITextView textView, ITextBuffer subjectBuffer) : base(textView, subjectBuffer)
        {
        }
    }

    public sealed class ReplaceCommandArgs : EditorCommandArgs
    {
        public ReplaceCommandArgs(ITextView textView, ITextBuffer subjectBuffer) : base(textView, subjectBuffer)
        {
        }
    }

    public sealed class ReplaceNextCommandArgs : EditorCommandArgs
    {
        public ReplaceNextCommandArgs(ITextView textView, ITextBuffer subjectBuffer) : base(textView, subjectBuffer)
        {
        }
    }

    public sealed class ReplaceAllCommandArgs : EditorCommandArgs
    {
        public ReplaceAllCommandArgs(ITextView textView, ITextBuffer subjectBuffer) : base(textView, subjectBuffer)
        {
        }
    }

    public sealed class ToggleMatchCaseCommandArgs : EditorCommandArgs
    {
        public ToggleMatchCaseCommandArgs(ITextView textView, ITextBuffer subjectBuffer) : base(textView, subjectBuffer)
        {
        }
    }

    public sealed class ToggleMatchWholeWordCommandArgs : EditorCommandArgs
    {
        public ToggleMatchWholeWordCommandArgs(ITextView textView, ITextBuffer subjectBuffer) : base(textView, subjectBuffer)
        {
        }
    }

    public sealed class ToggleUseRegularExpressionsCommandArgs : EditorCommandArgs
    {
        public ToggleUseRegularExpressionsCommandArgs(ITextView textView, ITextBuffer subjectBuffer) : base(textView, subjectBuffer)
        {
        }
    }

    public sealed class HideFindCommandArgs : EditorCommandArgs
    {
        public HideFindCommandArgs(ITextView textView, ITextBuffer subjectBuffer) : base(textView, subjectBuffer)
        {
        }
    }

    public sealed class HideReplaceCommandArgs : EditorCommandArgs
    {
        public HideReplaceCommandArgs(ITextView textView, ITextBuffer subjectBuffer) : base(textView, subjectBuffer)
        {
        }
    }

    public sealed class SetSearchStringFromSelectionCommandArgs : EditorCommandArgs
    {
        public SetSearchStringFromSelectionCommandArgs(ITextView textView, ITextBuffer subjectBuffer) : base(textView, subjectBuffer)
        {
        }
    }
}
