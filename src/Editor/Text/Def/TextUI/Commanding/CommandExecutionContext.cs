using System;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.Commanding
{
    /// <summary>
    /// Represents a command execution context, which is set up by a command handler service
    /// and provided to each command handler.
    /// </summary>
    public sealed class CommandExecutionContext
    {
        /// <summary>
        /// Creates new instance of the <see cref="CommandExecutionContext"/>.
        /// </summary>
        public CommandExecutionContext(IUIThreadOperationContext operationContext)
        {
            this.OperationContext = operationContext ?? throw new ArgumentNullException(nameof(operationContext));
        }

        /// <summary>
        /// Provides a context of executing a command handler on the UI thread, which
        /// enables two way shared cancellability and wait indication.
        /// </summary>
        public IUIThreadOperationContext OperationContext { get; }
    }
}

