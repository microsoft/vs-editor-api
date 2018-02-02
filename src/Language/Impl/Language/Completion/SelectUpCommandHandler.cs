using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Commanding;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text.Editor.Commanding.Commands;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.Language.Intellisense.Implementation
{
    /// <summary>
    /// Reacts to the up arrow command and attempts to scroll the completion list.
    /// </summary>
    [Name(nameof(SelectUpCommandHandler))]
    [ContentType("any")]
    [Export(typeof(ICommandHandler))]
    internal sealed class SelectUpCommandHandler : ICommandHandler<UpKeyCommandArgs>
    {
        [Import]
        IAsyncCompletionBroker broker;

        // TODO: Localize
        string INamed.DisplayName => "Handler for down arrow in completion";

        bool ICommandHandler<UpKeyCommandArgs>.ExecuteCommand(UpKeyCommandArgs args, CommandExecutionContext executionContext)
        {
            if (broker.IsCompletionActive(args.TextView))
            {
                broker.SelectUp(args.TextView);
                return true;
            }
            return false;
        }

        CommandState ICommandHandler<UpKeyCommandArgs>.GetCommandState(UpKeyCommandArgs args)
        {
            return broker.IsCompletionActive(args.TextView)
                ? CommandState.Available
                : CommandState.Unspecified;
        }
    }
}
