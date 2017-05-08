using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using System;

namespace Microsoft.VisualStudio.Text.Adornments
{
    /// <summary>
    /// An implementation of <see cref="IBlockContext"/>.
    /// </summary>
    public class BlockContext : IBlockContext
    {
        private readonly IBlockTag _blockTag;
        private readonly ITextView _view;
        private readonly object _content;
        /// <summary>
        /// Initializes a new instance of <see cref="IBlockContext"/> with the specified block tag.
        /// </summary>
        /// <param name="blockTag">The block tag associated with the structural block.</param>
        /// <param name="view">The text view associated with the structural block.</param>
        /// <param name="content">The content, including hiearchical parent statements, to be displayed in the tooltip.</param>
        public BlockContext(IBlockTag blockTag, ITextView view, object content)
        {
            if (blockTag == null)
                throw new ArgumentNullException("blockTag");

            if (view == null)
                throw new ArgumentNullException("view");

            if (content == null)
                throw new ArgumentNullException("content");

            _blockTag = blockTag;
            _view = view;
            _content = content;
        }

        /// <summary>
        /// Gets the block tag associated with this context.
        /// </summary>
        public IBlockTag BlockTag
        {
            get { return _blockTag; }
        }

        /// <summary>
        /// Gets the text view associated with this context.
        /// </summary>
        public ITextView TextView
        {
            get { return _view; }
        }

        /// <summary>
        /// Gets the content that will be displayed in the tool tip, including hierarchical parent content.
        /// </summary>
        public object Content
        {
            get { return _content; }
        }
    }
}
