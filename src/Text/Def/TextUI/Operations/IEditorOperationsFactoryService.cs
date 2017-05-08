// Copyright (c) Microsoft Corporation
// All rights reserved

namespace Microsoft.VisualStudio.Text.Operations
{
    using Microsoft.VisualStudio.Text.Editor;
    using System.ComponentModel.Composition;

    /// <summary>
    /// A service that provides <see cref="IEditorOperations"/> objects.
    /// </summary>
    /// <remarks>This is a MEF component part, and should be imported as follows:
    /// [Import]
    /// IEditorOperationsFactoryService factory = null;
    /// </remarks>
    public interface IEditorOperationsFactoryService
    {
        /// <summary>
        /// Gets the <see cref="IEditorOperations"/> objects for the specified <see cref="ITextView"/>.
        /// </summary>
        /// <param name="textView">The <see cref="ITextView"/>.</param>
        /// <returns>The <see cref="IEditorOperations"/>. </returns>
        IEditorOperations GetEditorOperations(ITextView textView);
    }
}
