// Copyright (c) Microsoft Corporation
// All rights reserved

namespace Microsoft.VisualStudio.Text.Formatting
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Provides information for the tag aggregator TagsChanged event,
    /// and returns the span of changed tags as a mapping span.
    /// </summary>
    public class TextAndAdornmentSequenceChangedEventArgs : EventArgs
    {   
        /// <summary>
        /// Gets the span over which tags have changed.
        /// </summary>
        public IMappingSpan Span { get; private set; }

        /// <summary>
        /// Initializes a new instance of <see cref="TextAndAdornmentSequenceChangedEventArgs"/> with the specified <see cref="IMappingSpan" />.
        /// </summary>
        /// <param name="span">The span that changed.</param>
        /// <exception cref="ArgumentNullException"><paramref name="span"/> is null.</exception>
        public TextAndAdornmentSequenceChangedEventArgs(IMappingSpan span)
        {
            if (span == null)
                throw new ArgumentNullException("span");

            this.Span = span;
        }
    }
}
