namespace Microsoft.Internal.VisualStudio.Language.Intellisense
{
    using System;
    using System.Collections.Generic;
    using Microsoft.VisualStudio.Language.Intellisense;
    using Microsoft.VisualStudio.Text;

    // Bug #512117: Remove compatibility shims for 2nd gen. Quick Info APIs.
#pragma warning disable 618
    /// <summary>
    /// This interface supports the product infrastructure and should not be used.
    /// </summary>
    [Obsolete("This interface supports legacy product infrastructure, is subject to breakage without notice, and should not be used")]
    internal interface ILegacyQuickInfoSource : IAsyncQuickInfoSource
    {
        /// <summary>
        /// This interface supports the product infrastructure and should not be used.
        /// </summary>
        void AugmentQuickInfoSession(IAsyncQuickInfoSession session, IList<object> content, out ITrackingSpan applicableToSpan);
    }
#pragma warning restore 618
}
