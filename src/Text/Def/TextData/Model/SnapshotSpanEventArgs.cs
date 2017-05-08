// Copyright (c) Microsoft Corporation
// All rights reserved

namespace Microsoft.VisualStudio.Text
{
    using System;

    /// <summary>
    /// Provides information for events that report changes affecting a span of text.
    /// </summary>
    public class SnapshotSpanEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the <see cref="SnapshotSpan"/>.
        /// </summary>
        public SnapshotSpan Span { get; private set; }

        /// <summary>
        /// Initializes a new instance of <see cref="SnapshotSpanEventArgs"/> with the specified <see cref="SnapshotSpan" />.
        /// </summary>
        /// <param name="span">The <see cref="SnapshotSpan" />.</param>
        public SnapshotSpanEventArgs(SnapshotSpan span)
        {
            Span = span;
        }
    }
}
