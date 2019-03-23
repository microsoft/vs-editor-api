// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace Microsoft.VisualStudio.Text.Editor
{
    public readonly struct InfoBarAction
    {
        public string Title { get; }
        public Action Handler { get; }
        public bool IsDefault { get; }

        public InfoBarAction(string title, Action handler, bool isDefault = false)
        {
            Title = title;
            Handler = handler;
            IsDefault = isDefault;
        }

        public void Invoke()
            => Handler?.Invoke();
    }

    public sealed class InfoBarViewModel
    {
        public string PrimaryLabelText { get; }

        public string SecondaryLabelText { get; }

        public IReadOnlyList<InfoBarAction> Actions { get; }

        public Action DismissedHandler { get; }

        public InfoBarViewModel(
            string primaryLabelText,
            string secondaryLabelText,
            IReadOnlyList<InfoBarAction> actions,
            Action dismissedHandler = null)
        {
            PrimaryLabelText = primaryLabelText;
            SecondaryLabelText = secondaryLabelText;
            Actions = actions;
            DismissedHandler = dismissedHandler;
        }

        public void InvokeDismissed()
            => DismissedHandler?.Invoke();
    }
}