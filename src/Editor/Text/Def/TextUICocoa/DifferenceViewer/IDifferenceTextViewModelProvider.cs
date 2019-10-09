//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License. See License.txt in the project root for license information.
//

using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.VisualStudio.Text.Differencing
{
    /// <summary>
    /// Provides <see cref="ITextViewModel"/> objects.
    /// </summary>
    /// <remarks>This is a MEF component part, and should be exported with the following attribute:
    /// [Export(NameSource=typeof(ITextViewModelProvider))]
    /// Component exporters must specify at least one ContentTypeAttribute characterizing the data
    /// models to which they apply.
    /// </remarks>
    public interface IDifferenceTextViewModelProvider
    {
        /// <summary>
        /// Creates an <see cref="ITextViewModel"/> for the given <see cref="ITextDataModel"/>.
        /// </summary>
        /// <param name="viewer">The <see cref="ICocoaDifferenceViewer"/> in which the views are being created.</param>
        /// <param name="viewType">The <see cref="DifferenceViewType"/> of the view being created.</param>
        /// <param name="dataModel">The <see cref="ITextDataModel"/> for which to create the <see cref="ITextViewModel"/>.</param>
        /// <returns>The <see cref="ITextViewModel"/> created for <paramref name="dataModel"/>, 
        /// or <c>null</c> if the text view model cannot be created.</returns>
        IDifferenceTextViewModel CreateTextViewModel(ICocoaDifferenceViewer viewer, DifferenceViewType viewType, ITextDataModel dataModel);
    }
}
