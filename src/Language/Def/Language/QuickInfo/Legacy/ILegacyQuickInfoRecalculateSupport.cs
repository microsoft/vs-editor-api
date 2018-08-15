namespace Microsoft.Internal.VisualStudio.Language.Intellisense
{
    using System;

#pragma warning disable 618
    /// <summary>
    /// This interface supports the product infrastructure and should not be used.
    /// </summary>
    [Obsolete("This interface supports legacy product infrastructure, is subject to breakage without notice, and should not be used")]
    internal interface ILegacyQuickInfoRecalculateSupport
    {
        void Recalculate();
    }
#pragma warning restore 618
}
