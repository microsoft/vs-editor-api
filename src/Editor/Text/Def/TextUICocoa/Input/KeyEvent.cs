//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License. See License.txt in the project root for license information.
//

using System;
using AppKit;

namespace Microsoft.VisualStudio.Text.Editor
{
    /// <summary>
    /// A native <see cref="NSEvent"/> key input event that allows for indicating that
    /// the event has been handled. This event will wrap <see cref="NSEventType.KeyUp"/>,
    /// <see cref="NSEventType.KeyDown"/>, and  <see cref="NSEventType.FlagsChanged"/>
    /// events. 
    /// </summary>
    public sealed class KeyEvent : InputEvent
    {
        internal KeyEvent(NSEvent @event) : base(@event)
        {
            switch (@event.Type)
            {
                case NSEventType.KeyUp:
                case NSEventType.KeyDown:
                case NSEventType.FlagsChanged:
                    break;
                default:
                    throw new ArgumentException(
                        "event type must be KeyUp, KeyDown, or FlagsChanged",
                        nameof(@event));
            }
        }
    }
}