using System;

namespace Microsoft.VisualStudio.Text.Tagging
{
    /// <summary>
    /// Represents the text marker tag, which is used to place text marker adornments on a view.
    /// </summary>
    public interface ITextMarkerTag : ITag
    {
        /// <summary>
        /// Gets the type of adornment to use.
        /// </summary>
        string Type { get; }
    }
}
