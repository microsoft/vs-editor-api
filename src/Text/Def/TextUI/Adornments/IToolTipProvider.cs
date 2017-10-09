//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License. See License.txt in the project root for license information.
//
namespace Microsoft.VisualStudio.Text.Adornments
{
    using Microsoft.VisualStudio.Text;

    /// <summary>
    /// Creates and displays tooltips, using an arbitrary object as content.
    /// </summary>
    public interface IToolTipProvider
    {
        /// <summary>
        /// Creates and displays a tooltip. 
        /// </summary>
        /// <param name="span">
        /// The range of text for which the tooltip is relevant.
        /// </param>
        /// <param name="toolTipContent">
        /// The content to be displayed in the tooltip. This must be a string or UIElement for the WPF tooltip adornment surface. 
        /// </param>
        /// <remarks>This is equivalent to ShowToolTip(..., PopupStyles.None).</remarks>
        void ShowToolTip(ITrackingSpan span, object toolTipContent);

        /// <summary>
        /// Creates and displays a tooltip. 
        /// </summary>
        /// <param name="span">
        /// The range of text for which the tooltip is relevant.
        /// </param>
        /// <param name="toolTipContent">
        /// The content to be displayed in the tooltip. This must be a string or UIElement for the WPF tooltip adornment surface.
        /// </param>
        /// <param name="style">
        /// <see cref="PopupStyles"/> for the tooltip.
        /// </param>
        void ShowToolTip(ITrackingSpan span, object toolTipContent, PopupStyles style);

        /// <summary>
        /// Removes the tooltip currently being displayed, if any.
        /// </summary>
        void ClearToolTip();
    }
}