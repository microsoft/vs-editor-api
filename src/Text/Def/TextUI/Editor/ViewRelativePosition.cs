// Copyright (c) Microsoft Corporation
// All rights reserved

namespace Microsoft.VisualStudio.Text.Editor
{
    /// <summary>
    /// Defines the meaning of the verticalOffset parameter in the <see cref="ITextView"/>.DisplayTextLineContaining(...).
    /// </summary>
    public enum ViewRelativePosition
    {
        /// <summary>
        /// The offset with respect to the top of the view.
        /// </summary>
        Top,
        /// <summary>
        /// The offset with respect to the bottom of the view.
        /// </summary>
        Bottom      
    }
}