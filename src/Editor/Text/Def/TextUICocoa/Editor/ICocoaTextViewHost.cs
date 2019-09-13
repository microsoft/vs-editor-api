//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License. See License.txt in the project root for license information.
//

using System;

using AppKit;

namespace Microsoft.VisualStudio.Text.Editor
{
    /// <summary>
    /// Contains an <see cref="ICocoaTextView"/> and the margins that surround it,
    /// such as a scrollbar or line number gutter.
    /// </summary>
    public interface ICocoaTextViewHost
    {
        /// <summary>
        /// Closes the text view host and its underlying text view.
        /// </summary>
        /// <exception cref="InvalidOperationException">The text view host is already closed.</exception>
        void Close();

        /// <summary>
        /// Determines whether this text view has been closed.
        /// </summary>
        bool IsClosed { get; }

        /// <summary>
        /// Occurs immediately after closing the text view.
        /// </summary>
        event EventHandler Closed;

        /// <summary>
        /// Notify when HostControl gets a new NSWindow (which could be null)
        /// </summary>
        event EventHandler HostControlMovedToWindow;

        /// <summary>
        /// Gets the <see cref="ICocoaTextViewMargin"/> with the given <paramref name="marginName"/> that is attached to an edge of this <see cref="ICocoaTextView"/>.
        /// </summary>
        /// <param name="marginName">The name of the <see cref="ITextViewMargin"/>.</param>
        /// <returns>The <see cref="ITextViewMargin"/> with a name that matches <paramref name="marginName"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="marginName"/> is null.</exception>
        ICocoaTextViewMargin GetTextViewMargin(string marginName);

        /// <summary>
        /// Gets the <see cref="ICocoaTextView"/> that is contained within this <see cref="ICocoaTextViewHost"/>.
        /// </summary>
        ICocoaTextView TextView { get; }

        /// <summary>
        /// Gets the <see cref="NSView"/> control for this <see cref="ICocoaTextViewHost"/>.
        /// </summary>
        /// <remarks> Use this property to display the <see cref="ICocoaTextViewHost"/> Cocoa control.</remarks>
        NSView HostControl { get; }

        /// <summary>
        /// Return <see langword="true"/> if any margin has a view element with
        /// input focus, which lets the host know not to handle some keyboard
        /// commands.
        /// </summary>
        bool MarginViewElementHasInputFocus { get; }
    }
}