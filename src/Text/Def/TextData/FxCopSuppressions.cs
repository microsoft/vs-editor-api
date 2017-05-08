#if CODE_ANALYSIS_BASELINE
using System.Diagnostics.CodeAnalysis;

[module: SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", 
                         Scope = "member", 
                         Target = "Microsoft.VisualStudio.Text.ITrackingSpan.GetEndPoint(Microsoft.VisualStudio.Text.ITextSnapshot):Microsoft.VisualStudio.Text.SnapshotPoint", 
                         MessageId = "EndPoint",
                         Justification="EndPoint makes sense because it really comprises two words (it means 'the ending SnapshotPoint'), in duality with StartPoint.")]
//[module: SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods", 
//                         Scope = "member", 
//                         Target = "Microsoft.VisualStudio.Text.NormalizedSpanCollection.op_Equality(Microsoft.VisualStudio.Text.NormalizedSpanCollection,Microsoft.VisualStudio.Text.NormalizedSpanCollection):System.Boolean", 
//                         Justification = "Argument already validated for null")]
[module: SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", 
                         Scope = "member",
                         Target = "Microsoft.VisualStudio.Text.ITextSnapshotLine.GetPositionOfNextNonWhiteSpaceCharacter(System.Int32):System.Int32",
                         MessageId = "NonWhite")]
[module: SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue",
                         Scope = "type",
                         Target = "Microsoft.VisualStudio.Text.TrackingFidelityMode",
                         Justification = "'None' as the zero value makes no sense in this context")]

[module: SuppressMessage("Microsoft.Design", "CA2210:AssembliesShouldHaveValidStrongNames", Scope="", Target="microsoft.visualstudio.text.data.dll", MessageId="", Justification="BASELINE: Original port of VisualStudio to ToolPlat")]
[module: SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", Scope="member", Target="Microsoft.VisualStudio.Text.ITextSnapshotLine.#End", MessageId="End", Justification="BASELINE: Original port of VisualStudio to ToolPlat")]
[module: SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Scope="member", Target="Microsoft.VisualStudio.Text.ITextSnapshotLine.#GetLineBreakText()", MessageId="", Justification="BASELINE: Original port of VisualStudio to ToolPlat")]
[module: SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Scope="member", Target="Microsoft.VisualStudio.Text.ITextSnapshotLine.#GetText()", MessageId="", Justification="BASELINE: Original port of VisualStudio to ToolPlat")]
[module: SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Scope="member", Target="Microsoft.VisualStudio.Text.ITextSnapshotLine.#GetTextIncludingLineBreak()", MessageId="", Justification="BASELINE: Original port of VisualStudio to ToolPlat")]
[module: SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Scope="member", Target="Microsoft.VisualStudio.Text.SnapshotPoint.#GetChar()", MessageId="", Justification="BASELINE: Original port of VisualStudio to ToolPlat")]
[module: SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Scope="member", Target="Microsoft.VisualStudio.Text.SnapshotPoint.#GetContainingLine()", MessageId="", Justification="BASELINE: Original port of VisualStudio to ToolPlat")]
[module: SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Scope="member", Target="Microsoft.VisualStudio.Text.SnapshotSpan.#GetText()", MessageId="", Justification="BASELINE: Original port of VisualStudio to ToolPlat")]
[module: SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Scope="member", Target="Microsoft.VisualStudio.Text.SnapshotSpan.#op_Equality(Microsoft.VisualStudio.Text.SnapshotSpan,Microsoft.VisualStudio.Text.SnapshotSpan)", MessageId="ss", Justification="BASELINE: Original port of VisualStudio to ToolPlat")]
[module: SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Scope="member", Target="Microsoft.VisualStudio.Text.SnapshotSpan.#op_Inequality(Microsoft.VisualStudio.Text.SnapshotSpan,Microsoft.VisualStudio.Text.SnapshotSpan)", MessageId="ss", Justification="BASELINE: Original port of VisualStudio to ToolPlat")]
[module: SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", Scope="member", Target="Microsoft.VisualStudio.Text.ITextVersion.#Next", MessageId="Next", Justification="BASELINE: Original port of VisualStudio to ToolPlat")]

[module: SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Scope="member", Target="Microsoft.VisualStudio.Text.Projection.ProjectionSourceSpansChangedEventArgs.#spanPosition", MessageId="", Justification="BASELINE: Original port of VisualStudio to ToolPlat")]
[module: SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "End", Scope = "member", Target = "Microsoft.VisualStudio.Text.IMappingSpan.#End")]
[module: SuppressMessage("Microsoft.Design","CA1021:AvoidOutParameters", MessageId="3#", Scope="member", Target="Microsoft.VisualStudio.Text.ITextDocumentFactoryService.#CreateAndLoadTextDocument(System.String,Microsoft.VisualStudio.Utilities.IContentType,System.Boolean,System.Boolean&)")]
[module: SuppressMessage("Microsoft.Design","CA1021:AvoidOutParameters", MessageId="3#", Scope="member", Target="Microsoft.VisualStudio.Text.ITextDocumentFactoryService.#CreateAndLoadTextDocument(System.String,Microsoft.VisualStudio.Utilities.IContentType,System.Text.Encoding,System.Boolean&)")]

// Differencing

[module: SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Scope = "member", Target = "Microsoft.VisualStudio.Text.Differencing.IDifferenceCollection`1.#MatchSequence")]
[module: SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Scope = "type", Target = "Microsoft.VisualStudio.Text.Differencing.MatchRange")]
[module: SuppressMessage("Microsoft.Naming","CA1710:IdentifiersShouldHaveCorrectSuffix", Scope="type", Target="Microsoft.VisualStudio.Text.Differencing.ITokenizedStringList", Justification="It is an IList, so it should end in \"List\", not \"Collection\".")]
[module: SuppressMessage("Microsoft.Naming","CA1710:IdentifiersShouldHaveCorrectSuffix", Scope="type", Target="Microsoft.VisualStudio.Text.Differencing.Match")]

[module: SuppressMessage("Microsoft.MSInternal", "CA908:AvoidTypesThatRequireJitCompilationInPrecompiledAssemblies", Scope = "member", Target = "Microsoft.VisualStudio.Text.Differencing.IDifferenceCollection`1.#MatchSequence")]
[module: SuppressMessage("Microsoft.MSInternal", "CA908:AvoidTypesThatRequireJitCompilationInPrecompiledAssemblies", Scope = "member", Target = "Microsoft.VisualStudio.Text.Differencing.IDifferenceCollection`1.#get_MatchSequence()")]
[module: SuppressMessage("Microsoft.MSInternal", "CA908:AvoidTypesThatRequireJitCompilationInPrecompiledAssemblies", Scope = "type", Target = "Microsoft.VisualStudio.Text.Differencing.Match")]
[module: SuppressMessage("Microsoft.MSInternal", "CA908:AvoidTypesThatRequireJitCompilationInPrecompiledAssemblies", Scope = "member", Target = "Microsoft.VisualStudio.Text.Differencing.Match.#GetEnumerator()")]
[module: SuppressMessage("Microsoft.MSInternal", "CA908:AvoidTypesThatRequireJitCompilationInPrecompiledAssemblies", Scope = "member", Target = "Microsoft.VisualStudio.Text.Differencing.Match.#System.Collections.IEnumerable.GetEnumerator()")]

#endif
