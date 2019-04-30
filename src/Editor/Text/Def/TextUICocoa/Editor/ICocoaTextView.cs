//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License. See License.txt in the project root for license information.
//

using System;

using AppKit;
using CoreGraphics;

namespace Microsoft.VisualStudio.Text.Editor
{
    public interface ICocoaTextView : ITextView3
    {
        /// <summary>
        /// Gets the <see cref="NSView"/> that renders the view.
        /// </summary>
        NSView VisualElement { get; }

        /// <summary>
        /// Gets or sets the background for the visual element.
        /// </summary>
        CGColor Background { get; set; }

        /// <summary>
        /// Occurs when the <see cref="Background"/> is set.
        /// </summary>
        event EventHandler<CGColor> BackgroundBrushChanged;

        /// <summary>
        /// Occurs when the <see cref="ZoomLevel"/> is set.
        /// </summary>
        event EventHandler<ZoomLevelChangedEventArgs> ZoomLevelChanged;

        NSCursor Cursor { get; set; }
    }
}