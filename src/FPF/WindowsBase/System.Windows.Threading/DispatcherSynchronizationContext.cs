// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;

namespace System.Windows.Threading
{
    public sealed class DispatcherSynchronizationContext : SynchronizationContext
    {
        readonly Dispatcher dispatcher;
        readonly DispatcherPriority priority;

        public DispatcherSynchronizationContext(Dispatcher dispatcher, DispatcherPriority priority)
        {
            this.dispatcher = dispatcher
                ?? throw new ArgumentNullException(nameof(dispatcher));
            this.priority = priority;
        }

        public DispatcherSynchronizationContext(Dispatcher dispatcher) : this(dispatcher, DispatcherPriority.Normal)
        {
        }

        public DispatcherSynchronizationContext() : this(Dispatcher.CurrentDispatcher)
        {
        }

        public override SynchronizationContext CreateCopy()
            => new DispatcherSynchronizationContext(dispatcher, priority);

        public override void Post(SendOrPostCallback d, object state)
            => dispatcher.BeginInvoke(d, priority, state);

        public override void Send(SendOrPostCallback d, object state)
            => dispatcher.Invoke(() => d(state), priority);
    }
}