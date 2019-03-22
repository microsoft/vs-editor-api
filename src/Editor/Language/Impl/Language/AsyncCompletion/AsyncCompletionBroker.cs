using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion.Data;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Projection;
using Microsoft.VisualStudio.Text.Utilities;
using Microsoft.VisualStudio.Threading;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion.Implementation
{
    [Export(typeof(IAsyncCompletionBroker))]
    [Export(typeof(AsyncCompletionBroker))]
    internal sealed class AsyncCompletionBroker : IAsyncCompletionBroker
    {
        [Import]
        private IGuardedOperations GuardedOperations;

        [Import]
        private JoinableTaskContext JoinableTaskContext;

        [Import]
        private IContentTypeRegistryService ContentTypeRegistryService;

        [Import]
        private CompletionAvailabilityUtility CompletionAvailability;

        [ImportMany]
        private IEnumerable<Lazy<IAsyncCompletionSourceProvider, IOrderableContentTypeAndOptionalTextViewRoleMetadata>> UnorderedCompletionSourceProviders;

        [ImportMany]
        private IEnumerable<Lazy<IAsyncCompletionItemManagerProvider, IOrderableContentTypeAndOptionalTextViewRoleMetadata>> UnorderedCompletionItemManagerProviders;

        [ImportMany]
        private IEnumerable<Lazy<IAsyncCompletionCommitManagerProvider, IOrderableContentTypeAndOptionalTextViewRoleMetadata>> UnorderedCompletionCommitManagerProviders;

        [ImportMany]
        private IEnumerable<Lazy<ICompletionPresenterProvider, IOrderableContentTypeAndOptionalTextViewRoleMetadata>> UnorderedPresenterProviders;

        // Used for telemetry
        [Import(AllowDefault = true)]
        private ILoggingServiceInternal Logger;

        // Used for legacy telemetry
        [Import(AllowDefault = true)]
        private ITextDocumentFactoryService TextDocumentFactoryService;

        private IList<Lazy<IAsyncCompletionSourceProvider, IOrderableContentTypeAndOptionalTextViewRoleMetadata>> _orderedCompletionSourceProviders;
        private IList<Lazy<IAsyncCompletionSourceProvider, IOrderableContentTypeAndOptionalTextViewRoleMetadata>> OrderedCompletionSourceProviders
            => _orderedCompletionSourceProviders ?? (_orderedCompletionSourceProviders = Orderer.Order(UnorderedCompletionSourceProviders));

        private IList<Lazy<IAsyncCompletionItemManagerProvider, IOrderableContentTypeAndOptionalTextViewRoleMetadata>> _orderedCompletionItemManagerProviders;
        private IList<Lazy<IAsyncCompletionItemManagerProvider, IOrderableContentTypeAndOptionalTextViewRoleMetadata>> OrderedCompletionItemManagerProviders
            => _orderedCompletionItemManagerProviders ?? (_orderedCompletionItemManagerProviders = Orderer.Order(UnorderedCompletionItemManagerProviders));

        private IList<Lazy<IAsyncCompletionCommitManagerProvider, IOrderableContentTypeAndOptionalTextViewRoleMetadata>> _orderedCompletionCommitManagerProviders;
        private IList<Lazy<IAsyncCompletionCommitManagerProvider, IOrderableContentTypeAndOptionalTextViewRoleMetadata>> OrderedCompletionCommitManagerProviders
            => _orderedCompletionCommitManagerProviders ?? (_orderedCompletionCommitManagerProviders = Orderer.Order(UnorderedCompletionCommitManagerProviders));

        private IList<Lazy<ICompletionPresenterProvider, IOrderableContentTypeAndOptionalTextViewRoleMetadata>> _orderedPresenterProviders;
        private IList<Lazy<ICompletionPresenterProvider, IOrderableContentTypeAndOptionalTextViewRoleMetadata>> OrderedPresenterProviders
            => _orderedPresenterProviders ?? (_orderedPresenterProviders = Orderer.Order(UnorderedPresenterProviders));

        private bool firstRun = true; // used only for diagnostics
        private bool _firstInvocationReported; // used for "time to code"
        private StableContentTypeComparer _contentTypeComparer;
        private Dictionary<IContentType, bool> _providerAvailabilityByContentType = new Dictionary<IContentType, bool>();

        public event EventHandler<CompletionTriggeredEventArgs> CompletionTriggered;

        #region IAsyncCompletionBroker implementation

        public bool IsCompletionActive(ITextView textView)
        {
            return textView.Properties.ContainsProperty(typeof(IAsyncCompletionSession));
        }

        public bool IsCompletionSupported(IContentType contentType)
        {
            // This will call HasCompletionProviders among doing other checks
            return CompletionAvailability.IsAvailable(contentType);
        }

        /// <summary>
        /// Returns whether there exist any <see cref="IAsyncCompletionSourceProvider"/>
        /// for the provided <see cref="IContentType"/> or any of its base content types.
        /// Since MEF parts don't change on runtime, the answer is cached per <see cref="IContentType"/> for faster retrieval.
        /// </summary>
        internal bool HasCompletionProviders(IContentType contentType)
        {
            // Use cache if available
            if (_providerAvailabilityByContentType.TryGetValue(contentType, out bool featureIsAvailable))
                return featureIsAvailable;

            featureIsAvailable = UnorderedCompletionSourceProviders.Any(n => n.Metadata.ContentTypes.Any(ct => contentType.IsOfType(ct)));

            _providerAvailabilityByContentType[contentType] = featureIsAvailable;
            return featureIsAvailable;
        }

        public IAsyncCompletionSession GetSession(ITextView textView)
        {
            if (textView.Properties.TryGetProperty(typeof(IAsyncCompletionSession), out IAsyncCompletionSession session))
            {
                return session;
            }
            return null;
        }

        public IAsyncCompletionSession TriggerCompletion(ITextView textView, CompletionTrigger trigger, SnapshotPoint triggerLocation, CancellationToken token)
        {
            var session = GetSession(textView);
            if (session != null)
            {
                return session;
            }

            // This is a simple check that only queries the feature service.
            // If it succeeds, we will map triggerLocation to available buffers to discover MEF parts.
            // This is expensive but projected languages require it to discover parts in all available buffers.
            // To avoid doing this work, call IsCompletionSupported with appropriate IContentType prior to calling TriggerCompletion
            if (!CompletionAvailability.IsAvailable(textView, contentTypeToCheckBlacklist: triggerLocation.Snapshot.ContentType))
                return null;

            if (!JoinableTaskContext.IsOnMainThread)
                throw new InvalidOperationException($"This method must be callled on the UI thread.");

            var telemetryHost = GetOrCreateTelemetry(textView);
            var telemetry = new CompletionSessionTelemetry(telemetryHost);

            GetCommitManagersAndChars(textView.BufferGraph, textView.Roles, textView, triggerLocation, GetCommitManagerProviders, telemetry,
                out var managersWithBuffers, out var potentialCommitChars);

            GetCompletionSources(textView.TextBuffer, textView.Roles, textView, textView.BufferGraph, trigger, triggerLocation, GetItemSourceProviders, telemetry, token,
                out var sourcesWithLocations, out var applicableToSpan);

            // No source declared an appropriate ApplicableToSpan
            if (applicableToSpan == default)
                return null;
            // No source wishes to participate
            if (!sourcesWithLocations.Any())
                return null;
             
            if (_contentTypeComparer == null)
                _contentTypeComparer = new StableContentTypeComparer(ContentTypeRegistryService);

            var itemManager = GetItemManager(textView.BufferGraph, textView.Roles, textView, triggerLocation, GetItemManagerProviders, _contentTypeComparer);
            var presenterProvider = GetPresenterProvider(textView.BufferGraph, textView.Roles, triggerLocation, GetPresenters, _contentTypeComparer);

            session = new AsyncCompletionSession(applicableToSpan, potentialCommitChars, JoinableTaskContext, presenterProvider, sourcesWithLocations, managersWithBuffers, itemManager, this, textView, telemetry, GuardedOperations);
            textView.Properties.AddProperty(typeof(IAsyncCompletionSession), session);

            textView.Closed += TextView_Closed;
            EmulateLegacyCompletionTelemetry(textView);
            GuardedOperations.RaiseEvent(this, CompletionTriggered, new CompletionTriggeredEventArgs(session, textView));

            return session;
        }

        #endregion

        #region Internal communication with AsyncCompletionSession

        /// <summary>
        /// This method is used by <see cref="IAsyncCompletionSession"/> to inform the broker that it should forget about the session.
        /// Invoked as a result of dismissing. This method does not dismiss the session!
        /// </summary>
        /// <param name="session">Session being dismissed</param>
#pragma warning disable CA1822 // Member does not access instance data and can be marked as static
        internal void ForgetSession(IAsyncCompletionSession session)
        {
            session.TextView.Properties.RemoveProperty(typeof(IAsyncCompletionSession));
        }
#pragma warning restore CA1822

        #endregion

        #region MEF part helper methods

        private void GetCommitManagersAndChars(
            IBufferGraph bufferGraph,
            ITextViewRoleSet roles,
            ITextView textViewForGetOrCreate, /* This name conveys that we're using ITextView only to init the MEF part. this is subject to change. */
            SnapshotPoint triggerLocation,
            Func<IContentType, ITextViewRoleSet, IReadOnlyList<Lazy<IAsyncCompletionCommitManagerProvider, IOrderableContentTypeAndOptionalTextViewRoleMetadata>>> getImports,
            CompletionSessionTelemetry telemetry,
            out IList<(IAsyncCompletionCommitManager, ITextBuffer)> managersWithBuffers,
            out ImmutableArray<char> potentialCommitChars)
        {
            var commitManagersWithData = MetadataUtilities<IAsyncCompletionCommitManagerProvider, IOrderableContentTypeAndOptionalTextViewRoleMetadata>
                .GetBuffersAndImports(bufferGraph, roles, triggerLocation, getImports);

            var potentialCommitCharsBuilder = ImmutableArray.CreateBuilder<char>();
            managersWithBuffers = new List<(IAsyncCompletionCommitManager, ITextBuffer)>(1);
            foreach (var (buffer, point, import) in commitManagersWithData)
            {
                telemetry.UiStopwatch.Restart();
                var managerProvider = GuardedOperations.InstantiateExtension(this, import);
                var manager = GuardedOperations.CallExtensionPoint(
                    errorSource: managerProvider,
                    call: () => managerProvider.GetOrCreate(textViewForGetOrCreate),
                    valueOnThrow: null);

                if (manager == null)
                    continue;

                GuardedOperations.CallExtensionPoint(
                    errorSource: manager,
                    call: () =>
                    {
                        var characters = manager.PotentialCommitCharacters;
                        potentialCommitCharsBuilder.AddRange(characters);
                    });
                managersWithBuffers.Add((manager, buffer));
                telemetry.UiStopwatch.Stop();
                telemetry.RecordObtainingCommitManagerData(manager, telemetry.UiStopwatch.ElapsedMilliseconds);
            }
            potentialCommitChars = potentialCommitCharsBuilder.ToImmutable();
        }

        private void GetCompletionSources(
            ITextBuffer editBuffer,
            ITextViewRoleSet roles,
            ITextView textViewForGetOrCreate, /* This name conveys that we're supposed to use ITextView only to init the MEF part. this is subject to change. */
            IBufferGraph bufferGraph,
            CompletionTrigger trigger,
            SnapshotPoint triggerLocation,
            Func<IContentType, ITextViewRoleSet, IReadOnlyList<Lazy<IAsyncCompletionSourceProvider, IOrderableContentTypeAndOptionalTextViewRoleMetadata>>> getImports,
            CompletionSessionTelemetry telemetry,
            CancellationToken token,
            out List<(IAsyncCompletionSource Source, SnapshotPoint Point)> sourcesWithLocations,
            out SnapshotSpan applicableToSpan)
        {
            var sourcesWithData = MetadataUtilities<IAsyncCompletionSourceProvider, IOrderableContentTypeAndOptionalTextViewRoleMetadata>
                .GetBuffersAndImports(bufferGraph, roles, triggerLocation, getImports);

            var applicableToSpanBuilder = default(SnapshotSpan);
            bool applicableToSpanExists = false;
            bool anySourceParticipates = false;
            bool anySourceExclusive = false;
            var sourcesWithLocationsBuider = new List<(IAsyncCompletionSource, SnapshotPoint, CompletionParticipation)>(sourcesWithData.Count());

            foreach (var (buffer, point, import) in sourcesWithData)
            {
                telemetry.UiStopwatch.Restart();

                var sourceProvider = GuardedOperations.InstantiateExtension(this, import);
                var source = GuardedOperations.CallExtensionPoint(
                    errorSource: sourceProvider,
                    call: () => sourceProvider.GetOrCreate(textViewForGetOrCreate),
                    valueOnThrow: null);

                if (source == null)
                    continue;

                GuardedOperations.CallExtensionPoint(
                    errorSource: source,
                    call: () =>
                    {
                        // Iterate through all sources and add them to collection
                        CompletionStartData startData;
                        startData = source.InitializeCompletion(trigger, point, token);

                        if (!applicableToSpanExists && startData.ApplicableToSpan != default)
                        {
                            applicableToSpanExists = true;
                            applicableToSpanBuilder = startData.ApplicableToSpan;
                        }
                        if (startData.Participation == CompletionParticipation.ProvidesItems)
                        {
                            anySourceParticipates = true;
                        }
                        else if (startData.Participation == CompletionParticipation.ExclusivelyProvidesItems)
                        {
                            anySourceParticipates = true;
                            anySourceExclusive = true;
                        }
                        sourcesWithLocationsBuider.Add((source, point, startData.Participation));
                    });

                telemetry.UiStopwatch.Stop();
                telemetry.RecordObtainingSourceSpan(source, telemetry.UiStopwatch.ElapsedMilliseconds);
            }

            // Map the applicable to span to the view's edit buffer and use it for completion,
            if (applicableToSpanExists)
            {
                var mappingSpan = bufferGraph.CreateMappingSpan(applicableToSpanBuilder, SpanTrackingMode.EdgeInclusive);
                applicableToSpanBuilder = mappingSpan.GetSpans(editBuffer)[0];
            }

            // Copying temporary values because we can't access out&ref params in lambdas
            if (anySourceExclusive)
            {
                sourcesWithLocations = sourcesWithLocationsBuider.Where(n => n.Item3 == CompletionParticipation.ExclusivelyProvidesItems).Select(n => (n.Item1, n.Item2)).ToList();
            }
            else if (anySourceParticipates)
            {
                sourcesWithLocations = sourcesWithLocationsBuider.Where(n => n.Item3 == CompletionParticipation.ProvidesItems).Select(n => (n.Item1, n.Item2)).ToList();
            }
            else
            {
                sourcesWithLocations = new List<(IAsyncCompletionSource Source, SnapshotPoint Point)>();
            }
            applicableToSpan = applicableToSpanBuilder;
        }

        private IAsyncCompletionItemManager GetItemManager(
            IBufferGraph bufferGraph,
            ITextViewRoleSet textViewRoles,
            ITextView textViewForGetOrCreate, /* This name conveys that we're using ITextView only to init the MEF part. this is subject to change. */
            SnapshotPoint triggerLocation,
            Func<IContentType, ITextViewRoleSet, IReadOnlyList<Lazy<IAsyncCompletionItemManagerProvider, IOrderableContentTypeAndOptionalTextViewRoleMetadata>>> getImports,
            StableContentTypeComparer contentTypeComparer
            )
        {
            var itemManagerProvidersWithData = MetadataUtilities<IAsyncCompletionItemManagerProvider, IOrderableContentTypeAndOptionalTextViewRoleMetadata>
                .GetOrderedBuffersAndImports(bufferGraph, textViewRoles, triggerLocation, getImports, contentTypeComparer);
            if (!itemManagerProvidersWithData.Any())
            {
                // This should never happen because we provide a default and IsCompletionFeatureAvailable would have returned false 
                throw new InvalidOperationException("No completion services not found. Completion will be unavailable.");
            }

            var bestItemManagerProvider = GuardedOperations.InstantiateExtension(this, itemManagerProvidersWithData.First().import);
            return GuardedOperations.CallExtensionPoint(bestItemManagerProvider, () => bestItemManagerProvider.GetOrCreate(textViewForGetOrCreate), null);
        }

        private ICompletionPresenterProvider GetPresenterProvider(
            IBufferGraph bufferGraph,
            ITextViewRoleSet textViewRoles,
            SnapshotPoint triggerLocation,
            Func<IContentType, ITextViewRoleSet, IReadOnlyList<Lazy<ICompletionPresenterProvider, IOrderableContentTypeAndOptionalTextViewRoleMetadata>>> getImports,
            StableContentTypeComparer contentTypeComparer)
        {
            var presenterProvidersWithData = MetadataUtilities<ICompletionPresenterProvider, IOrderableContentTypeAndOptionalTextViewRoleMetadata>
                .GetOrderedBuffersAndImports(bufferGraph, textViewRoles, triggerLocation, getImports, contentTypeComparer);
            ICompletionPresenterProvider presenterProvider = null;
            if (presenterProvidersWithData.Any())
                presenterProvider = GuardedOperations.InstantiateExtension(this, presenterProvidersWithData.First().import);

            if (firstRun)
            {
                System.Diagnostics.Debug.Assert(presenterProvider != null, $"No instance of {nameof(ICompletionPresenterProvider)} is loaded. Completion will work without the UI.");
                firstRun = false;
            }

            return presenterProvider;
        }

        private IReadOnlyList<Lazy<IAsyncCompletionSourceProvider, IOrderableContentTypeAndOptionalTextViewRoleMetadata>> GetItemSourceProviders(IContentType contentType, ITextViewRoleSet textViewRoles)
        {
            return OrderedCompletionSourceProviders.Where(n => n.Metadata.ContentTypes.Any(c => contentType.IsOfType(c)) && (n.Metadata.TextViewRoles == null || textViewRoles.ContainsAny(n.Metadata.TextViewRoles))).ToList();
        }

        private IReadOnlyList<Lazy<IAsyncCompletionItemManagerProvider, IOrderableContentTypeAndOptionalTextViewRoleMetadata>> GetItemManagerProviders(IContentType contentType, ITextViewRoleSet textViewRoles)
        {
            return OrderedCompletionItemManagerProviders.Where(n => n.Metadata.ContentTypes.Any(c => contentType.IsOfType(c)) && (n.Metadata.TextViewRoles == null || textViewRoles.ContainsAny(n.Metadata.TextViewRoles))).OrderBy(n => n.Metadata.ContentTypes, _contentTypeComparer).ToList();
        }

        private IReadOnlyList<Lazy<IAsyncCompletionCommitManagerProvider, IOrderableContentTypeAndOptionalTextViewRoleMetadata>> GetCommitManagerProviders(IContentType contentType, ITextViewRoleSet textViewRoles)
        {
            return OrderedCompletionCommitManagerProviders.Where(n => n.Metadata.ContentTypes.Any(c => contentType.IsOfType(c)) && (n.Metadata.TextViewRoles == null || textViewRoles.ContainsAny(n.Metadata.TextViewRoles))).ToList();
        }

        private IReadOnlyList<Lazy<ICompletionPresenterProvider, IOrderableContentTypeAndOptionalTextViewRoleMetadata>> GetPresenters(IContentType contentType, ITextViewRoleSet textViewRoles)
        {
            return OrderedPresenterProviders.Where(n => n.Metadata.ContentTypes.Any(c => contentType.IsOfType(c)) && (n.Metadata.TextViewRoles == null || textViewRoles.ContainsAny(n.Metadata.TextViewRoles))).OrderBy(n => n.Metadata.ContentTypes, _contentTypeComparer).ToList();
        }

        #endregion

        #region Telemetry

        private CompletionTelemetryHost GetOrCreateTelemetry(ITextView textView)
        {
            if (textView.Properties.TryGetProperty(typeof(CompletionTelemetryHost), out CompletionTelemetryHost telemetry))
            {
                return telemetry;
            }
            else
            {
                var newTelemetry = new CompletionTelemetryHost(Logger, this);
                textView.Properties.AddProperty(typeof(CompletionTelemetryHost), newTelemetry);
                return newTelemetry;
            }
        }

#pragma warning disable CA1822 // Member does not access instance data and can be marked as static
        private static void SendTelemetry(ITextView textView)
        {
            if (textView.Properties.TryGetProperty(typeof(CompletionTelemetryHost), out CompletionTelemetryHost telemetry))
            {
                telemetry.Send();
                textView.Properties.RemoveProperty(typeof(CompletionTelemetryHost));
            }
        }
#pragma warning restore CA1822

        // Parity with legacy telemetry
        private void EmulateLegacyCompletionTelemetry(ITextView textView)
        {
            if (Logger == null || _firstInvocationReported)
                return;

            string GetFileExtension(ITextBuffer buffer)
            {
                var documentFactoryService = TextDocumentFactoryService;
                if (buffer != null && documentFactoryService != null)
                {
                    documentFactoryService.TryGetTextDocument(buffer, out ITextDocument currentDocument);
                    if (currentDocument != null && currentDocument.FilePath != null)
                    {
                        return System.IO.Path.GetExtension(currentDocument.FilePath);
                    }
                }
                return null;
            }
            var fileExtension = GetFileExtension(textView.TextBuffer) ?? "Unknown";
            var reportedContentType = textView.TextBuffer.ContentType?.ToString() ?? "Unknown";

            _firstInvocationReported = true;
            Logger.PostEvent(TelemetryEventType.Operation, "VS/Editor/IntellisenseFirstRun/Opened", TelemetryResult.Success,
                ("VS.Editor.IntellisenseFirstRun.Opened.ContentType", reportedContentType),
                ("VS.Editor.IntellisenseFirstRun.Opened.FileExtension", fileExtension));
        }

        #endregion

        private void TextView_Closed(object sender, EventArgs e)
        {
            var view = (ITextView)sender;
            view.Closed -= TextView_Closed;
            GetSession(view)?.Dismiss();
            try
            {
                SendTelemetry(view);
            }
            catch (Exception ex)
            {
                GuardedOperations.HandleException(this, ex);
            }
        }
    }
}
