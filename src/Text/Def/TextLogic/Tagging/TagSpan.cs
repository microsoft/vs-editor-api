using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.VisualStudio.Text.Tagging
{
    /// <summary>
    /// Associates an <see cref="ITag" /> with a given <see cref="SnapshotSpan" />.
    /// </summary>
    /// <typeparam name="T">The type, which must be a subclass of <see cref="ITag"/>.</typeparam>
    /// <remarks>
    /// Use <see cref="TagSpan&lt;T&gt;" /> as the implementation of this
    /// interface.
    /// </remarks>
    public interface ITagSpan<out T> where T : ITag
    {
        /// <summary>
        /// Gets the tag located in this span.
        /// </summary>
        T Tag { get; }

        /// <summary>
        /// Gets the snapshot span for this tag.
        /// </summary>
        SnapshotSpan Span { get; }
    }

    /// <summary>
    /// The implementation of ITagSpan&lt;T&gt;.
    /// </summary>
    public class TagSpan<T> : ITagSpan<T> where T : ITag
    {
        #region ITagSpan<T> members

        /// <summary>
        /// Gets the tag located in this span.
        /// </summary>
        public T Tag { get; private set; }

        /// <summary>
        /// Gets the snapshot span for this tag.
        /// </summary>
        public SnapshotSpan Span { get; private set; }

        #endregion

        /// <summary>
        /// Initializes a new instance of a <see cref="TagSpan&lt;T&gt;"/> with the specified snapshot span and tag.
        /// </summary>
        /// <param name="span">The <see cref="SnapshotSpan"/> with which to associate the tag.</param>
        /// <param name="tag">The tag associated with the span.</param>
        /// <exception cref="ArgumentNullException"><paramref name="tag"/> is null.</exception>
        public TagSpan(SnapshotSpan span, T tag)
        {
            if (tag == null)
                throw new ArgumentNullException("tag");

            Span = span;
            Tag = tag;
        }
    }
}
