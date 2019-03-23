using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace Microsoft.VisualStudio.Utilities
{
    /// <summary>
    /// A proxy service for exposing best implementation to the MEF composition.
    /// </summary>
    internal abstract class BaseProxyService<T> where T : class
    {
        protected abstract IEnumerable<Lazy<T, IOrderable>> UnorderedImplementations { get; set; }

        private T bestImpl;

        protected virtual T BestImplementation
        {
            get
            {
                if (this.bestImpl == null)
                {
                    var orderedImpls = Orderer.Order(UnorderedImplementations);
                    if (orderedImpls.Count == 0)
                    {
                        throw new ImportCardinalityMismatchException($"Expected to import at least one export of {typeof(T).FullName}, but got none.");
                    }

                    this.bestImpl = orderedImpls[0].Value;
                }

                return this.bestImpl;
            }
        }
    }
}
