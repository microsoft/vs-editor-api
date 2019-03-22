namespace Microsoft.VisualStudio.Language.Intellisense.Implementation
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Internal.VisualStudio.Language.Intellisense;
    using Microsoft.VisualStudio.Text;

#pragma warning disable 618
    internal partial class AsyncQuickInfoSession : ILegacyQuickInfoRecalculateSupport
    {
        #region ILegacyQuickInfoRefreshSupport

        // Bug #512117: Remove compatibility shims for 2nd gen. Quick Info APIs.
        public void Recalculate()
        {
            this.JoinableTaskContext.Factory.Run(async delegate
            {
                await this.UpdateAsync(allowUpdate: true, cancellationToken: CancellationToken.None).ConfigureAwait(true);
            });
        }

        #endregion

        // Bug #512117: Remove compatibility shims for 2nd gen. Quick Info APIs.
        private async Task<bool> TryComputeContentFromLegacySourceAsync(
            IAsyncQuickInfoSource source,
            IList<object> items,
            IList<ITrackingSpan> applicableToSpans)
        {
            if (source is ILegacyQuickInfoSource legacySource)
            {
#pragma warning restore 618

                // Legacy sources expect to be on the UI thread.
                await this.JoinableTaskContext.Factory.SwitchToMainThreadAsync();

                legacySource.AugmentQuickInfoSession(this, items, out var applicableToSpan);

                if (applicableToSpan != null)
                {
                    applicableToSpans.Add(applicableToSpan);
                }

                return true;
            }

            return false;
        }
    }
}
