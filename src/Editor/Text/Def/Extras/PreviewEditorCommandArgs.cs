namespace Microsoft.VisualStudio.Text.Editor.Commanding.Commands
{
    public sealed class ProvideEditorFeedbackCommandArgs : EditorCommandArgs
    {
        public ProvideEditorFeedbackCommandArgs(ITextView textView, ITextBuffer subjectBuffer) : base(textView, subjectBuffer)
        {
        }
    }

    public sealed class DisableEditorPreviewCommandArgs : EditorCommandArgs
    {
        public DisableEditorPreviewCommandArgs(ITextView textView, ITextBuffer subjectBuffer) : base(textView, subjectBuffer)
        {
        }
    }

    public sealed class LearnAboutTheEditorCommandArgs : EditorCommandArgs
    {
        public LearnAboutTheEditorCommandArgs(ITextView textView, ITextBuffer subjectBuffer) : base(textView, subjectBuffer)
        {
        }
    }
}