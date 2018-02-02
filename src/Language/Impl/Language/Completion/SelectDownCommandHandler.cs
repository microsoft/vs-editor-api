using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Commanding;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text.Editor.Commanding.Commands;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.Language.Intellisense.Implementation
{
    /// <summary>
    /// Reacts to the down arrow command and attempts to scroll the completion list.
    /// </summary>
    [Name(nameof(SelectDownCommandHandler))]
    [ContentType("any")]
    [Export(typeof(ICommandHandler))]
    internal sealed class SelectDownCommandHandler : ICommandHandler<DownKeyCommandArgs>
    {
        [Import]
        IAsyncCompletionBroker broker;

        // TODO: Localize
        string INamed.DisplayName => "Handler for down arrow in completion";

        bool ICommandHandler<DownKeyCommandArgs>.ExecuteCommand(DownKeyCommandArgs args, CommandExecutionContext executionContext)
        {
            if (broker.IsCompletionActive(args.TextView))
            {
                broker.SelectDown(args.TextView);
                return true;
            }
            return false;
        }

        CommandState ICommandHandler<DownKeyCommandArgs>.GetCommandState(DownKeyCommandArgs args)
        {
            return broker.IsCompletionActive(args.TextView)
                ? CommandState.Available
                : CommandState.Unspecified;
        }
    }
}
