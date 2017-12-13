namespace Microsoft.VisualStudio.Text.Adornments
{
    using Microsoft.VisualStudio.Core.Imaging;

    /// <summary>
    /// Represents an image in an <see cref="IToolTipService"/> <see cref="IToolTipPresenter"/>.
    /// </summary>
    ///
    /// <remarks>
    /// <see cref="ImageElement"/>s should be constructed with <see cref="Microsoft.VisualStudio.Core.Imaging.ImageId"/>s
    /// that correspond to an image on that platform.
    /// </remarks>
    public sealed class ImageElement
    {
        /// <summary>
        /// Creates a new instance of an image element.
        /// </summary>
        /// <param name="iamgeId"> A unique identifier for an image.</param>
        public ImageElement(ImageId imageId)
        {
            this.ImageId = imageId;
        }

        /// <summary>
        /// A unique identifier for an image.
        /// </summary>
        public ImageId ImageId { get; }
    }
}
