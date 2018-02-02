namespace Microsoft.VisualStudio.Text.Operations.Implementation
{
    using System;
    using System.Collections.Generic;
    using Microsoft.VisualStudio.Commanding;
    using Microsoft.VisualStudio.Text.Editor;

    internal class ExpandContractSelectionImplementation
    {
        private readonly IEditorOptions editorOptions;
        private readonly ITextStructureNavigatorSelectorService navigatorSelectorService;
        private bool ignoreSelectionChangedEvent;

        public static ExpandContractSelectionImplementation GetOrCreateExpandContractState(
            ITextView textView,
            IEditorOptionsFactoryService editorOptionsFactoryService,
            ITextStructureNavigatorSelectorService navigator)
        {
            return textView.Properties.GetOrCreateSingletonProperty<ExpandContractSelectionImplementation>(
                typeof(ExpandContractSelectionImplementation),
                () => new ExpandContractSelectionImplementation(
                    navigator,
                    editorOptionsFactoryService.GetOptions(textView),
                    textView));
        }

        private ExpandContractSelectionImplementation(
            ITextStructureNavigatorSelectorService navigatorSelectorService,
            IEditorOptions editorOptions,
            ITextView textView)
        {
            this.editorOptions = editorOptions;
            this.navigatorSelectorService = navigatorSelectorService;
            textView.Selection.SelectionChanged += this.OnSelectionChanged;
        }

        // Internal for testing.
        internal readonly Stack<Tuple<VirtualSnapshotSpan, TextSelectionMode>> previousExpansionsStack
            = new Stack<Tuple<VirtualSnapshotSpan, TextSelectionMode>>();

        public CommandState GetExpandCommandState(ITextView textView) => CommandState.Available;

        public CommandState GetContractCommandState(ITextView textView)
        {
            if (this.previousExpansionsStack.Count > 0)
            {
                return CommandState.Available;
            }

            return CommandState.Unavailable;
        }

        public bool ExpandSelection(ITextView textView)
        {
            try
            {
                this.ignoreSelectionChangedEvent = true;

                var navigator = this.GetNavigator(textView);
                VirtualSnapshotSpan currentSelection = textView.Selection.StreamSelectionSpan;
                previousExpansionsStack.Push(Tuple.Create(currentSelection, textView.Selection.Mode));

                SnapshotSpan newSelection;

                // If the current language has opt-ed out, return the span of the current word instead.
                if (this.editorOptions.GetOptionValue(ExpandContractSelectionOptions.ExpandContractSelectionEnabledKey))
                {
                    // On first invocation, select the current word.
                    if (currentSelection.Length == 0)
                    {
                        newSelection = this.GetNavigator(textView).GetExtentOfWord(currentSelection.Start.Position).Span;
                    }
                    else
                    {
                        newSelection = this.GetNavigator(textView).GetSpanOfEnclosing(currentSelection.SnapshotSpan);
                    }
                }
                else
                {
                    // Since the span of the current word can be left or right associative relative to the caret
                    // in different contexts, to avoid different selections on subsequent invocations of Expand
                    // Selection, always use the center point in the selection to compute the span of the current word.
                    var centerPoint = currentSelection.Start.Position.Add(
                        (currentSelection.End.Position.Position - currentSelection.Start.Position.Position) / 2);
                    newSelection = navigator.GetExtentOfWord(centerPoint).Span;
                }

                textView.Selection.Mode = TextSelectionMode.Stream;
                textView.Selection.Select(newSelection, isReversed: false);
            }
            finally
            {
                this.ignoreSelectionChangedEvent = false;
            }

            return true; //return true if command is handled
        }

        public bool ContractSelection(ITextView textView)
        {
            try
            {
                this.ignoreSelectionChangedEvent = true;

                if (this.previousExpansionsStack.Count > 0)
                {
                    Tuple<VirtualSnapshotSpan, TextSelectionMode> previousExpansion = this.previousExpansionsStack.Pop();
                    VirtualSnapshotSpan previousExpansionSpan = previousExpansion.Item1;
                    TextSelectionMode previousExpansionSelectionMode = previousExpansion.Item2;

                    textView.Selection.Mode = previousExpansionSelectionMode;
                    textView.Selection.Select(previousExpansionSpan.Start, previousExpansionSpan.End);
                }
            }
            finally
            {
                this.ignoreSelectionChangedEvent = false;
            }
                               
            return true;//return true if command is handled
        }

        private void OnSelectionChanged(object sender, EventArgs eventArgs)
        {
            if (!this.ignoreSelectionChangedEvent)
            {
                this.previousExpansionsStack.Clear();
            }
        }

        private ITextStructureNavigator GetNavigator(ITextView textView)
            => this.navigatorSelectorService.GetTextStructureNavigator(textView.TextBuffer);
    }
}
