//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License. See License.txt in the project root for license information.
//

using System;

using Microsoft.VisualStudio.Core.Imaging;

namespace Microsoft.VisualStudio.Text.OverviewMargin
{
    public static class OverviewMarginCatalog
    {
        public static readonly Guid CatalogId = new Guid("47efec21-fd36-445d-99a4-238f66ab764c");

        public static object GetImage(this IImageService imageService, OverviewMarginImageId imageId, ImageTags imageTags = default)
        	=> imageService.GetImage(new ImageDescription(new ImageId(CatalogId, (int)imageId), 16, imageTags));
    }

    public enum OverviewMarginImageId
    {
        Busy = 1,
        Ok,
        Warning,
        Error,
        Suggestion,
        Hide
    }
}
