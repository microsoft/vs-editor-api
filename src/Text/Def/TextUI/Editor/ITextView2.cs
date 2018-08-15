//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License. See License.txt in the project root for license information.
//

using System;

namespace Microsoft.VisualStudio.Text.Editor
{
    /// <summary>
    /// Extensions to <see cref="ITextView"/>, augmenting functionality. For every member here
    /// there should also be an extension method in <see cref="TextViewExtensions"/>.
    /// </summary>
    public interface ITextView2 : ITextView
    {
        /// <summary>
        /// Determines whether the view is in the process of being laid out or is preparing to be laid out.
        /// </summary>
        /// <remarks>
        /// As opposed to <see cref="ITextView.InLayout"/>, it is safe to get the <see cref="ITextView.TextViewLines"/>
        /// but attempting to queue another layout will cause a reentrant layout exception.
        /// </remarks>
        bool InOuterLayout
        {
            get;
        }

        /// <summary>
        /// Gets an object for managing selections within the view.
        /// </summary>
        IMultiSelectionBroker MultiSelectionBroker
        {
            get;
        }


        /// <summary>
        /// Raised whenever the view's MaxTextRightCoordinate is changed.
        /// </summary>
        /// <remarks>
        /// This event will only be rasied if the MaxTextRightCoordinate is changed by changing the MinMaxTextRightCoordinate property
        /// (it will not be raised as a side-effect of a layout even if the layout does change the MaxTextRightCoordinate).
        /// </remarks>
        event EventHandler MaxTextRightCoordinateChanged;
    }
}
