// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;

namespace System.Windows.Threading
{
    public sealed class Dispatcher
    {
        public Thread Thread { get; }

        internal DispatcherSynchronizationContext DefaultSynchronizationContext { get; }

        public event DispatcherUnhandledExceptionEventHandler UnhandledException;

        internal void OnUnhandledException(Exception exception)
        {
            var unhandledException = UnhandledException;
            if (unhandledException != null)
            {
                var args = new DispatcherUnhandledExceptionEventArgs(this, exception);

                unhandledException(this, args);

                if (args.Handled)
                    return;
            }

            throw new Exception(
                $"Unhandled exception in dispatcher thread {Thread.ManagedThreadId} '{Thread.Name}'",
                exception);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool CheckAccess()
            => Thread.CurrentThread == Thread;

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void VerifyAccess()
        {
            if (Thread.CurrentThread != Thread)
                throw new InvalidOperationException("Invoked from a different thread");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void ValidateArguments(Delegate method, DispatcherPriority priority)
        {
            if (method == null)
                throw new ArgumentNullException(nameof(method));

            if (priority < 0 || priority > DispatcherPriority.Send)
                throw new InvalidEnumArgumentException(nameof(priority));

            if (priority == DispatcherPriority.Inactive)
                throw new ArgumentException("priority must not be inactive", nameof(priority));
        }

        public DispatcherOperation BeginInvoke (DispatcherPriority priority, Delegate method, object arg)
            => BeginInvoke (method, priority, arg);

        public DispatcherOperation BeginInvoke(Delegate method, params object[] args)
            => BeginInvoke(method, DispatcherPriority.Normal, args);

        public DispatcherOperation BeginInvoke(Delegate method, DispatcherPriority priority, params object[] args)
        {
            ValidateArguments(method, priority);
            return new DispatcherOperation(
                this,
                method,
                priority,
                args,
                CancellationToken.None,
                null).BeginInvoke();
        }

        public DispatcherOperation InvokeAsync(Action callback)
            => InvokeAsync(callback, DispatcherPriority.Normal, null, default);

        public DispatcherOperation InvokeAsync(Action callback, DispatcherPriority priority)
            => InvokeAsync(callback, priority, null, default);

        public DispatcherOperation InvokeAsync(Action callback, DispatcherPriority priority, CancellationToken cancellationToken)
            => InvokeAsync(callback, priority, null, cancellationToken);

        DispatcherOperation InvokeAsync(Delegate method, DispatcherPriority priority, object[] args, CancellationToken cancellationToken)
        {
            ValidateArguments(method, priority); 
            return new DispatcherOperation(
                this,
                method,
                priority,
                args,
                cancellationToken,
                new DispatcherOperationTaskSource()).InvokeAsync();
        }

        public void Invoke(Action callback)
            => Invoke(callback, DispatcherPriority.Send, CancellationToken.None, Timeout.InfiniteTimeSpan);

        public void Invoke(Action callback, DispatcherPriority priority)
            => Invoke(callback, priority, CancellationToken.None, Timeout.InfiniteTimeSpan);

        void Invoke(Action callback, DispatcherPriority priority, CancellationToken cancellationToken)
            => Invoke(callback, priority, cancellationToken, Timeout.InfiniteTimeSpan);

        void Invoke(Action callback, DispatcherPriority priority, CancellationToken cancellationToken, TimeSpan timeout)
        {
            ValidateArguments(callback, priority);
            new DispatcherOperation(
                this,
                callback,
                priority,
                null,
                cancellationToken,
                null).Invoke(timeout);
        }

        #region Adapted directly from WPF Dispatcher

        Dispatcher()
        {
            tlsDispatcher = this; // use TLS for ownership only
            Thread = Thread.CurrentThread;

            lock (globalLock)
                dispatchers.Add(new WeakReference(this));

            DefaultSynchronizationContext = new DispatcherSynchronizationContext(this);
        }

        [ThreadStatic]
        static Dispatcher tlsDispatcher; // use TLS for ownership only

        static readonly object globalLock = new object();
        static readonly List<WeakReference> dispatchers = new List<WeakReference>();
        static WeakReference possibleDispatcher = new WeakReference(null);

        public static Dispatcher CurrentDispatcher
            => FromThread(Thread.CurrentThread) ?? new Dispatcher();

        public static Dispatcher FromThread(Thread thread)
        {
            if (thread == null)
                return null;

            lock (globalLock)
            {
                Dispatcher dispatcher = null;

                dispatcher = possibleDispatcher.Target as Dispatcher;
                if (dispatcher == null || dispatcher.Thread != thread)
                {
                    dispatcher = null;

                    for (int i = 0; i < dispatchers.Count; i++)
                    {
                        if (dispatchers[i].Target is Dispatcher dispatcherForThread)
                        {
                            if (dispatcherForThread.Thread == thread)
                                dispatcher = dispatcherForThread;
                        }
                        else
                        {
                            dispatchers.RemoveAt(i);
                            i--;
                        }
                    }

                    if (dispatcher != null)
                    {
                        if (possibleDispatcher.IsAlive)
                            possibleDispatcher.Target = dispatcher;
                        else
                            possibleDispatcher = new WeakReference(dispatcher);
                    }
                }

                return dispatcher;
            }
        }

        #endregion
    }
}