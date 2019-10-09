//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License. See License.txt in the project root for license information.
//

using System;

using AppKit;

namespace Microsoft.VisualStudio.Text.Editor
{
    /// <summary>
    /// Base class wrapping a native <see cref="NSEvent"/> input event
    /// that allows for indicating that the event has been handled.
    /// </summary>
    public abstract class InputEventArgs : EventArgs
    {
        /// <summary>
        /// The event thas is being processed.
        /// </summary>
        public NSEvent Event { get; }

        /// <summary>
        /// The type of event that is being processed.
        /// </summary>
        public NSEventType Type => Event.Type;

        /// <summary>
        /// Modifier flags for the event.
        /// </summary>
        public NSEventModifierMask ModifierFlags => Event.ModifierFlags;

        /// <summary>
        /// Destination window for the event.
        /// </summary>
        public NSWindow Window => Event.Window;

        /// <summary>
        /// Whether or not this event has been handled.
        /// </summary>
        public bool Handled { get; set; }

        private protected InputEventArgs(NSEvent @event)
            => Event = @event ?? throw new ArgumentNullException(nameof(@event));
    }
}