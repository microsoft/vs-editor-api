//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License. See License.txt in the project root for license information.
//
// This file contain implementations details that are subject to change without notice.
// Use at your own risk.
//
using System;
using System.IO;
using System.Threading;
using Microsoft.VisualStudio.Text.Utilities;

namespace Microsoft.VisualStudio.Text.Implementation
{
    internal static class TextImageLoader
    {
        public const int BlockSize = 16384;

        internal static StringRebuilder Load(TextReader reader, long fileSize,
                                             out bool hasConsistentLineEndings, out int longestLineLength,
                                             int blockSize = 0,
                                             int minCompressedBlockSize = TextImageLoader.BlockSize)                                             // Exposed for unit tests
        {
            LineEndingState lineEnding = LineEndingState.Unknown;
            int currentLineLength = 0;
            longestLineLength = 0;

            bool useCompressedStringRebuilders = (fileSize >= TextModelOptions.CompressedStorageFileSizeThreshold);
            if (blockSize == 0)
                blockSize = useCompressedStringRebuilders ? TextModelOptions.CompressedStoragePageSize : TextImageLoader.BlockSize;

            PageManager pageManager = null;
            char[] buffer;
            if (useCompressedStringRebuilders)
            {
                pageManager = new PageManager();
                buffer = new char[blockSize];
            }
            else
            {
                buffer = TextImageLoader.AcquireBuffer(blockSize);
            }

            StringRebuilder content = StringRebuilderForChars.Empty;
            try
            {
                while (true)
                {
                    int read = TextImageLoader.LoadNextBlock(reader, buffer);

                    if (read == 0)
                        break;

                    var lineBreaks = TextImageLoader.ParseBlock(buffer, read, ref lineEnding, ref currentLineLength, ref longestLineLength);

                    char[] bufferForStringBuilder = buffer;
                    if (read < (buffer.Length / 2))
                    {
                        // We read far less characters than buffer so copy the contents to a new buffer and reuse the original buffer.
                        bufferForStringBuilder = new char[read];
                        Array.Copy(buffer, bufferForStringBuilder, read);
                    }
                    else
                    {
                        // We're using most of buffer so allocate a new block for the next chunk.
                        buffer = new char[blockSize];
                    }

                    var newContent = (useCompressedStringRebuilders && (read > minCompressedBlockSize))
                                     ? StringRebuilderForCompressedChars.Create(new Page(pageManager, bufferForStringBuilder, read), lineBreaks)
                                     : StringRebuilderForChars.Create(bufferForStringBuilder, read, lineBreaks);

                    content = content.Insert(content.Length, newContent);
                }

                longestLineLength = Math.Max(longestLineLength, currentLineLength);
            }
            finally
            {
                if (!useCompressedStringRebuilders)
                {
                    TextImageLoader.ReleaseBuffer(buffer);
                }
            }

            hasConsistentLineEndings = lineEnding != LineEndingState.Inconsistent;

            return content;
        }

        public static int LoadNextBlock(TextReader reader, char[] buffer)
        {
            // Reserve 1 spot for a potential CR at the end of the buffer (in which we want to add the next LF, if it exists)
            int read = reader.ReadBlock(buffer, 0, buffer.Length - 1);
            if ((read == buffer.Length - 1) && (buffer[read - 1] == '\r'))
            {
                // Last character read was a CR and there is, probably since we read the entire block, more to go.
                var next = reader.Peek();
                if (next == '\n')
                {
                    // We had a crlf that spanned the end of the buffer. Add it to the buffer and carry on.
                    // In theory we could append anything other than another CR but having the block end at
                    // the end of a line is a good thing.
                    reader.Read();
                    buffer[read++] = '\n';
                }
            }

            return read;
        }

        private static ILineBreaks ParseBlock(char[] buffer, int length,
                                              ref LineEndingState lineEnding, ref int currentLineLength, ref int longestLineLength)
        {
            // Note that the lineBreaks created here will (internally) use the pooled list of line breaks.
            IPooledLineBreaksEditor lineBreaks = LineBreakManager.CreatePooledLineBreakEditor(length);

            int index = 0;
            while (index < length)
            {
                int breakLength = TextUtilities.LengthOfLineBreak(buffer, index, length);
                if (breakLength == 0)
                {
                    ++currentLineLength;
                    ++index;
                }
                else
                {
                    lineBreaks.Add(index, breakLength);
                    longestLineLength = Math.Max(longestLineLength, currentLineLength);
                    currentLineLength = 0;

                    if (lineEnding != LineEndingState.Inconsistent)
                    {
                        if (breakLength == 2)
                        {
                            if (lineEnding == LineEndingState.Unknown)
                                lineEnding = LineEndingState.CRLF;
                            else if (lineEnding != LineEndingState.CRLF)
                                lineEnding = LineEndingState.Inconsistent;
                        }
                        else
                        {
                            LineEndingState newLineEndingState;
                            switch (buffer[index])
                            {
                                // This code needs to be kep consistent with TextUtilities.LengthOfLineBreak()
                                case '\r': newLineEndingState = LineEndingState.CR; break;
                                case '\n': newLineEndingState = LineEndingState.LF; break;
                                case '\u0085': newLineEndingState = LineEndingState.NEL; break;
                                case '\u2028': newLineEndingState = LineEndingState.LS; break;
                                case '\u2029': newLineEndingState = LineEndingState.PS; break;
                                default: throw new InvalidOperationException("Unexpected line ending");
                            }

                            if (lineEnding == LineEndingState.Unknown)
                                lineEnding = newLineEndingState;
                            else if (lineEnding != newLineEndingState)
                                lineEnding = LineEndingState.Inconsistent;
                        }
                    }

                    index += breakLength;
                }
            }

            lineBreaks.ReleasePooledLineBreaks();

            return lineBreaks;
        }

        internal enum LineEndingState
        {
            Unknown = 0,
            CRLF = 1,
            CR = 2,
            LF = 3,
            NEL = 4,            // unicode Next Line 0085
            LS = 5,             // unicode Line Separator 2028
            PS = 6,             // unicode Paragraph Separator 2029
            Inconsistent = 7,
        }

        private static char[] pooledBuffer;

        private static char[] AcquireBuffer(int size)
        {
            char[] buffer = Volatile.Read(ref pooledBuffer);
            if (buffer != null && buffer.Length >= size)
            {
                if (buffer == Interlocked.CompareExchange(ref pooledBuffer, null, buffer))
                {
                    return buffer;
                }
            }

            return new char[size];
        }

        private static void ReleaseBuffer(char[] buffer)
        {
            Interlocked.CompareExchange(ref pooledBuffer, buffer, null);
        }
    }
}
