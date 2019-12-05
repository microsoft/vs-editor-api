// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.ComponentModel;

namespace System.Windows.Threading
{
    public abstract class DispatcherObject
    {
        [EditorBrowsable(EditorBrowsableState.Never)]
        public Dispatcher Dispatcher { get; }

        protected DispatcherObject ()
            => Dispatcher = Dispatcher.CurrentDispatcher;

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool CheckAccess()
            => Dispatcher.CheckAccess();

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void VerifyAccess()
            => Dispatcher.VerifyAccess();
    }
}