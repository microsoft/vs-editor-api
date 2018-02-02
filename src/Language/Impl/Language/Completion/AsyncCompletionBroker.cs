using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using System.Collections.Immutable;
using Microsoft.VisualStudio.Threading;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Text.Utilities;

#if NET46
using System.ComponentModel.Composition;
#else
using System.Composition;
#endif

namespace Microsoft.VisualStudio.Language.Intellisense.Implementation
{
    [Export(typeof(IAsyncCompletionBroker))]
    internal class AsyncCompletionBroker : IAsyncCompletionBroker
    {
        [Import]
        private JoinableTaskContext JtContext { get; set; }

        [Import(AllowDefault = true)]
        internal ILoggingServiceInternal Logger { get; set; }

        [Import(AllowDefault = true)]
        internal ITextDocumentFactoryService TextDocumentFactoryService { get; set; }

        [Import]
        private IGuardedOperations GuardedOperation { get; set; }

        [Import]
        private IContentTypeRegistryService ContentTypeRegistryService { get; set; }

        [ImportMany]
        private IEnumerable<Lazy<ICompletionUIProvider, IOrderableContentTypeMetadata>> UnorderedUiFactories { get; set; }

        [ImportMany]
        private IEnumerable<Lazy<IAsyncCompletionItemSource, IOrderableContentTypeMetadata>> UnorderedCompletionItemSources { get; set; }

        [ImportMany]
        private IEnumerable<Lazy<IAsyncCompletionService, IOrderableContentTypeMetadata>> UnorderedCompletionServices { get; set; }

        private IList<Lazy<ICompletionUIProvider, IOrderableContentTypeMetadata>> _orderedUiFactories;
        internal IList<Lazy<ICompletionUIProvider, IOrderableContentTypeMetadata>> OrderedUiFactories
            => _orderedUiFactories ?? (_orderedUiFactories = Orderer.Order(UnorderedUiFactories));

        private IList<Lazy<IAsyncCompletionItemSource, IOrderableContentTypeMetadata>> _orderedCompletionItemSources;
        internal IList<Lazy<IAsyncCompletionItemSource, IOrderableContentTypeMetadata>> OrderedCompletionItemSources
            => _orderedCompletionItemSources ?? (_orderedCompletionItemSources = Orderer.Order(UnorderedCompletionItemSources));

        private IList<Lazy<IAsyncCompletionService, IOrderableContentTypeMetadata>> _orderedCompletionServices;
        internal IList<Lazy<IAsyncCompletionService, IOrderableContentTypeMetadata>> OrderedCompletionServices
            => _orderedCompletionServices ?? (_orderedCompletionServices = Orderer.Order(UnorderedCompletionServices));

        private ImmutableDictionary<IContentType, ImmutableArray<string>> _commitCharacters = ImmutableDictionary<IContentType, ImmutableArray<string>>.Empty;
        private ImmutableDictionary<IContentType, ImmutableArray<IAsyncCompletionItemSource>> _cachedCompletionItemSources = ImmutableDictionary<IContentType, ImmutableArray<IAsyncCompletionItemSource>>.Empty;
        private ImmutableDictionary<IContentType, IAsyncCompletionService> _cachedCompletionServices = ImmutableDictionary<IContentType, IAsyncCompletionService>.Empty;
        private ImmutableDictionary<IContentType, ICompletionUIProvider> _cachedUiFactories = ImmutableDictionary<IContentType, ICompletionUIProvider>.Empty;
        bool firstRun = true; // used only for diagnostics
        private bool _firstInvocationReported; // used for "time to code"

        void IAsyncCompletionBroker.Dismiss(ITextView view)
        {
            var session = GetSession(view);
            if (session == null)
            {
                return;
            }
            view.Properties.RemoveProperty(typeof(IAsyncCompletionSession));
            session.DismissAndHide();
        }

        void IAsyncCompletionBroker.Commit(ITextView view, string edit)
        {
            var session = GetSession(view);
            if (session == null)
            {
                return;
            }

            session.Commit(edit);
            ((IAsyncCompletionBroker)this).Dismiss(view);
        }

        void IAsyncCompletionBroker.OpenOrUpdate(ITextView view, ITrackingSpan trackedEdit, CompletionTrigger trigger, SnapshotPoint triggerLocation)
        {
            var session = GetSession(view);
            if (session == null)
            {
                // Either the session was dismissed, or Begin was not called yet.
                // what would be a good way to tell the two apart?
                // throw new InvalidOperationException("No completion session available for this view. Call Begin first.");
                return;
            }
            session.OpenOrUpdate(view, trackedEdit, trigger, triggerLocation);
        }

        void IAsyncCompletionBroker.TriggerCompletion(ITextView view, SnapshotPoint triggerLocation)
        {
            var session = GetSession(view);
            if (session != null)
            {
                // Session already exists.
                return;
            }

            // TODO: Handle the race condition: two consecutive OpenAsync. Both create completion
            // not knowing of one another. The second invocation should use the if-block.

            var sourcesWithLocations = CompletionUtilities.GetCompletionSourcesWithMappedLocations(view, triggerLocation, GetCompletionItemSources);
            if (!sourcesWithLocations.Any())
            {
                // There is no completion source available for this buffer
                return;
            }

            var buffers = CompletionUtilities.GetBuffersForTriggerPoint(view, triggerLocation).ToImmutableArray();
            var service = buffers
                .Select(b => GetCompletionService(b.ContentType))
                .FirstOrDefault(s => s != null);

            if (service == null)
            {
                // There is no modern completion service available for this buffer
                return;
            }

            var uiFactory = buffers
                .Select(b => GetUiFactory(b.ContentType))
                .FirstOrDefault(s => s != null);

            if (firstRun)
            {
                System.Diagnostics.Debug.Assert(uiFactory != null, $"No instance of {nameof(ICompletionUIProvider)} is loaded. The prototype completion will work without the UI.");
                firstRun = false;
            }
            session = new AsyncCompletionSession(JtContext.Factory, uiFactory, sourcesWithLocations, service, this, view);
            view.Properties.AddProperty(typeof(IAsyncCompletionSession), session);
            view.Closed += TextView_Closed;

            // Additionally, emulate the legacy completion telemetry
            emulateLegacyCompletionTelemetry(buffers.FirstOrDefault()?.ContentType, view);
        }

        private ImmutableArray<IAsyncCompletionItemSource> GetCompletionItemSources(IContentType contentType)
        {
            if (_cachedCompletionItemSources.TryGetValue(contentType, out var cachedSources))
            {
                return cachedSources;
            }

            ImmutableArray<IAsyncCompletionItemSource> result = ImmutableArray<IAsyncCompletionItemSource>.Empty;
            foreach (var item in OrderedCompletionItemSources)
            {
                if (item.Metadata.ContentTypes.Any(n => contentType.IsOfType(n)))
                {
                    result = result.Add(item.Value);
                }
            }
            _cachedCompletionItemSources = _cachedCompletionItemSources.Add(contentType, result);
            return result;
        }

        private IAsyncCompletionService GetCompletionService(IContentType contentType)
        {
            if (_cachedCompletionServices.TryGetValue(contentType, out var service))
            {
                return service;
            }

            IAsyncCompletionService bestService = GuardedOperation.InvokeBestMatchingFactory(
                providerHandles: OrderedCompletionServices,
                getter: n => n,
                dataContentType: contentType,
                contentTypeRegistryService: ContentTypeRegistryService,
                errorSource: this);

            _cachedCompletionServices = _cachedCompletionServices.Add(contentType, bestService);
            return bestService;
        }

        private ICompletionUIProvider GetUiFactory(IContentType contentType)
        {
            if (_cachedUiFactories.TryGetValue(contentType, out var factory))
            {
                return factory;
            }

            ICompletionUIProvider bestFactory = GuardedOperation.InvokeBestMatchingFactory(
                providerHandles: OrderedUiFactories,
                getter: n => n,
                dataContentType: contentType,
                contentTypeRegistryService: ContentTypeRegistryService,
                errorSource: this);

            _cachedUiFactories = _cachedUiFactories.Add(contentType, bestFactory);
            return bestFactory;
        }

        private void TextView_Closed(object sender, EventArgs e)
        {
            ((IAsyncCompletionBroker)this).Dismiss((ITextView)sender);
            try
            {
                Task.Run(async () =>
                {
                    await Task.WhenAll(OrderedCompletionItemSources
                        .Where(n => n.IsValueCreated)
                        .Select(n => n.Value)
                        .Select(n => n.HandleViewClosedAsync((ITextView)sender)));
                });
            }
            catch
            {

            }
        }

        bool IAsyncCompletionBroker.IsCompletionActive(ITextView view)
        {
            return view.Properties.ContainsProperty(typeof(IAsyncCompletionSession));
        }

        bool IAsyncCompletionBroker.ShouldCommitCompletion(ITextView view, string edit, SnapshotPoint triggerLocation)
        {
            if (!((IAsyncCompletionBroker)this).IsCompletionActive(view))
            {
                return false;
            }

            foreach (var contentType in CompletionUtilities.GetBuffersForTriggerPoint(view, triggerLocation).Select(b => b.ContentType))
            {
                if (TryGetKnownCommitCharacters(contentType, out var commitChars))
                {
                    if (commitChars.Contains(edit[0].ToString()))
                    {
                        if (GetSession(view).ShouldCommit(view, edit, triggerLocation))
                            return true;
                    }
                }
            }
            return false;
        }

        private bool TryGetKnownCommitCharacters(IContentType contentType, out ImmutableArray<string> commitChars)
        {
            if (_commitCharacters.TryGetValue(contentType, out commitChars))
            {
                return commitChars.Any();
            }
            var commitCharsBuilder = ImmutableArray.CreateBuilder<string>();
            foreach (var source in GetCompletionItemSources(contentType))
            {
                commitCharsBuilder.AddRange(source.GetPotentialCommitCharacters());
            }
            commitChars = commitCharsBuilder.Distinct().ToImmutableArray();
            _commitCharacters = _commitCharacters.Add(contentType, commitChars);
            return commitChars.Any();
        }

        bool IAsyncCompletionBroker.ShouldTriggerCompletion(ITextView view, string edit, SnapshotPoint triggerLocation)
        {
            var sourcesWithLocations = CompletionUtilities.GetCompletionSourcesWithMappedLocations(view, triggerLocation, GetCompletionItemSources);
            return sourcesWithLocations.Any(p => p.Key.ShouldTriggerCompletion(edit, p.Value));
        }

        private IAsyncCompletionSession GetSession(ITextView view)
        {
            if (view.Properties.TryGetProperty(typeof(IAsyncCompletionSession), out IAsyncCompletionSession property))
            {
                return property;
            }
            return null;
        }

        void IAsyncCompletionBroker.SelectUp(ITextView view) => GetSession(view)?.SelectUp();
        void IAsyncCompletionBroker.SelectDown(ITextView view) => GetSession(view)?.SelectDown();
        void IAsyncCompletionBroker.SelectPageUp(ITextView view) => GetSession(view)?.SelectPageUp();
        void IAsyncCompletionBroker.SelectPageDown(ITextView view) => GetSession(view)?.SelectPageDown();

        // Helper methods for telemetry
        internal string GetItemSourceName(IAsyncCompletionItemSource source) => OrderedCompletionItemSources.FirstOrDefault(n => n.Value == source)?.Metadata.Name ?? string.Empty;
        internal string GetCompletionServiceName(IAsyncCompletionService service) => OrderedCompletionServices.FirstOrDefault(n => n.Value == service)?.Metadata.Name ?? string.Empty;

        // Partiy with legacy telemetry
        private void emulateLegacyCompletionTelemetry(IContentType contentType, ITextView view)
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
            var fileExtension = GetFileExtension(view.TextBuffer) ?? "Unknown";
            var reportedContentType = contentType?.ToString() ?? "Unknown";

            _firstInvocationReported = true;
            Logger.PostEvent(TelemetryEventType.Operation, "VS/Editor/IntellisenseFirstRun/Opened", TelemetryResult.Success,
                ("VS.Editor.IntellisenseFirstRun.Opened.ContentType", reportedContentType),
                ("VS.Editor.IntellisenseFirstRun.Opened.FileExtension", fileExtension));
        }
    }
}
