//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License. See License.txt in the project root for license information.
//
namespace Microsoft.VisualStudio.Text.Adornments
{
    using System;
    using Microsoft.VisualStudio.Text.Editor;

    /// <summary>
    /// Gets an existing tooltip adornment provider from the cached list, or creates one if there is not one
    /// in the cache.
    /// </summary>
    /// <remarks>This is a MEF component part, and should be exported with the following attribute:
    /// [Export(typeof(IToolTipProviderFactory))]
    /// </remarks>
    [Obsolete("Use " + nameof(IToolTipService) + " instead")]
    public interface IToolTipProviderFactory
    {
        /// <summary>
        /// Gets the cached <see cref="IToolTipProvider"/> for a given <see cref="ITextView"/>. 
        /// If one does not exist, creates and caches a new <see cref="IToolTipProvider"/> with the <see cref="ITextView"/>.
        /// </summary>
        /// <param name="textView">The <see cref="ITextView"/> with which to get the <see cref="IToolTipProvider"/>.</param>
        /// <returns>The cached <see cref="IToolTipProvider"/> for <paramref name="textView"/>.</returns>
        IToolTipProvider GetToolTipProvider(ITextView textView);
    }
}
