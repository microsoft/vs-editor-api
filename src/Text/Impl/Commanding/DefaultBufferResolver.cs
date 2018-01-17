using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Editor.Commanding;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.UI.Text.Commanding.Implementation
{
    [Export(typeof(ICommandingTextBufferResolverProvider))]
    [ContentType("any")]
    internal class DefaultBufferResolverProvider : ICommandingTextBufferResolverProvider
    {
        public ICommandingTextBufferResolver CreateResolver(ITextView textView)
        {
            return new DefaultBufferResolver(textView);
        }
    }

    internal class DefaultBufferResolver : ICommandingTextBufferResolver
    {
        private readonly ITextView _textView;

        public DefaultBufferResolver(ITextView textView)
        {
            _textView = textView ?? throw new ArgumentNullException(nameof(textView));
        }

        public IEnumerable<ITextBuffer> ResolveBuffersForCommand<TArgs>() where TArgs : EditorCommandArgs
        {
            var mappingPoint = _textView.BufferGraph.CreateMappingPoint(_textView.Caret.Position.BufferPosition, PointTrackingMode.Negative);
            return _textView.BufferGraph.GetTextBuffers((b) => mappingPoint.GetPoint(b, PositionAffinity.Successor) != null);
        }
    }
}
