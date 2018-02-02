using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Commanding;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Editor.Commanding.Commands;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.Text.Operations.Implementation
{
    [Export(typeof(ICommandHandler))]
    [Name(nameof(ExpandContractSelectionCommandHandler))]
    [ContentType("any")]
    [TextViewRole(PredefinedTextViewRoles.PrimaryDocument)]
    [TextViewRole(PredefinedTextViewRoles.EmbeddedPeekTextView)]
    internal sealed class ExpandContractSelectionCommandHandler
        : ICommandHandler<ExpandSelectionCommandArgs>, ICommandHandler<ContractSelectionCommandArgs>
    {
        [ImportingConstructor]
        public ExpandContractSelectionCommandHandler(
            IEditorOptionsFactoryService editorOptionsFactoryService,
            ITextStructureNavigatorSelectorService navigatorSelectorService)
        {
            this.EditorOptionsFactoryService = editorOptionsFactoryService;
            this.NavigatorSelectorService = navigatorSelectorService;
        }

        public IEditorOptionsFactoryService EditorOptionsFactoryService { get; }

        private readonly ITextStructureNavigatorSelectorService NavigatorSelectorService;

        public string DisplayName => Strings.ExpandContractSelectionCommandHandlerName;

        public CommandState GetCommandState(ExpandSelectionCommandArgs args)
        {
            var storedCommandState = ExpandContractSelectionImplementation.GetOrCreateExpandContractState(
                args.TextView,
                this.EditorOptionsFactoryService,
                this.NavigatorSelectorService);
            return storedCommandState.GetExpandCommandState(args.TextView);
        }

        public CommandState GetCommandState(ContractSelectionCommandArgs args)
        {
            var storedCommandState = ExpandContractSelectionImplementation.GetOrCreateExpandContractState(
                args.TextView,
                this.EditorOptionsFactoryService,
                this.NavigatorSelectorService);
            return storedCommandState.GetContractCommandState(args.TextView);
        }

        public bool ExecuteCommand(ExpandSelectionCommandArgs args, CommandExecutionContext context)
        {
            var storedCommandState = ExpandContractSelectionImplementation.GetOrCreateExpandContractState(
                args.TextView,
                this.EditorOptionsFactoryService,
                this.NavigatorSelectorService);
            return storedCommandState.ExpandSelection(args.TextView);
        }

        public bool ExecuteCommand(ContractSelectionCommandArgs args, CommandExecutionContext context)
        {
            var storedCommandState = ExpandContractSelectionImplementation.GetOrCreateExpandContractState(
                args.TextView,
                this.EditorOptionsFactoryService,
                this.NavigatorSelectorService);
            return storedCommandState.ContractSelection(args.TextView);
        }
    }
}
