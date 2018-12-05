//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License. See License.txt in the project root for license information.
//
namespace Microsoft.VisualStudio.Text.Differencing
{
    using System;

    public interface IDifferenceViewer2 : IDifferenceViewer
    {
        /// <summary>
        /// Raised when the difference viewer is fully initialized.
        /// </summary>
        event EventHandler Initialized;
    }
}
