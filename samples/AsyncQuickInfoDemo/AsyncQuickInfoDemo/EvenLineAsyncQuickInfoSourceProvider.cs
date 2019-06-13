namespace AsyncQuickInfoDemo
{
    using System.ComponentModel.Composition;
    using Microsoft.VisualStudio.Text;
    using Microsoft.VisualStudio.Language.Intellisense;
    using Microsoft.VisualStudio.Utilities;

    [Export(typeof(IAsyncQuickInfoSourceProvider))]
    [Name("Even Line Async Quick Info Provider")]
    [ContentType("any")]
    [Order]
    internal sealed class EvenLineAsyncQuickInfoSourceProvider : IAsyncQuickInfoSourceProvider
    {
        public IAsyncQuickInfoSource TryCreateQuickInfoSource(ITextBuffer textBuffer)
        {
            return new EvenLineAsyncQuickInfoSource(textBuffer);
        }
    }
}