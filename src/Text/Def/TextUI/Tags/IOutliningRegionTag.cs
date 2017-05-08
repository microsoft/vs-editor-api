using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.VisualStudio.Text.Tagging
{
    /// <summary>
    /// Provides a tag for outlining regions.
    /// </summary>
    public interface IOutliningRegionTag : ITag
    {
        /// <summary>
        /// Determines whether the region is collapsed by default.
        /// </summary>
        bool IsDefaultCollapsed { get; }

        /// <summary>
        /// Determines whether a region is an implementation region.
        /// </summary>
        /// <remarks>
        /// Implementation regions are the blocks of code following a method definition. 
        /// They are used for commands such as the Visual Studio Collapse to Definition command, 
        /// which hides the implementation region and leaves only the method definition exposed.
        /// </remarks>
        bool IsImplementation { get; }

        /// <summary>
        /// Gets the data object for the collapsed UI. If the default is set, returns null.
        /// </summary>
        object CollapsedForm { get; }

        /// <summary>
        /// Gets the data object for the collapsed UI tooltip. If the default is set, returns null.
        /// </summary>
        object CollapsedHintForm { get; }
    }
}
