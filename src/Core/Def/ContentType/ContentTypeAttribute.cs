// Copyright (C) Microsoft Corporation.  All Rights Reserved.

using System;

namespace Microsoft.VisualStudio.Utilities
{
    using System.ComponentModel.Composition;

    /// <summary>
    /// Declares an association between an extension part and a particular content type.
    /// </summary>
    /// <seealso cref="IContentType"></seealso>
    /// <seealso cref="IContentTypeRegistryService"></seealso>
    /// <seealso cref="ContentTypeDefinition"></seealso>
    public sealed class ContentTypeAttribute : MultipleBaseMetadataAttribute
    {
        private string contentTypes;

        /// <summary>
        /// Initializes a new instance of <see cref="ContentTypeAttribute"/>.
        /// </summary>
        /// <param name="name">The content type name. 
        /// Content type names are case-insensitive.</param>
        /// <exception cref="ArgumentNullException"><paramref name="name"/>is null or an empty string.</exception>
        public ContentTypeAttribute(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }
            contentTypes = name;
        }

        /// <summary>
        /// The content type name.
        /// </summary>
        public string ContentTypes
        {
            get
            {
                return contentTypes;
            }
        }
    }
}
