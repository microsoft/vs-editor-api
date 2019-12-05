namespace System.Windows
{
    public static class DataFormats
    {
        public static readonly string Text = "public.utf8-plain-text";
        public static readonly string UnicodeText = "public.utf8-plain-text";
        public static readonly string Rtf = "public.rtf";
        public static readonly string Html = "public.html";
        public static readonly string CommaSeparatedValue = "public.utf8-tab-separated-values-text";
        public static readonly string FileDrop = "";

        internal static string ConvertToDataFormats(TextDataFormat textDataformat)
        {
            switch (textDataformat)
            {
                case TextDataFormat.Text:
                    return Text;
                case TextDataFormat.UnicodeText:
                    return UnicodeText;
                case TextDataFormat.Rtf:
                    return Rtf;
                case TextDataFormat.Html:
                    return Html;
                default:
                    return UnicodeText;
            }
        }
    }
}