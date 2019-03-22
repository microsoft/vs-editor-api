namespace Microsoft.VisualStudio.Text.Editor.Commanding.Commands
{
    public sealed class RedoCommandArgs : EditorCommandArgs
    {
        public readonly int Count;

        public RedoCommandArgs(ITextView textView, ITextBuffer subjectBuffer, int count = 1) : base(textView, subjectBuffer)
        {
            this.Count = count;
        }
    }
}
