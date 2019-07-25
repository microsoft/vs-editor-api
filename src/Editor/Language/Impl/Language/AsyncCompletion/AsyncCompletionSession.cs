using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion.Data;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Projection;
using Microsoft.VisualStudio.Text.Utilities;
using Microsoft.VisualStudio.Threading;
using Microsoft.VisualStudio.Utilities;
using Strings = Microsoft.VisualStudio.Language.Intellisense.Implementation.Strings;

namespace Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion.Implementation
{
    /// <summary>
    /// Holds a state of the session
    /// and a reference to the UI element
    /// </summary>
    internal class AsyncCompletionSession : IAsyncCompletionSession, IAsyncCompletionSessionOperations2, IModelComputationCallbackHandler<CompletionModel>
    {
        // Available data and services
        private readonly IList<(IAsyncCompletionSource Source, SnapshotPoint Point)> _completionSources;
        private readonly IList<(IAsyncCompletionCommitManager, ITextBuffer)> _commitManagers;
        private readonly IAsyncCompletionItemManager _completionItemManager;
        private readonly JoinableTaskContext JoinableTaskContext;
        private readonly ICompletionPresenterProvider _presenterProvider;
        private readonly AsyncCompletionBroker _broker;
        private readonly ITextView _textView;
        private readonly IGuardedOperations _guardedOperations;
        private readonly ImmutableArray<char> _potentialCommitChars;

        // Presentation:
        private ICompletionPresenter _gui; // Must be accessed from GUI thread
        private readonly int PageStepSize;
        private const int FirstIndex = 0;

        // Computation state machine
        private ModelComputation<CompletionModel> _computation;
        private readonly CancellationTokenSource _computationCancellation = new CancellationTokenSource();
        private int _lastFilteringTaskId;

        // IAsyncCompletionSessionOperations properties for shims
        public bool IsStarted => _computation != null;

        // ------------------------------------------------------------------------
        // Fixed completion model data that is guaranteed not to change when another thread accesses it.
        // Rare exceptions:
        // * Session was triggered in virtual whitespace.
        //      We are in a command handler on the UI thread. We may change ApplicableToSpan until first call to OpenOrUpdate, which begins asynchronous work.

        private ITrackingSpan _applicableToSpan;
        private bool _canChangeApplicableToSpan = true;

        /// <summary>
        /// Span pertinent to this completion.
        /// </summary>
        public ITrackingSpan ApplicableToSpan
        {
            get => _applicableToSpan;
            set
            {
                if (!_canChangeApplicableToSpan)
                    throw new InvalidOperationException($"{nameof(ApplicableToSpan)} may not be changed after completion items were received.");
                _applicableToSpan = value ?? throw new ArgumentNullException(nameof(value));
            }
        }

        /// <summary>
        /// Stores the initial reason this session was triggererd.
        /// </summary>
        private CompletionTrigger InitialTrigger { get; set; }

        /// <summary>
        /// Text to display in place of suggestion mode when filtered text is empty.
        /// </summary>
        private SuggestionItemOptions SuggestionItemOptions { get; set; }

        /// <summary>
        /// Source that will provide tooltip for the suggestion item.
        /// </summary>
        private IAsyncCompletionSource SuggestionModeCompletionItemSource { get; set; }

        // ------------------------------------------------------------------------

        /// <summary>
        /// Telemetry aggregator for this session
        /// </summary>
        private readonly CompletionSessionTelemetry _telemetry;

        /// <summary>
        /// Records noteworthy event which led to committing or dismissing
        /// </summary>
        CompletionSessionState _finalSessionState;

        /// <summary>
        /// Self imposed maximum delay for commits due to user double-clicking completion item in the UI
        /// </summary>
        private static readonly TimeSpan MaxCommitDelayWhenClicked = TimeSpan.FromSeconds(1);

        private static SuggestionItemOptions DefaultSuggestionModeOptions = new SuggestionItemOptions(string.Empty, Strings.SuggestionModeDefaultTooltip);

        // Facilitate special experiences
        private bool _selectionModeBeforeNoResultFallback;
        private bool _selectionModeBeforeCaretLocationFallback;
        private bool _inNoResultFallback;
        private bool _inCaretLocationFallback;
        private bool _ignoreCaretMovement;
        private string _previouslySelectedItemText;

        public event EventHandler<CompletionItemEventArgs> ItemCommitted;
        public event EventHandler Dismissed;
        public event EventHandler<ComputedCompletionItemsEventArgs> ItemsUpdated;

        public ITextView TextView => _textView;

        // When set, UI will no longer be updated
        public bool IsDismissed { get; private set; }

        public PropertyCollection Properties { get; }

        public AsyncCompletionSession(
            SnapshotSpan initialApplicableToSpan,
            ImmutableArray<char> potentialCommitChars,
            JoinableTaskContext joinableTaskContext,
            ICompletionPresenterProvider presenterProvider,
            IList<(IAsyncCompletionSource, SnapshotPoint)> completionSources,
            IList<(IAsyncCompletionCommitManager, ITextBuffer)> commitManagers,
            IAsyncCompletionItemManager completionService,
            AsyncCompletionBroker broker,
            ITextView textView,
            CompletionSessionTelemetry telemetry,
            IGuardedOperations guardedOperations)
        {
            _potentialCommitChars = potentialCommitChars;
            JoinableTaskContext = joinableTaskContext;
            _presenterProvider = presenterProvider;
            _broker = broker;
            _completionSources = completionSources; // still prorotype at the momemnt.
            _commitManagers = commitManagers;
            _completionItemManager = completionService;
            _textView = textView;
            _guardedOperations = guardedOperations;
            ApplicableToSpan = initialApplicableToSpan.Snapshot.CreateTrackingSpan(initialApplicableToSpan, SpanTrackingMode.EdgeInclusive);
            _telemetry = telemetry;
            PageStepSize = presenterProvider?.Options.ResultsPerPage ?? 1;
            _textView.Caret.PositionChanged += OnCaretPositionChanged;
            Properties = new PropertyCollection();
            _telemetry.E2EStopwatch.Start();
        }

        /// <summary>
        /// Creates a minimal instance of <see cref="AsyncCompletionSession"/> which is capable of
        /// holding properties and communicating with <see cref="IAsyncCompletionSource"/>s.
        /// This session must be dismissed when no longer needed.
        /// </summary>
        internal static AsyncCompletionSession CreateAggregatingSession(
            SnapshotSpan applicableToSpan,
            JoinableTaskContext joinableTaskContext,
            List<(IAsyncCompletionSource Source, SnapshotPoint Point)> completionSources,
            AsyncCompletionBroker broker,
            ITextView textView,
            CompletionSessionTelemetry telemetry,
            IGuardedOperations guardedOperations)
        {
            return new AsyncCompletionSession(
                applicableToSpan,
                default,
                joinableTaskContext,
                default,
                completionSources,
                default,
                default,
                broker,
                textView,
                telemetry,
                guardedOperations);
        }

        bool IAsyncCompletionSession.ShouldCommit(char typedChar, SnapshotPoint triggerLocation, CancellationToken token)
        {
            if (IsDismissed)
                return false;
            if (!JoinableTaskContext.IsOnMainThread)
                throw new InvalidOperationException($"This method must be callled on the UI thread.");
            if (!_potentialCommitChars.Contains(typedChar))
                return false;

            var rootSnapshot = AsyncCompletionBroker.GetRootSnapshot(TextView);
            var points = MappingHelper.GetPointsAtLocation(triggerLocation, rootSnapshot);
            for (int i = 0; i < _commitManagers.Count; i++)
            {
                var commitManager = _commitManagers[i].Item1;

                // Among current SnapshotPoints, pick the one which matches the commit manager's buffer
                SnapshotPoint relevantPoint = default;
                var pointsEnumerator = points.GetEnumerator();
                while (pointsEnumerator.MoveNext())
                {
                    if (pointsEnumerator.Current.Snapshot.TextBuffer == _commitManagers[i].Item2)
                    {
                        relevantPoint = pointsEnumerator.Current;
                        break;
                    }
                }
                if (relevantPoint == default)
                    continue;

                var shouldCommit = _guardedOperations.CallExtensionPoint(
                    errorSource: commitManager,
                    call: () => commitManager.ShouldCommitCompletion(this, relevantPoint, typedChar, token),
                    valueOnThrow: false);

                if (shouldCommit)
                    return true;
            }
            return false;
        }

        bool IAsyncCompletionSession.CommitIfUnique(CancellationToken token)
        {
            if (IsDismissed)
                return false;

            if (!JoinableTaskContext.IsOnMainThread)
                throw new InvalidOperationException($"This method must be callled on the UI thread.");

            _telemetry.UiStopwatch.Restart();
            var lastModel = _computation.WaitAndGetResult(cancelUi: true, token);
            _telemetry.UiStopwatch.Stop();
            _telemetry.RecordBlockingWaitForComputation(_telemetry.UiStopwatch.ElapsedMilliseconds);

            if (lastModel == null)
            {
                return false;
            }
            else if (lastModel.Uninitialized)
            {
                return false;
            }
            else if (lastModel.UniqueItem != null)
            {
                _finalSessionState = CompletionSessionState.CommittedThroughCompleteWord;
                var behavior = CommitItem(default, lastModel.UniqueItem, ApplicableToSpan, token);
                if (behavior == CommitBehavior.CancelCommit)
                {
                    // Show the UI, because waitAndGetResult canceled showing the UI.
                    UpdateUiInner(lastModel); // We are on the UI thread, so we may call UpdateUiInner
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else if (!lastModel.PresentedItems.IsDefaultOrEmpty && lastModel.PresentedItems.Length == 1)
            {
                _finalSessionState = CompletionSessionState.CommittedThroughCompleteWord;
                var behavior = CommitItem(default, lastModel.PresentedItems[0].CompletionItem, ApplicableToSpan, token);
                if (behavior == CommitBehavior.CancelCommit)
                {
                    // Show the UI, because waitAndGetResult canceled showing the UI.
                    UpdateUiInner(lastModel); // We are on the UI thread, so we may call UpdateUiInner
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                // Show the UI, because waitAndGetResult canceled showing the UI.
                UpdateUiInner(lastModel); // We are on the UI thread, so we may call UpdateUiInner
                return false;
            }
        }

        CommitBehavior IAsyncCompletionSession.Commit(char typedChar, CancellationToken token)
        {
            if (IsDismissed)
                return CommitBehavior.None;

            if (!JoinableTaskContext.IsOnMainThread)
                throw new InvalidOperationException($"This method must be callled on the UI thread.");

            // We are in either low latency mode or suggestion mode
            // user did not press tab, and we don't have results yet
            // => dismiss
            if ((_computation.RecentModel == default || _computation.RecentModel.Uninitialized)
                && (CompletionUtilities.GetSuggestionModeOption(_textView) || CompletionUtilities.GetNonBlockingCompletionOption(_textView))
                && !(typedChar.Equals(default) || typedChar.Equals('\t')))
            {
                _finalSessionState = CompletionSessionState.DismissedDueToNonBlockingMode;
                ((IAsyncCompletionSession)this).Dismiss();
                return CommitBehavior.RaiseFurtherReturnKeyAndTabKeyCommandHandlers;
            }

            CompletionModel lastModel;

            // We are in low latency mode
            // => use recently computed model, but don't block waiting for one
            if (CompletionUtilities.GetNonBlockingCompletionOption(_textView))
            {
                lastModel = _computation.RecentModel;
            }
            else
            {
                _telemetry.UiStopwatch.Restart();
                lastModel = _computation.WaitAndGetResult(cancelUi: true, token);
                _telemetry.UiStopwatch.Stop();
                _telemetry.RecordBlockingWaitForComputation(_telemetry.UiStopwatch.ElapsedMilliseconds);
            }

            if (lastModel == null)
            {
                // Typically, we return default model when the token is canceled.
                // To provide accurate telemetry, check the tokens
                if (token.IsCancellationRequested || _computationCancellation.IsCancellationRequested)
                {
                    _finalSessionState = CompletionSessionState.DismissedDueToCancellation;
                }
                ((IAsyncCompletionSession)this).Dismiss();
                return CommitBehavior.None;
            }
            else if (lastModel.Uninitialized)
            {
                ((IAsyncCompletionSession)this).Dismiss();
                return CommitBehavior.RaiseFurtherReturnKeyAndTabKeyCommandHandlers;
            }
            else if (lastModel.UseSoftSelection
                && !(typedChar.Equals(default)
                    || typedChar.Equals('\t')
                    || TypedCharShouldNotDismissInSoftSelection(typedChar)))
            {
                // In soft selection mode, allow commit under the following circumstances:
                // 1. User commits explicitly (click, tab)
                // 2. User typed a character which is excluded from list of potential commit characters in the given session
                // Otherwise, dismiss the session
                ((IAsyncCompletionSession)this).Dismiss();
                return CommitBehavior.RaiseFurtherReturnKeyAndTabKeyCommandHandlers;
            }
            else if (lastModel.SelectSuggestionItem && string.IsNullOrWhiteSpace(lastModel.SuggestionItem?.InsertText))
            {
                // When suggestion mode is selected, don't commit empty suggestion
                return CommitBehavior.None;
            }
            else if (lastModel.SelectSuggestionItem)
            {
                // Commit the suggestion mode item
                _finalSessionState = CompletionSessionState.CommittedSuggestionItem;
                return CommitItem(typedChar, lastModel.SuggestionItem, ApplicableToSpan, token);
            }
            else if (lastModel.PresentedItems.IsDefaultOrEmpty)
            {
                // There is nothing to commit
                Dismiss();
                return CommitBehavior.None;
            }
            else
            {
                // Regular commit
                _finalSessionState = CompletionSessionState.Committed;
                return CommitItem(typedChar, lastModel.PresentedItems[lastModel.SelectedIndex].CompletionItem, ApplicableToSpan, token);
            }
        }

        private CommitBehavior CommitItem(char typedChar, CompletionItem itemToCommit, ITrackingSpan applicableToSpan, CancellationToken token)
        {
            CommitBehavior behavior = CommitBehavior.None;
            if (IsDismissed)
                return behavior;

            _telemetry.UiStopwatch.Restart();
            IAsyncCompletionCommitManager managerWhoCommitted = null;

            bool commitHandled = false;
            foreach (var commitManager in _commitManagers)
            {
                var commitResult = _guardedOperations.CallExtensionPoint(
                    errorSource: commitManager,
                    call: () => commitManager.Item1.TryCommit(this, commitManager.Item2 /* buffer */, itemToCommit, typedChar, token),
                    valueOnThrow: CommitResult.Unhandled);

                if (commitResult.Behavior == CommitBehavior.CancelCommit)
                {
                    // Return quickly without dismissing.
                    // Return this behavior so that CommitIfUnique displays the UI
                    _telemetry.UiStopwatch.Stop();
                    return commitResult.Behavior;
                }

                if (behavior == CommitBehavior.None) // Don't override behavior returned by higher priority commit manager
                    behavior = commitResult.Behavior;

                commitHandled |= commitResult.IsHandled;
                if (commitResult.IsHandled)
                {
                    managerWhoCommitted = commitManager.Item1;
                    break;
                }
                if (token.IsCancellationRequested)
                {
                    break;
                }
            }
            if (!commitHandled && !token.IsCancellationRequested)
            {
                // Fallback if item is still not committed.
                InsertIntoBuffer(_textView, applicableToSpan, itemToCommit.InsertText);
            }

            _telemetry.UiStopwatch.Stop();
            _telemetry.E2EStopwatch.Stop();

            _guardedOperations.RaiseEvent(this, ItemCommitted, new CompletionItemEventArgs(itemToCommit));
            _telemetry.RecordCommitted(_telemetry.UiStopwatch.ElapsedMilliseconds, managerWhoCommitted);

            Dismiss();

            return behavior;
        }

        private static void InsertIntoBuffer(ITextView view, ITrackingSpan applicableToSpan, string insertText)
        {
            var buffer = view.TextBuffer;
            var replacedSpan = applicableToSpan.GetSpan(buffer.CurrentSnapshot);

            // If edit would be effectively a no-op, leave early so we don't create a no-op undo operation
            var replacedText = replacedSpan.GetText();
            if (insertText.Equals(replacedText, StringComparison.Ordinal))
                return;

            // If the commit is a result of typing a commit character, the handler of TypeCharCommandArgs must replay typing:
            // At this instant, ApplicableToSpan already contains the typed char and braces added by the Brace Completion feature.
            // Replacing this span will forfeit the braces. Therefore, the typing must be replayed so that the matching brace is inserted.
            var bufferEdit = buffer.CreateEdit();
            bufferEdit.Replace(replacedSpan, insertText);
            bufferEdit.Apply();
        }

        private bool TypedCharShouldNotDismissInSoftSelection(char typedChar)
        {
            if (typedChar == default)
                return false;
            if (Properties.TryGetProperty<ImmutableArray<char>>("ExcludedCommitCharacters", out var excludedChars))
            {
                return excludedChars.Contains(typedChar);
            }
            return false;
        }

        public void Dismiss()
        {
            if (IsDismissed)
                return;

            IsDismissed = true;
            _broker.ForgetSession(this);
            _textView.Caret.PositionChanged -= OnCaretPositionChanged;
            _computationCancellation.Cancel();

            // This method may be invoked on any thread. We promised extenders we will raise Dismissed event on UI thread.
            if (Dismissed != null)
            {
                JoinableTaskContext.Factory.Run(async () =>
                {
                    await JoinableTaskContext.Factory.SwitchToMainThreadAsync();
                    _guardedOperations.RaiseEvent(this, Dismissed);
                });
            }

            if (_gui != null)
            {
                var copyOfGui = _gui;
                _guardedOperations.CallExtensionPointAsync(
                    errorSource: _gui,
                    asyncAction: async () =>
                    {
                        await JoinableTaskContext.Factory.SwitchToMainThreadAsync();
                        _telemetry.UiStopwatch.Restart();
                        copyOfGui.FiltersChanged -= OnFiltersChanged;
                        copyOfGui.CommitRequested -= OnCommitRequested;
                        copyOfGui.CompletionItemSelected -= OnItemSelected;
                        copyOfGui.CompletionClosed -= OnGuiClosed;
                        copyOfGui.Close();
                        _telemetry.UiStopwatch.Stop();
                        _telemetry.RecordClosing(_telemetry.UiStopwatch.ElapsedMilliseconds);

                        await Task.Yield();
                        _telemetry.Save(_completionItemManager, _presenterProvider, _finalSessionState);
                    });
                _gui = null;
            }
            else
            {
                _telemetry.Save(_completionItemManager, _presenterProvider, _finalSessionState);
            }
        }

        void IAsyncCompletionSession.OpenOrUpdate(CompletionTrigger trigger, SnapshotPoint triggerLocation, CancellationToken commandToken)
        {
            if (IsDismissed)
                return;

            if (!JoinableTaskContext.IsOnMainThread)
                throw new InvalidOperationException($"This method must be callled on the UI thread.");

            var rootSnapshot = AsyncCompletionBroker.GetRootSnapshot(TextView);
            commandToken.Register(_computationCancellation.Cancel);

            _canChangeApplicableToSpan = false; // Don't allow changing the ApplicableToSpan from now on.

            if (_computation == null)
            {
                _computation = new ModelComputation<CompletionModel>(
                    PrioritizedTaskScheduler.AboveNormalInstance,
                    JoinableTaskContext,
                    (model, token) => GetInitialModel(trigger, triggerLocation, rootSnapshot, token),
                    _computationCancellation.Token,
                    _guardedOperations,
                    this
                    );
            }

            var taskId = Interlocked.Increment(ref _lastFilteringTaskId);
            _computation.Enqueue((model, token) => UpdateSnapshot(model, trigger, triggerLocation, rootSnapshot, taskId, token), updateUi: true);
        }

        ComputedCompletionItems IAsyncCompletionSession.GetComputedItems(CancellationToken token)
        {
            if (_computation == null)
                return ComputedCompletionItems.Empty; // Call OpenOrUpdate first to kick off computation

            var model = _computation.WaitAndGetResult(cancelUi: true, token); // We don't want user initiated action to hide UI
            return ComputeCompletionItems(model);
        }

        #region IAsyncCompletionSessionOperations implementation

        public void InvokeAndCommitIfUnique(CompletionTrigger trigger, SnapshotPoint triggerLocation, CancellationToken token)
        {
            if (IsDismissed)
                return;

            if (_computation == null)
            {
                // Compute the unique item.
                // Don't recompute If we already have a model, so that we don't change user's selection.
                ((IAsyncCompletionSession)this).OpenOrUpdate(trigger, triggerLocation, token);
            }

            if (((IAsyncCompletionSession)this).CommitIfUnique(token))
            {
                ((IAsyncCompletionSession)this).Dismiss();
            }
        }

        public void SetSuggestionMode(bool useSuggestionMode)
        {
            _computation.Enqueue((model, token) => ToggleCompletionModeInner(model, useSuggestionMode, token), updateUi: true);
        }

        public void SelectDown()
        {
            if (_computation.RecentModel == default || _computation.RecentModel.Uninitialized)
            {
                // https://github.com/dotnet/roslyn/issues/31131 Dismiss completion so that up and down gestures are not blocked
                Dismiss();
            }
            _computation.Enqueue((model, token) => UpdateSelectedItem(model, +1, token), updateUi: true);
        }

        public void SelectPageDown()
        {
            if (_computation.RecentModel == default || _computation.RecentModel.Uninitialized)
            {
                // https://github.com/dotnet/roslyn/issues/31131 Dismiss completion so that up and down gestures are not blocked
                Dismiss();
            }
            _computation.Enqueue((model, token) => UpdateSelectedItem(model, +PageStepSize, token), updateUi: true);
        }

        public void SelectUp()
        {
            if (_computation.RecentModel == default || _computation.RecentModel.Uninitialized)
            {
                // https://github.com/dotnet/roslyn/issues/31131 Dismiss completion so that up and down gestures are not blocked
                Dismiss();
            }
            _computation.Enqueue((model, token) => UpdateSelectedItem(model, -1, token), updateUi: true);
        }

        public void SelectPageUp()
        {
            if (_computation.RecentModel == default || _computation.RecentModel.Uninitialized)
            {
                // https://github.com/dotnet/roslyn/issues/31131 Dismiss completion so that up and down gestures are not blocked
                Dismiss();
            }
            _computation.Enqueue((model, token) => UpdateSelectedItem(model, -PageStepSize, token), updateUi: true);
        }

        public void SelectCompletionItem(CompletionItem item)
        {
            // To prevent inifinite loops, UI interacts with computation using the OnItemSelected event handler
            _computation.Enqueue((model, token) => UpdateSelectedItem(model, item, false, token), updateUi: true);
        }

        #endregion

        #region Internal methods that are implementation specific

        internal void IgnoreCaretMovement(bool ignore)
        {
            if (IsDismissed)
                return; // This method will be called after committing. Don't act on it.

            _ignoreCaretMovement = ignore;
            if (!ignore)
            {
                // Don't let the session exist in invalid state: ensure that the location of the session is still valid
                HandleCaretPositionChanged(_textView.Caret.Position);
            }
        }

        #endregion

        private void OnFiltersChanged(object sender, CompletionFilterChangedEventArgs args)
        {
            var taskId = Interlocked.Increment(ref _lastFilteringTaskId);
            _computation.Enqueue((model, token) => UpdateFilters(model, args.Filters, taskId, token), updateUi: true);
        }

        static bool CheckFilterAccessKey(CompletionFilterWithState filter, string accessKey)
            => string.Equals(
                filter.Filter.AccessKey,
                accessKey,
                StringComparison.OrdinalIgnoreCase);

        public bool CanToggleFilter(string accessKey)
        {
            var recentModel = _computation?.RecentModel;

            if (recentModel == null ||
                recentModel.Uninitialized ||
                recentModel.Filters.IsDefaultOrEmpty)
                return false;

            return recentModel
                .Filters
                .Any(filter => CheckFilterAccessKey(filter, accessKey));
        }

        public void ToggleFilter(string accessKey)
        {
            var taskId = Interlocked.Increment(ref _lastFilteringTaskId);
            _computation.Enqueue((model, token) =>
            {
                var newFilters = model
                    .Filters
                    .Select(filter =>
                    {
                        if (CheckFilterAccessKey(filter, accessKey))
                            return filter.WithSelected(!filter.IsSelected);

                        return filter;
                    })
                    .ToImmutableArray();

                return UpdateFilters(model, newFilters, taskId, token);
            },
            updateUi: true);
        }

        /// <summary>
        /// Handler for GUI requesting commit, usually through double-clicking.
        /// There is no UI for cancellation, so use self-imposed expiration.
        /// </summary>
        private void OnCommitRequested(object sender, CompletionItemEventArgs args)
        {
            try
            {
                if (_computation == null)
                    return;
                var expiringTokenSource = new CancellationTokenSource(MaxCommitDelayWhenClicked);
                _finalSessionState = CompletionSessionState.CommittedThroughClick;
                CommitItem(default, args.Item, ApplicableToSpan, expiringTokenSource.Token);
            }
            catch (Exception ex)
            {
                _guardedOperations.HandleException(this, ex);
            }
        }

        private void OnItemSelected(object sender, CompletionItemSelectedEventArgs args)
        {
            // Note 1: Use this only to react to selection changes initiated by user's mouse\touch operation in the UI, since they cancel the soft selection
            _computation.Enqueue((model, token) => UpdateSelectedItem(model, args.SelectedItem, args.SuggestionItemSelected, token), updateUi: true);
        }

        private void OnGuiClosed(object sender, CompletionClosedEventArgs args)
        {
            _finalSessionState = CompletionSessionState.DismissedThroughUI;
            Dismiss();
        }

        /// <summary>
        /// Monitors when user scrolled outside of the applicable span.
        /// Note that this event is NOT raised during regular typing.
        /// It is raised by brace completion, but at the same time we set _ignoreCaretMovement.
        /// </summary>
        private void OnCaretPositionChanged(object sender, CaretPositionChangedEventArgs e)
        {
            // http://source.roslyn.io/#Microsoft.CodeAnalysis.EditorFeatures/Implementation/IntelliSense/Completion/Controller_CaretPositionChanged.cs,40
            if (_ignoreCaretMovement)
                return;

            HandleCaretPositionChanged(e.NewPosition);
        }

        async Task IModelComputationCallbackHandler<CompletionModel>.UpdateUI(CompletionModel model, CancellationToken token)
        {
            if (_presenterProvider == null) return;
            await JoinableTaskContext.Factory.SwitchToMainThreadAsync(token);
            if (token.IsCancellationRequested) return;
            UpdateUiInner(model);
        }

        /// <summary>
        /// Opens or updates the UI. Must be called on UI thread.
        /// </summary>
        /// <param name="model"></param>
        private void UpdateUiInner(CompletionModel model)
        {
            if (IsDismissed)
                return;
            if (model == null)
                throw new ArgumentNullException(nameof(model));
            if (model.Uninitialized)
                return; // Language service wishes to not show completion yet.
            if (!JoinableTaskContext.IsOnMainThread)
                throw new InvalidOperationException($"This method must be callled on the UI thread.");

            // TODO: Consider building CompletionPresentationViewModel in BG and passing it here
            _telemetry.UiStopwatch.Restart();
            if (_gui == null)
            {
                _gui = _guardedOperations.CallExtensionPoint(errorSource: _presenterProvider, call: () => _presenterProvider.GetOrCreate(_textView), valueOnThrow: null);
                if (_gui != null)
                {
                    _guardedOperations.CallExtensionPoint(
                        errorSource: _gui,
                        call: () =>
                        {
                            _gui = _presenterProvider.GetOrCreate(_textView);
                            _gui.FiltersChanged += OnFiltersChanged;
                            _gui.CommitRequested += OnCommitRequested;
                            _gui.CompletionItemSelected += OnItemSelected;
                            _gui.CompletionClosed += OnGuiClosed;
                            _gui.Open(this, new CompletionPresentationViewModel(model.PresentedItems, model.Filters,
                                model.SelectedIndex, ApplicableToSpan, model.UseSoftSelection, model.DisplaySuggestionItem,
                                model.SelectSuggestionItem, model.SuggestionItem, SuggestionItemOptions));
                            _telemetry.E2EStopwatch.Stop();
                        });
                }
            }
            else
            {
                _guardedOperations.CallExtensionPoint(
                    errorSource: _gui,
                    call: () => _gui.Update(this, new CompletionPresentationViewModel(model.PresentedItems, model.Filters,
                        model.SelectedIndex, ApplicableToSpan, model.UseSoftSelection, model.DisplaySuggestionItem,
                        model.SelectSuggestionItem, model.SuggestionItem, SuggestionItemOptions)));
            }
            _telemetry.UiStopwatch.Stop();
            _telemetry.RecordRendering(_telemetry.UiStopwatch.ElapsedMilliseconds);
        }

        /// <summary>
        /// Gets <see cref="CompletionContext"/> from available <see cref="IAsyncCompletionSource"/>s
        /// </summary>
        /// <returns>Aggregate data built from all received <see cref="CompletionContext"/>s</returns>
        internal async Task<CompletionSourceConnectionResult>ConnectToCompletionSources(CompletionTrigger trigger, SnapshotPoint triggerLocation, ITextSnapshot rootSnapshot, CancellationToken token)
        {
            bool sourceUsesSuggestionMode = false;
            SuggestionItemOptions requestedSuggestionItemOptions = null;
            InitialSelectionHint initialSelectionHint = InitialSelectionHint.RegularSelection;
            var initialItemsBuilder = ImmutableArray.CreateBuilder<CompletionItem>();

            // We use rootSnapshot to obtain buffers that participate in comple
            var points = MappingHelper.GetPointsAtLocation(triggerLocation, rootSnapshot);

            for (int i = 0; i < _completionSources.Count; i++)
            {
                var sourceAndLocation = _completionSources[i]; // Capture the source, since `i` will change during the async call

                _telemetry.ComputationStopwatch.Restart();
                var context = await _guardedOperations.CallExtensionPointAsync(
                    errorSource: sourceAndLocation.Source,
                    asyncCall: () =>
                    {
                        // from the set of points I got above, get the point which matches the buffer
                        var mappedPoint = points.FirstOrDefault(n => n.Snapshot.TextBuffer == sourceAndLocation.Point.Snapshot.TextBuffer);
                        if (mappedPoint == default)
                            return Task.FromResult(CompletionContext.Empty);

                        // Don't use rootSnapshot: ApplicableToSpan is defined on the triggerLocation's snapshot
                        return sourceAndLocation.Source.GetCompletionContextAsync(this, trigger, mappedPoint, ApplicableToSpan.GetSpan(triggerLocation.Snapshot), token);
                    },
                    valueOnThrow: null
                ).ConfigureAwait(true);
                _telemetry.ComputationStopwatch.Stop();
                _telemetry.RecordObtainingSourceContext(sourceAndLocation.Source, _telemetry.ComputationStopwatch.ElapsedMilliseconds);

                if (token.IsCancellationRequested)
                    return CompletionSourceConnectionResult.Canceled;
                if (context == null)
                    continue;

                sourceUsesSuggestionMode |= context.SuggestionItemOptions != null;

                // Set initial selection option, in order of precedence: soft selection, regular selection
                if (context.SelectionHint == InitialSelectionHint.SoftSelection)
                    initialSelectionHint = InitialSelectionHint.SoftSelection;

                if (!context.Items.IsDefaultOrEmpty)
                    initialItemsBuilder.AddRange(context.Items);
                // We use SuggestionModeOptions of the first source that provides it
                if (requestedSuggestionItemOptions == null && context.SuggestionItemOptions != null)
                    requestedSuggestionItemOptions = context.SuggestionItemOptions;
            }
            return new CompletionSourceConnectionResult(sourceUsesSuggestionMode, requestedSuggestionItemOptions, initialSelectionHint, initialItemsBuilder.ToImmutable());
        }

        /// <summary>
        /// Creates a new model and populates it with initial data
        /// </summary>
        private async Task<CompletionModel> GetInitialModel(CompletionTrigger trigger, SnapshotPoint triggerLocation, ITextSnapshot rootSnapshot, CancellationToken token)
        {
            var completionData = await ConnectToCompletionSources(trigger, triggerLocation, rootSnapshot, token).ConfigureAwait(true);

            // Do not continue without items
            if (completionData.IsCanceled)
            {
                return default;
            }
            else if (completionData.InitialCompletionItems.IsDefaultOrEmpty)
            {
                return CompletionModel.GetUninitializedModel(triggerLocation.Snapshot);
            }

            // If no source provided suggestion item options, provide default options for suggestion mode
            SuggestionItemOptions = completionData.RequestedSuggestionItemOptions ?? DefaultSuggestionModeOptions;

            // Store the data that won't change throughout the session
            InitialTrigger = trigger;
            SuggestionModeCompletionItemSource = new SuggestionModeCompletionItemSource(SuggestionItemOptions);

            var availableFilters = completionData.InitialCompletionItems
                .SelectMany(n => n.Filters)
                .Distinct()
                .Select(n => new CompletionFilterWithState(n, true))
                .ToImmutableArray();

            var viewUsesSuggestionMode = CompletionUtilities.GetSuggestionModeOption(_textView);
            var useSuggestionMode = completionData.SourceUsesSuggestionMode || viewUsesSuggestionMode;
            // Select suggestion item only if source explicity provided it. This means that debugger view or ctrl+alt+space won't select the suggestion item.
            var selectSuggestionItem = completionData.SourceUsesSuggestionMode;
            // Use soft selection if suggestion item is present, unless source selects that item. Also, use soft selection if source wants to.
            var useSoftSelection = useSuggestionMode && !selectSuggestionItem || completionData.InitialSelectionHint == InitialSelectionHint.SoftSelection;

            _telemetry.ComputationStopwatch.Restart();
            var sortedList = await _guardedOperations.CallExtensionPointAsync(
                errorSource: _completionItemManager,
                asyncCall: () => _completionItemManager.SortCompletionListAsync(
                    session: this,
                    data: new AsyncCompletionSessionInitialDataSnapshot(completionData.InitialCompletionItems, triggerLocation.Snapshot, InitialTrigger),
                    token: token),
                valueOnThrow: completionData.InitialCompletionItems).ConfigureAwait(true);
            _telemetry.ComputationStopwatch.Stop();
            _telemetry.RecordProcessing(_telemetry.ComputationStopwatch.ElapsedMilliseconds, completionData.InitialCompletionItems.Length);
            _telemetry.RecordKeystroke();

            return new CompletionModel(completionData.InitialCompletionItems, sortedList, triggerLocation.Snapshot,
                availableFilters, useSoftSelection, useSuggestionMode, selectSuggestionItem, suggestionItem: null);
        }

        /// <summary>
        /// User has moved the caret. Ensure that the caret is still within the applicable span. If not, dismiss the session.
        /// </summary>
        private void HandleCaretPositionChanged(CaretPosition caretPosition)
        {
            var currentTaskId = _lastFilteringTaskId;
            _computation?.Enqueue((model, token) => UpdateCaretPosition(model, caretPosition, token), updateUi: true);
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        private async Task<CompletionModel> UpdateCaretPosition(CompletionModel model, CaretPosition caretPosition, CancellationToken token)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            if (token.IsCancellationRequested || model == null)
                return default;

            var caretSnapshot = caretPosition.BufferPosition.Snapshot;
            var immediateCaretPosition = caretPosition.BufferPosition;
            var immediateSpanStart = ApplicableToSpan.GetStartPoint(caretSnapshot);
            CompletionModel updatedModel = model;
            if (!ApplicableToSpan.GetSpan(caretSnapshot).IntersectsWith(new SnapshotSpan(immediateCaretPosition, 0)))
            {
                // Caret is outside of the applicable to span
                _finalSessionState = CompletionSessionState.DismissedDueToCaretLeaving;
                Dismiss();
            }
            else if (immediateCaretPosition == immediateSpanStart && !_inCaretLocationFallback)
            {
                // Caret is at the beginning of the applicable to span; enter the special soft selection mode
                _selectionModeBeforeCaretLocationFallback = model.UseSoftSelection;
                updatedModel = model.WithSoftSelection(true);
                _inCaretLocationFallback = true;
            }
            else if (immediateCaretPosition != immediateSpanStart && _inCaretLocationFallback)
            {
                // Caret is within the applicable to span; leave the special soft selection mode
                updatedModel = model.WithSoftSelection(_selectionModeBeforeCaretLocationFallback);
                _inCaretLocationFallback = false;
            }
            return updatedModel;
        }

        /// <summary>
        /// Sets or unsets suggestion mode.
        /// </summary>
#pragma warning disable CA1822 // Member does not access instance data and can be marked as static
#pragma warning disable CA1801 // Parameter token is never used
        private Task<CompletionModel> ToggleCompletionModeInner(CompletionModel model, bool useSuggestionMode, CancellationToken token)
        {
            return Task.FromResult(model.WithSuggestionItemVisibility(useSuggestionMode));
        }
#pragma warning restore CA1822
#pragma warning restore CA1801

        /// <summary>
        /// User has typed. Update the known snapshot, filter the items and update the model.
        /// </summary>
        private async Task<CompletionModel> UpdateSnapshot(CompletionModel model, CompletionTrigger trigger, SnapshotPoint updateLocation, ITextSnapshot rootSnapshot, int thisId, CancellationToken token)
        {
            // Always record keystrokes, even if filtering is preempted
            _telemetry.RecordKeystroke();

            // Completion got cancelled
            if (token.IsCancellationRequested || model == null)
                return default;

            // Dismiss if we are outside of the applicable span
            var instantaneousSnapshot = updateLocation.Snapshot;
            var currentlyApplicableToSpan = ApplicableToSpan.GetSpan(instantaneousSnapshot);
            if (updateLocation < currentlyApplicableToSpan.Start
                || updateLocation > currentlyApplicableToSpan.End)
            {
                ((IAsyncCompletionSession)this).Dismiss();
                return model;
            }
            // If the applicable to span was empty, is empty again, and user is deleting, then dismiss
            if (currentlyApplicableToSpan.IsEmpty
                && model.ApplicableToSpanWasEmpty
                && (trigger.Reason == CompletionTriggerReason.Deletion || trigger.Reason == CompletionTriggerReason.Backspace))
            {
                _finalSessionState = CompletionSessionState.DismissedDueToBackspace;
                ((IAsyncCompletionSession)this).Dismiss();
                return model;
            }
            // If user is backspacing at the beginning of a span, dismiss
            if (updateLocation == currentlyApplicableToSpan.Start && trigger.Reason == CompletionTriggerReason.Backspace)
            {
                if (_inCaretLocationFallback)
                {
                    // If user was previously at the beginning of the span, this backspace will dismiss completion
                    _finalSessionState = CompletionSessionState.DismissedDueToBackspace;
                    ((IAsyncCompletionSession)this).Dismiss();
                    return model;
                }
                else
                {
                    // Caret just moved to the beginning of the span, enter soft selection
                    _selectionModeBeforeCaretLocationFallback = model.UseSoftSelection;
                    model = model.WithSoftSelection(true);
                    _inCaretLocationFallback = true;
                }
            }

            // Record whether the applicable to span is empty
            model = model.WithApplicableToSpanStatus(currentlyApplicableToSpan.IsEmpty);

            // The model previously received no items, but we are called again because user typed something.
            // There is a chance that language service will provide items this time.
            // Due to timing issues, if we dismiss and start another session, we would miss some user actions.
            // Instead, attempt to get items again within this session.
            if (model.Uninitialized && thisId > 1) // Don't attempt to get items on the very first UpdateSnapshot
            {
                // Attempt to get new completion items
                model = await GetInitialModel(trigger, updateLocation, rootSnapshot, token).ConfigureAwait(true);
                if (model == default) // This happens when computation has been cancelled
                {
                    _finalSessionState = CompletionSessionState.DismissedDueToCancellation;
                    ((IAsyncCompletionSession)this).Dismiss();
                    return model;
                }
            }

            // If we still have no items, dismiss, unless there is another task queued (because user has typed).
            if (model.Uninitialized)
            {
                _finalSessionState = CompletionSessionState.DismissedUninitialized;
                var dismissed = await TryDismissSafely(thisId).ConfigureAwait(true);
                return model;
            }

            // There is another taks queued: We are preempted, store the most recent snapshot for the upcoming invocation of UpdateSnapshot
            if (thisId != _lastFilteringTaskId)
                return model.WithSnapshot(instantaneousSnapshot);

            _telemetry.ComputationStopwatch.Restart();

            var filteredCompletion = await _guardedOperations.CallExtensionPointAsync(
                errorSource: _completionItemManager,
                asyncCall: () => _completionItemManager.UpdateCompletionListAsync(
                    session: this,
                    data: new AsyncCompletionSessionDataSnapshot(
                        model.InitialItems,
                        instantaneousSnapshot,
                        trigger,
                        InitialTrigger,
                        model.Filters,
                        model.UseSoftSelection,
                        model.DisplaySuggestionItem),
                    token: token),
                valueOnThrow: null).ConfigureAwait(true);

            // Error cases are handled by logging them above and dismissing the session.
            if (filteredCompletion == null)
            {
                _finalSessionState = CompletionSessionState.DismissedDuringFiltering;
                ((IAsyncCompletionSession)this).Dismiss();
                return model;
            }

            // Other error cases that we attribute to the IAsyncCompletionItemManager
            if (filteredCompletion.SelectedItemIndex == -1 && !model.DisplaySuggestionItem)
            {
                _guardedOperations.HandleException(errorSource: _completionItemManager,
                    e: new InvalidOperationException($"{nameof(IAsyncCompletionItemManager)} recommended selecting suggestion item when there is no suggestion item."));
                _finalSessionState = CompletionSessionState.DismissedDuringFiltering;
                ((IAsyncCompletionSession)this).Dismiss();
                return model;
            }

            int selectedIndex = filteredCompletion.SelectedItemIndex;
            bool selectedIndexOverridden = false; // Used when ApplicableToSpan is empty

            // Special experience when there are no returned items:
            ImmutableArray<CompletionItemWithHighlight> returnedItems;
            if (filteredCompletion.Items.IsDefault)
            {
                // Prevent null references when service returns default(ImmutableArray)
                returnedItems = ImmutableArray<CompletionItemWithHighlight>.Empty;
            }
            else if (filteredCompletion.Items.IsEmpty)
            {
                if (model.PresentedItems.IsDefaultOrEmpty)
                {
                    // There were no previously visible results. Return a valid empty array
                    returnedItems = ImmutableArray<CompletionItemWithHighlight>.Empty;
                }
                else
                {
                    // Show previously visible results, without highlighting
                    returnedItems = model.PresentedItems.Select(n => new CompletionItemWithHighlight(n.CompletionItem)).ToImmutableArray();
                    selectedIndex = model.SelectedIndex;
                    if (!_inNoResultFallback)
                    {
                        // Enter the no results mode to preserve the selection state
                        _selectionModeBeforeNoResultFallback = model.UseSoftSelection;
                        _inNoResultFallback = true;
                        model = model.WithSoftSelection(true);
                    }
                }
            }
            else
            {
                // Default behavior, we received completion items
                returnedItems = filteredCompletion.Items;

                if (_inNoResultFallback)
                {
                    // we were in the no result mode and just received no items. Restore the selection mode.
                    model = model.WithSoftSelection(_selectionModeBeforeNoResultFallback);
                    _inNoResultFallback = false;
                }

                // Special experience when ApplicableToSpan is empty: attempt to select last selected item
                if (currentlyApplicableToSpan.IsEmpty && !string.IsNullOrEmpty(_previouslySelectedItemText))
                {
                    int indexOfPreviouslySelectedItem = -1;
                    for (int i = 0; i < filteredCompletion.Items.Length; i++)
                    {
                        if (filteredCompletion.Items[i].CompletionItem.DisplayText.Equals(_previouslySelectedItemText, StringComparison.Ordinal))
                        {
                            indexOfPreviouslySelectedItem = i;
                            break;
                        }
                    }
                    if (indexOfPreviouslySelectedItem != -1)
                    {
                        // We found a matching item
                        model = model.WithSelectedIndex(indexOfPreviouslySelectedItem, preserveSoftSelection: true).WithSoftSelection(true);
                        selectedIndexOverridden = true;
                    }
                }

                // Leave the caret location fallback if user just typed something
                if (_inCaretLocationFallback && trigger.Reason == CompletionTriggerReason.Insertion)
                {
                    // User just typed something, so we can't be at the beginning of applicable to span. Revert the selection mode.
                    model = model.WithSoftSelection(_selectionModeBeforeCaretLocationFallback);
                    _inCaretLocationFallback = false;
                }
            }

            _telemetry.ComputationStopwatch.Stop();
            _telemetry.RecordProcessing(_telemetry.ComputationStopwatch.ElapsedMilliseconds, returnedItems.Length);

            // Allow the item manager to control the selection of the suggestion item
            if (model.DisplaySuggestionItem)
            {
                if (filteredCompletion.SelectedItemIndex == -1)
                    model = model.WithSuggestionItemSelected();
                else if (!selectedIndexOverridden)
                    model = model.WithSelectedIndex(selectedIndex, preserveSoftSelection: true);
                // If suggestion item is present, we default to soft selection.
                model = model.WithSoftSelection(true);

                _previouslySelectedItemText = string.Empty;
            }
            else if (!selectedIndexOverridden && !returnedItems.IsDefaultOrEmpty)
            {
                model = model.WithSelectedIndex(selectedIndex, preserveSoftSelection: true);
                _previouslySelectedItemText = returnedItems[selectedIndex].CompletionItem.DisplayText;
            }

            // Allow the item manager to override the selection style.
            // Our recommendation for extenders is to use UpdateSelectionHint.NoChange whenever possible
            if (filteredCompletion.SelectionHint == UpdateSelectionHint.SoftSelected)
                model = model.WithSoftSelection(true);
            else if (filteredCompletion.SelectionHint == UpdateSelectionHint.Selected)
                model = model.WithSoftSelection(false);

            // Prepare the suggestionItem if user ever activates suggestion mode
            var enteredText = currentlyApplicableToSpan.GetText();
            var suggestionItem = new CompletionItem(enteredText, SuggestionModeCompletionItemSource);

            return model.WithSnapshotItemsAndFilters(updateLocation.Snapshot, returnedItems, filteredCompletion.UniqueItem, suggestionItem, filteredCompletion.Filters);
        }

        /// <summary>
        /// Dismisses this <see cref="AsyncCompletionSession"/> only if called from the last task.
        /// If there are any extra tasks, this method will return <c>false</c>
        /// </summary>
        /// <param name="currentTaskId"></param>
        /// <returns></returns>
        private async Task<bool> TryDismissSafely(int currentTaskId)
        {
            await JoinableTaskContext.Factory.SwitchToMainThreadAsync();

            // Tasks are enqueued on the UI thread, so we know that _lastFilteringTaskId won't change
            if (currentTaskId < _lastFilteringTaskId)
            {
                // This is not the last task, so we should not dismiss.
                return false;
            }
            else
            {
                Dismiss();
                return true;
            }
        }

        /// <summary>
        /// Reacts to user toggling a filter
        /// </summary>
        /// <param name="newFilters">Filters with updated Selected state, as indicated by the user.</param>
        private async Task<CompletionModel> UpdateFilters(CompletionModel model, ImmutableArray<CompletionFilterWithState> newFilters, int thisId, CancellationToken token)
        {
            _telemetry.RecordChangingFilters();
            _telemetry.RecordKeystroke();

            // This operation just got preempted, preserve new filters until next time we have a chance to update the completion list.
            if (token.IsCancellationRequested || thisId != _lastFilteringTaskId)
                return model.WithFilters(newFilters);

            var filteredCompletion = await _guardedOperations.CallExtensionPointAsync(
                errorSource: _completionItemManager,
                asyncCall: () => _completionItemManager.UpdateCompletionListAsync(
                    session: this,
                    data: new AsyncCompletionSessionDataSnapshot(
                        model.InitialItems,
                        model.Snapshot,
                        new CompletionTrigger(CompletionTriggerReason.FilterChange, model.Snapshot),
                        InitialTrigger,
                        newFilters,
                        model.UseSoftSelection,
                        model.DisplaySuggestionItem),
                    token: token),
                valueOnThrow: null).ConfigureAwait(true);

            // Handle error cases by logging the issue and discarding the request to filter
            if (filteredCompletion == null)
                return model;
            if (filteredCompletion.Filters.Length != newFilters.Length)
            {
                _guardedOperations.HandleException(
                    errorSource: _completionItemManager,
                    e: new InvalidOperationException("Completion service returned incorrect set of filters."));
                return model;
            }

            return model.WithFilters(filteredCompletion.Filters).WithPresentedItems(filteredCompletion.Items, filteredCompletion.SelectedItemIndex);
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
#pragma warning disable CA1801 // Parameter token is never used
        /// <summary>
        /// Reacts to user scrolling the list using keyboard
        /// </summary>
        private async Task<CompletionModel> UpdateSelectedItem(CompletionModel model, int offset, CancellationToken token)
#pragma warning restore CS1998
#pragma warning restore CA1801
        {
            _telemetry.RecordScrolling();
            _telemetry.RecordKeystroke();

            if (!model.PresentedItems.Any())
            {
                // No-op if there are no items, unless there is a suggestion item.
                if (model.DisplaySuggestionItem)
                {
                    return model.WithSuggestionItemSelected(); // Select the sole item which is a suggestion item.
                }
                return model;
            }

            var lastIndex = model.PresentedItems.Count() - 1;
            var currentIndex = model.SelectSuggestionItem ? -1 : model.SelectedIndex;

            if (offset > 0) // Scrolling down. Stop at last index and don't wrap around.
            {
                if (model.UseSoftSelection && currentIndex > -1)
                {
                    return model.WithSoftSelection(false); // Switch from soft selection to full selection
                }
                else if (currentIndex == lastIndex)
                {
                    return model; // Don't wrap around
                }

                var newIndex = currentIndex + offset;
                return model.WithSelectedIndex(Math.Min(newIndex, lastIndex));
            }
            else // Scrolling up. Stop at first index and don't wrap around.
            {
                if (model.UseSoftSelection && currentIndex > -1)
                {
                    return model.WithSoftSelection(false); // Switch from soft selection to full selection
                }
                if (currentIndex < FirstIndex) // Suggestion mode item is selected.
                {
                    return model; // Don't wrap around
                }
                else if (currentIndex == FirstIndex) // The first item is selected.
                {
                    if (model.DisplaySuggestionItem) // If there is a suggestion, select it.
                        return model.WithSuggestionItemSelected();
                    else
                        return model; // Don't wrap around
                }
                var newIndex = currentIndex + offset;
                return model.WithSelectedIndex(Math.Max(newIndex, FirstIndex));
            }
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
#pragma warning disable CA1801 // Parameter token is never used
        /// <summary>
        /// Reacts to user selecting a specific item in the list
        /// </summary>
        private async Task<CompletionModel> UpdateSelectedItem(CompletionModel model, CompletionItem selectedItem, bool suggestionItemSelected, CancellationToken token)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
#pragma warning restore CA1801
        {
            _telemetry.RecordScrolling();
            if (suggestionItemSelected)
            {
                return model.WithSuggestionItemSelected();
            }
            else
            {
                for (int i = 0; i < model.PresentedItems.Length; i++)
                {
                    if (model.PresentedItems[i].CompletionItem == selectedItem)
                    {
                        return model.WithSelectedIndex(i);
                    }
                }
                // This item is not in the model
                return model;
            }
        }

        void IModelComputationCallbackHandler<CompletionModel>.ComputationFinished(CompletionModel model)
        {
            if (ItemsUpdated == null)
                return;
            ThreadPool.QueueUserWorkItem(new WaitCallback(RaiseCompletionItemsComputedEventOnBackground), model);
        }

        void IModelComputationCallbackHandler<CompletionModel>.DismissDueToCancellation()
        {
            _finalSessionState = CompletionSessionState.DismissedDueToCancellation;
            Dismiss();
        }

        void IModelComputationCallbackHandler<CompletionModel>.DismissDueToError()
        {
            _finalSessionState = CompletionSessionState.DismissedDueToUnhandledError;
            Dismiss();
        }

        private void RaiseCompletionItemsComputedEventOnBackground(object parameter)
        {
            if (IsDismissed)
                return;
            var handlers = ItemsUpdated;
            if (handlers == null)
                return;
            if (!(parameter is CompletionModel model))
                return;

            var computedItems = ComputeCompletionItems(model);

            // Warning: if the event handler throws and anyone blocks UI thread now, there will be a deadlock.
            // This won't happen for now, because all callers of this method are private and nobody waits on them.

            _guardedOperations.RaiseEvent(this, ItemsUpdated, new ComputedCompletionItemsEventArgs(computedItems));
        }

        private static ComputedCompletionItems ComputeCompletionItems(CompletionModel model)
        {
            if (model == null || model.Uninitialized)
                return ComputedCompletionItems.Empty;

            return new ComputedCompletionItems(
                itemsWithHighlight: model.PresentedItems,
                suggestionItem: model.DisplaySuggestionItem ? model.SuggestionItem : null,
                selectedItem: model.SelectSuggestionItem
                    ? model.SuggestionItem
                    : model.PresentedItems.IsDefaultOrEmpty || model.SelectedIndex < 0
                        ? null
                        : model.PresentedItems[model.SelectedIndex].CompletionItem,
                suggestionItemSelected: model.SelectSuggestionItem,
                usesSoftSelection: model.UseSoftSelection);
        }
    }
}
