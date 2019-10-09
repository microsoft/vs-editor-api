using System;
using AppKit;

namespace Microsoft.VisualStudio.Text.Editor
{
    public sealed class DragEventArgs : EventArgs
    {
        /// <summary>
        /// The dragginginfo thas is being processed.
        /// </summary>
        public NSDraggingInfo DraggingInfo { get; }

        /// <summary>
        /// Determines what to return for DragEnter and DragUpdate events to Cocoa
        /// </summary>
        public NSDragOperation DragOperation { get; set; }

        /// <summary>
        /// Whether or not this event has been handled.
        /// </summary>
        public bool Handled { get; set; }

        public DragEventArgs(NSDraggingInfo draggingInfo)
        {
            DraggingInfo = draggingInfo;
        }
    }
}