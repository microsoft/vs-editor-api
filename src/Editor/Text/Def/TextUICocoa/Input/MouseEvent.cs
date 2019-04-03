//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License. See License.txt in the project root for license information.
//

using AppKit;

namespace Microsoft.VisualStudio.Text.Editor
{
    /// <summary>
    /// A native <see cref="NSEvent"/> mouse input event that 
    /// allows for indicating that the event has been handled.
    /// </summary>
    public sealed class MouseEvent : InputEvent
    {
        internal MouseEvent(NSEvent @event) : base(@event)
        {
        }
    }
}