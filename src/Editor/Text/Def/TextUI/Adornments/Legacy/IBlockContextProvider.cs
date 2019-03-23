//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License. See License.txt in the project root for license information.
//
using System;
using System.Threading;
using System.Threading.Tasks;

// TODO: DevDiv bug #369787: remove this in Dev16 when IBlockTag and related facilities are deprecated.
namespace Microsoft.VisualStudio.Text.Adornments
{
    /// <summary>
    /// Creates a <see cref="IBlockContextSource"/> for a given buffer.
    /// </summary>
    /// <remarks>This is a MEF component part, and should be exported as follows:
    /// [Export(typeof(IBlockContextProvider))]
    /// Component exporters must add the Name and Order attribute to define the order of the provider in the provider chain.
    /// </remarks>
    [Obsolete("Use IStructureContextProvider instead")]
    public interface IBlockContextProvider
    {
#pragma warning disable 618
        /// <summary>
        /// Creates a block context source for the given text buffer.
        /// </summary>
        /// <param name="textBuffer">The text buffer for which to create a provider.</param>
        /// <param name="token">The cancelation token for this asynchronous method call.</param>
        /// <returns>A valid <see cref="IBlockContextSource" /> instance, or null if none could be created.</returns>
        Task<IBlockContextSource> TryCreateBlockContextSourceAsync(ITextBuffer textBuffer, CancellationToken token);
#pragma warning restore 618
    }
}
