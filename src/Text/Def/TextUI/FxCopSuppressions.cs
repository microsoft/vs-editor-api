#if CODE_ANALYSIS_BASELINE
using System.Diagnostics.CodeAnalysis;

[module: SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Scope = "member", Target = "Microsoft.VisualStudio.Text.Editor.ITextView.ViewportHeight", MessageId = "Viewport", Justification = "Spelling ok")]
[module: SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Scope = "member", Target = "Microsoft.VisualStudio.Text.Editor.ITextView.ViewportWidth", MessageId = "Viewport", Justification = "Spelling ok")]
[module: SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Scope = "member", Target = "Microsoft.VisualStudio.Text.Editor.ITextView.ViewportLeftChanged", MessageId = "Viewport", Justification = "Spelling ok")]
[module: SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Scope = "member", Target = "Microsoft.VisualStudio.Text.Editor.ITextView.ViewportHeightChanged", MessageId = "Viewport", Justification = "Spelling ok")]
[module: SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Scope = "member", Target = "Microsoft.VisualStudio.Text.Editor.ITextView.ViewportWidthChanged", MessageId = "Viewport", Justification = "Spelling ok")]
[module: SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Scope = "member", Target = "Microsoft.VisualStudio.Text.Editor.ITextView.ViewScroller", MessageId = "Scroller", Justification = "Spelling ok")]
[module: SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Scope = "member", Target = "Microsoft.VisualStudio.Text.Editor.ITextView.ViewportLeft", MessageId = "Viewport", Justification = "Spelling ok")]
[module: SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Scope = "type", Target = "Microsoft.VisualStudio.Text.Editor.IViewScroller", MessageId = "Scroller", Justification = "Spelling ok")]
[module: SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Scope = "member", Target = "Microsoft.VisualStudio.Text.Editor.IViewScroller.ScrollViewportVerticallyByLine(Microsoft.VisualStudio.Text.Editor.ScrollDirection):System.Void", MessageId = "Viewport", Justification = "Spelling ok")]
[module: SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", Scope = "member", Target = "Microsoft.VisualStudio.Text.Editor.IViewScroller.ScrollViewportVerticallyByLine(Microsoft.VisualStudio.Text.Editor.ScrollDirection):System.Void", MessageId = "ByLine", Justification = "Spelling ok")]
[module: SuppressMessage("Microsoft.Naming","CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId="ByLines", Scope="member", Target="Microsoft.VisualStudio.Text.Editor.IViewScroller.#ScrollViewportVerticallyByLines(Microsoft.VisualStudio.Text.Editor.ScrollDirection,System.Int32)", Justification="Spelling ok")]
[module: SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Scope = "member", Target = "Microsoft.VisualStudio.Text.Editor.IViewScroller.ScrollViewportVerticallyByPixels(System.Double):System.Void", MessageId = "Viewport", Justification = "Spelling ok")]
[module: SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Scope = "member", Target = "Microsoft.VisualStudio.Text.Editor.IViewScroller.ScrollViewportVerticallyByPage(Microsoft.VisualStudio.Text.Editor.ScrollDirection):System.Void", MessageId = "Viewport", Justification = "Spelling ok")]
[module: SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Scope = "member", Target = "Microsoft.VisualStudio.Text.Editor.IViewScroller.ScrollViewportHorizontallyByPixels(System.Double):System.Void", MessageId = "Viewport", Justification = "Spelling ok")]
[module: SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Scope="member", Target="Microsoft.VisualStudio.Text.Editor.IVerticalScrollBar.GetBufferPositionOfYCoordinate(System.Double):System.Nullable`1<System.Int32>", MessageId="0#y", Justification = "Spelling ok")]
[module: SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Scope="member", Target="Microsoft.VisualStudio.Text.Editor.IVerticalScrollBar.GetYCoordinateOfPosition(System.Int32):System.Double", MessageId="Coodinate", Justification = "Spelling ok")]
[module: SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Scope = "member", Target = "Microsoft.VisualStudio.Text.Editor.PredefinedTextViewRoles.Zoomable", MessageId = "Zoomable", Justification = "Spelling ok")]

[module: SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", Scope = "member", Target = "Microsoft.VisualStudio.Text.Editor.ITextSelection.#Select(Microsoft.VisualStudio.Text.SnapshotSpan,System.Boolean)", MessageId = "Select", Justification = "BASELINE: Original port of VisualStudio to ToolPlat")]
[module: SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", Scope = "member", Target = "Microsoft.VisualStudio.Text.Editor.ITextSelection.#Select(Microsoft.VisualStudio.Text.VirtualSnapshotPoint,Microsoft.VisualStudio.Text.VirtualSnapshotPoint)", MessageId = "Select", Justification = "BASELINE: Original port of VisualStudio to ToolPlat")]
[module: SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", Scope = "member", Target = "Microsoft.VisualStudio.Text.Editor.ITextSelection.#Select(Microsoft.VisualStudio.Text.VirtualSnapshotPoint,Microsoft.VisualStudio.Text.VirtualSnapshotPoint)", MessageId = "Select")]
[module: SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", Scope = "member", Target = "Microsoft.VisualStudio.Text.Editor.ITextSelection.#End", MessageId = "End")]
[module: SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Scope = "member", Target = "Microsoft.VisualStudio.Text.Editor.IVerticalScrollBar.#GetBufferPositionOfYCoordinate(System.Double)", MessageId = "y", Justification = "BASELINE: Original port of VisualStudio to ToolPlat")]
[module: SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly", Scope="member", Target="Microsoft.VisualStudio.Text.Editor.TextViewLayoutChangedEventArgs.#.ctor(System.Nullable`1<Microsoft.VisualStudio.Text.Span>,Microsoft.VisualStudio.Text.ITextSnapshot,Microsoft.VisualStudio.Text.ITextSnapshot)", MessageId="", Justification="BASELINE: Original port of VisualStudio to ToolPlat")]
[module: SuppressMessage("Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments", Scope = "type", Target = "Microsoft.VisualStudio.Text.Editor.TextViewRoleAttribute", Justification = "Funky form necessary for MEF")]
[module: SuppressMessage("Microsoft.Design", "CA1018:MarkAttributesWithAttributeUsage", Scope = "type", Target = "Microsoft.VisualStudio.Text.Editor.TextViewRoleAttribute")]
[module: SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Scope = "type", Target = "Microsoft.VisualStudio.Text.Editor.ITextViewRoleSet")]

// Operations
[module: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "Microsoft.VisualStudio.Text.Operations", MessageId = "", Justification = "BASELINE: Original port of VisualStudio to ToolPlat")]
[module: SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Scope = "member", Target = "Microsoft.VisualStudio.Text.Operations.IEditorOperations.#GotoLine(System.Int32)", MessageId = "Goto", Justification = "BASELINE: Original port of VisualStudio to ToolPlat")]
[module: SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", Scope="member", Target="Microsoft.VisualStudio.Text.Operations.IEditorOperations.#PageUp(System.Boolean)", MessageId="Select", Justification="BASELINE: Original port of VisualStudio to ToolPlat")]
[module: SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Tabify", Scope = "member", Target = "Microsoft.VisualStudio.Text.Operations.IEditorOperations.#Tabify()", Justification = "These names match the accepted terms for the operations")]
[module: SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Untabify", Scope = "member", Target = "Microsoft.VisualStudio.Text.Operations.IEditorOperations.#Untabify()", Justification = "These names match the accepted terms for the operations")]
[module: SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "InLine", Scope = "member", Target = "Microsoft.VisualStudio.Text.Operations.IEditorOperations.#MoveToLastNonWhiteSpaceCharacter()", Justification = "In and line refer to two words")]
[module: SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", Scope = "member", Target = "Microsoft.VisualStudio.Text.Operations.IEditorOperations.MoveLineUp(System.Boolean):System.Void", MessageId = "LineUp", Justification = "Spelling ok")]
[module: SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "NonWhite", Scope = "member", Target = "Microsoft.VisualStudio.Text.Operations.IEditorOperations.#MoveToLastNonWhiteSpaceCharacter(System.Boolean)", Justification = "Non and white are two separate words in this case")]
[module: SuppressMessage("Microsoft.Design","CA1021:AvoidOutParameters", MessageId="1#", Scope="member", Target="Microsoft.VisualStudio.Text.Operations.IEditorOperations.#InsertTextAsBox(System.String,Microsoft.VisualStudio.Text.VirtualSnapshotPoint&,Microsoft.VisualStudio.Text.VirtualSnapshotPoint&)", Justification="This returns the corners of the box and whether or not it succeeded, so it needs out parameters.")]
[module: SuppressMessage("Microsoft.Design","CA1021:AvoidOutParameters", MessageId="2#", Scope="member", Target="Microsoft.VisualStudio.Text.Operations.IEditorOperations.#InsertTextAsBox(System.String,Microsoft.VisualStudio.Text.VirtualSnapshotPoint&,Microsoft.VisualStudio.Text.VirtualSnapshotPoint&)", Justification="This returns the corners of the box and whether or not it succeeded, so it needs out parameters.")]

// Options
[module: SuppressMessage("Microsoft.Design","CA1020:AvoidNamespacesWithFewTypes", Scope="namespace", Target="Microsoft.VisualStudio.Text.Editor.OptionsExtensionMethods")]

[module: SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Scope="member", Target="Microsoft.VisualStudio.Text.Editor.ITextCaret.#MoveTo(Microsoft.VisualStudio.Text.Formatting.ITextViewLine,System.Double)", MessageId="x")]
[module: SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Scope="member", Target="Microsoft.VisualStudio.Text.Editor.ITextCaret.#MoveTo(Microsoft.VisualStudio.Text.Formatting.ITextViewLine,System.Double,System.Boolean)", MessageId="x")]
[module: SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Scope="member", Target="Microsoft.VisualStudio.Text.Editor.ITextCaret.#MoveTo(Microsoft.VisualStudio.Text.Formatting.ITextViewLine,System.Double,System.Boolean,System.Boolean)", MessageId="x")]

[module: SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Scope="member", Target="Microsoft.VisualStudio.Text.Editor.ITextViewLineCollection.#GetTextViewLineContainingYCoordinate(System.Double)", MessageId="y")]
[module: SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Scope="member", Target="Microsoft.VisualStudio.Text.Formatting.ITextViewLine.#GetBufferPositionFromXCoordinate(System.Double,System.Boolean)", MessageId="x")]
[module: SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Scope="member", Target="Microsoft.VisualStudio.Text.Formatting.ITextViewLine.#GetBufferPositionFromXCoordinate(System.Double)", MessageId="x")]
[module: SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Scope = "member", Target = "Microsoft.VisualStudio.Text.Formatting.ITextViewLine.#GetVirtualBufferPositionFromXCoordinate(System.Double)", MessageId = "x")]
[module: SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Scope = "member", Target = "Microsoft.VisualStudio.Text.Formatting.ITextViewLine.#GetInsertionBufferPositionFromXCoordinate(System.Double)", MessageId = "x")]
[module: SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", Scope = "member", Target = "Microsoft.VisualStudio.Text.Formatting.ITextViewLine.#End", MessageId = "End")]
[module: SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Scope = "member", Target = "Microsoft.VisualStudio.Text.Formatting.TextBounds.#.ctor(System.Double,System.Double,System.Double,System.Double,System.Double,System.Double)", MessageId = "bidi")]

//Tagging
[module: SuppressMessage("Microsoft.Naming","CA1721:PropertyNamesShouldNotMatchGetMethods", Scope="member", Target="Microsoft.VisualStudio.Text.Tagging.SquiggleTag.#Type")]
[module: SuppressMessage("Microsoft.Naming","CA1721:PropertyNamesShouldNotMatchGetMethods", Scope="member", Target="Microsoft.VisualStudio.Text.Tagging.TextMarkerTag.#Type")]
[module: SuppressMessage("Microsoft.Design","CA1020:AvoidNamespacesWithFewTypes", Scope="namespace", Target="Microsoft.VisualStudio.Text.Tagging")]

[module: SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Scope = "member", Target = "Microsoft.VisualStudio.Text.Editor.CaretPosition.#.ctor(System.Int32,Microsoft.VisualStudio.Text.IMappingPoint,Microsoft.VisualStudio.Text.Editor.PositionAffinity,System.Int32,System.Double)", MessageId = "x")]

[module: SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", Scope="member", Target="Microsoft.VisualStudio.Text.Formatting.ITextAndAdornmentCollection.#End", MessageId="End", Justification="BASELINE: Original port of VisualStudio to ToolPlat")]


[module: SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId="Tagger", Scope="member", Target="Microsoft.VisualStudio.Text.Adornments.ITextMarkerProviderFactory.#GetTextMarkerTagger(Microsoft.VisualStudio.Text.ITextBuffer)", Justification="That is the accepted spelling.")]
[module: SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId="Tagger", Scope="member", Target="Microsoft.VisualStudio.Text.Adornments.ISquiggleProviderFactory.#GetSquiggleTagger(Microsoft.VisualStudio.Text.ITextBuffer)", Justification="That is the accepted spelling.")]

[module: SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", Scope="member", Target="Microsoft.VisualStudio.Text.Editor.ITextView.#IsMouseOverViewOrAdornments", MessageId="OverView")]
[module: SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Tagger", Scope = "member", Target = "Microsoft.VisualStudio.Text.Tagging.IViewTaggerProvider.#CreateTagger`1(Microsoft.VisualStudio.Text.Editor.ITextView,Microsoft.VisualStudio.Text.ITextBuffer)")]
[module: SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Scope = "member", Target = "Microsoft.VisualStudio.Text.Tagging.IViewTaggerProvider.#CreateTagger`1(Microsoft.VisualStudio.Text.Editor.ITextView,Microsoft.VisualStudio.Text.ITextBuffer)")]
[module: SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Tagger", Scope = "type", Target = "Microsoft.VisualStudio.Text.Tagging.IViewTaggerProvider")]
[module: SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Scope = "member", Target = "Microsoft.VisualStudio.Text.Tagging.IViewTagAggregatorFactoryService.#CreateTagAggregator`1(Microsoft.VisualStudio.Text.Editor.ITextView)")]
[module: SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Scope = "member", Target = "Microsoft.VisualStudio.Text.Tagging.IViewTagAggregatorFactoryService.#CreateTagAggregator`1(Microsoft.VisualStudio.Text.Editor.ITextView,Microsoft.VisualStudio.Text.Tagging.TagAggregatorOptions)")]

//IncrementalSearch
[module: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "Microsoft.VisualStudio.Text.IncrementalSearch", Justification = "Merging the contents with another namespace would group objects of varying purposes into the same namespace.")]

// Outlining
[module: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "Microsoft.VisualStudio.Text.Outlining", MessageId = "", Justification = "BASELINE: Original port of VisualStudio to ToolPlat")]

// Visible Whitespace
[module: SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "Whitespace", Scope = "member", Target = "Microsoft.VisualStudio.Text.Editor.DefaultTextViewOptions.#UseVisibleWhitespaceId", Justification = "'VisibleWhitespace' makes sense as 'Whitespace' implies the programmatic whitespace character.")]
[module: SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "Whitespace", Scope = "member", Target = "Microsoft.VisualStudio.Text.Editor.OptionsExtensionMethods.TextViewOptionExtensions.#IsVisibleWhitespaceEnabled(Microsoft.VisualStudio.Text.Editor.IEditorOptions)", Justification = "'VisibleWhitespace' makes sense as 'Whitespace' implies the programmatic whitespace character.")]
[module: SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", Scope = "type", Target = "Microsoft.VisualStudio.Text.Editor.UseVisibleWhitespace", Justification = "'VisibleWhitespace' makes sense as 'Whitespace' implies the programmatic whitespace character.")]

// Classification
[module: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "Microsoft.VisualStudio.Text.Classification")]

// Brace Completion
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments", Scope = "type", Target = "Microsoft.VisualStudio.Text.BraceCompletion.BracePairAttribute", Justification = "The accessor just has a different name")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces", Scope = "type", Target = "Microsoft.VisualStudio.Text.BraceCompletion.IBraceCompletionDefaultProvider", Justification = "This interface is empty by design and just used for exporting attributes")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "OverType", Scope = "member", Target = "Microsoft.VisualStudio.Text.BraceCompletion.IBraceCompletionSession.#PreOverType(System.Boolean&)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "OverType", Scope = "member", Target = "Microsoft.VisualStudio.Text.BraceCompletion.IBraceCompletionSession.#PostOverType()")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "OverType", Scope = "member", Target = "Microsoft.VisualStudio.Text.BraceCompletion.IBraceCompletionContext.#AllowOverType(Microsoft.VisualStudio.Text.BraceCompletion.IBraceCompletionSession)")]
#endif
