// Copyright (C) Microsoft Corporation.  All Rights Reserved.

using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.Utilities
{
    /// <summary>
    /// Identifies a file name.
    /// </summary>
    public sealed class FileNameAttribute : SingletonBaseMetadataAttribute
    {
        /// <summary>
        /// Constructs a new instance of the attribute.
        /// </summary>
        /// <param name="fileName">The file extension.</param>
        /// <exception cref="ArgumentNullException"><paramref name="fileName"/> is null or empty.</exception>
        public FileNameAttribute(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentNullException(nameof(fileName));
            }

            this.FileName = fileName;
        }

        /// <summary>
        /// Gets the file name.
        /// </summary>
        public string FileName
        {
            get;
        }
    }
}
