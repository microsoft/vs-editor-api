using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.Text.UI.Utilities
{
    [Export(typeof(IStatusBarService))]
    internal class StatusBarService : BaseProxyService<IStatusBarService>, IStatusBarService
    {
        [ImportImplementations(typeof(IStatusBarService))]
        protected override IEnumerable<Lazy<IStatusBarService, IOrderable>> UnorderedImplementations { get; set; }

        public Task SetTextAsync(string text)
        {
            return BestImplementation.SetTextAsync(text);
        }
    }
}
