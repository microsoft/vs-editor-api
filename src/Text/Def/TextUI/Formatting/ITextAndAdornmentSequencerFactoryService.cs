// Copyright (c) Microsoft Corporation
// All rights reserved

namespace Microsoft.VisualStudio.Text.Formatting
{
    using Microsoft.VisualStudio.Text.Editor;

    /// <summary>
    /// Service to create an instance of an <see cref="ITextAndAdornmentSequencer"/>.
    /// <remarks>This is a MEF component part, and should be imported as follows:
    /// [Import]
    /// ITextAndAdornmentSequencerFactoryService factory = null;
    /// </remarks>
    /// </summary>
    public interface ITextAndAdornmentSequencerFactoryService
    {
        /// <summary>
        /// Creates an <see cref="ITextAndAdornmentSequencer"/> for the specified <see cref="ITextView"/>.
        /// </summary>
        /// <param name="view">The <see cref="ITextView"/>.</param>
        /// <returns>The <see cref="ITextAndAdornmentSequencer"/>.</returns>
        ITextAndAdornmentSequencer Create(ITextView view);
    }
}
