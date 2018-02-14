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
        [Import]
        private JoinableTaskContext JtContext { get; set; }

        [Import(AllowDefault = true)]
        internal ILoggingServiceInternal Logger { get; set; }

        // This may be used to GetExtentOfWord, but it doesn't fully work yet, so we're not using it.
        [Import(AllowDefault = true)]
        internal ITextDocumentFactoryService TextDocumentFactoryService { get; set; }

        [Import]
        internal IGuardedOperations GuardedOperations { get; set; }

        [Import]
        private IContentTypeRegistryService ContentTypeRegistryService { get; set; }

        [Import]
        internal ITextStructureNavigatorSelectorService TextStructureNavigatorSelectorService { get; set; }

        [ImportMany]
        private IEnumerable<Lazy<ICompletionPresenterProvider, IOrderableContentTypeMetadata>> UnorderedPresenterProviders { get; set; }

        // TODO: use the provider pattern
        [ImportMany]
        private IEnumerable<Lazy<IAsyncCompletionItemSource, IOrderableContentTypeMetadata>> UnorderedCompletionItemSources { get; set; }

        [ImportMany]
        private IEnumerable<Lazy<IAsyncCompletionService, IOrderableContentTypeMetadata>> UnorderedCompletionServices { get; set; }

        private IList<Lazy<ICompletionPresenterProvider, IOrderableContentTypeMetadata>> _orderedPresenterProviders;
        internal IList<Lazy<ICompletionPresenterProvider, IOrderableContentTypeMetadata>> OrderedPresenterProviders
            => _orderedPresenterProviders ?? (_orderedPresenterProviders = Orderer.Order(UnorderedPresenterProviders));

        private IList<Lazy<IAsyncCompletionItemSource, IOrderableContentTypeMetadata>> _orderedCompletionItemSources;
        internal IList<Lazy<IAsyncCompletionItemSource, IOrderableContentTypeMetadata>> OrderedCompletionItemSources
            => _orderedCompletionItemSources ?? (_orderedCompletionItemSources = Orderer.Order(UnorderedCompletionItemSources));

        private IList<Lazy<IAsyncCompletionService, IOrderableContentTypeMetadata>> _orderedCompletionServices;
        internal IList<Lazy<IAsyncCompletionService, IOrderableContentTypeMetadata>> OrderedCompletionServices
            => _orderedCompletionServices ?? (_orderedCompletionServices = Orderer.Order(UnorderedCompletionServices));

        private ImmutableDictionary<IContentType, ImmutableSortedSet<char>> _commitCharacters = ImmutableDictionary<IContentType, ImmutableSortedSet<char>>.Empty;
        private ImmutableDictionary<IContentType, ImmutableArray<IAsyncCompletionItemSource>> _cachedCompletionItemSources = ImmutableDictionary<IContentType, ImmutableArray<IAsyncCompletionItemSource>>.Empty;
        private ImmutableDictionary<IContentType, IAsyncCompletionService> _cachedCompletionServices = ImmutableDictionary<IContentType, IAsyncCompletionService>.Empty;
        private ImmutableDictionary<IContentType, ICompletionPresenterProvider> _cachedUiFactories = ImmutableDictionary<IContentType, ICompletionPresenterProvider>.Empty;
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

            var sourcesWithLocations = CompletionUtilities.GetCompletionSourcesWithMappedLocations(view, triggerLocation, GetCompletionItemSources);
            if (!sourcesWithLocations.Any())
            {
                // There is no completion source available for this buffer
                return null;
            }

            var buffers = CompletionUtilities.GetBuffersForTriggerPoint(view, triggerLocation).ToImmutableArray();
            var service = buffers
                .Select(b => GetCompletionService(b.ContentType))
                .FirstOrDefault(s => s != null);

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

        private ImmutableArray<IAsyncCompletionItemSource> GetCompletionItemSources(IContentType contentType)
        {
            if (_cachedCompletionItemSources.TryGetValue(contentType, out var cachedSources))
            {
                return cachedSources;
            }

            var builder = ImmutableArray.CreateBuilder<IAsyncCompletionItemSource>();
            foreach (var item in OrderedCompletionItemSources)
            {
                if (item.Metadata.ContentTypes.Any(n => contentType.IsOfType(n)))
                {
                    builder.Add(item.Value);
                }
            }
            var result = builder.ToImmutable();
            _cachedCompletionItemSources = _cachedCompletionItemSources.Add(contentType, result);
            return result;
        }

        private IAsyncCompletionService GetCompletionService(IContentType contentType)
        {
            if (_cachedCompletionServices.TryGetValue(contentType, out var service))
            {
                return service;
            }

            IAsyncCompletionService bestService = GuardedOperations.InvokeBestMatchingFactory(
                providerHandles: OrderedCompletionServices,
                getter: n => n,
                dataContentType: contentType,
                contentTypeRegistryService: ContentTypeRegistryService,
                errorSource: this);

            _cachedCompletionServices = _cachedCompletionServices.Add(contentType, bestService);
            return bestService;
        }

        private ICompletionPresenterProvider GetUiFactory(IContentType contentType)
        {
            if (_cachedUiFactories.TryGetValue(contentType, out var factory))
            {
                return factory;
            }

            ICompletionPresenterProvider bestFactory = GuardedOperations.InvokeBestMatchingFactory(
                providerHandles: OrderedPresenterProviders,
                getter: n => n,
                dataContentType: contentType,
                contentTypeRegistryService: ContentTypeRegistryService,
                errorSource: this);

            _cachedUiFactories = _cachedUiFactories.Add(contentType, bestFactory);
            return bestFactory;
        }

        internal bool TryGetKnownCommitCharacters(IContentType contentType, out ImmutableSortedSet<char> commitChars)
        {
            if (_commitCharacters.TryGetValue(contentType, out commitChars))
            {
                return commitChars.Any();
            }
            var allCommitChars = new List<char>();
            foreach (var source in GetCompletionItemSources(contentType))
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
            GetSession((ITextView)sender).Dismiss();
            // TODO: unlink this
            Task.Run(async () =>
            {
                await Task.WhenAll(OrderedCompletionItemSources
                    .Where(n => n.IsValueCreated)
                    .Select(n => n.Value)
                    .Select(n => GuardedOperations.CallExtensionPointAsync(n, () => n.HandleViewClosedAsync((ITextView)sender))));
            });
        }

        bool IAsyncCompletionBroker.IsCompletionActive(ITextView view)
        {
            return view.Properties.ContainsProperty(typeof(IAsyncCompletionSession));
        }

        bool IAsyncCompletionBroker.ShouldTriggerCompletion(ITextView textView, char typeChar, SnapshotPoint triggerLocation)
        {
            var sourcesWithLocations = CompletionUtilities.GetCompletionSourcesWithMappedLocations(textView, triggerLocation, GetCompletionItemSources);
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
        internal string GetItemSourceName(IAsyncCompletionItemSource source) => OrderedCompletionItemSources.FirstOrDefault(n => n.Value == source)?.Metadata.Name ?? string.Empty;
        internal string GetCompletionServiceName(IAsyncCompletionService service) => OrderedCompletionServices.FirstOrDefault(n => n.Value == service)?.Metadata.Name ?? string.Empty;

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

            featureIsAvailable = UnorderedCompletionItemSources
                    .Any(n => n.Metadata.ContentTypes.Any(ct => view.TextBuffer.ContentType.IsOfType(ct)));
            featureIsAvailable &= UnorderedCompletionServices
                    .Any(n => n.Metadata.ContentTypes.Any(ct => view.TextBuffer.ContentType.IsOfType(ct)));
            view.Properties.AddProperty(IsCompletionAvailableProperty, featureIsAvailable);
            return featureIsAvailable;
        }
    }
}
