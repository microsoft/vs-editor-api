//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License. See License.txt in the project root for license information.
//

using System;

namespace Microsoft.VisualStudio.Core.Imaging
{
    [Flags]
    public enum ImageTags
    {
        None = 0 << 0,
        Dark = 1 << 0,
        Disabled = 1 << 1,
        Error = 1 << 2,
        Hover = 1 << 3,
        Pressed = 1 << 4,
        Selected = 1 << 5,
        Template = 1 << 6
    }
}
