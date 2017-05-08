using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;

namespace Microsoft.VisualStudio.Text.Differencing
{
    /// <summary>
    /// Represents a collection of <see cref="Difference"/> objects extracted from two lists of same-typed elements,
    /// given a maximal match sequence generated from a difference algorithm.
    /// </summary>
    /// <typeparam name="T">The element type of the compared lists.</typeparam>
    public interface IDifferenceCollection<T> : IEnumerable<Difference>
    {
        /// <summary>
        /// Gets the original match sequence that was used to create this difference collection.
        /// </summary>
        IEnumerable<Tuple<int, int>> MatchSequence
        {
            get;
        }

        /// <summary>
        /// Gets the left sequence that was used to create this difference collection.
        /// </summary>
        IList<T> LeftSequence
        {
            get;
        }

        /// <summary>
        /// Gets the right sequence that was used to create this difference collection.
        /// </summary>
        IList<T> RightSequence
        {
            get;
        }

        /// <summary>
        /// Returns the difference collection as a list. 
        /// </summary>
        /// <remarks>Since the difference collection itself implements the IEnumerable interface,
        /// you can use it to iterate over the differences.</remarks>
        IList<Difference> Differences
        {
            get;
        }
    }
}
