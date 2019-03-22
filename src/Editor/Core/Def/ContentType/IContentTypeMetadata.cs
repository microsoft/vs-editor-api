//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License. See License.txt in the project root for license information.
//
using System.Collections.Generic;

namespace Microsoft.VisualStudio.Utilities
{
    /// <summary>
    /// Represents MEF metadata view corresponding to the <see cref="ContentTypeAttribute"/>s.
    /// </summary>
    public interface IContentTypeMetadata
    {
        /// <summary>
        /// List of declared content types.
        /// </summary>
        IEnumerable<string> ContentTypes { get; }
    }
}
