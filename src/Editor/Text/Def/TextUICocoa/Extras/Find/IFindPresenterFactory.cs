//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License. See License.txt in the project root for license information.
//

using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.VisualStudio.Text.Find
{
    /// <summary>
    /// Instantiates a Find/Replace UI presenter for a given text view.
    /// The factory is a singleton that is provided via a MEF [Export]
    /// and can be consumed via a MEF [Import].
    /// </summary>
    public interface IFindPresenterFactory
    {
        IFindPresenter TryGetFindPresenter(ITextView textView);
    }
}