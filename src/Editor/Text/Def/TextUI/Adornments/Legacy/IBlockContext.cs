//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License. See License.txt in the project root for license information.
//
using System;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;

// TODO: DevDiv bug #369787: remove this in Dev16 when IBlockTag and related facilities are deprecated.
namespace Microsoft.VisualStudio.Text.Adornments
{
    /// <summary>
    /// Defines the block context used to display structural block information.
    /// </summary>
    [Obsolete("Use IStructureTag APIs instead")]
    public interface IBlockContext
    {
        /// <summary>
        /// Gets the content that will be displayed in the tool tip, including hierarchical parent content.
        /// </summary>
        object Content { get; }

        /// <summary>
        /// Gets the block tag associated with this context.
        /// </summary>
        IBlockTag BlockTag { get; }
        
        /// <summary>
        /// Gets the text view associated with this context.
        /// </summary>
        ITextView TextView { get; }
    }
}
