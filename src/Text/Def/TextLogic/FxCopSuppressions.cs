#if CODE_ANALYSIS_BASELINE
using System.Diagnostics.CodeAnalysis;

[module: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope="namespace", Target="Microsoft.VisualStudio.Text.Document", MessageId="", Justification="BASELINE: Original port of VisualStudio to ToolPlat")]
[module: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "Microsoft.VisualStudio.Text", MessageId = "", Justification = "BASELINE: Original port of VisualStudio to ToolPlat")]
[module: SuppressMessage("Microsoft.Design", "CA2210:AssembliesShouldHaveValidStrongNames", Scope = "", Target = "microsoft.visualstudio.text.logic.dll", MessageId = "", Justification = "BASELINE: Original port of VisualStudio to ToolPlat")]
[module: SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces", Scope = "type", Target = "Microsoft.VisualStudio.Text.Classification.IClassificationTypeDefinition")]
[module: SuppressMessage("Microsoft.Design", "CA1018:MarkAttributesWithAttributeUsage", Scope = "type", Target = "Microsoft.VisualStudio.Text.Classification.ClassificationTypeAttribute", MessageId = "", Justification = "BASELINE: Original port of VisualStudio to ToolPlat")]
[module: SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope="member", Target="Microsoft.VisualStudio.Text.Operations.FindData.#.ctor(Microsoft.VisualStudio.Text.ITextSnapshot)", MessageId="", Justification="Used by unit tests")]

[module: SuppressMessage("Microsoft.Design","CA1004:GenericMethodsShouldProvideTypeParameter", Scope="member", Target="Microsoft.VisualStudio.Text.Editor.IEditorOptions.#GetOptionValue`1(System.String)")]
[module: SuppressMessage("Microsoft.Design","CA1020:AvoidNamespacesWithFewTypes", Scope="namespace", Target="Microsoft.VisualStudio.Text.Editor.OptionsExtensionMethods")]
[module: SuppressMessage("Microsoft.Design","CA1045:DoNotPassTypesByReference", MessageId="0#", Scope="member", Target="Microsoft.VisualStudio.Text.Editor.EditorOptionDefinition.#IsValid(System.Object&)")]
[module: SuppressMessage("Microsoft.Design","CA1007:UseGenericsWhereAppropriate", Scope="member", Target="Microsoft.VisualStudio.Text.Editor.EditorOptionDefinition.#IsValid(System.Object&)")]
[module: SuppressMessage("Microsoft.Naming","CA1716:IdentifiersShouldNotMatchKeywords", MessageId="Default", Scope="member", Target="Microsoft.VisualStudio.Text.Editor.EditorOptionDefinition`1.#Default")]
[module: SuppressMessage("Microsoft.Design","CA1045:DoNotPassTypesByReference", MessageId="0#", Scope="member", Target="Microsoft.VisualStudio.Text.Editor.EditorOptionDefinition`1.#IsValid(!0&)")]

// Tagging

[module: SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces", Scope = "type", Target = "Microsoft.VisualStudio.Text.Tagging.ITag", Justification = "Intentional")]

[module: SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces", Scope = "type", Target = "Microsoft.VisualStudio.Text.Tagging.IElisionTag", Justification = "Intentional")]

[module: SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Scope = "member", Target = "Microsoft.VisualStudio.Text.Tagging.ITagAggregator`1.#GetTags(Microsoft.VisualStudio.Text.SnapshotSpan)")]

[module: SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Scope = "member", Target = "Microsoft.VisualStudio.Text.Tagging.ITagAggregator`1.#GetTags(Microsoft.VisualStudio.Text.IMappingSpan)")]
[module: SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Scope = "member", Target = "Microsoft.VisualStudio.Text.Tagging.ITagAggregator`1.#GetTags(Microsoft.VisualStudio.Text.NormalizedSnapshotSpanCollection)")]

[module: SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Scope = "member", Target = "Microsoft.VisualStudio.Text.Tagging.ITagger.#GetTags`1(Microsoft.VisualStudio.Text.SnapshotSpan)")]
[module: SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Scope = "member", Target = "Microsoft.VisualStudio.Text.Tagging.ITaggerProvider.#CreateTagger`1(Microsoft.VisualStudio.Text.ITextBuffer)")]

[module: SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Tagger", Scope = "type", Target = "Microsoft.VisualStudio.Text.Tagging.ITaggerProvider", Justification = "This isn't misspelled.")]
[module: SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Tagger", Scope = "type", Target = "Microsoft.VisualStudio.Text.Tagging.ITagger`1", Justification = "This isn't misspelled")]
[module: SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Tagger", Scope = "member", Target = "Microsoft.VisualStudio.Text.Tagging.ITaggerProvider.#CreateTagger`1(Microsoft.VisualStudio.Text.ITextBuffer)", Justification = "This is not misspelled.")]

[module: SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods", Scope = "member", Target = "Microsoft.VisualStudio.Text.Tagging.TextMarkerTag.#Type")]

[module: SuppressMessage("Microsoft.Design","CA1004:GenericMethodsShouldProvideTypeParameter", Scope="member", Target="Microsoft.VisualStudio.Text.Tagging.IBufferTagAggregatorFactoryService.#CreateTagAggregator`1(Microsoft.VisualStudio.Text.ITextBuffer)")]
[module: SuppressMessage("Microsoft.Design","CA1004:GenericMethodsShouldProvideTypeParameter", Scope="member", Target="Microsoft.VisualStudio.Text.Tagging.IBufferTagAggregatorFactoryService.#CreateTagAggregator`1(Microsoft.VisualStudio.Text.ITextBuffer,Microsoft.VisualStudio.Text.Tagging.TagAggregatorOptions)")]
[module: SuppressMessage("Microsoft.Design","CA1006:DoNotNestGenericTypesInMemberSignatures", Scope="member", Target="Microsoft.VisualStudio.Text.Tagging.ITagger`1.#GetTags(Microsoft.VisualStudio.Text.NormalizedSnapshotSpanCollection)")]
[module: SuppressMessage("Microsoft.Design","CA1006:DoNotNestGenericTypesInMemberSignatures", Scope="member", Target="Microsoft.VisualStudio.Text.Tagging.SimpleTagger`1.#RemoveTagSpans(System.Predicate`1<Microsoft.VisualStudio.Text.Tagging.TrackingTagSpan`1<!0>>)", Justification="Need to nest the generic type")]

[module: SuppressMessage("Microsoft.Design","CA1018:MarkAttributesWithAttributeUsage", Scope="type", Target="Microsoft.VisualStudio.Text.Tagging.TagTypeAttribute")]
[module: SuppressMessage("Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments", Scope="type", Target="Microsoft.VisualStudio.Text.Tagging.TagTypeAttribute")]


[module: SuppressMessage("Microsoft.Naming","CA1725:ParameterNamesShouldMatchBaseDeclaration", MessageId="1#", Scope="member", Target="Microsoft.VisualStudio.Text.Tagging.TrackingTagSpanComparer`1.#Compare(Microsoft.VisualStudio.Text.Tagging.TrackingTagSpan`1<!0>,Microsoft.VisualStudio.Text.Tagging.TrackingTagSpan`1<!0>)")]
[module: SuppressMessage("Microsoft.Naming","CA1725:ParameterNamesShouldMatchBaseDeclaration", MessageId="0#", Scope="member", Target="Microsoft.VisualStudio.Text.Tagging.TrackingTagSpanComparer`1.#Compare(Microsoft.VisualStudio.Text.Tagging.TrackingTagSpan`1<!0>,Microsoft.VisualStudio.Text.Tagging.TrackingTagSpan`1<!0>)")]
[module: SuppressMessage("Microsoft.Design","CA1006:DoNotNestGenericTypesInMemberSignatures", Scope="member", Target="Microsoft.VisualStudio.Text.Tagging.SimpleTagger`1.#GetTaggedSpans(Microsoft.VisualStudio.Text.SnapshotSpan)")]
[module: SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId="Tagger", Scope="type", Target="Microsoft.VisualStudio.Text.Tagging.SimpleTagger`1")]

[module: SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Scope="member", Target="Microsoft.VisualStudio.Text.VirtualSnapshotSpan.#GetText()", MessageId="", Justification="Matches method on SnapshotSpan.")]

[module: SuppressMessage("Microsoft.Naming","CA1716:IdentifiersShouldNotMatchKeywords", MessageId="Do", Scope="member", Target="Microsoft.VisualStudio.Text.Operations.ITextUndoPrimitive.#Do()")]

//ToDo: To be looked at
[module: SuppressMessage("Microsoft.Design","CA1020:AvoidNamespacesWithFewTypes", Scope="namespace", Target="Microsoft.VisualStudio.Text.Tagging", Justification="ToDo: To be looked at")]

#endif
