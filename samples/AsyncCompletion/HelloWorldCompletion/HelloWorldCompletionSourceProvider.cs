using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using System;
using System.ComponentModel.Composition;

namespace HelloWorldCompletion
{
    [Export(typeof(IAsyncCompletionSourceProvider))]
    [ContentType("CSharp")]
    [Name("Hello World completion item source")]
    internal class HelloWorldCompletionSourceProvider : IAsyncCompletionSourceProvider
    {
        Lazy<HelloWorldCompletionSource> Source = new Lazy<HelloWorldCompletionSource>(() => new HelloWorldCompletionSource());

        public IAsyncCompletionSource GetOrCreate(ITextView textView)
        {
            return Source.Value;
        }
    }
}
