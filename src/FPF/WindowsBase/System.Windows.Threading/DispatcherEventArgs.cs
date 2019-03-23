// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace System.Windows.Threading
{
    public class DispatcherEventArgs : EventArgs
    {
        public Dispatcher Dispatcher { get; }

        internal DispatcherEventArgs(Dispatcher dispatcher)
            => Dispatcher = dispatcher;
    }
}