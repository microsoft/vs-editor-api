//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License. See License.txt in the project root for license information.
//
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.Text.Utilities
{
    /// <summary>
    /// Metadata which includes Content Types and Text View Roles
    /// </summary>
    public interface IOrderableContentTypeAndOptionalTextViewRoleMetadata : IContentTypeMetadata, IOrderable
    {
        [DefaultValue(null)]
        IEnumerable<string> TextViewRoles { get; }
    }
}
