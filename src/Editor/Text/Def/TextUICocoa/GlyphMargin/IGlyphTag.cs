//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License. See License.txt in the project root for license information.
//

using Microsoft.VisualStudio.Text.Tagging;

namespace Microsoft.VisualStudio.Text.Editor
{
    /// <summary>
    /// Represents a glyph tag, which is consumed by the glyph margin
    /// to place glyph visuals.
    /// </summary>
    public interface IGlyphTag : ITag
    {
    }
}