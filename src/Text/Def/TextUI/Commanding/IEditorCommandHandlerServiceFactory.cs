namespace Microsoft.VisualStudio.Text.Editor.Commanding
{
    /// <summary>
    /// A factory producing <see cref="IEditorCommandHandlerService"/> used to execute commands in a given text view.
    /// </summary>
    /// <remarks>
    /// This is a MEF component and should be imported as
    /// 
    /// [Import]
    /// private IEditorCommandHandlerServiceFactory factory;
    /// </remarks>
    public interface IEditorCommandHandlerServiceFactory
    {
        /// <summary>
        /// Gets or creates a <see cref="IEditorCommandHandlerService"/> for a given <see cref="ITextView"/>.
        /// </summary>
        /// <param name="textView">A text view to get or create <see cref="IEditorCommandHandlerService"/> for.</param>
        IEditorCommandHandlerService GetService(ITextView textView);

        /// <summary>
        /// Gets or creates a <see cref="IEditorCommandHandlerService"/> for a given <see cref="ITextView"/> and <see cref="ITextBuffer"/>.
        /// </summary>
        /// <param name="textView">A text view to get or create <see cref="IEditorCommandHandlerService"/> for.</param>
        /// <param name="subjectBuffer">A text buffer to get or create <see cref="IEditorCommandHandlerService"/> for.</param>
        IEditorCommandHandlerService GetService(ITextView textView, ITextBuffer subjectBuffer);
    }
}
