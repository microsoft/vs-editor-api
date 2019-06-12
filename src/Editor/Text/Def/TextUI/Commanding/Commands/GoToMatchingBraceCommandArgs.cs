namespace Microsoft.VisualStudio.Text.Editor.Commanding.Commands
{
    public class GoToMatchingBraceCommandArgs : EditorCommandArgs
    {
        public GoToMatchingBraceCommandArgs(ITextView textView, ITextBuffer subjectBuffer) : base(textView, subjectBuffer)
        {
        }
    }
}