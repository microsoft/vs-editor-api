using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.VisualStudio.Text.Adornments
{
    /// <summary>
    /// Creates a <see cref="IBlockContextSource"/> for a given buffer.
    /// </summary>
    /// <remarks>This is a MEF component part, and should be exported as follows:
    /// [Export(typeof(IBlockContextProvider))]
    /// Component exporters must add the Name and Order attribute to define the order of the provider in the provider chain.
    /// </remarks>
    public interface IBlockContextProvider
    {
        /// <summary>
        /// Creates a block context source for the given text buffer.
        /// </summary>
        /// <param name="textBuffer">The text buffer for which to create a provider.</param>
        /// <param name="token">The cancelation token for this asynchronous method call.</param>
        /// <returns>A valid <see cref="IBlockContextSource" /> instance, or null if none could be created.</returns>
        Task<IBlockContextSource> TryCreateBlockContextSourceAsync(ITextBuffer textBuffer, CancellationToken token);
    }
}
