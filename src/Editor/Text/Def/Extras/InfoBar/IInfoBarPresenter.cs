// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.VisualStudio.Text.Editor
{
    public interface IInfoBarPresenter
    {
        void Present(InfoBarViewModel viewModel);
        void Dismiss(InfoBarViewModel viewModel);
        void DismissAll();
    }
}