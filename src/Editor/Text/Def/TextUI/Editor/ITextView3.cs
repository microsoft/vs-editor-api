//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License. See License.txt in the project root for license information.
//

using System;

using Microsoft.VisualStudio.Text.Formatting;

namespace Microsoft.VisualStudio.Text.Editor
{
    public interface ITextView3 : ITextView2
    {
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