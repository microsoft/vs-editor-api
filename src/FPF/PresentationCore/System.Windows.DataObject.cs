using System.Collections.Generic;

namespace System.Windows
{
    public interface IDataObject
    {
        bool GetDataPresent(string format, bool autoConvert);
        bool GetDataPresent(Type format);
        bool GetDataPresent(string format);
        object GetData(string format);
        object GetData(string format, bool autoConvert);
        string[] GetFormats();
    }

    public sealed class DataObject : IDataObject
    {
        public readonly struct DataItem
        {
            public readonly string Format;
            public readonly object Data;

            public DataItem (string format, object data)
            {
                Format = format;
                Data = data;
            }
        }

        readonly List<DataItem> items = new List<DataItem>(4);
        public IReadOnlyList<DataItem> Items => items;

        public void SetData(string format, object data)
            => items.Add(new DataItem(format, data));

        public void SetText(string textData)
            => SetText(textData, TextDataFormat.UnicodeText);

        public void SetText(string textData, TextDataFormat format)
            => SetData(DataFormats.ConvertToDataFormats(format), textData);

        public bool GetDataPresent(Type format)
        {
            if (format == typeof(string))
                return GetDataPresent(DataFormats.Text);

            return GetDataPresent(format.FullName);
        }

        public bool GetDataPresent(string format)
            => GetDataPresent(format, true);

        public bool GetDataPresent(string format, bool autoConvert)
        {
            foreach (var item in items)
            {
                if (string.Equals(item.Format, format, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        public object GetData(string format)
            => GetData(format, true);

        public object GetData(string format, bool autoConvert)
        {
            foreach (var item in items)
            {
                if (string.Equals(item.Format, format, StringComparison.OrdinalIgnoreCase))
                {
                    if (item.Data is string tsv &&
                        string.Equals(item.Format, DataFormats.CommaSeparatedValue))
                        return tsv.Replace('\t', ',');

                    return item.Data;
                }
            }

            return null;
        }

        public string[] GetFormats()
        {
            var formats = new string[items.Count];
            for (int i = 0; i < items.Count; i++)
                formats[i] = items[i].Format;
            return formats;
        }
    }
}