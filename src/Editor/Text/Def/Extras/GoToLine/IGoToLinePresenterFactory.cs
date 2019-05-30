//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License. See License.txt in the project root for license information.
//

using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.VisualStudio.Text.Extras.GoToLine
{
    public interface IGoToLinePresenterFactory
    {
        bool TryGetGoToLinePresenter(ITextView textView, out IGoToLinePresenter goToLinePresenter);
    }
}