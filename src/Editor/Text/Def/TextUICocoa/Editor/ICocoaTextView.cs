//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License. See License.txt in the project root for license information.
//

using System;

using AppKit;
using CoreGraphics;
using Microsoft.VisualStudio.Text.Formatting;

namespace Microsoft.VisualStudio.Text.Editor
{
    public interface ICocoaTextView : ITextView2
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

        void PushCursor(object context, NSCursor cursor);

        void PopCursor(object context);


        /// <summary>
        /// Gets or sets the Zoom level for the <see cref="ITextView3"/> between 20% to 400%
        /// </summary>
        double ZoomLevel { get; set; }

        IXPlatAdornmentLayer GetXPlatAdornmentLayer(string name);

        ITextViewLineSource FormattedLineSource { get; }

        void Focus();

        bool IsKeyboardFocused { get; }
        event EventHandler IsKeyboardFocusedChanged;

        IViewSynchronizationManager SynchronizationManager { get; set; }
    }
}