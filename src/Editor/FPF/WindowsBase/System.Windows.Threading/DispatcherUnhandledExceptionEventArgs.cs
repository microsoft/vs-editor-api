namespace System.Windows.Threading
{
    public delegate void DispatcherUnhandledExceptionEventHandler(
        object sender,
        DispatcherUnhandledExceptionEventArgs e);

    public sealed class DispatcherUnhandledExceptionEventArgs : DispatcherEventArgs
    {
        public Exception Exception { get; }
        public bool Handled { get; set; }

        internal DispatcherUnhandledExceptionEventArgs(Dispatcher dispatcher, Exception exception)
            : base(dispatcher)
            => Exception = exception;
    }
}