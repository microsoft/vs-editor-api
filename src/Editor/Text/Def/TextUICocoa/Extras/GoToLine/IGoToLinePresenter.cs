//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License. See License.txt in the project root for license information.
//

namespace Microsoft.VisualStudio.Text.Extras.GoToLine
{
    /// <summary>
    /// Encapsulates the Go To Line UI in a similar fashion to
    /// <see cref="Find.IFindPresenter"/>
    /// </summary>
    public interface IGoToLinePresenter
    {
        void Show();
        void Hide();
        bool NavigateToLine(int lineNumber, int? columnNumber);
    }
}