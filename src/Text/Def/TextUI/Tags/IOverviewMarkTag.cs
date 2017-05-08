// Copyright (c) Microsoft Corporation
// All rights reserved

namespace Microsoft.VisualStudio.Text.Tagging
{
    /// <summary>
    /// Provides the information needed to render a mark in the overview margin.
    /// </summary>
    public interface IOverviewMarkTag : ITag
    {
        /// <summary>
        /// Gets the name of the EditorFormatDefinition whose background color is used to draw the mark.
        /// </summary>
        string MarkKindName { get; }
    }
}
