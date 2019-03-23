//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License. See License.txt in the project root for license information.
//
namespace Microsoft.VisualStudio.Text.Differencing
{
    using System;

    public interface IDifferenceBuffer2 : IDifferenceBuffer
    {
        /// <summary>
        /// The source of the left buffer in the difference. Can be set to null.
        /// </summary>
        new ITextBuffer BaseLeftBuffer { get; set; }

        event EventHandler<BufferChangedEventArgs> BaseLeftBufferChanged;
    }

    public class BufferChangedEventArgs : EventArgs
    {
        public BufferChangedEventArgs(ITextBuffer oldBuffer, ITextBuffer newBuffer)
        {
            this.OldBuffer = oldBuffer;
            this.NewBuffer = newBuffer;
        }

        public ITextBuffer OldBuffer { get; }
        public ITextBuffer NewBuffer { get; }
    }
}
