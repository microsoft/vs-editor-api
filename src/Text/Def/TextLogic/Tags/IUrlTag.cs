using System;
namespace Microsoft.VisualStudio.Text.Tagging
{
    /// <summary>
    /// A tag that represents a URL.
    /// </summary>
    public interface IUrlTag : ITag
    {
        /// <summary>
        /// The URL.
        /// </summary>
        Uri Url { get; }
    }
}
