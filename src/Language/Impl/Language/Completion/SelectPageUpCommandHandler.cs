using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Commanding;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text.Editor.Commanding.Commands;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.Language.Intellisense.Implementation
{
    /// <summary>
    /// Reacts to the page up command and attempts to scroll the completion list.
    /// </summary>
    [Name(nameof(SelectPageUpCommandHandler))]
    [ContentType("any")]
    [Export(typeof(ICommandHandler))]
    internal sealed class SelectPageUpCommandHandler : ICommandHandler<PageUpKeyCommandArgs>
    {
        [Import]
        IAsyncCompletionBroker broker;

        // TODO: Localize
        string INamed.DisplayName => "Handler for page down in completion";

        bool ICommandHandler<PageUpKeyCommandArgs>.ExecuteCommand(PageUpKeyCommandArgs args, CommandExecutionContext executionContext)
        {
            if (broker.IsCompletionActive(args.TextView))
            {
                broker.SelectPageUp(args.TextView);
                return true;
            }
            return false;
        }

        CommandState ICommandHandler<PageUpKeyCommandArgs>.GetCommandState(PageUpKeyCommandArgs args)
        {
            return broker.IsCompletionActive(args.TextView)
                ? CommandState.Available
                : CommandState.Unspecified;
        }
    }
}
