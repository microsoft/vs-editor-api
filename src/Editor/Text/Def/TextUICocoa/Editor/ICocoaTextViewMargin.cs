//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License. See License.txt in the project root for license information.
//

using System;

using AppKit;

namespace Microsoft.VisualStudio.Text.Editor
{
    /// <summary>
    /// Represents margins that are attached to an edge of an <see cref="ICocoaTextView"/>.
    /// </summary>
    public interface ICocoaTextViewMargin : ITextViewMargin
    {
        /// <summary>
        /// Gets the <see cref="NSView"/> that renders the margin.
        /// </summary>
        /// <exception cref="ObjectDisposedException"> if the margin is disposed.</exception>
        NSView VisualElement { get; }

        /// <summary>
        /// Return <see langword="true"/> if some component of <see cref="VisualElement"/>
        /// has input focus. This lets the host know not to handle keyboard commands.
        /// </summary>
        bool ViewElementHasInputFocus { get; }
    }
}