//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License. See License.txt in the project root for license information.
//

namespace Microsoft.VisualStudio.Utilities
{
    public interface IFilePathToContentTypeProvider
    {
        bool TryGetContentTypeForFilePath(string filePath, out IContentType contentType);
    }
}
