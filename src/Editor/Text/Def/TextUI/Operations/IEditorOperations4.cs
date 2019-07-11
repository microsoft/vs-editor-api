//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License. See License.txt in the project root for license information.
//
namespace Microsoft.VisualStudio.Text.Operations
{
    /// <summary>
    /// Defines operations relating to the editor, in addition to operations defined by <see cref="IEditorOperations3"/>.
    /// </summary>
    public interface IEditorOperations4 : IEditorOperations3
    {
        /// <summary>
        /// Determines whether zooming operations are possible.
        /// </summary>
        bool CanZoomTo { get; }

        /// <summary>
        /// Determines whether a zoom-in operation is possible.
        /// </summary>
        bool CanZoomIn { get; }

        /// <summary>
        /// Determines whether a zoom-out operation is possible.
        /// </summary>
        bool CanZoomOut { get; }

        /// <summary>
        /// Determines whether resetting zoom to 100% operation is possible.
        /// </summary>
        bool CanZoomReset { get; }

        /// <summary>
        /// Resets the text view zoom level to 100%.
        /// </summary>
        void ZoomReset();

        /// <summary>
        /// Sorts the selected lines in alphabetical order.
        /// </summary>
        void SortSelectedLines();

        /// <summary>
        /// Joins the selected lines into a single one.
        /// </summary>
        void JoinSelectedLines();
    }
}