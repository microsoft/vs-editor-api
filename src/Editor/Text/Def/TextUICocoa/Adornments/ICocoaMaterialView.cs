//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License. See License.txt in the project root for license information.
//

using AppKit;

namespace Microsoft.VisualStudio.Text.Adornments
{
    /// <summary>
    /// A Cocoa container <see cref="NSView"/> with configurable material, shadow, and
    /// border radius suitable for hosting other views as adornments in the editor. The
    /// container uses auto-layout and constrains its leading, trailing, top, and bottom
    /// edges to the provided <see cref="ICocoaMaterialView.ContentView"/>.
    /// </summary>
    public interface ICocoaMaterialView
    {
        /// <summary>
        /// The subview to host in the material view. Must not be a subview of another <see cref="NSView"/>.
        /// </summary>
        NSView ContentView { get; set; }

        /// <summary>
        /// Optonal insets for the hosted <see cref="ContentView"/>.
        /// </summary>
        NSEdgeInsets EdgeInsets { get; set; }

        /// <summary>
        /// Optional corner radius.
        /// </summary>
        int CornerRadius { get; set; }

        /// <summary>
        /// Optional material. Defaults to <see cref="NSVisualEffectMaterial.Popover"/> on macOS 10.14
        /// (Mojave) or newer. This property is ignored for macOS 10.13 and older, and will always
        /// have an <see cref="NSVisualEffectMaterial.AppearanceBased"/> material.
        /// </summary>
        NSVisualEffectMaterial Material { get; set; }
    }
}