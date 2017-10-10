using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Text.Utilities;

namespace Microsoft.VisualStudio.Text.Implementation
{
    public static class LineBreakManager
    {
        public readonly static ILineBreaks Empty = new ShortLineBreaksEditor(0);

        public static ILineBreaksEditor CreateLineBreakEditor(int maxLength, int initialCapacity = 0)
        {
            return (maxLength < short.MaxValue)
                   ? (ILineBreaksEditor)(new ShortLineBreaksEditor(initialCapacity))
                   : (ILineBreaksEditor)(new IntLineBreaksEditor(initialCapacity));
        }

        public static ILineBreaks CreateLineBreaks(string source)
        {
            ILineBreaksEditor lineBreaks = null;

            int index = 0;
            while (index < source.Length)
            {
                int breakLength = TextUtilities.LengthOfLineBreak(source, index, source.Length);
                if (breakLength == 0)
                {
                    ++index;
                }
                else
                {
                    if (lineBreaks == null)
                        lineBreaks = LineBreakManager.CreateLineBreakEditor(source.Length);

                    lineBreaks.Add(index, breakLength);
                    index += breakLength;
                }
            }

            return lineBreaks ?? Empty;
        }

        private class ShortLineBreaksEditor : ILineBreaksEditor
        {
            private const ushort MaskForPosition = 0x7fff;
            private const ushort MaskForLength = 0x8000;

            private readonly static List<ushort> Empty = new List<ushort>(0);
            private List<ushort> _lineBreaks = Empty;

            public ShortLineBreaksEditor(int initialCapacity)
            {
                if (initialCapacity > 0)
                    _lineBreaks = new List<ushort>(initialCapacity);
            }

            public int Length => _lineBreaks.Count;

            public int LengthOfLineBreak(int index)
            {
                return ((_lineBreaks[index] & MaskForLength) != 0 ? 2 : 1);
            }

            public int StartOfLineBreak(int index)
            {
                return (int)(_lineBreaks[index] & MaskForPosition);
            }
            public int EndOfLineBreak(int index)
            {
                int lineBreak = _lineBreaks[index];
                return (lineBreak & MaskForPosition) + 
                       (((lineBreak & MaskForLength) != 0) ? 2 : 1);
            }

            public void Add(int start, int length)
            {
                if ((start < 0) || (start > short.MaxValue))
                    throw new ArgumentOutOfRangeException(nameof(start));
                if ((length < 1) || (length > 2))
                    throw new ArgumentOutOfRangeException(nameof(length));

                if (_lineBreaks == Empty)
                    _lineBreaks = new List<ushort>();

                if (length == 1)
                    _lineBreaks.Add((ushort)start);
                else if (length == 2)
                    _lineBreaks.Add((ushort)(start | MaskForLength));
            }
        }

        private class IntLineBreaksEditor : ILineBreaksEditor
        {
            private const uint MaskForPosition = 0x7fffffff;
            private const uint MaskForLength = 0x80000000;

            private readonly static List<uint> Empty = new List<uint>(0);
            private List<uint> _lineBreaks = Empty;

            public IntLineBreaksEditor(int initialCapacity)
            {
                if (initialCapacity > 0)
                    _lineBreaks = new List<uint>(initialCapacity);
            }

            public int Length => _lineBreaks.Count;

            public int LengthOfLineBreak(int index)
            {
                return (_lineBreaks[index] & MaskForLength) != 0 ? 2 : 1;
            }

            public int StartOfLineBreak(int index)
            {
                return (int)(_lineBreaks[index] & MaskForPosition);
            }

            public int EndOfLineBreak(int index)
            {
                uint lineBreak = _lineBreaks[index];
                return (int)((lineBreak & MaskForPosition) +
                             (((lineBreak & MaskForLength) != 0) ? 2 : 1));
            }

            public void Add(int start, int length)
            {
                if ((start < 0) || (start > int.MaxValue))
                    throw new ArgumentOutOfRangeException(nameof(start));
                if ((length < 1) || (length > 2))
                    throw new ArgumentOutOfRangeException(nameof(length));

                if (_lineBreaks == Empty)
                    _lineBreaks = new List<uint>();

                if (length == 1)
                    _lineBreaks.Add((uint)start);
                else if (length == 2)
                    _lineBreaks.Add((uint)(start | MaskForLength));
            }
        }
    }
}
