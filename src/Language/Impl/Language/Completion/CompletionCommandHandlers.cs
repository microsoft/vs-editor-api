using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Commanding;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Editor.Commanding.Commands;
using Microsoft.VisualStudio.Text.Utilities;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.Language.Intellisense.Implementation
{
    /// <summary>
    /// Reacts to the down arrow command and attempts to scroll the completion list.
    /// </summary>
    [Name(nameof(CompletionCommandHandlers))]
    [ContentType("any")]
    [Export(typeof(ICommandHandler))]
    internal sealed class CompletionCommandHandlers :
        ICommandHandler<DownKeyCommandArgs>,
        ICommandHandler<PageDownKeyCommandArgs>,
        ICommandHandler<PageUpKeyCommandArgs>,
        ICommandHandler<UpKeyCommandArgs>,
        IChainedCommandHandler<BackspaceKeyCommandArgs>,
        ICommandHandler<EscapeKeyCommandArgs>,
        ICommandHandler<InvokeCompletionListCommandArgs>,
        ICommandHandler<CommitUniqueCompletionListItemCommandArgs>,
        ICommandHandler<InsertSnippetCommandArgs>,
        ICommandHandler<ToggleCompletionModeCommandArgs>,
        IChainedCommandHandler<DeleteKeyCommandArgs>,
        ICommandHandler<WordDeleteToEndCommandArgs>,
        ICommandHandler<WordDeleteToStartCommandArgs>,
        ICommandHandler<ReturnKeyCommandArgs>,
        ICommandHandler<TabKeyCommandArgs>,
        IChainedCommandHandler<TypeCharCommandArgs>
    {
        [Import]
        IAsyncCompletionBroker Broker { get; set; }

        [Import]
        IExperimentationServiceInternal ExperimentationService { get; set; }

        string INamed.DisplayName => Strings.CompletionCommandHandlerName;

        /// <summary>
        /// Helper method that returns command state for commands
        /// that are always available - unless the completion feature is available.
        /// </summary>
        /// <param name="view"></param>
        /// <returns></returns>
        private CommandState Available(ITextView view)
        {
            return ModernCompletionFeature.GetFeatureState(ExperimentationService)
                && Broker.IsCompletionSupported(view)
                ? CommandState.Available
                : CommandState.Unspecified;
        }

        /// <summary>
        /// Helper method that returns command state for commands
        /// that are available when completion is active.
        /// </summary>
        /// <remarks>
        /// Completion might be active only if the feature is available, so we're skipping other checks.
        /// </remarks>
        private CommandState AvailableIfCompletionIsUp(ITextView view)
        {
            return Broker.IsCompletionActive(view)
                ? CommandState.Available
                : CommandState.Unspecified;
        }

        CommandState IChainedCommandHandler<BackspaceKeyCommandArgs>.GetCommandState(BackspaceKeyCommandArgs args, Func<CommandState> nextCommandHandler)
           => AvailableIfCompletionIsUp(args.TextView);

        void IChainedCommandHandler<BackspaceKeyCommandArgs>.ExecuteCommand(BackspaceKeyCommandArgs args, Action nextCommandHandler, CommandExecutionContext executionContext)
        {
            // Execute other commands in the chain to see the change in the buffer.
            nextCommandHandler();

            // We are only inteterested in the top buffer. Currently, commanding implementation calls us multiple times, once per each buffer.
            if (args.TextView.BufferGraph.TopBuffer != args.SubjectBuffer)
                return;

            var session = Broker.GetSession(args.TextView);
            if (session != null)
            {
                var trigger = new CompletionTrigger(CompletionTriggerReason.Deletion);
                var location = args.TextView.Caret.Position.BufferPosition;
                session.OpenOrUpdate(args.TextView, trigger, location);
            }
        }

        CommandState ICommandHandler<EscapeKeyCommandArgs>.GetCommandState(EscapeKeyCommandArgs args)
            => AvailableIfCompletionIsUp(args.TextView);

        bool ICommandHandler<EscapeKeyCommandArgs>.ExecuteCommand(EscapeKeyCommandArgs args, CommandExecutionContext executionContext)
        {
            var session = Broker.GetSession(args.TextView);
            if (session != null)
            {
                session.Dismiss();
                return true;
            }
            return false;
        }

        CommandState ICommandHandler<InvokeCompletionListCommandArgs>.GetCommandState(InvokeCompletionListCommandArgs args)
            => Available(args.TextView);

        bool ICommandHandler<InvokeCompletionListCommandArgs>.ExecuteCommand(InvokeCompletionListCommandArgs args, CommandExecutionContext executionContext)
        {
            var trigger = new CompletionTrigger(CompletionTriggerReason.Invoke);
            var location = args.TextView.Caret.Position.BufferPosition;
            var session = Broker.TriggerCompletion(args.TextView, location);
            session.OpenOrUpdate(args.TextView, trigger, location);
            return true;
        }

        CommandState ICommandHandler<CommitUniqueCompletionListItemCommandArgs>.GetCommandState(CommitUniqueCompletionListItemCommandArgs args)
            => Available(args.TextView);

        bool ICommandHandler<CommitUniqueCompletionListItemCommandArgs>.ExecuteCommand(CommitUniqueCompletionListItemCommandArgs args, CommandExecutionContext executionContext)
        {
            var trigger = new CompletionTrigger(CompletionTriggerReason.InvokeAndCommitIfUnique);
            var location = args.TextView.Caret.Position.BufferPosition;
            var session = Broker.TriggerCompletion(args.TextView, location);
            session.OpenOrUpdate(args.TextView, trigger, location);
            // TODO: figure out dismissing. who should dismiss? here, OpenOrUpdate dismisses. Else, commit dismisses.
            return true;
        }

        CommandState ICommandHandler<InsertSnippetCommandArgs>.GetCommandState(InsertSnippetCommandArgs args)
            => Available(args.TextView);

        bool ICommandHandler<InsertSnippetCommandArgs>.ExecuteCommand(InsertSnippetCommandArgs args, CommandExecutionContext executionContext)
        {
            System.Diagnostics.Debug.WriteLine("!!!! InsertSnippetCommandArgs");
            return false;
        }

        CommandState ICommandHandler<ToggleCompletionModeCommandArgs>.GetCommandState(ToggleCompletionModeCommandArgs args)
            => Available(args.TextView);

        bool ICommandHandler<ToggleCompletionModeCommandArgs>.ExecuteCommand(ToggleCompletionModeCommandArgs args, CommandExecutionContext executionContext)
        {
            var session = Broker.GetSession(args.TextView) as AsyncCompletionSession; // we are accessing an internal method
            if (session != null)
            {
                session.ToggleSuggestionMode();
                return true; // TODO: See if the toobar button gets updated.
            }
            return false;
        }

        CommandState IChainedCommandHandler<DeleteKeyCommandArgs>.GetCommandState(DeleteKeyCommandArgs args, Func<CommandState> nextCommandHandler)
            => AvailableIfCompletionIsUp(args.TextView);

        void IChainedCommandHandler<DeleteKeyCommandArgs>.ExecuteCommand(DeleteKeyCommandArgs args, Action nextCommandHandler, CommandExecutionContext executionContext)
        {
            // Execute other commands in the chain to see the change in the buffer.
            nextCommandHandler();

            // We are only inteterested in the top buffer. Currently, commanding implementation calls us multiple times, once per each buffer.
            if (args.TextView.BufferGraph.TopBuffer != args.SubjectBuffer)
                return;

            var session = Broker.GetSession(args.TextView);
            if (session != null)
            {
                var trigger = new CompletionTrigger(CompletionTriggerReason.Deletion);
                var location = args.TextView.Caret.Position.BufferPosition;
                session.OpenOrUpdate(args.TextView, trigger, location);
            }
        }

        CommandState ICommandHandler<WordDeleteToEndCommandArgs>.GetCommandState(WordDeleteToEndCommandArgs args)
            => AvailableIfCompletionIsUp(args.TextView);

        bool ICommandHandler<WordDeleteToEndCommandArgs>.ExecuteCommand(WordDeleteToEndCommandArgs args, CommandExecutionContext executionContext)
        {
            var session = Broker.GetSession(args.TextView);
            if (session != null)
            {
                session.Dismiss();
                return false; // return false so that the editor can handle this event
            }
            return false;
        }

        CommandState ICommandHandler<WordDeleteToStartCommandArgs>.GetCommandState(WordDeleteToStartCommandArgs args)
            => AvailableIfCompletionIsUp(args.TextView);

        bool ICommandHandler<WordDeleteToStartCommandArgs>.ExecuteCommand(WordDeleteToStartCommandArgs args, CommandExecutionContext executionContext)
        {
            var session = Broker.GetSession(args.TextView);
            if (session != null)
            {
                session.Dismiss();
                return false; // return false so that the editor can handle this event
            }
            return false;
        }

        CommandState ICommandHandler<ReturnKeyCommandArgs>.GetCommandState(ReturnKeyCommandArgs args)
            => AvailableIfCompletionIsUp(args.TextView);

        bool ICommandHandler<ReturnKeyCommandArgs>.ExecuteCommand(ReturnKeyCommandArgs args, CommandExecutionContext executionContext)
        {
            var session = Broker.GetSession(args.TextView);
            if (session != null)
            {
                session.Commit(executionContext.OperationContext.UserCancellationToken);
                session.Dismiss(); // TODO: Currently the implementation needs UI thread
                return true;
            }
            return false;
        }

        CommandState ICommandHandler<TabKeyCommandArgs>.GetCommandState(TabKeyCommandArgs args)
            => AvailableIfCompletionIsUp(args.TextView);

        bool ICommandHandler<TabKeyCommandArgs>.ExecuteCommand(TabKeyCommandArgs args, CommandExecutionContext executionContext)
        {
            var session = Broker.GetSession(args.TextView);
            if (session != null)
            {
                session.Commit(executionContext.OperationContext.UserCancellationToken);
                session.Dismiss(); // TODO: Currently the implementation needs UI thread
                return true;
            }
            return false;
        }

        CommandState IChainedCommandHandler<TypeCharCommandArgs>.GetCommandState(TypeCharCommandArgs args, Func<CommandState> nextCommandHandler)
            => Available(args.TextView);

        void IChainedCommandHandler<TypeCharCommandArgs>.ExecuteCommand(TypeCharCommandArgs args, Action nextCommandHandler, CommandExecutionContext executionContext)
        {
            // Execute other commands in the chain to see the change in the buffer.
            nextCommandHandler();

            // We are only inteterested in the top buffer. Currently, commanding implementation calls us multiple times, once per each buffer.
            if (args.TextView.BufferGraph.TopBuffer != args.SubjectBuffer)
                return;

            var view = args.TextView;
            var location = view.Caret.Position.BufferPosition;
            var sessionToCommit = Broker.GetSession(args.TextView);
            if (sessionToCommit?.ShouldCommit(view, args.TypedChar, location) == true)
            {
                sessionToCommit.Commit(executionContext.OperationContext.UserCancellationToken, args.TypedChar);
                sessionToCommit.Dismiss(); // TODO: Currently the implementation needs UI thread
                // Snapshot has changed when committing. Update it for when we try to trigger new session.
                location = view.Caret.Position.BufferPosition;
            }

            var trigger = new CompletionTrigger(CompletionTriggerReason.Insertion, args.TypedChar);
            var session = Broker.GetSession(args.TextView);
            if (session != null)
            {
                session.OpenOrUpdate(view, trigger, location);
            }
            else if (Broker.ShouldTriggerCompletion(view, args.TypedChar, location))
            {
                var newSession = Broker.TriggerCompletion(view, location);
                newSession.OpenOrUpdate(view, trigger, location);
            }
        }

        CommandState ICommandHandler<DownKeyCommandArgs>.GetCommandState(DownKeyCommandArgs args)
            => AvailableIfCompletionIsUp(args.TextView);

        bool ICommandHandler<DownKeyCommandArgs>.ExecuteCommand(DownKeyCommandArgs args, CommandExecutionContext executionContext)
        {
            var session = Broker.GetSession(args.TextView) as AsyncCompletionSession; // we are accessing an internal method
            if (session != null)
            {
                session.SelectDown();
                return true;
            }
            return false;
        }

        CommandState ICommandHandler<PageDownKeyCommandArgs>.GetCommandState(PageDownKeyCommandArgs args)
            => AvailableIfCompletionIsUp(args.TextView);

        bool ICommandHandler<PageDownKeyCommandArgs>.ExecuteCommand(PageDownKeyCommandArgs args, CommandExecutionContext executionContext)
        {
            var session = Broker.GetSession(args.TextView) as AsyncCompletionSession; // we are accessing an internal method
            if (session != null)
            {
                session.SelectPageDown();
                return true;
            }
            return false;
        }

        CommandState ICommandHandler<PageUpKeyCommandArgs>.GetCommandState(PageUpKeyCommandArgs args)
            => AvailableIfCompletionIsUp(args.TextView);

        bool ICommandHandler<PageUpKeyCommandArgs>.ExecuteCommand(PageUpKeyCommandArgs args, CommandExecutionContext executionContext)
        {
            var session = Broker.GetSession(args.TextView) as AsyncCompletionSession; // we are accessing an internal method
            if (session != null)
            {
                session.SelectPageUp();
                return true;
            }
            return false;
        }

        CommandState ICommandHandler<UpKeyCommandArgs>.GetCommandState(UpKeyCommandArgs args)
            => AvailableIfCompletionIsUp(args.TextView);

        bool ICommandHandler<UpKeyCommandArgs>.ExecuteCommand(UpKeyCommandArgs args, CommandExecutionContext executionContext)
        {
            var session = Broker.GetSession(args.TextView) as AsyncCompletionSession; // we are accessing an internal method
            if (session != null)
            {
                session.SelectUp();
                return true;
            }
            return false;
        }
    }
}
