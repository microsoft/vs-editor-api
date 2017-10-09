//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License. See License.txt in the project root for license information.
//
using System;

namespace Microsoft.VisualStudio.Utilities
{
    /// <summary>
    /// Provides a display name for an editor component part.
    /// </summary>    
    /// <remarks>
    /// This attribute should be localized wherever it is used.
    /// </remarks>
    public sealed class DisplayNameAttribute : SingletonBaseMetadataAttribute
    {
        private string displayName;

        /// <summary>
        /// Initializes a new instance of <see cref="DisplayNameAttribute"/>.
        /// </summary>
        /// <param name="displayName">The display name of an editor component part.</param>
        public DisplayNameAttribute(string displayName)
        {
            if (displayName == null)
            {
                throw new ArgumentNullException("displayName");
            }
            this.displayName = displayName;
        }

        /// <summary>
        /// Gets the display name of an editor component part.
        /// </summary>
        public string DisplayName
        {
            get
            {
                return this.displayName;
            }
        }
    }
}