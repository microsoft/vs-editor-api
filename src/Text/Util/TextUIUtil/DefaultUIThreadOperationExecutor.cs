using System;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.UI.Text.Commanding.Implementation
{
    [ExportImplementation(typeof(IUIThreadOperationExecutor))]
    [Name("default")]
    internal class DefaultUIThreadOperationExecutor : IUIThreadOperationExecutor
    {
        public IUIThreadOperationContext BeginExecute(string title, string description, bool allowCancel, bool showProgress)
        {
            return new DefaultUIThreadOperationContext(allowCancel, description);
        }

        public UIThreadOperationStatus Execute(string title, string description, bool allowCancel, bool showProgress, Action<IUIThreadOperationContext> action)
        {
            var context = new DefaultUIThreadOperationContext(allowCancel, description);
            action(context);
            return UIThreadOperationStatus.Completed;
        }
    }

    internal class DefaultUIThreadOperationContext : AbstractUIThreadOperationContext
    {
        public DefaultUIThreadOperationContext(bool allowCancellation, string description)
            : base(allowCancellation, description)
        {
        }
    }
}
