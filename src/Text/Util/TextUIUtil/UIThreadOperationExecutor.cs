using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.UI.Text.Commanding.Implementation
{
    [Export(typeof(IUIThreadOperationExecutor))]
    internal class UIThreadOperationExecutor : IUIThreadOperationExecutor
    {
        [ImportImplementations(typeof(IUIThreadOperationExecutor))]
        private IEnumerable<Lazy<IUIThreadOperationExecutor, IOrderable>> _unorderedImplementations;

        private IUIThreadOperationExecutor _bestImpl;

        private IUIThreadOperationExecutor BestImplementation
        {
            get
            {
                if (_bestImpl == null)
                {
                    var orderedImpls = Orderer.Order(_unorderedImplementations);
                    if (orderedImpls.Count == 0)
                    {
                        throw new ImportCardinalityMismatchException($"Expected to import at least one export of {typeof(IUIThreadOperationExecutor).FullName}, but got none.");
                    }

                    _bestImpl = orderedImpls[0].Value;
                }

                return _bestImpl;
            }
        }

        public IUIThreadOperationContext BeginExecute(string title, string description, bool allowCancel, bool showProgress)
        {
            return BestImplementation.BeginExecute(title, description, allowCancel, showProgress);
        }

        public UIThreadOperationStatus Execute(string title, string description, bool allowCancel, bool showProgress, Action<IUIThreadOperationContext> action)
        {
            return BestImplementation.Execute(title, description, allowCancel, showProgress, action);
        }
    }
}
