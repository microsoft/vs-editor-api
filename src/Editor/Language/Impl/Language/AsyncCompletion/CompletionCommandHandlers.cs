using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Commanding;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion.Data;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Editor.Commanding.Commands;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Text.Utilities;
using Microsoft.VisualStudio.Utilities;
using CommonImplementation = Microsoft.VisualStudio.Language.Intellisense.Implementation;

namespace Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion.Implementation
{
    /// <summary>
    /// Reacts to the down arrow command and attempts to scroll the completion list.
    /// </summary>
    [Name(PredefinedCompletionNames.CompletionCommandHandler)]
    [ContentType("text")]
    [TextViewRole(PredefinedTextViewRoles.Interactive)]
    [Export(typeof(ICommandHandler))]
    internal sealed class CompletionCommandHandler :
        IChainedCommandHandler<BackspaceKeyCommandArgs>,
        IDynamicCommandHandler<BackspaceKeyCommandArgs>,
        ICommandHandler<CommitUniqueCompletionListItemCommandArgs>,
        ICommandHandler<CutCommandArgs>,
        IChainedCommandHandler<DeleteKeyCommandArgs>,
        IDynamicCommandHandler<DeleteKeyCommandArgs>,
        ICommandHandler<DownKeyCommandArgs>,
        ICommandHandler<EscapeKeyCommandArgs>,
        IDynamicCommandHandler<EscapeKeyCommandArgs>,
        ICommandHandler<InsertSnippetCommandArgs>,
        ICommandHandler<InvokeCompletionListCommandArgs>,
        ICommandHandler<PageDownKeyCommandArgs>,
        ICommandHandler<PageUpKeyCommandArgs>,
        ICommandHandler<PasteCommandArgs>,
        ICommandHandler<RedoCommandArgs>,
        ICommandHandler<RenameCommandArgs>,
        IChainedCommandHandler<ReturnKeyCommandArgs>,
        IDynamicCommandHandler<ReturnKeyCommandArgs>,
        ICommandHandler<SaveCommandArgs>,
        ICommandHandler<SelectAllCommandArgs>,
        ICommandHandler<SurroundWithCommandArgs>,
        IChainedCommandHandler<TabKeyCommandArgs>,
        IDynamicCommandHandler<TabKeyCommandArgs>,
        ICommandHandler<ToggleCompletionModeCommandArgs>,
        IChainedCommandHandler<TypeCharCommandArgs>,
        IDynamicCommandHandler<TypeCharCommandArgs>,
        ICommandHandler<UndoCommandArgs>,
        ICommandHandler<UpKeyCommandArgs>,
        ICommandHandler<WordDeleteToEndCommandArgs>,
        ICommandHandler<WordDeleteToStartCommandArgs>
    {
        [Import]
        private IAsyncCompletionBroker Broker;

        [Import]
        private ITextUndoHistoryRegistry UndoHistoryRegistry;

        [Import]
        private IEditorOperationsFactoryService EditorOperationsFactoryService;

        [Import]
        private CompletionAvailabilityUtility CompletionAvailability;

        string INamed.DisplayName => CommonImplementation.Strings.CompletionCommandHandlerName;

        /// <summary>
        /// Helper method that returns command state for commands
        /// which are available as long as the completion feature is available.
        /// </summary>
        private CommandState GetCommandStateIfCompletionIsAvailable(IContentType contentType, ITextView textView)
        {
            return CompletionAvailability.IsAvailable(contentType, textView)
                ? CommandState.Available
                : CommandState.Unspecified;
        }

        /// <summary>
        /// Helper method that returns command state for commands
        /// which are available IF AND ONLY IF completion is active,
        /// even if the commands would be otherwise unavailable.
        /// </summary>
        private CommandState GetCommandStateIfCompletionIsActive(ITextView textView)
        {
            return Broker.IsCompletionActive(textView)
                ? CommandState.Available
                : CommandState.Unspecified;
        }

        /// <summary>
        /// Helper method that returns command state for commands
        /// which are available when completion is either currently active, or available.
        /// This is used by commands that may trigger completion session on a specified buffer, or interact with an active completion session on another buffer
        /// </summary>
        private CommandState GetCommandStateIfCompletionIsActiveOrAvailable(IContentType contentType, ITextView textView)
        {
            return Broker.IsCompletionActive(textView) || CompletionAvailability.IsAvailable(contentType, textView)
                ? CommandState.Available
                : CommandState.Unspecified;
        }

        /// <summary>
        /// Helper method that returns command state for the suggestion mode toggle button.
        /// This command state controls not only whether the toggle button is enabled, but also if it's toggled.
        /// </summary>
        private CommandState GetCommandStateForSuggestionModeToggle(IContentType contentType, ITextView textView)
        {
            var isAvailable = CompletionAvailability.IsAvailable(contentType, textView);
            var isChecked = CompletionUtilities.GetSuggestionModeOption(textView);
            return new CommandState(isAvailable, isChecked);
        }

        /// <summary>
        /// Realizes the virtual space and updates session's applicable to span.
        /// We invoke this method after the session has triggered, because we don't want to act if there would be no completion.
        /// </summary>
        private void RealizeVirtualSpaceUpdateApplicableToSpan(IAsyncCompletionSessionOperations session, ITextView textView)
        {
            if (session == null // We may only act if we have internal reference to the session
                || !textView.Caret.InVirtualSpace // We only act if caret is in virtual space
                || !session.ApplicableToSpan.GetSpan(textView.TextSnapshot).IsEmpty) // We only act if the applicable to span is of zero length (at the beginning of the line)
            {
                return;
            }

            // Realize the virtual space before triggering the session by inserting nothing through the editor opertaions.
            IEditorOperations editorOperations = EditorOperationsFactoryService.GetEditorOperations(textView);
            editorOperations?.InsertText("");

            // ApplicableToSpan just grew to include the realized white space.
            // We know that ApplicableToSpan was zero length, so let's recreate a zero length span at the caret location.
            // This method executed synchronously, and therefore we know that it is safe to modify the applicable to span.
            session.ApplicableToSpan = textView.TextSnapshot.CreateTrackingSpan(
                start: textView.Caret.Position.BufferPosition.Position,
                length: 0,
                trackingMode: SpanTrackingMode.EdgeInclusive);
        }

        // ----- Command handlers:

        CommandState IChainedCommandHandler<BackspaceKeyCommandArgs>.GetCommandState(BackspaceKeyCommandArgs args, Func<CommandState> nextCommandHandler)
           => nextCommandHandler();

        bool IDynamicCommandHandler<BackspaceKeyCommandArgs>.CanExecuteCommand(BackspaceKeyCommandArgs args)
            => Broker.IsCompletionActive(args.TextView) || Broker.IsCompletionSupported(args.SubjectBuffer.ContentType);

        void IChainedCommandHandler<BackspaceKeyCommandArgs>.ExecuteCommand(BackspaceKeyCommandArgs args, Action nextCommandHandler, CommandExecutionContext executionContext)
        {
            var snapshotBeforeEdit = args.TextView.TextSnapshot;
            // Execute other commands in the chain to see the change in the buffer.
            nextCommandHandler();

            var session = Broker.GetSession(args.TextView);
            var location = args.TextView.Caret.Position.BufferPosition;
            var trigger = new CompletionTrigger(CompletionTriggerReason.Backspace, snapshotBeforeEdit);

            if (session != null)
            {
                session.OpenOrUpdate(trigger, location, executionContext.OperationContext.UserCancellationToken);
            }
            else
            {
                var newSession = Broker.TriggerCompletion(args.TextView, trigger, location, executionContext.OperationContext.UserCancellationToken);
                newSession?.OpenOrUpdate(trigger, location, executionContext.OperationContext.UserCancellationToken);
            }
        }

        CommandState ICommandHandler<EscapeKeyCommandArgs>.GetCommandState(EscapeKeyCommandArgs args)
            => GetCommandStateIfCompletionIsActive(args.TextView);

        bool IDynamicCommandHandler<EscapeKeyCommandArgs>.CanExecuteCommand(EscapeKeyCommandArgs args)
            => Broker.IsCompletionActive(args.TextView);

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
            => GetCommandStateIfCompletionIsAvailable(args.SubjectBuffer.ContentType, args.TextView);

        bool ICommandHandler<InvokeCompletionListCommandArgs>.ExecuteCommand(InvokeCompletionListCommandArgs args, CommandExecutionContext executionContext)
        {
            if (!GetCommandStateIfCompletionIsAvailable(args.SubjectBuffer.ContentType, args.TextView).IsAvailable)
                return false;

            var trigger = new CompletionTrigger(CompletionTriggerReason.Invoke, args.TextView.TextSnapshot);
            var location = args.TextView.Caret.Position.BufferPosition;
            var session = Broker.TriggerCompletion(args.TextView, trigger, location, executionContext.OperationContext.UserCancellationToken);

            if (session is IAsyncCompletionSessionOperations sessionInternal)
            {
                RealizeVirtualSpaceUpdateApplicableToSpan(sessionInternal, args.TextView);
                location = args.TextView.Caret.Position.BufferPosition; // Buffer may have changed. Update the location.
                session.OpenOrUpdate(trigger, location, executionContext.OperationContext.UserCancellationToken);
                return true;
            }
            return false;
        }

        CommandState ICommandHandler<CommitUniqueCompletionListItemCommandArgs>.GetCommandState(CommitUniqueCompletionListItemCommandArgs args)
            => GetCommandStateIfCompletionIsAvailable(args.SubjectBuffer.ContentType, args.TextView);

        bool ICommandHandler<CommitUniqueCompletionListItemCommandArgs>.ExecuteCommand(CommitUniqueCompletionListItemCommandArgs args, CommandExecutionContext executionContext)
        {
            if (!GetCommandStateIfCompletionIsAvailable(args.SubjectBuffer.ContentType, args.TextView).IsAvailable)
                return false;

            var snapshotBeforeEdit = args.TextView.TextSnapshot;
            var trigger = new CompletionTrigger(CompletionTriggerReason.InvokeAndCommitIfUnique, args.TextView.TextSnapshot);
            var location = args.TextView.Caret.Position.BufferPosition;
            var session = Broker.TriggerCompletion(args.TextView, trigger, location, executionContext.OperationContext.UserCancellationToken);

            if (session is IAsyncCompletionSessionOperations sessionInternal)
            {
                RealizeVirtualSpaceUpdateApplicableToSpan(sessionInternal, args.TextView);
                location = args.TextView.Caret.Position.BufferPosition; // Buffer may have changed. Update the location.
                sessionInternal.InvokeAndCommitIfUnique(trigger, location, executionContext.OperationContext.UserCancellationToken);
                return true;
            }
            return false;
        }

        CommandState ICommandHandler<InsertSnippetCommandArgs>.GetCommandState(InsertSnippetCommandArgs args)
            => CommandState.Unspecified;

        bool ICommandHandler<InsertSnippetCommandArgs>.ExecuteCommand(InsertSnippetCommandArgs args, CommandExecutionContext executionContext)
        {
            Broker.GetSession(args.TextView)?.Dismiss();
            return false;
        }

        CommandState ICommandHandler<SurroundWithCommandArgs>.GetCommandState(SurroundWithCommandArgs args)
            => CommandState.Unspecified;

        bool ICommandHandler<SurroundWithCommandArgs>.ExecuteCommand(SurroundWithCommandArgs args, CommandExecutionContext executionContext)
        {
            Broker.GetSession(args.TextView)?.Dismiss();
            return false;
        }

        CommandState ICommandHandler<ToggleCompletionModeCommandArgs>.GetCommandState(ToggleCompletionModeCommandArgs args)
            => GetCommandStateForSuggestionModeToggle(args.SubjectBuffer.ContentType, args.TextView);

        bool ICommandHandler<ToggleCompletionModeCommandArgs>.ExecuteCommand(ToggleCompletionModeCommandArgs args, CommandExecutionContext executionContext)
        {
            var toggledValue = !CompletionUtilities.GetSuggestionModeOption(args.TextView);
            CompletionUtilities.SetSuggestionModeOption(args.TextView, toggledValue);

            if (Broker.GetSession(args.TextView) is IAsyncCompletionSessionOperations sessionInternal) // we are accessing an internal method
            {
                sessionInternal.SetSuggestionMode(toggledValue);
                return true;
            }
            return false;
        }

        CommandState IChainedCommandHandler<DeleteKeyCommandArgs>.GetCommandState(DeleteKeyCommandArgs args, Func<CommandState> nextCommandHandler)
            => nextCommandHandler();

        bool IDynamicCommandHandler<DeleteKeyCommandArgs>.CanExecuteCommand(DeleteKeyCommandArgs args)
            => Broker.IsCompletionActive(args.TextView) || Broker.IsCompletionSupported(args.SubjectBuffer.ContentType);

        void IChainedCommandHandler<DeleteKeyCommandArgs>.ExecuteCommand(DeleteKeyCommandArgs args, Action nextCommandHandler, CommandExecutionContext executionContext)
        {
            var snapshotBeforeEdit = args.TextView.TextSnapshot;
            // Execute other commands in the chain to see the change in the buffer.
            nextCommandHandler();

            var session = Broker.GetSession(args.TextView);
            var location = args.TextView.Caret.Position.BufferPosition;
            var trigger = new CompletionTrigger(CompletionTriggerReason.Deletion, snapshotBeforeEdit);

            if (session != null)
            {
                session.OpenOrUpdate(trigger, location, executionContext.OperationContext.UserCancellationToken);
            }
            else
            {
                var newSession = Broker.TriggerCompletion(args.TextView, trigger, location, executionContext.OperationContext.UserCancellationToken);
                newSession?.OpenOrUpdate(trigger, location, executionContext.OperationContext.UserCancellationToken);
            }
        }

        CommandState ICommandHandler<WordDeleteToEndCommandArgs>.GetCommandState(WordDeleteToEndCommandArgs args)
            => CommandState.Unspecified;

        bool ICommandHandler<WordDeleteToEndCommandArgs>.ExecuteCommand(WordDeleteToEndCommandArgs args, CommandExecutionContext executionContext)
        {
            Broker.GetSession(args.TextView)?.Dismiss();
            return false;
        }

        CommandState ICommandHandler<WordDeleteToStartCommandArgs>.GetCommandState(WordDeleteToStartCommandArgs args)
            => CommandState.Unspecified;

        bool ICommandHandler<WordDeleteToStartCommandArgs>.ExecuteCommand(WordDeleteToStartCommandArgs args, CommandExecutionContext executionContext)
        {
            Broker.GetSession(args.TextView)?.Dismiss();
            return false;
        }

        CommandState ICommandHandler<SaveCommandArgs>.GetCommandState(SaveCommandArgs args)
            => CommandState.Unspecified;

        bool ICommandHandler<SaveCommandArgs>.ExecuteCommand(SaveCommandArgs args, CommandExecutionContext executionContext)
        {
            Broker.GetSession(args.TextView)?.Dismiss();
            return false;
        }

        CommandState ICommandHandler<SelectAllCommandArgs>.GetCommandState(SelectAllCommandArgs args)
            => CommandState.Unspecified;

        bool ICommandHandler<SelectAllCommandArgs>.ExecuteCommand(SelectAllCommandArgs args, CommandExecutionContext executionContext)
        {
            Broker.GetSession(args.TextView)?.Dismiss();
            return false;
        }

        CommandState ICommandHandler<RenameCommandArgs>.GetCommandState(RenameCommandArgs args)
            => CommandState.Unspecified;

        bool ICommandHandler<RenameCommandArgs>.ExecuteCommand(RenameCommandArgs args, CommandExecutionContext executionContext)
        {
            Broker.GetSession(args.TextView)?.Dismiss();
            return false;
        }

        CommandState ICommandHandler<UndoCommandArgs>.GetCommandState(UndoCommandArgs args)
            => CommandState.Unspecified;

        bool ICommandHandler<UndoCommandArgs>.ExecuteCommand(UndoCommandArgs args, CommandExecutionContext executionContext)
        {
            Broker.GetSession(args.TextView)?.Dismiss();
            return false;
        }

        CommandState ICommandHandler<RedoCommandArgs>.GetCommandState(RedoCommandArgs args)
            => CommandState.Unspecified;

        bool ICommandHandler<RedoCommandArgs>.ExecuteCommand(RedoCommandArgs args, CommandExecutionContext executionContext)
        {
            Broker.GetSession(args.TextView)?.Dismiss();
            return false;
        }

        CommandState ICommandHandler<CutCommandArgs>.GetCommandState(CutCommandArgs args)
            => CommandState.Unspecified;

        bool ICommandHandler<CutCommandArgs>.ExecuteCommand(CutCommandArgs args, CommandExecutionContext executionContext)
        {
            Broker.GetSession(args.TextView)?.Dismiss();
            return false;
        }

        CommandState ICommandHandler<PasteCommandArgs>.GetCommandState(PasteCommandArgs args)
            => CommandState.Unspecified;

        bool ICommandHandler<PasteCommandArgs>.ExecuteCommand(PasteCommandArgs args, CommandExecutionContext executionContext)
        {
            Broker.GetSession(args.TextView)?.Dismiss();
            return false;
        }

        CommandState IChainedCommandHandler<ReturnKeyCommandArgs>.GetCommandState(ReturnKeyCommandArgs args, Func<CommandState> nextCommandHandler)
            => nextCommandHandler();


        bool IDynamicCommandHandler<ReturnKeyCommandArgs>.CanExecuteCommand(ReturnKeyCommandArgs args)
            => Broker.IsCompletionActive(args.TextView) || Broker.IsCompletionSupported(args.SubjectBuffer.ContentType);

        void IChainedCommandHandler<ReturnKeyCommandArgs>.ExecuteCommand(ReturnKeyCommandArgs args, Action nextCommandHandler, CommandExecutionContext executionContext)
        {
            if (!GetCommandStateIfCompletionIsAvailable(args.SubjectBuffer.ContentType, args.TextView).IsAvailable)
            {
                // In IChainedCommandHandler, we have to explicitly call the next command handler
                nextCommandHandler();
                return;
            }
            char typedChar = '\n';

            var session = Broker.GetSession(args.TextView);
            if (session != null)
            {
                var commitBehavior = session.Commit(typedChar, executionContext.OperationContext.UserCancellationToken);
                session.Dismiss();

                // Mark this command as handled (return true),
                // unless extender set the RaiseFurtherCommandHandlers flag - with exception of the debugger text view
                if ((commitBehavior & CommitBehavior.RaiseFurtherReturnKeyAndTabKeyCommandHandlers) == 0
                    || CompletionUtilities.IsDebuggerTextView(args.TextView))
                    return;
            }

            var snapshotBeforeEdit = args.TextView.TextSnapshot;
            nextCommandHandler();

            // Buffer has changed. Update it for when we try to trigger new session.
            var location = args.TextView.Caret.Position.BufferPosition;

            var trigger = new CompletionTrigger(CompletionTriggerReason.Insertion, snapshotBeforeEdit, typedChar);
            var newSession = Broker.TriggerCompletion(args.TextView, trigger, location, executionContext.OperationContext.UserCancellationToken);
            if (newSession is IAsyncCompletionSessionOperations sessionInternal)
            {
                RealizeVirtualSpaceUpdateApplicableToSpan(sessionInternal, args.TextView);
            }
            location = args.TextView.Caret.Position.BufferPosition; // Buffer may have changed. Update the location.
            newSession?.OpenOrUpdate(trigger, location, executionContext.OperationContext.UserCancellationToken);
        }

        CommandState IChainedCommandHandler<TabKeyCommandArgs>.GetCommandState(TabKeyCommandArgs args, Func<CommandState> nextCommandHandler)
            => nextCommandHandler();

        bool IDynamicCommandHandler<TabKeyCommandArgs>.CanExecuteCommand(TabKeyCommandArgs args)
            => Broker.IsCompletionActive(args.TextView) || Broker.IsCompletionSupported(args.SubjectBuffer.ContentType);

        void IChainedCommandHandler<TabKeyCommandArgs>.ExecuteCommand(TabKeyCommandArgs args, Action nextCommandHandler, CommandExecutionContext executionContext)
        {
            if (!GetCommandStateIfCompletionIsAvailable(args.SubjectBuffer.ContentType, args.TextView).IsAvailable)
            {
                // In IChainedCommandHandler, we have to explicitly call the next command handler
                nextCommandHandler();
                return;
            }
            char typedChar = '\t';

            var session = Broker.GetSession(args.TextView);
            if (session != null)
            {
                var commitBehavior = session.Commit(typedChar, executionContext.OperationContext.UserCancellationToken);
                session.Dismiss();

                // Mark this command as handled (return true),
                // unless extender set the RaiseFurtherCommandHandlers flag - with exception of the debugger text view
                if ((commitBehavior & CommitBehavior.RaiseFurtherReturnKeyAndTabKeyCommandHandlers) == 0
                    || CompletionUtilities.IsDebuggerTextView(args.TextView))
                    return;
            }
            var snapshotBeforeEdit = args.TextView.TextSnapshot;
            nextCommandHandler();

            // Buffer has changed. Update it for when we try to trigger new session.
            var location = args.TextView.Caret.Position.BufferPosition;

            var trigger = new CompletionTrigger(CompletionTriggerReason.Insertion, snapshotBeforeEdit, typedChar);
            var newSession = Broker.TriggerCompletion(args.TextView, trigger, location, executionContext.OperationContext.UserCancellationToken);
            newSession?.OpenOrUpdate(trigger, location, executionContext.OperationContext.UserCancellationToken);
        }

        CommandState IChainedCommandHandler<TypeCharCommandArgs>.GetCommandState(TypeCharCommandArgs args, Func<CommandState> nextCommandHandler)
            => nextCommandHandler();

        bool IDynamicCommandHandler<TypeCharCommandArgs>.CanExecuteCommand(TypeCharCommandArgs args)
            => CompletionAvailability.IsAvailable(args.SubjectBuffer.ContentType, args.TextView);

        void IChainedCommandHandler<TypeCharCommandArgs>.ExecuteCommand(TypeCharCommandArgs args, Action nextCommandHandler, CommandExecutionContext executionContext)
        {
            if (!GetCommandStateIfCompletionIsAvailable(args.SubjectBuffer.ContentType, args.TextView).IsAvailable)
            {
                // In IChainedCommandHandler, we have to explicitly call the next command handler
                nextCommandHandler();
                return;
            }

            var view = args.TextView;
            var location = view.Caret.Position.BufferPosition;
            var initialTextSnapshot = args.SubjectBuffer.CurrentSnapshot;

            // Note regarding undo: When completion and brace completion happen together, completion should be first on the undo stack.
            // Effectively, we want to first undo the completion, leaving brace completion intact. Second undo should undo brace completion.
            // To achieve this, we create a transaction in which we commit and reapply brace completion (via nextCommandHandler).
            // Please read "Note regarding undo" comments in this method that explain the implementation choices.
            // Hopefully an upcoming upgrade of the undo mechanism will allow us to undo out of order and vastly simplify this method.

            // Note regarding undo: In a corner case of typing closing brace over existing closing brace,
            // Roslyn brace completion does not perform an edit. It moves the caret outside of session's applicable span,
            // which dismisses the session. Put the session in a state where it will not dismiss when caret leaves the applicable span.
            var sessionToCommit = Broker.GetSession(args.TextView);
            if (sessionToCommit != null)
            {
                ((AsyncCompletionSession)sessionToCommit).IgnoreCaretMovement(ignore: true);
            }

            var snapshotBeforeEdit = args.TextView.TextSnapshot;
            // Execute other commands in the chain to see the change in the buffer. This includes brace completion.
            // Note regarding undo: This will be 2nd in the undo stack
            nextCommandHandler();

            // if on different version than initialTextSnapshot, we will NOT rollback and we will NOT replay the nextCommandHandler
            // DP to figure out why ShouldCommit returns false or Commit doesn't do anything
            var braceCompletionSpecialHandling = args.SubjectBuffer.CurrentSnapshot.Version == initialTextSnapshot.Version;

            // Pass location from before calling nextCommandHandler
            // so that extenders get the same view of the buffer in both ShouldCommit and Commit
            if (sessionToCommit?.ShouldCommit(args.TypedChar, location, executionContext.OperationContext.UserCancellationToken) == true)
            {
                // Buffer has changed, update the snapshot
                location = view.Caret.Position.BufferPosition;

                // Note regarding undo: this transaction will be 1st in the undo stack
                using (var undoTransaction = new CaretPreservingEditTransaction("Completion", view, UndoHistoryRegistry, EditorOperationsFactoryService))
                {
                    if (!braceCompletionSpecialHandling)
                        UndoUtilities.RollbackToBeforeTypeChar(initialTextSnapshot, args.SubjectBuffer);
                    // Now the buffer doesn't have the commit character nor the matching brace, if any

                    var commitBehavior = sessionToCommit.Commit(args.TypedChar, executionContext.OperationContext.UserCancellationToken);

                    if (!braceCompletionSpecialHandling && (commitBehavior & CommitBehavior.SuppressFurtherTypeCharCommandHandlers) == 0)
                        nextCommandHandler(); // Replay the key, so that we get brace completion.

                    // Complete the transaction before stopping it.
                    undoTransaction.Complete();
                }
            }

            // Restore the default state where session dismisses when caret is outside of the applicable span.
            if (sessionToCommit != null)
            {
               ((AsyncCompletionSession)sessionToCommit).IgnoreCaretMovement(ignore: false);
            }

            // Buffer might have changed. Update it for when we try to trigger new session.
            location = view.Caret.Position.BufferPosition;

            var trigger = new CompletionTrigger(CompletionTriggerReason.Insertion, snapshotBeforeEdit, args.TypedChar);
            var session = Broker.GetSession(args.TextView);
            if (session != null)
            {
                session.OpenOrUpdate(trigger, location, executionContext.OperationContext.UserCancellationToken);
            }
            else
            {
                var newSession = Broker.TriggerCompletion(args.TextView, trigger, location, executionContext.OperationContext.UserCancellationToken);
                newSession?.OpenOrUpdate(trigger, location, executionContext.OperationContext.UserCancellationToken);
            }
        }

        CommandState ICommandHandler<DownKeyCommandArgs>.GetCommandState(DownKeyCommandArgs args)
            => GetCommandStateIfCompletionIsActive(args.TextView);

        bool ICommandHandler<DownKeyCommandArgs>.ExecuteCommand(DownKeyCommandArgs args, CommandExecutionContext executionContext)
        {
            if (Broker.GetSession(args.TextView) is AsyncCompletionSession session) // we are accessing an internal method
            {
                session.SelectDown();
                return true;
            }
            return false;
        }

        CommandState ICommandHandler<PageDownKeyCommandArgs>.GetCommandState(PageDownKeyCommandArgs args)
            => GetCommandStateIfCompletionIsActive(args.TextView);

        bool ICommandHandler<PageDownKeyCommandArgs>.ExecuteCommand(PageDownKeyCommandArgs args, CommandExecutionContext executionContext)
        {
            if (Broker.GetSession(args.TextView) is AsyncCompletionSession session) // we are accessing an internal method
            {
                session.SelectPageDown();
                return true;
            }
            return false;
        }

        CommandState ICommandHandler<PageUpKeyCommandArgs>.GetCommandState(PageUpKeyCommandArgs args)
            => GetCommandStateIfCompletionIsActive(args.TextView);

        bool ICommandHandler<PageUpKeyCommandArgs>.ExecuteCommand(PageUpKeyCommandArgs args, CommandExecutionContext executionContext)
        {
            if (Broker.GetSession(args.TextView) is AsyncCompletionSession session) // we are accessing an internal method
            {
                session.SelectPageUp();
                return true;
            }
            return false;
        }

        CommandState ICommandHandler<UpKeyCommandArgs>.GetCommandState(UpKeyCommandArgs args)
            => GetCommandStateIfCompletionIsActive(args.TextView);

        bool ICommandHandler<UpKeyCommandArgs>.ExecuteCommand(UpKeyCommandArgs args, CommandExecutionContext executionContext)
        {
            if (Broker.GetSession(args.TextView) is AsyncCompletionSession session) // we are accessing an internal method
            {
                session.SelectUp();
                return true;
            }
            return false;
        }
    }
}
