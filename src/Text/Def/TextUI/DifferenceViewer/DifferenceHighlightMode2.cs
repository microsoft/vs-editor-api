// Copyright (c) Microsoft Corporation
// All rights reserved

    namespace Microsoft.VisualStudio.Text.Differencing
{
    /// <summary>
    /// The highlight mode for this <see cref="IDifferenceViewer"/>.
    /// </summary>
    public enum DifferenceHighlightMode2
    {
        /// <summary>
        /// In this mode, line differences should be displayed only to the last character on each line.
        /// </summary>
        CodeContour = DifferenceHighlightMode.CodeContour,

        /// <summary>
        /// In this mode, line differences should be displayed so that they take up the entire width of the viewport.
        /// </summary>
        WholeLine = DifferenceHighlightMode.WholeLine,

        /// <summary>
        /// In this mode, line and word differences are shown as outlined rectangles.
        /// </summary>
        BlockOutline
    }
}
