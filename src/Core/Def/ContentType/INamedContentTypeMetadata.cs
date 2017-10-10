//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License. See License.txt in the project root for license information.
//
namespace Microsoft.VisualStudio.Utilities
{
    /// <summary>
    /// Represents MEF metadata view combining <see cref="IContentTypeMetadata"/> and <see cref="INameAndReplacesMetadata"/> views.
    /// </summary>
    public interface INamedContentTypeMetadata : IContentTypeMetadata, INameAndReplacesMetadata
    {
    }
}
