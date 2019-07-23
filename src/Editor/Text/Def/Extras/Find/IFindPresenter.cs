//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License. See License.txt in the project root for license information.
//

namespace Microsoft.VisualStudio.Text.Find
{
    /// <summary>
    /// Encapsulates the Find/Replace UI that can be provided by an extension.
    /// A presenter is per view, is associated with its underlying FindController
    /// and is instantiated by the IFindPresenterFactory.
    /// </summary>
    public interface IFindPresenter
    {
        void ShowFind(bool usePreviousTerm = false, bool takeFocus = true);
        void ShowReplace();
        void Hide();
        bool IsVisible { get; }
        bool IsFocused { get; }
    }
}