using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Text.Editor;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Editor.Commanding;
using Microsoft.VisualStudio.Commanding;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Threading;
using System.Linq;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Utilities;

namespace Microsoft.VisualStudio.UI.Text.Commanding.Implementation
{
    [Export(typeof(IEditorCommandHandlerServiceFactory))]
    internal class EditorCommandHandlerServiceFactory : IEditorCommandHandlerServiceFactory
    {
        private readonly IEnumerable<Lazy<ICommandHandler, ICommandHandlerMetadata>> _commandHandlers;
        private readonly IList<Lazy<ICommandingTextBufferResolverProvider, IContentTypeMetadata>> _bufferResolverProviders;
        private readonly IContentTypeRegistryService _contentTypeRegistryService;

        [ImportingConstructor]
        public EditorCommandHandlerServiceFactory(
            [ImportMany]IEnumerable<Lazy<ICommandHandler, ICommandHandlerMetadata>> commandHandlers,
            [ImportMany]IEnumerable<Lazy<ICommandingTextBufferResolverProvider, IContentTypeMetadata>> bufferResolvers,
            IUIThreadOperationExecutor uiThreadOperationExecutor,
            JoinableTaskContext joinableTaskContext,
            IStatusBarService statusBar,
            IContentTypeRegistryService contentTypeRegistryService,
            IGuardedOperations guardedOperations,
            [Import(AllowDefault = true)] ILoggingServiceInternal loggingService)
        {
            UIThreadOperationExecutor = uiThreadOperationExecutor;
            JoinableTaskContext = joinableTaskContext;
            StatusBar = statusBar;
            GuardedOperations = guardedOperations;
            LoggingService = loggingService;

            _contentTypeRegistryService = contentTypeRegistryService;
            ContentTypeComparer = new StableContentTypeComparer(_contentTypeRegistryService);
            _commandHandlers = OrderCommandHandlers(commandHandlers);
            if (!bufferResolvers.Any())
            {
                throw new ImportCardinalityMismatchException($"Expected to import at least one {typeof(ICommandingTextBufferResolver).Name}");
            }

            _bufferResolverProviders = bufferResolvers.ToList();
        }

        internal IGuardedOperations GuardedOperations { get; }

        internal ILoggingServiceInternal LoggingService { get; }

        internal JoinableTaskContext JoinableTaskContext { get; }

        internal IUIThreadOperationExecutor UIThreadOperationExecutor { get; }

        internal IStatusBarService StatusBar { get; }

        internal StableContentTypeComparer ContentTypeComparer { get; }

        public IEditorCommandHandlerService GetService(ITextView textView)
        {
            return textView.Properties.GetOrCreateSingletonProperty(() =>
            {
                var bufferResolverProvider = GuardedOperations.InvokeBestMatchingFactory(_bufferResolverProviders, textView.TextBuffer.ContentType, _contentTypeRegistryService, errorSource: this);
                ICommandingTextBufferResolver bufferResolver = null;
                GuardedOperations.CallExtensionPoint(() => bufferResolver = bufferResolverProvider.CreateResolver(textView));
                bufferResolver = bufferResolver ?? new DefaultBufferResolver(textView);
                return new EditorCommandHandlerService(this, textView, _commandHandlers, bufferResolver);
            });
        }

        public IEditorCommandHandlerService GetService(ITextView textView, ITextBuffer subjectBuffer)
        {
            if (subjectBuffer == null)
            {
                return GetService(textView);
            }

            // We cannot cache view/buffer affinitized service instance in the buffer property bag as the
            // buffer can be used by another text view, see https://devdiv.visualstudio.com/DevDiv/_workitems/edit/563472.
            // There is no good way to cache it without holding onto the buffer (which can be disconnected
            // from the text view anytime).
            return new EditorCommandHandlerService(this, textView, _commandHandlers, new SingleBufferResolver(subjectBuffer));
        }

        private IEnumerable<Lazy<ICommandHandler, ICommandHandlerMetadata>> OrderCommandHandlers(IEnumerable<Lazy<ICommandHandler, ICommandHandlerMetadata>> commandHandlers)
        {
            return commandHandlers.OrderBy((handler) => handler.Metadata.ContentTypes, ContentTypeComparer);
        }
    }
}
