using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Commanding;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text.Editor.Commanding.Commands;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.Language.Intellisense.Implementation
{
    /// <summary>
    /// Reacts to the page down command and attempts to scroll the completion list.
    /// </summary>
    [Name(nameof(SelectPageDownCommandHandler))]
    [ContentType("any")]
    [Export(typeof(ICommandHandler))]
    internal sealed class SelectPageDownCommandHandler : ICommandHandler<PageDownKeyCommandArgs>
    {
        [Import]
        IAsyncCompletionBroker broker;

        // TODO: Localize
        string INamed.DisplayName => "Handler for page down in completion";

        bool ICommandHandler<PageDownKeyCommandArgs>.ExecuteCommand(PageDownKeyCommandArgs args, CommandExecutionContext executionContext)
        {
            if (broker.IsCompletionActive(args.TextView))
            {
                broker.SelectPageDown(args.TextView);
                return true;
            }
            return false;
        }

        CommandState ICommandHandler<PageDownKeyCommandArgs>.GetCommandState(PageDownKeyCommandArgs args)
        {
            return broker.IsCompletionActive(args.TextView)
                ? CommandState.Available
                : CommandState.Unspecified;
        }
    }
}
