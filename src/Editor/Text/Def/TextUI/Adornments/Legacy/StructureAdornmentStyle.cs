//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License. See License.txt in the project root for license information.
//
using System;

namespace Microsoft.VisualStudio.Text.Adornments
{
    [Obsolete("Use StructureTipStyle instead")]
    public class StructureAdornmentStyle
    {
        public virtual object BorderBrush { get; protected set; }

        public virtual object BackgroundBrush { get; protected set; }
    }
}
