using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Text.Utilities;

namespace Microsoft.VisualStudio.Text.Implementation
{
    /// <summary>
    /// Information about the line breaks contained in an <see cref="StringRebuilderForChars"/>.
    /// </summary>
    public interface ILineBreaks
    {
        /// <summary>
        /// The number of line breaks in the <see cref="StringRebuilderForChars"/>.
        /// </summary>
        int Length { get; }

        /// <summary>
        /// The starting position of the <paramref name="index"/>th line break.
        /// </summary>
        int StartOfLineBreak(int index);

        /// <summary>
        /// The starting position of the <paramref name="index"/>th line break.
        /// </summary>
        int EndOfLineBreak(int index);
    }

    public interface ILineBreaksEditor : ILineBreaks
    {
        /// <summary>
        /// Add a line break at <paramref name="start"/> with <paramref name="length"/>
        /// </summary>
        void Add(int start, int length);
    }
}
