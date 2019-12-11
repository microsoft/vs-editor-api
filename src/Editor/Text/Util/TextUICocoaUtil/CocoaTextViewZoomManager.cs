
using System;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.Text.Utilities
{
    [ExportImplementation(typeof(ITextViewZoomManager))]
    [Name("Cocoa zoom manager")]
    [Order(Before = "default")]
    internal class CocoaTextViewZoomManager : ITextViewZoomManager
    {
        public double ZoomLevel(ITextView textView) => ((ICocoaTextView)textView).ZoomLevel;

        public void ZoomIn(ITextView textView)
        {
            if (textView is null)
            {
                throw new ArgumentNullException(nameof(textView));
            }

            ICocoaTextView cocoaTextView = textView as ICocoaTextView;
            if (cocoaTextView != null && cocoaTextView.Roles.Contains(PredefinedTextViewRoles.Zoomable))
            {
                double zoomLevel = cocoaTextView.ZoomLevel * ZoomConstants.ScalingFactor;
                if (zoomLevel < ZoomConstants.MaxZoom || Math.Abs(zoomLevel - ZoomConstants.MaxZoom) < 0.00001)
                {
                    cocoaTextView.Options.GlobalOptions.SetOptionValue(DefaultTextViewOptions.ZoomLevelId, zoomLevel);
                }
            }
        }

        public void ZoomOut(ITextView textView)
        {
            if (textView is null)
            {
                throw new ArgumentNullException(nameof(textView));
            }

            ICocoaTextView cocoaTextView = textView as ICocoaTextView;
            if (cocoaTextView != null && cocoaTextView.Roles.Contains(PredefinedTextViewRoles.Zoomable))
            {
                double zoomLevel = cocoaTextView.ZoomLevel / ZoomConstants.ScalingFactor;
                if (zoomLevel > ZoomConstants.MinZoom || Math.Abs(zoomLevel - ZoomConstants.MinZoom) < 0.00001)
                {
                    cocoaTextView.Options.GlobalOptions.SetOptionValue(DefaultTextViewOptions.ZoomLevelId, zoomLevel);
                }
            }
        }

        public void ZoomTo(ITextView textView, double zoomLevel)
        {
            if (textView is null)
            {
                throw new ArgumentNullException(nameof(textView));
            }

            ICocoaTextView cocoaTextView = textView as ICocoaTextView;
            if (cocoaTextView != null && cocoaTextView.Roles.Contains(PredefinedTextViewRoles.Zoomable))
            {
                cocoaTextView.Options.GlobalOptions.SetOptionValue(DefaultTextViewOptions.ZoomLevelId, zoomLevel);
            }
        }
    }
}
