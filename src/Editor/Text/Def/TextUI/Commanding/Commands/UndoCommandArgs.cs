namespace Microsoft.VisualStudio.Text.Editor.Commanding.Commands
{
    public sealed class UndoCommandArgs : EditorCommandArgs
    {
        public readonly int Count;

        public UndoCommandArgs(ITextView textView, ITextBuffer subjectBuffer, int count = 1) : base(textView, subjectBuffer)
        {
            this.Count = count;
        }
    }
}
