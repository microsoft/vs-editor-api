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

namespace Microsoft.VisualStudio.UI.Text.Commanding.Implementation
{
    [Export(typeof(IEditorCommandHandlerServiceFactory))]
    internal class EditorCommandHandlerServiceFactory : IEditorCommandHandlerServiceFactory
    {
        private readonly IEnumerable<Lazy<ICommandHandler, ICommandHandlerMetadata>> _commandHandlers;
        private readonly IList<Lazy<ICommandingTextBufferResolverProvider, IContentTypeMetadata>> _bufferResolverProviders;
        private readonly IUIThreadOperationExecutor _uiThreadOperationExecutor;
        private readonly JoinableTaskContext _joinableTaskContext;
        private readonly IContentTypeRegistryService _contentTypeRegistryService;
        private readonly IGuardedOperations _guardedOperations;
        private readonly StableContentTypeComparer _contentTypeComparer;

        [ImportingConstructor]
        public EditorCommandHandlerServiceFactory(
            [ImportMany]IEnumerable<Lazy<ICommandHandler, ICommandHandlerMetadata>> commandHandlers,
            [ImportMany]IEnumerable<Lazy<ICommandingTextBufferResolverProvider, IContentTypeMetadata>> bufferResolvers,
            IUIThreadOperationExecutor uiThreadOperationExecutor,
            JoinableTaskContext joinableTaskContext,
            IContentTypeRegistryService contentTypeRegistryService,
            IGuardedOperations guardedOperations)
        {
            _uiThreadOperationExecutor = uiThreadOperationExecutor;
            _joinableTaskContext = joinableTaskContext;
            _guardedOperations = guardedOperations;
            _contentTypeRegistryService = contentTypeRegistryService;
            _contentTypeComparer = new StableContentTypeComparer(_contentTypeRegistryService);
            _commandHandlers = OrderCommandHandlers(commandHandlers);
            if (!bufferResolvers.Any())
            {
                throw new ImportCardinalityMismatchException($"Expected to import at least one {typeof(ICommandingTextBufferResolver).Name}");
            }

            _bufferResolverProviders = bufferResolvers.ToList();
        }

        public IEditorCommandHandlerService GetService(ITextView textView)
        {
            return textView.Properties.GetOrCreateSingletonProperty(() =>
            {
                var bufferResolverProvider = _guardedOperations.InvokeBestMatchingFactory(_bufferResolverProviders, textView.TextBuffer.ContentType, _contentTypeRegistryService, errorSource: this);
                ICommandingTextBufferResolver bufferResolver = null;
                _guardedOperations.CallExtensionPoint(() => bufferResolver = bufferResolverProvider.CreateResolver(textView));
                bufferResolver = bufferResolver ?? new DefaultBufferResolver(textView);
                return new EditorCommandHandlerService(textView, _commandHandlers, _uiThreadOperationExecutor, _joinableTaskContext,
                   _contentTypeComparer, bufferResolver, _guardedOperations);
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
            return new EditorCommandHandlerService(textView, _commandHandlers, _uiThreadOperationExecutor,
                _joinableTaskContext, _contentTypeComparer,
                new SingleBufferResolver(subjectBuffer), _guardedOperations);
        }

        private IEnumerable<Lazy<ICommandHandler, ICommandHandlerMetadata>> OrderCommandHandlers(IEnumerable<Lazy<ICommandHandler, ICommandHandlerMetadata>> commandHandlers)
        {
            return commandHandlers.OrderBy((handler) => handler.Metadata.ContentTypes, _contentTypeComparer);
        }
    }
}
