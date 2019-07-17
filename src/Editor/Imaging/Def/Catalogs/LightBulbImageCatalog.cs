//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License. See License.txt in the project root for license information.
//

using System;

using Microsoft.VisualStudio.Core.Imaging;

namespace Microsoft.VisualStudio.Language.Intellisense
{
    public static class LightBulbImageCatalog
    {
        public static readonly Guid CatalogId = new Guid("f09c8ce2-d515-4226-a430-dec104c81f05");

        public static object GetImage(this IImageService imageService, LightBulbImageId imageId, ImageTags imageTags = default)
            => imageService.GetImage(new ImageDescription(new ImageId(CatalogId, (int)imageId), 16, imageTags));
    }

    public enum LightBulbImageId
    {
        OnlyActions = 1,
        Fixes,
        ErrorFixes,
    }
}
