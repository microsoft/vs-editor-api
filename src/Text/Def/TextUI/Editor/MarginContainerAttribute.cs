// ****************************************************************************
// Copyright (C) Microsoft Corporation.  All Rights Reserved.
// ****************************************************************************
using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.Text.Editor
{
    /// <summary>
    /// Specifies the type of margin container.
    /// </summary>
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Field, AllowMultiple = false)]
    public sealed class MarginContainerAttribute : SingletonBaseMetadataAttribute
    {
        private readonly string marginContainer;

        /// <summary>
        /// Instantiates a new instance of a <see cref="MarginContainerAttribute"/>.
        /// </summary>
        /// <param name="marginContainer">The name of the container for this margin.</param>
        /// <exception cref="ArgumentNullException"><paramref name="marginContainer"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="marginContainer"/> is an empty string.</exception>
        public MarginContainerAttribute(string marginContainer)
        {
            if (marginContainer == null)
                throw new ArgumentNullException("marginContainer");
            if (marginContainer.Length == 0)
                throw new ArgumentException("marginContainer is an empty string.");

            this.marginContainer = marginContainer;
        }

        /// <summary>
        /// The name of the margin container.
        /// </summary>
        public string MarginContainer
        {
            get 
            { 
                return this.marginContainer; 
            }
        }
    }
}