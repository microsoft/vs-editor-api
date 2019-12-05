using System.Collections;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace System.Windows
{
    public static class TextDecorations
    {
        public static TextDecoration Baseline { get; } = new TextDecoration();
        public static TextDecoration Overline { get; } = new TextDecoration();
        public static TextDecoration Strikethrough { get; } = new TextDecoration();
        public static TextDecoration Underline { get; } = new TextDecoration();
    }

    public sealed class TextDecoration : Animatable
    {
        public Pen Pen { get; set; }
    }

    public sealed class TextDecorationCollection : Animatable, IList<TextDecoration>
    {
        readonly List<TextDecoration> textDecorations = new List<TextDecoration>();

        public TextDecoration this[int index]
        {
            get => textDecorations[index];
            set => textDecorations[index] = value;
        }

        public int Count => textDecorations.Count;

        public bool IsReadOnly => false;

        public void Add(IEnumerable<TextDecoration> textDecorations)
            => this.textDecorations.AddRange(textDecorations);

        public void Add(TextDecoration textDecoration)
            => this.textDecorations.Add(textDecoration);

        public void Clear()
            => textDecorations.Clear();

        public bool Contains(TextDecoration item)
            => textDecorations.Contains(item);

        public void CopyTo(TextDecoration[] array, int arrayIndex)
            => textDecorations.CopyTo(array, arrayIndex);

        public int IndexOf(TextDecoration item)
            => textDecorations.IndexOf(item);

        public void Insert(int index, TextDecoration item)
            => textDecorations.Insert(index, item);

        public bool Remove(TextDecoration item)
            => textDecorations.Remove(item);

        public void RemoveAt(int index)
            => textDecorations.RemoveAt(index);

        public IEnumerator<TextDecoration> GetEnumerator()
            => ((IList<TextDecoration>)textDecorations).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => ((IList<TextDecoration>)textDecorations).GetEnumerator();
    }
}