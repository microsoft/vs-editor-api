//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License. See License.txt in the project root for license information.
//

using AppKit;
using CoreGraphics;

namespace Microsoft.VisualStudio.Text.Editor
{
    /// <summary>
    /// A native <see cref="NSEvent"/> mouse input event that 
    /// allows for indicating that the event has been handled.
    /// </summary>
    public sealed class MouseEventArgs : InputEventArgs
    {
        /// <summary>
        /// The number of mouse clicks associated with a mouse-down or mouse-up event.
        /// </summary>
        public int ClickCount => (int)Event.ClickCount;

        /// <summary>
        /// Location of the mouse in window coordinates.
        /// </summary>
        public CGPoint LocationInWindow => Event.LocationInWindow;

        internal MouseEventArgs(NSEvent @event) : base(@event)
        {
        }
    }
}