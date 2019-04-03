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
    public abstract class InputEvent
    {
        /// <summary>
        /// The event thas is being processed.
        /// </summary>
        public NSEvent Event { get; }

        /// <summary>
        /// Whether or not this event has been handled.
        /// </summary>
        public bool Handled { get; set; }

        private protected InputEvent(NSEvent @event)
            => Event = @event ?? throw new ArgumentNullException(nameof(@event));
    }
}