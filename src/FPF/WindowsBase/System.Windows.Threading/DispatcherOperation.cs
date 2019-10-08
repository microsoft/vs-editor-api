// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using CoreFoundation;
using Foundation;

namespace System.Windows.Threading
{
    public enum DispatcherOperationStatus
    {
        Pending,
        Aborted,
        Completed,
        Executing
    }

    public delegate object DispatcherOperationCallback(object arg);

    public sealed class DispatcherOperation
    {
        readonly Delegate method;
        readonly object[] args;
        readonly CancellationToken cancellationToken;
        readonly DispatcherOperationTaskSource taskSource;

        public Dispatcher Dispatcher { get; }
        public DispatcherPriority Priority { get; }

        Exception exception;
        public object Result { get; private set; }
        public DispatcherOperationStatus Status { get; private set; }

        public event EventHandler Aborted;
        public event EventHandler Completed;

        internal DispatcherOperation(
            Dispatcher dispatcher,
            Delegate method,
            DispatcherPriority priority,
            object[] args,
            CancellationToken cancellationToken,
            DispatcherOperationTaskSource taskSource)
        {
            Dispatcher = dispatcher;
            this.method = method;
            Priority = priority;
            this.args = args;
            this.cancellationToken = cancellationToken;
            this.taskSource = taskSource;
            Status = DispatcherOperationStatus.Pending;
        }

        internal DispatcherOperation BeginInvoke()
        {
            NSRunLoop.Main.BeginInvokeOnMainThread(() =>
            {
                CoreInvoke(beginInvokeBehavior: true);
                if (exception != null)
                    Dispatcher.OnUnhandledException(exception);
            });

            return this;
        }

        internal DispatcherOperation InvokeAsync()
        {
            if (taskSource == null)
                throw new InvalidOperationException();

            NSRunLoop.Main.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    CoreInvoke(beginInvokeBehavior: false);
                }
                catch (OperationCanceledException)
                {
                    taskSource.SetCanceled();
                    return;
                }

                if (cancellationToken.IsCancellationRequested)
                    taskSource.SetCanceled();
                else if (exception != null)
                    taskSource.SetException(exception);
                else
                    taskSource.SetResult();
            });

            return this;
        }

        internal void Invoke(TimeSpan timeout)
        {
            if (timeout != Timeout.InfiniteTimeSpan)
                throw new NotSupportedException("timeouts are not supported");

            var mainQueue = DispatchQueue.MainQueue;

            if (DispatchQueue.CurrentQueue != mainQueue)
                NSRunLoop.Main.InvokeOnMainThread(() => CoreInvoke(beginInvokeBehavior: false));
            else
                CoreInvoke(beginInvokeBehavior: false);

            if (exception != null)
                throw exception;
        }

        void CoreInvoke(bool beginInvokeBehavior)
        {
            Status = DispatcherOperationStatus.Executing;

            var oldSynchronizationContext = SynchronizationContext.Current;

            try
            {
                SynchronizationContext.SetSynchronizationContext(Dispatcher.DefaultSynchronizationContext);

                try
                {
                    if (method is Action action)
                        action();
                    else
                        Result = method.DynamicInvoke(args);
                }
                catch (Exception e)
                {
                    exception = e;
                }

                if (beginInvokeBehavior)
                {
                    if (exception is OperationCanceledException)
                    {
                        Status = DispatcherOperationStatus.Aborted;
                        exception = null;
                        Aborted?.Invoke(this, EventArgs.Empty);
                    }
                    else
                    {
                        Status = DispatcherOperationStatus.Completed;
                        Completed?.Invoke(this, EventArgs.Empty);
                    }
                }
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(oldSynchronizationContext);
            }
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public TaskAwaiter GetAwaiter()
        {
            if (taskSource == null)
                throw new InvalidOperationException();

            return taskSource.GetTask().GetAwaiter();
        }

        public Task Task
        {
            get
            {
                if (taskSource == null)
                    throw new InvalidOperationException();

                return taskSource.GetTask();
            }
        }
    }
}