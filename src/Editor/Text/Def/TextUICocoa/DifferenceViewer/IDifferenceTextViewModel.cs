//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License. See License.txt in the project root for license information.
//
namespace Microsoft.VisualStudio.Text.Differencing
{
    using Microsoft.VisualStudio.Text.Editor;

    /// <summary>
    /// Represents a set of zero or more <see cref="ITextBuffer"/> objects that are unique to the presentation of text
    /// in a particular <see cref="ITextView"/>.
    /// </summary>
    public interface IDifferenceTextViewModel : ITextViewModel
    {
        /// <summary>
        /// A pointer to the difference viewer that created the view that uses this IDifferenceTextViewModel.
        /// </summary>
        ICocoaDifferenceViewer Viewer { get; }

        /// <summary>
        /// The type of the view that uses this IDifferenceTextViewModel.
        /// </summary>
        DifferenceViewType ViewType { get; }
    }
}
