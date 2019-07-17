//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License. See License.txt in the project root for license information.
//

using System;

using Microsoft.VisualStudio.Core.Imaging;

namespace Microsoft.VisualStudio.Language.Intellisense
{
    public static class SignatureHelpImageCatalog
    {
        public static readonly Guid CatalogId = new Guid("95fdedcb-dc13-48a8-8165-ed1fff877d9a");

        public static object GetImage(this IImageService imageService, SignatureHelpImageId imageId)
            => imageService.GetImage(new ImageDescription(new ImageId(CatalogId, (int)imageId), 16, ImageTags.Template)) as object;
    }

    public enum SignatureHelpImageId
    {
        PreviousSignatureButton = 1,
        NextSignatureButton
    }
}
