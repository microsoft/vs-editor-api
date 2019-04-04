//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License. See License.txt in the project root for license information.
//

using AppKit;

namespace Microsoft.VisualStudio.Text.Adornments
{
    /// <summary>
    /// Creates Cocoa container views suitable for adornments in the editor.
    /// </summary>
    public interface ICocoaViewFactory
    {
        /// <summary>
        /// Creates an <see cref="NSView"/> container view as an <see cref="ICocoaMaterialView"/>.
        /// </summary>
        ICocoaMaterialView CreateMaterialView();
    }
}