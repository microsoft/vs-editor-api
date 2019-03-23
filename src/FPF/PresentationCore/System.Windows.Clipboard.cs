using AppKit;
using Foundation;

namespace System.Windows
{
    public static class Clipboard
    {
        static readonly NSPasteboard pasteboard = NSPasteboard.GeneralPasteboard;
        static readonly string[] textTypes = { DataFormats.UnicodeText };

        public static bool ContainsText()
            => pasteboard.CanReadItemWithDataConformingToTypes(textTypes);

        public static void SetDataObject(object data, bool copy)
        {
            if (data is DataObject dataObject)
            {
                pasteboard.ClearContents();

                foreach (var item in dataObject.Items)
                {
                    switch (item.Data)
                    {
                        case string stringData:
                            pasteboard.SetStringForType(
                                stringData,
                                item.Format);
                            break;
                        case bool boolItem:
                            pasteboard.SetDataForType(
                                NSData.FromArray(new byte[] { boolItem ? (byte)1 : (byte)0 }),
                                item.Format);
                            break;
                    }
                }
            }
        }

        public static IDataObject GetDataObject()
        {
            var dataObject = new DataObject ();
            // Beside copying and pasting UnicodeText to/from pasteboard
            // editor inserts booleans like "VisualStudioEditorOperationsLineCutCopyClipboardTag"
            // which allows editor to know whole line was copied into pasteboard so on paste
            // it inserts line into new line, so we enumerate over all types and if length == 1
            // we just assume it's boolean we set in method above
            foreach (var type in pasteboard.Types)
            {
                if (type == DataFormats.UnicodeText)
                {
                    dataObject.SetText (pasteboard.GetStringForType (type));
                    continue;
                }
                var data = pasteboard.GetDataForType (type);
                if (data != null && data.Length == 1)
                {
                    dataObject.SetData (type, data: data [0] != 0);
                }
            }
            return dataObject;
        }
    }
}