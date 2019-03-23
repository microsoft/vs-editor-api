//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License. See License.txt in the project root for license information.
//
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using System;
using System.Threading;
using System.Threading.Tasks;

// TODO: DevDiv bug #369787: remove this in Dev16 when IBlockTag and related facilities are deprecated.
namespace Microsoft.VisualStudio.Text.Adornments
{
    /// <summary>
    /// Provides content for structural block tool tips for a given <see cref="IBlockTag"/>.
    /// </summary>
    [Obsolete("Use IStructureContextSource instead")]
    public interface IBlockContextSource : IDisposable
    {
        /// <summary>
        /// Gets the contexts for the given block tag.
        /// </summary>
        /// <param name="blockTag">The block tag for which the context is requested.</param>
        /// <param name="view">The text view associated with the current context.</param>
        /// <param name="token">The cancellation token for this asynchronous method call.</param>
        /// <returns>The <see cref="IBlockContext" /> to be displayed in the tool tip.</returns>
        Task<IBlockContext> GetBlockContextAsync(IBlockTag blockTag, ITextView view, CancellationToken token);
    }
}
