//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License. See License.txt in the project root for license information.
//

namespace Microsoft.VisualStudio.Core.Imaging
{
    public interface IImageService
    {
        object GetImage(ImageDescription description);
    }

    public static class ImageServiceExtensions
    {
        public static object GetImage(
            this IImageService imageService,
            ImageId id,
            ImageTags tags = ImageTags.None)
            => imageService.GetImage(new ImageDescription(id, tags));

        public static object GetImage(
            this IImageService imageService,
            ImageId id,
            int size,
            ImageTags tags = ImageTags.None)
            => imageService.GetImage(new ImageDescription(id, size, tags));
    }
}
