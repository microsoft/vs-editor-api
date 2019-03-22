using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Text.Utilities;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.Text.MultiSelection.Implementation
{
    [Export(typeof(IMultiSelectionBrokerFactory))]
    [Export(typeof(IFeatureController))]
    internal class MultiSelectionBrokerFactory : IMultiSelectionBrokerFactory, IFeatureController
    {
        [Import]
        internal ISmartIndentationService SmartIndentationService { get; set; }

        [Import]
        internal ITextStructureNavigatorSelectorService TextStructureNavigatorSelectorService { get; set; }

        [Import]
        internal IContentTypeRegistryService ContentTypeRegistryService { get; set; }

        [Import(AllowDefault = true)]
        internal ILoggingServiceInternal LoggingService { get; set; }

        [Import]
        internal IFeatureServiceFactory FeatureServiceFactory { get; set; }

        [Import]
        internal IEditorOptionsFactoryService EditorOptionsFactoryService { get; set; }

        [Import]
        internal IGuardedOperations GuardedOperations { get; set; }

        public IMultiSelectionBroker CreateBroker(ITextView textView)
        {
            return new MultiSelectionBroker(textView, this);
        }
    }
}
