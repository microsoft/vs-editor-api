namespace Microsoft.VisualStudio.Core.Imaging
{
    using System;

    /// <summary>
    /// Unique identifier for Visual Studio image asset.
    /// </summary>
    /// <remarks>
    /// On Windows systems, <see cref="ImageId"/> can be converted to and from
    /// various other image representations via the ImageIdExtensions extension methods.
    /// </remarks>
    public struct ImageId
    {
        /// <summary>
        /// The <see cref="Guid"/> identifying the group to which this image belongs.
        /// </summary>
        public readonly Guid Guid;

        /// <summary>
        /// The <see cref="int"/> identifying the particular image from the group that this id maps to.
        /// </summary>
        public readonly int Id;

        /// <summary>
        /// Creates a new instance of ImageId.
        /// </summary>
        /// <param name="guid">The <see cref="Guid"/> identifying the group to which this image belongs.</param>
        /// <param name="id">The <see cref="int"/> identifying the particular image from the group that this id maps to.</param>
        public ImageId(Guid guid, int id)
        {
            this.Guid = guid;
            this.Id = id;
        }
    }
}
