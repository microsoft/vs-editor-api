namespace Microsoft.Internal.VisualStudio.Language.Intellisense
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.Language.Intellisense;
    using Microsoft.VisualStudio.Text;
    using Microsoft.VisualStudio.Text.Editor;
    using Microsoft.VisualStudio.Utilities;

    // Bug #512117: Remove compatibility shims for 2nd gen. Quick Info APIs.
#pragma warning disable 618
    /// <summary>
    /// This interface supports the product infrastructure and should not be used.
    /// </summary>
    [Obsolete("This interface supports legacy product infrastructure, is subject to breakage without notice, and should not be used")]
    internal interface ILegacyQuickInfoBrokerSupport : IAsyncQuickInfoBroker
    {
        /// <summary>
        /// This method supports the product infrastructure and should not be used.
        /// </summary>
        Task<IAsyncQuickInfoSession> TriggerQuickInfoAsync(
            ITextView textView,
            ITrackingPoint triggerPoint,
            QuickInfoSessionOptions options,
            PropertyCollection propertyCollection,
            CancellationToken cancellationToken);
    }
#pragma warning restore 618
}
