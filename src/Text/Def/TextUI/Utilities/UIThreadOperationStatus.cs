namespace Microsoft.VisualStudio.Utilities
{
    /// <summary>
    /// Represents a status of executing a potentially long running operation on the UI thread.
    /// </summary>
    public enum UIThreadOperationStatus
    {
        /// <summary>
        /// An operation was successfully completed.
        /// </summary>
        Completed,

        /// <summary>
        /// An operation was cancelled.
        /// </summary>
        Canceled,
    }
}
