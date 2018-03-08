using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Text.Utilities;
using Microsoft.VisualStudio.Threading;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.Language.Intellisense.Implementation
{
    [Export(typeof(IAsyncCompletionBroker))]
    internal class AsyncCompletionBroker : IAsyncCompletionBroker
    {
        [Import(AllowDefault = true)]
        private ILoggingServiceInternal Logger;

        [Import]
        private IGuardedOperations GuardedOperations;

        [Import]
        private JoinableTaskContext JoinableTaskContext;

        [Import]
        private IContentTypeRegistryService ContentTypeRegistryService;

        // Used exclusively for legacy telemetry
        [Import(AllowDefault = true)]
        private ITextDocumentFactoryService TextDocumentFactoryService;

        [ImportMany]
        private IEnumerable<Lazy<ICompletionPresenterProvider, IOrderableContentTypeMetadata>> UnorderedPresenterProviders;

        [ImportMany]
        private IEnumerable<Lazy<IAsyncCompletionItemSourceProvider, IOrderableContentTypeMetadata>> UnorderedCompletionItemSourceProviders;

        [ImportMany]
        private IEnumerable<Lazy<IAsyncCompletionServiceProvider, IOrderableContentTypeMetadata>> UnorderedCompletionServiceProviders;

        private IList<Lazy<ICompletionPresenterProvider, IOrderableContentTypeMetadata>> _orderedPresenterProviders;
        private IList<Lazy<ICompletionPresenterProvider, IOrderableContentTypeMetadata>> OrderedPresenterProviders
            => _orderedPresenterProviders ?? (_orderedPresenterProviders = Orderer.Order(UnorderedPresenterProviders));

        private IList<Lazy<IAsyncCompletionItemSourceProvider, IOrderableContentTypeMetadata>> _orderedCompletionItemSourceProviders;
        private IList<Lazy<IAsyncCompletionItemSourceProvider, IOrderableContentTypeMetadata>> OrderedCompletionItemSourceProviders
            => _orderedCompletionItemSourceProviders ?? (_orderedCompletionItemSourceProviders = Orderer.Order(UnorderedCompletionItemSourceProviders));

        private IList<Lazy<IAsyncCompletionServiceProvider, IOrderableContentTypeMetadata>> _orderedCompletionServiceProviders;
        private IList<Lazy<IAsyncCompletionServiceProvider, IOrderableContentTypeMetadata>> OrderedCompletionServiceProviders
            => _orderedCompletionServiceProviders ?? (_orderedCompletionServiceProviders = Orderer.Order(UnorderedCompletionServiceProviders));

        private ImmutableDictionary<IContentType, ImmutableSortedSet<char>> _commitCharacters = ImmutableDictionary<IContentType, ImmutableSortedSet<char>>.Empty;
        private ImmutableDictionary<IContentType, ImmutableArray<IAsyncCompletionItemSourceProvider>> _cachedCompletionItemSourceProviders = ImmutableDictionary<IContentType, ImmutableArray<IAsyncCompletionItemSourceProvider>>.Empty;
        private ImmutableDictionary<IContentType, ImmutableArray<IAsyncCompletionServiceProvider>> _cachedCompletionServiceProviders = ImmutableDictionary<IContentType, ImmutableArray<IAsyncCompletionServiceProvider>>.Empty;
        private ImmutableDictionary<IContentType, ICompletionPresenterProvider> _cachedPresenterProviders = ImmutableDictionary<IContentType, ICompletionPresenterProvider>.Empty;
        private bool firstRun = true; // used only for diagnostics
        private bool _firstInvocationReported; // used for "time to code"
        private StableContentTypeComparer _contentTypeComparer;
        private const string IsCompletionAvailableProperty = "IsCompletionAvailable";

        private Dictionary<IContentType, bool> FeatureAvailabilityByContentType = new Dictionary<IContentType, bool>();

        bool IAsyncCompletionBroker.IsCompletionSupported(IContentType contentType)
        {
            bool featureIsAvailable;
            if (FeatureAvailabilityByContentType.TryGetValue(contentType, out featureIsAvailable))
            {
                return featureIsAvailable;
            }

            featureIsAvailable = UnorderedCompletionItemSourceProviders
                    .Any(n => n.Metadata.ContentTypes.Any(ct => contentType.IsOfType(ct)));
            featureIsAvailable &= UnorderedCompletionServiceProviders
                    .Any(n => n.Metadata.ContentTypes.Any(ct => contentType.IsOfType(ct)));

            FeatureAvailabilityByContentType[contentType] = featureIsAvailable;
            return featureIsAvailable;
        }

        IAsyncCompletionSession IAsyncCompletionBroker.TriggerCompletion(ITextView textView, SnapshotPoint triggerLocation, char typedChar)
        {
            var session = GetSession(textView);
            if (session != null)
            {
                return session;
            }

            var sourcesWithData = MetadataUtilities<IAsyncCompletionItemSourceProvider>.GetBuffersAndImports(textView, triggerLocation, GetCompletionItemSourceProviders);
            var cachedData = new CompletionSourcesWithData(sourcesWithData);
            foreach (var sourceWithData in sourcesWithData)
            {
                var sourceProvider = GuardedOperations.InstantiateExtension(this, sourceWithData.import); // TODO: consider caching this
                var source = sourceProvider.GetOrCreate(textView);
                var candidateSpan = GuardedOperations.CallExtensionPoint(
                    errorSource: source,
                    call: () => source.ShouldTriggerCompletion(typedChar, sourceWithData.point),
                    valueOnThrow: null
                );

                if (candidateSpan.HasValue)
                {
                    var mappingSpan = textView.BufferGraph.CreateMappingSpan(candidateSpan.Value, SpanTrackingMode.EdgeInclusive);
                    var applicableSpan = mappingSpan.GetSpans(textView.TextBuffer)[0];
                    return TriggerCompletion(textView, triggerLocation, applicableSpan, cachedData);
                }
            }
            return null;
        }

        private IAsyncCompletionSession TriggerCompletion(ITextView textView, SnapshotPoint triggerLocation, SnapshotSpan applicableSpan, CompletionSourcesWithData sources)
        {
            var session = GetSession(textView);
            if (session != null)
            {
                return session;
            }

            if (!sources.Data.Any())
            {
                // There is no completion source available for this buffer
                return null;
            }

            //var sourcesWithLocations = new Dictionary<IAsyncCompletionItemSource, SnapshotPoint>();
            var potentialCommitCharsBuilder = ImmutableArray.CreateBuilder<char>();
            var sourcesWithLocations = new Dictionary<IAsyncCompletionItemSource, SnapshotPoint>();
            foreach (var sourceWithData in sources.Data)
            {
                var sourceProvider = GuardedOperations.InstantiateExtension(this, sourceWithData.import); // TODO: consider caching this
                GuardedOperations.CallExtensionPoint(
                    errorSource: sourceProvider,
                    call: () =>
                    {
                        var source = sourceProvider.GetOrCreate(textView);
                        potentialCommitCharsBuilder.AddRange(source.GetPotentialCommitCharacters());
                        sourcesWithLocations[source] = sourceWithData.point;
                    });
            }

            if (_contentTypeComparer == null)
                _contentTypeComparer = new StableContentTypeComparer(ContentTypeRegistryService);

            var servicesWithLocations = MetadataUtilities<IAsyncCompletionServiceProvider>.GetOrderedBuffersAndImports(textView, triggerLocation, GetServiceProviders, _contentTypeComparer);
            var bestServiceWithData = servicesWithLocations.FirstOrDefault();
            var serviceProvider = GuardedOperations.InstantiateExtension(this, bestServiceWithData.import); // TODO: consider caching this
            var service = GuardedOperations.CallExtensionPoint(serviceProvider, () => serviceProvider.GetOrCreate(textView), null);
            if (service == null)
            {
                // This should never happen because we provide a default and IsCompletionFeatureAvailable would have returned false 
                throw new InvalidOperationException("No completion services not found. Completion will be unavailable.");
            }

            var presentationProvidersWithLocations = MetadataUtilities<ICompletionPresenterProvider>.GetOrderedBuffersAndImports(textView, triggerLocation, GetPresenters, _contentTypeComparer);
            var bestPresentationProviderWithLocation = presentationProvidersWithLocations.FirstOrDefault();
            var presenterProvider = GuardedOperations.InstantiateExtension(this, bestPresentationProviderWithLocation.import); // TODO: consider caching this

            if (firstRun)
            {
                System.Diagnostics.Debug.Assert(presenterProvider != null, $"No instance of {nameof(ICompletionPresenterProvider)} is loaded. Completion will work without the UI.");
                firstRun = false;
            }
            var telemetry = GetOrCreateTelemetry(textView);

            session = new AsyncCompletionSession(applicableSpan, potentialCommitCharsBuilder.ToImmutable(), JoinableTaskContext.Factory, presenterProvider, sourcesWithLocations, service, this, textView, telemetry, GuardedOperations);
            textView.Properties.AddProperty(typeof(IAsyncCompletionSession), session);
            textView.Closed += TextView_Closed;

            // Additionally, emulate the legacy completion telemetry
            EmulateLegacyCompletionTelemetry(bestServiceWithData.buffer?.ContentType, textView);

            return session;
        }

        private IReadOnlyList<Lazy<IAsyncCompletionItemSourceProvider, IOrderableContentTypeMetadata>> GetCompletionItemSourceProviders(IContentType contentType, ITextViewRoleSet textViewRoles)
        {
            // TODO: Respect the text view roles
            return OrderedCompletionItemSourceProviders.Where(n => n.Metadata.ContentTypes.Any(c => contentType.IsOfType(c))).ToList();
        }
        private IReadOnlyList<Lazy<IAsyncCompletionServiceProvider, IOrderableContentTypeMetadata>> GetServiceProviders(IContentType contentType, ITextViewRoleSet textViewRoles)
        {
            // TODO: Respect the text view roles
            return OrderedCompletionServiceProviders.Where(n => n.Metadata.ContentTypes.Any(c => contentType.IsOfType(c))).OrderBy(n => n.Metadata.ContentTypes, _contentTypeComparer).ToList();
        }
        private IReadOnlyList<Lazy<ICompletionPresenterProvider, IOrderableContentTypeMetadata>> GetPresenters(IContentType contentType, ITextViewRoleSet textViewRoles)
        {
            // TODO: Respect the text view roles
            return OrderedPresenterProviders.Where(n => n.Metadata.ContentTypes.Any(c => contentType.IsOfType(c))).OrderBy(n => n.Metadata.ContentTypes, _contentTypeComparer).ToList();
        }

        // TODO: Evaluate the methods below and clean them up. We should have one reliable way to get parts in correct order
        private ImmutableArray<IAsyncCompletionItemSourceProvider> GetCompletionItemSourceProviders(IContentType contentType)
        {
            if (_cachedCompletionItemSourceProviders.TryGetValue(contentType, out var cachedSourceProviders))
            {
                return cachedSourceProviders;
            }

            var providers = GuardedOperations.InvokeMatchingFactories(
                lazyFactories: OrderedCompletionItemSourceProviders,
                getter: n => n,
                dataContentType: contentType,
                errorSource: this);

            var result = providers.ToImmutableArray();
            _cachedCompletionItemSourceProviders = _cachedCompletionItemSourceProviders.Add(contentType, result);
            return result;
        }

        private ImmutableArray<IAsyncCompletionServiceProvider> GetCompletionServiceProviders(IContentType contentType)
        {
            if (_cachedCompletionServiceProviders.TryGetValue(contentType, out var serviceProvider))
            {
                return serviceProvider;
            }

            var providers = GuardedOperations.InvokeMatchingFactories(
                lazyFactories: OrderedCompletionServiceProviders,
                getter: n => n,
                dataContentType: contentType,
                errorSource: this);

            var result = providers.ToImmutableArray();
            _cachedCompletionServiceProviders = _cachedCompletionServiceProviders.Add(contentType, result);
            return result;
        }

        private ICompletionPresenterProvider GetUiFactory(IContentType contentType)
        {
            if (_cachedPresenterProviders.TryGetValue(contentType, out var factory))
            {
                return factory;
            }

            ICompletionPresenterProvider bestFactory = GuardedOperations.InvokeBestMatchingFactory(
                providerHandles: OrderedPresenterProviders,
                getter: n => n,
                dataContentType: contentType,
                contentTypeRegistryService: ContentTypeRegistryService,
                errorSource: this);

            _cachedPresenterProviders = _cachedPresenterProviders.Add(contentType, bestFactory);
            return bestFactory;
        }

        internal bool TryGetKnownCommitCharacters(IContentType contentType, ITextView view, out ImmutableSortedSet<char> commitChars)
        {
            if (_commitCharacters.TryGetValue(contentType, out commitChars))
            {
                return commitChars.Any();
            }
            var allCommitChars = new List<char>();
            foreach (var source in
                GetCompletionItemSourceProviders(contentType)
                    .Select(n => n.GetOrCreate(view)))
            {
                GuardedOperations.CallExtensionPoint(
                    errorSource: source,
                    call: () => allCommitChars.AddRange(source.GetPotentialCommitCharacters())
                );
            }
            commitChars = ImmutableSortedSet.CreateRange(allCommitChars);
            _commitCharacters = _commitCharacters.Add(contentType, commitChars);
            return commitChars.Any();
        }

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

        bool IAsyncCompletionBroker.IsCompletionActive(ITextView textView)
        {
            return textView.Properties.ContainsProperty(typeof(IAsyncCompletionSession));
        }

        public IAsyncCompletionSession GetSession(ITextView textView)
        {
            if (textView.Properties.TryGetProperty(typeof(IAsyncCompletionSession), out IAsyncCompletionSession session))
            {
                return session;
            }
            return null;
        }

        /// <summary>
        /// This method is used by <see cref="IAsyncCompletionSession"/> to inform the broker that it should forget about the session. Used when dismissing.
        /// This method does not dismiss the session!
        /// </summary>
        /// <param name="session">Session being dismissed</param>
        internal void ForgetSession(IAsyncCompletionSession session)
        {
            session.TextView.Properties.RemoveProperty(typeof(IAsyncCompletionSession));
        }

        /// <summary>
        /// Wrapper around complex parameters. This is a candidate for refactoring.
        /// </summary>
        private struct CompletionSourcesWithData
        {
            internal IEnumerable<(ITextBuffer buffer, SnapshotPoint point, Lazy<IAsyncCompletionItemSourceProvider, IOrderableContentTypeMetadata> import)> Data;

            public CompletionSourcesWithData(IEnumerable<(ITextBuffer buffer, SnapshotPoint point, Lazy<IAsyncCompletionItemSourceProvider, IOrderableContentTypeMetadata> import)> data)
            {
                Data = data;
            }
        }

        // Helper methods for telemetry:
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

        private void SendTelemetry(ITextView textView)
        {
            if (textView.Properties.TryGetProperty(typeof(CompletionTelemetryHost), out CompletionTelemetryHost telemetry))
            {
                telemetry.Send();
                textView.Properties.RemoveProperty(typeof(CompletionTelemetryHost));
            }
        }

        internal string GetItemSourceName(IAsyncCompletionItemSource source) => OrderedCompletionItemSourceProviders.FirstOrDefault(n => n.IsValueCreated && n.Value == source)?.Metadata.Name ?? string.Empty;
        internal string GetCompletionServiceName(IAsyncCompletionService service) => OrderedCompletionServiceProviders.FirstOrDefault(n => n.IsValueCreated && n.Value == service)?.Metadata.Name ?? string.Empty;
        internal string GetCompletionPresenterProviderName(ICompletionPresenterProvider provider) => OrderedPresenterProviders.FirstOrDefault(n => n.IsValueCreated && n.Value == provider)?.Metadata.Name ?? string.Empty;

        // Parity with legacy telemetry
        private void EmulateLegacyCompletionTelemetry(IContentType contentType, ITextView textView)
        {
            if (Logger == null || _firstInvocationReported)
                return;

            string GetFileExtension(ITextBuffer buffer)
            {
                var documentFactoryService = TextDocumentFactoryService;
                if (buffer != null && documentFactoryService != null)
                {
                    ITextDocument currentDocument = null;
                    documentFactoryService.TryGetTextDocument(buffer, out currentDocument);
                    if (currentDocument != null && currentDocument.FilePath != null)
                    {
                        return System.IO.Path.GetExtension(currentDocument.FilePath);
                    }
                }
                return null;
            }
            var fileExtension = GetFileExtension(textView.TextBuffer) ?? "Unknown";
            var reportedContentType = contentType?.ToString() ?? "Unknown";

            _firstInvocationReported = true;
            Logger.PostEvent(TelemetryEventType.Operation, "VS/Editor/IntellisenseFirstRun/Opened", TelemetryResult.Success,
                ("VS.Editor.IntellisenseFirstRun.Opened.ContentType", reportedContentType),
                ("VS.Editor.IntellisenseFirstRun.Opened.FileExtension", fileExtension));
        }
    }
}
