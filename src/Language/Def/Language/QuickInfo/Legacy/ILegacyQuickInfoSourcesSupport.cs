namespace Microsoft.Internal.VisualStudio.Language.Intellisense
{
    using System;
    using System.Collections.Generic;
    using Microsoft.VisualStudio.Language.Intellisense;

    // Bug #512117: Remove compatibility shims for 2nd gen. Quick Info APIs.
#pragma warning disable 618
    /// <summary>
    /// This interface supports the product infrastructure and should not be used.
    /// </summary>
    [Obsolete("This interface supports legacy product infrastructure, is subject to breakage without notice, and should not be used")]
    internal interface ILegacyQuickInfoSourcesSupport
    {
        /// <summary>
        /// This interface supports the product infrastructure and should not be used.
        /// </summary>
        IEnumerable<Lazy<IAsyncQuickInfoSourceProvider, LegacyQuickInfoMetadata>> LegacySources { get; }
    }
#pragma warning restore 618
}
