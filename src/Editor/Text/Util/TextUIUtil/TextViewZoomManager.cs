using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.Text.Utilities
{
    [Export(typeof(ITextViewZoomManager))]
    internal class TextViewZoomManager : BaseProxyService<ITextViewZoomManager>, ITextViewZoomManager
    {
        [ImportImplementations(typeof(ITextViewZoomManager))]
        protected override IEnumerable<Lazy<ITextViewZoomManager, IOrderable>> UnorderedImplementations { get; set; }

        public void ZoomIn(ITextView textView)
        {
            BestImplementation.ZoomIn(textView);
        }

        public void ZoomOut(ITextView textView)
        {
            BestImplementation.ZoomOut(textView);
        }

        public void ZoomTo(ITextView textView, double zoomLevel)
        {
            BestImplementation.ZoomTo(textView, zoomLevel);
        }

        public double ZoomLevel(ITextView textView)
        {
            return BestImplementation.ZoomLevel(textView);
        }
    }
}
