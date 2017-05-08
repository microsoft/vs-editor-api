// Copyright (C) Microsoft Corporation.  All Rights Reserved.

using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.Utilities
{
    /// <summary>
    /// Identifies a file extension.
    /// </summary>
    public sealed class FileExtensionAttribute : SingletonBaseMetadataAttribute
    {
        private string fileExtension;

        /// <summary>
        /// Constructs a new instance of the attribute.
        /// </summary>
        /// <param fileExtension="fileExtension">The file extension.</param>
        /// <exception cref="ArgumentNullException"><paramref name="fileExtension"/> is null or empty.</exception>
        public FileExtensionAttribute(string fileExtension)
        {
            if (string.IsNullOrWhiteSpace(fileExtension))
            {
                throw new ArgumentNullException("fileExtension");
            }
            this.fileExtension = fileExtension;
        }

        /// <summary>
        /// Gets the file extension.
        /// </summary>
        public string FileExtension
        {
            get 
            { 
                return this.fileExtension;
            }
        }
    }
}
