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
        internal ILoggingServiceInternal Logger { get; set; }

        // This may be used to GetExtentOfWord, but it doesn't fully work yet, so we're not using it.
        [Import(AllowDefault = true)]
        internal ITextDocumentFactoryService TextDocumentFactoryService { get; set; }

        [Import]
        internal IGuardedOperations GuardedOperations { get; set; }

        [Import]
        internal ITextStructureNavigatorSelectorService TextStructureNavigatorSelectorService { get; set; }

        [Import]
        private JoinableTaskContext JtContext;

        [Import]
        private IContentTypeRegistryService ContentTypeRegistryService;

        [ImportMany]
        private IEnumerable<Lazy<ICompletionPresenterProvider, IOrderableContentTypeMetadata>> UnorderedPresenterProviders;

        [ImportMany]
        private IEnumerable<Lazy<IAsyncCompletionItemSourceProvider, IOrderableContentTypeMetadata>> UnorderedCompletionItemSourceProviders;

        [ImportMany]
        private IEnumerable<Lazy<IAsyncCompletionServiceProvider, IOrderableContentTypeMetadata>> UnorderedCompletionServiceProviders;

        private IList<Lazy<ICompletionPresenterProvider, IOrderableContentTypeMetadata>> _orderedPresenterProviders;
        internal IList<Lazy<ICompletionPresenterProvider, IOrderableContentTypeMetadata>> OrderedPresenterProviders
            => _orderedPresenterProviders ?? (_orderedPresenterProviders = Orderer.Order(UnorderedPresenterProviders));

        private IList<Lazy<IAsyncCompletionItemSourceProvider, IOrderableContentTypeMetadata>> _orderedCompletionItemSourceProviders;
        internal IList<Lazy<IAsyncCompletionItemSourceProvider, IOrderableContentTypeMetadata>> OrderedCompletionItemSourceProviders
            => _orderedCompletionItemSourceProviders ?? (_orderedCompletionItemSourceProviders = Orderer.Order(UnorderedCompletionItemSourceProviders));

        private IList<Lazy<IAsyncCompletionServiceProvider, IOrderableContentTypeMetadata>> _orderedCompletionServiceProviders;
        internal IList<Lazy<IAsyncCompletionServiceProvider, IOrderableContentTypeMetadata>> OrderedCompletionServiceProviders
            => _orderedCompletionServiceProviders ?? (_orderedCompletionServiceProviders = Orderer.Order(UnorderedCompletionServiceProviders));

        private ImmutableDictionary<IContentType, ImmutableSortedSet<char>> _commitCharacters = ImmutableDictionary<IContentType, ImmutableSortedSet<char>>.Empty;
        private ImmutableDictionary<IContentType, ImmutableArray<IAsyncCompletionItemSourceProvider>> _cachedCompletionItemSourceProviders = ImmutableDictionary<IContentType, ImmutableArray<IAsyncCompletionItemSourceProvider>>.Empty;
        private ImmutableDictionary<IContentType, IAsyncCompletionServiceProvider> _cachedCompletionServiceProviders = ImmutableDictionary<IContentType, IAsyncCompletionServiceProvider>.Empty;
        private ImmutableDictionary<IContentType, ICompletionPresenterProvider> _cachedPresenterProviders = ImmutableDictionary<IContentType, ICompletionPresenterProvider>.Empty;
        private bool firstRun = true; // used only for diagnostics
        private bool _firstInvocationReported; // used for "time to code"

        internal void DismissSession(IAsyncCompletionSession session)
        {
            session.TextView.Properties.RemoveProperty(typeof(IAsyncCompletionSession));
        }

        IAsyncCompletionSession IAsyncCompletionBroker.TriggerCompletion(ITextView view, SnapshotPoint triggerLocation)
        {
            var session = GetSession(view);
            if (session != null)
            {
                return session;
            }

            // TODO: Handle the race condition: two consecutive OpenAsync. Both create completion
            // not knowing of one another. The second invocation should use the if-block.

            var sourcesWithLocations = CompletionUtilities.GetCompletionSourcesWithMappedLocations(view, triggerLocation, GetCompletionItemSourceProviders);
            if (!sourcesWithLocations.Any())
            {
                // There is no completion source available for this buffer
                return null;
            }

            var buffers = CompletionUtilities.GetBuffersForTriggerPoint(view, triggerLocation).ToImmutableArray();
            var service = buffers
                .Select(b => GetCompletionService(b.ContentType))
                .FirstOrDefault(s => s != null)
                ?.GetOrCreate(view);

            if (service == null)
            {
                // This should never happen because we provide a default and IsCompletionFeatureAvailable would have returned false 
                throw new InvalidOperationException("Default completion service was not found. Completion will be unavailable.");
            }

            var uiFactory = buffers
                .Select(n => GetUiFactory(n.ContentType))
                .FirstOrDefault(n => n != null);

            if (firstRun)
            {
                System.Diagnostics.Debug.Assert(uiFactory != null, $"No instance of {nameof(ICompletionPresenterProvider)} is loaded. Completion will work without the UI.");
                firstRun = false;
            }
            session = new AsyncCompletionSession(JtContext.Factory, uiFactory, sourcesWithLocations, service, this, view);
            view.Properties.AddProperty(typeof(IAsyncCompletionSession), session);
            view.Closed += TextView_Closed;

            // Additionally, emulate the legacy completion telemetry
            EmulateLegacyCompletionTelemetry(buffers.FirstOrDefault()?.ContentType, view);

            return session;
        }

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

        private IAsyncCompletionServiceProvider GetCompletionService(IContentType contentType)
        {
            if (_cachedCompletionServiceProviders.TryGetValue(contentType, out var serviceProvider))
            {
                return serviceProvider;
            }

            IAsyncCompletionServiceProvider bestServiceProvider = GuardedOperations.InvokeBestMatchingFactory(
                providerHandles: OrderedCompletionServiceProviders,
                getter: n => n,
                dataContentType: contentType,
                contentTypeRegistryService: ContentTypeRegistryService,
                errorSource: this);

            _cachedCompletionServiceProviders = _cachedCompletionServiceProviders.Add(contentType, bestServiceProvider);
            return bestServiceProvider;
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
            GetSession((ITextView)sender)?.Dismiss();
            // TODO: confirm that it's indeed ok to not unsusbcribe
        }

        bool IAsyncCompletionBroker.IsCompletionActive(ITextView view)
        {
            return view.Properties.ContainsProperty(typeof(IAsyncCompletionSession));
        }

        bool IAsyncCompletionBroker.ShouldTriggerCompletion(ITextView textView, char typeChar, SnapshotPoint triggerLocation)
        {
            var sourcesWithLocations = CompletionUtilities.GetCompletionSourcesWithMappedLocations(textView, triggerLocation, GetCompletionItemSourceProviders);
            return sourcesWithLocations.Any(p => GuardedOperations.CallExtensionPoint(
                errorSource: p.Key,
                call: () => p.Key.ShouldTriggerCompletion(typeChar, p.Value),
                valueOnThrow: false));
        }

        public IAsyncCompletionSession GetSession(ITextView textView)
        {
            if (textView.Properties.TryGetProperty(typeof(IAsyncCompletionSession), out IAsyncCompletionSession session))
            {
                return session;
            }
            return null;
        }

        // Helper methods for telemetry
        internal string GetItemSourceName(IAsyncCompletionItemSource source) => OrderedCompletionItemSourceProviders.FirstOrDefault(n => n.Value == source)?.Metadata.Name ?? string.Empty;
        internal string GetCompletionServiceName(IAsyncCompletionService service) => OrderedCompletionServiceProviders.FirstOrDefault(n => n.Value == service)?.Metadata.Name ?? string.Empty;

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

        const string IsCompletionAvailableProperty = "IsCompletionAvailable";
        bool IAsyncCompletionBroker.IsCompletionSupported(ITextView view)
        {
            bool featureIsAvailable;
            if (view.Properties.TryGetProperty(IsCompletionAvailableProperty, out featureIsAvailable))
            {
                return featureIsAvailable;
            }

            featureIsAvailable = UnorderedCompletionItemSourceProviders
                    .Any(n => n.Metadata.ContentTypes.Any(ct => view.TextBuffer.ContentType.IsOfType(ct)));
            featureIsAvailable &= UnorderedCompletionServiceProviders
                    .Any(n => n.Metadata.ContentTypes.Any(ct => view.TextBuffer.ContentType.IsOfType(ct)));
            view.Properties.AddProperty(IsCompletionAvailableProperty, featureIsAvailable);
            return featureIsAvailable;
        }
    }
}
