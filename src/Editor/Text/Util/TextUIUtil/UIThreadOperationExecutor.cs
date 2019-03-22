using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.Text.Utilities
{
    [Export(typeof(IUIThreadOperationExecutor))]
    internal class UIThreadOperationExecutor : BaseProxyService<IUIThreadOperationExecutor>, IUIThreadOperationExecutor
    {
        [ImportImplementations(typeof(IUIThreadOperationExecutor))]
        protected override IEnumerable<Lazy<IUIThreadOperationExecutor, IOrderable>> UnorderedImplementations { get; set; }

        public IUIThreadOperationContext BeginExecute(string title, string defaultDescription, bool allowCancellation, bool showProgress)
        {
            return BestImplementation.BeginExecute(title, defaultDescription, allowCancellation, showProgress);
        }

        public IUIThreadOperationContext BeginExecute(UIThreadOperationExecutionOptions executionOptions)
        {
            return BestImplementation.BeginExecute(executionOptions);
        }

        public UIThreadOperationStatus Execute(string title, string defaultDescription, bool allowCancellation, bool showProgress, Action<IUIThreadOperationContext> action)
        {
            return BestImplementation.Execute(title, defaultDescription, allowCancellation, showProgress, action);
        }

        public UIThreadOperationStatus Execute(UIThreadOperationExecutionOptions executionOptions, Action<IUIThreadOperationContext> action)
        {
            return BestImplementation.Execute(executionOptions, action);
        }
    }
}