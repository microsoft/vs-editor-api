//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License. See License.txt in the project root for license information.
//
using System;

// TODO: DevDiv bug #369787: remove this in Dev16 when IBlockTag and related facilities are deprecated.
namespace Microsoft.VisualStudio.Text.Adornments
{
    /// <summary>
    /// Enumerates the predefined structural block types.
    /// </summary>
    [Obsolete("Use PredefinedStructureTagTypes instead")]
    public static class PredefinedStructureTypes
    {
        /// <summary>
        /// Represents structural blocks, with vertical line adornments displayed.
        /// </summary>
        public const string Structural = "Structural";

        /// <summary>
        /// Represents non-structural blocks, with no vertical line adornments
        /// displayed, only expand and collapse.
        /// </summary>
        public const string Nonstructural = "Nonstructural";

        public const string PropertyBlock = "PropertyBlock";
        public const string AccessorBlock = "AccessorBlock";
        public const string AnonymousMethodBlock = "AnonymousMethodBlock";    // i.e. lambda bodies
        public const string Constructor = "Constructor";
        public const string Destructor = "Destructor";
        public const string Operator = "Operator";
        public const string Method = "Method";
        public const string Namespace = "Namespace";
        public const string Class = "Class";
        public const string Interface = "Interface";
        public const string Struct = "Struct";
        public const string TryCatchFinally = "TryCatchFinally";
        public const string Conditional = "Conditional";                    // i.e. If statements, ‘switch’ statements.i.e conditionals+branches
        public const string Case = "Case";
        public const string Loop = "Loop";                                  // i.e. While/For/Foreach/Do/Until (i.e.loops)
        public const string Standalone = "Standalone";                      // i.e. stand-alone {} used for scoping.

        public const string Lock = "Context";                               // i.e. using/lock/checked/unchecked. Need a better name for this.

        public const string Unknown = "Unknown";
    }
}
