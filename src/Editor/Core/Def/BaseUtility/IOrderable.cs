//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License. See License.txt in the project root for license information.
//
using System.ComponentModel;
using System.Collections.Generic;

namespace Microsoft.VisualStudio.Utilities
{
    /// <summary>
    /// Associated with an orderable part. 
    /// </summary>
    /// <remarks>This interface is helpful when importing orderable parts.</remarks> 
    public interface IOrderable
    {
        /// <summary>
        /// Uniquely identifies a part with respect to parts of the same type.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The parts before which this part should appear in the list.
        /// </summary>
        [DefaultValue(null)]
        IEnumerable<string> Before { get; }

        /// <summary>
        /// The parts after which this part should appear in the list.
        /// </summary>
        [DefaultValue(null)]
        IEnumerable<string> After { get; }
    }
}
