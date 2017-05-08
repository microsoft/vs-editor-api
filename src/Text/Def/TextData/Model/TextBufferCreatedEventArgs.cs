// Copyright (c) Microsoft Corporation
// All rights reserved

namespace Microsoft.VisualStudio.Text
{
    using System;

    /// <summary>
    /// Provides information about a newly created <see cref="ITextBuffer"/>.
    /// </summary>
    public class TextBufferCreatedEventArgs : EventArgs
    {
        /// <summary>
        /// The newly created <see cref="ITextBuffer"/>.
        /// </summary>
        public ITextBuffer TextBuffer { get; private set; }

        /// <summary>
        /// Constructs a <see cref="TextBufferCreatedEventArgs"/>.
        /// </summary>
        /// <param name="textBuffer">The <see cref="ITextBuffer"/> which was created.</param>
        public TextBufferCreatedEventArgs(ITextBuffer textBuffer)
        {
            if (textBuffer == null)
            {
                throw new ArgumentNullException("textBuffer");
            }
            TextBuffer = textBuffer;
        }
    }
}