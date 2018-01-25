//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License. See License.txt in the project root for license information.
//
// This file contain implementations details that are subject to change without notice.
// Use at your own risk.
//

namespace Microsoft.VisualStudio.Utilities.Implementation
{
    public interface IFilePathToContentTypeMetadata : IOrderable
    {
        [System.ComponentModel.DefaultValue(null)]
        string FileExtension { get; }

        [System.ComponentModel.DefaultValue(null)]
        string FileName { get; }
    }
}
