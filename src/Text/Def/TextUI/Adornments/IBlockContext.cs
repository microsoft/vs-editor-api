using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;

namespace Microsoft.VisualStudio.Text.Adornments
{
    /// <summary>
    /// Defines the block context used to display structural block information.
    /// </summary>
    public interface IBlockContext
    {
        /// <summary>
        /// Gets the content that will be displayed in the tool tip, including hierarchical parent content.
        /// </summary>
        object Content { get; }

        /// <summary>
        /// Gets the block tag associated with this context.
        /// </summary>
        IBlockTag BlockTag { get; }
        
        /// <summary>
        /// Gets the text view associated with this context.
        /// </summary>
        ITextView TextView { get; }
    }
}
