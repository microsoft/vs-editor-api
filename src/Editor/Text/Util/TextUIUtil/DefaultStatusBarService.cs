using System.Threading.Tasks;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.Text.Utilities
{
    [ExportImplementation(typeof(IStatusBarService))]
    [Name("default")]
    internal class DefaultStatusBarService : IStatusBarService
    {
        public Task SetTextAsync(string text)
        {
            return Task.CompletedTask;
        }
    }
}
