// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.VisualStudio.Text.Editor
{
    public interface IInfoBarPresenterFactory
    {
        IInfoBarPresenter TryGetInfoBarPresenter(ITextView textView);
    }
}