using System;
using System.Collections.Generic;

namespace Microsoft.VisualStudio.Text.Differencing
{
    /// <summary>
    /// A tokenized representation of a string into abutting and non-overlapping segments.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This interface implements IList so that it can be used with 
    /// <see cref="IDifferenceService" />, which finds the differences between two sequences represented
    /// as ILists.</para>
    /// </remarks>
    public interface ITokenizedStringList : IList<string>
    {
        /// <summary>
        /// The original string that was tokenized.
        /// </summary>
        string Original { get; }

        /// <summary>
        /// Maps the index of an element to its span in the original list.
        /// </summary>
        /// <param name="index">The index of the element in the element list.</param>
        /// <returns>The span of the element.</returns>
        /// <exception cref="ArgumentOutOfRangeException">The specified index is either negative or exceeds the list's Count property.</exception>
        /// <remarks>This method returns a zero-length span at the end of the string if index
        /// is equal to the list's Count property.</remarks>
        Span GetElementInOriginal(int index);

        /// <summary>
        /// Maps a span of elements in this list to the span in the original list.
        /// </summary>
        /// <param name="span">The span of elements in the elements list.</param>
        /// <returns>The span mapped onto the original list.</returns>
        Span GetSpanInOriginal(Span span);
    }
}