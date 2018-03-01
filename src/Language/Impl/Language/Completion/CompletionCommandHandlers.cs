using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Commanding;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Editor.Commanding.Commands;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Text.Utilities;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.Language.Intellisense.Implementation
{
    /// <summary>
    /// Reacts to the down arrow command and attempts to scroll the completion list.
    /// </summary>
    [Name(KnownCompletionNames.CompletionCommandHandlers)]
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
        IAsyncCompletionBroker Broker;

        [Import]
        IExperimentationServiceInternal ExperimentationService;

        [Import]
        ITextUndoHistoryRegistry UndoHistoryRegistry;

        [Import]
        IEditorOperationsFactoryService EditorOperationsFactoryService;

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
            var applicableSpan = Broker.ShouldTriggerCompletion(args.TextView, default(char), location);
            if (applicableSpan.HasValue)
            {
                var session = Broker.TriggerCompletion(args.TextView, applicableSpan.Value);
                session?.OpenOrUpdate(args.TextView, trigger, location);
                return true;
            }
            return false;
        }

        CommandState ICommandHandler<CommitUniqueCompletionListItemCommandArgs>.GetCommandState(CommitUniqueCompletionListItemCommandArgs args)
            => Available(args.TextView);

        bool ICommandHandler<CommitUniqueCompletionListItemCommandArgs>.ExecuteCommand(CommitUniqueCompletionListItemCommandArgs args, CommandExecutionContext executionContext)
        {
            var trigger = new CompletionTrigger(CompletionTriggerReason.InvokeAndCommitIfUnique);
            var location = args.TextView.Caret.Position.BufferPosition;
            var applicableSpan = Broker.ShouldTriggerCompletion(args.TextView, default(char), location);
            if (applicableSpan.HasValue)
            {
                var session = Broker.TriggerCompletion(args.TextView, applicableSpan.Value) as AsyncCompletionSession;
                session?.InvokeAndCommitIfUnique(args.TextView, trigger, location, executionContext.OperationContext.UserCancellationToken);
                return true;
            }
            return false;
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
                return true; // TODO: Investigate. If we return false, we get called again. No matter what we return, the button in the UI does not update. 
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
                session.Dismiss();
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
                session.Dismiss();
                return true;
            }
            return false;
        }

        CommandState IChainedCommandHandler<TypeCharCommandArgs>.GetCommandState(TypeCharCommandArgs args, Func<CommandState> nextCommandHandler)
            => Available(args.TextView);

        void IChainedCommandHandler<TypeCharCommandArgs>.ExecuteCommand(TypeCharCommandArgs args, Action nextCommandHandler, CommandExecutionContext executionContext)
        {
            var initialTextSnapshot = args.TextView.TextSnapshot;

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
                using (var undoTransaction = new CaretPreservingEditTransaction("Completion", view, UndoHistoryRegistry, EditorOperationsFactoryService))
                {
                    UndoUtilities.RollbackToBeforeTypeChar(initialTextSnapshot, args.SubjectBuffer);
                    // Now the buffer doesn't have the commit character nor the matching brace, if any

                    var customBehavior = sessionToCommit.Commit(executionContext.OperationContext.UserCancellationToken, args.TypedChar);

                    if ((customBehavior & CustomCommitBehavior.SurpressFurtherCommandHandlers) == 0)
                        nextCommandHandler(); // Replay the key, so that we get brace completion.

                    // Complete the transaction before stopping it.
                    undoTransaction.Complete();
                }
                // Snapshot has changed when committing. Update it for when we try to trigger new session.
                location = view.Caret.Position.BufferPosition;
            }

            var trigger = new CompletionTrigger(CompletionTriggerReason.Insertion, args.TypedChar);
            var session = Broker.GetSession(args.TextView);
            if (session != null)
            {
                session.OpenOrUpdate(view, trigger, location);
            }
            else
            {
                var applicableSpan = Broker.ShouldTriggerCompletion(view, args.TypedChar, location);
                if (applicableSpan.HasValue)
                {
                    var newSession = Broker.TriggerCompletion(view, applicableSpan.Value);
                    newSession?.OpenOrUpdate(view, trigger, location);
                }
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
                System.Diagnostics.Debug.WriteLine("Completions's DownKey command handler returns true (handled)");
                return true;
            }
            System.Diagnostics.Debug.WriteLine("Completions's DownKey command handler returns false (unhandled)");
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
                System.Diagnostics.Debug.WriteLine("Completions's UpKey command handler returns true (handled)");
                return true;
            }
            System.Diagnostics.Debug.WriteLine("Completions's UpKey command handler returns false (unhandled)");
            return false;
        }
    }
}
