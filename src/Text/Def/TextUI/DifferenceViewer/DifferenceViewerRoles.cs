namespace Microsoft.VisualStudio.Text.Differencing
{
    /// <summary>
    /// The text view roles associated with an <see cref="IDifferenceViewer"/>.
    /// </summary>
    public static class DifferenceViewerRoles
    {
        /// <summary>
        /// The text view role for any view owned by an <see cref="IDifferenceViewer"/>.
        /// </summary>
        public const string DiffTextViewRole = "DIFF";

        /// <summary>
        /// The text view role for the <see cref="IDifferenceViewer.LeftView"/>.
        /// </summary>
        public const string LeftViewTextViewRole = "LEFTDIFF";

        /// <summary>
        /// The text view role for the <see cref="IDifferenceViewer.RightView"/>.
        /// </summary>
        public const string RightViewTextViewRole = "RIGHTDIFF";

        /// <summary>
        /// The text view role for the <see cref="IDifferenceViewer.InlineView"/>.
        /// </summary>
        public const string InlineViewTextViewRole = "INLINEDIFF";
    }
}
