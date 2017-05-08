#if CODE_ANALYSIS_BASELINE
using System.Diagnostics.CodeAnalysis;

//[module: SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods", Scope = "member",
//    Target = "Microsoft.VisualStudio.Utilities.Verify.ThrowIfStringArgumentNullOrEmpty(System.String,System.String):System.Void",
//    Justification = "The argument is verified in the overloaded ThrowIfStringArgumentNullOrEmpty")]


//[module: SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods", Scope = "member", 
//    Target = "Microsoft.VisualStudio.Utilities.Verify.ThrowIfStringArgumentNullOrEmpty(System.String):System.Void",
//    Justification = "The argument is verified in the overloaded ThrowIfStringArgumentNullOrEmpty")]

[module: SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId="Orderer", Scope="type", Target="Microsoft.VisualStudio.Utilities.Orderer")]
[module: SuppressMessage("Microsoft.Design","CA1004:GenericMethodsShouldProvideTypeParameter", Scope="member", Target="Microsoft.VisualStudio.Utilities.ResourceEvaluator.#Get`1(Microsoft.VisualStudio.AssetSystem.IExtension,System.String)")]
[module: SuppressMessage("Microsoft.Design","CA1004:GenericMethodsShouldProvideTypeParameter", Scope="member", Target="Microsoft.VisualStudio.Utilities.ResourceEvaluator.#Get`1(Microsoft.VisualStudio.AssetSystem.IExtension,System.String,System.Globalization.CultureInfo)")]

[module: SuppressMessage("Microsoft.Design","CA1020:AvoidNamespacesWithFewTypes", Scope="namespace", Target="Microsoft.VisualStudio.Utilities")]
[module: SuppressMessage("Microsoft.Design","CA1018:MarkAttributesWithAttributeUsage", Scope="type", Target="Microsoft.VisualStudio.Utilities.DefaultAttribute")]
[module: SuppressMessage("Microsoft.Performance","CA1822:MarkMembersAsStatic", Scope="member", Target="Microsoft.VisualStudio.Utilities.DefaultAttribute.#IsDefault")]
[module: SuppressMessage("Microsoft.Design","CA1018:MarkAttributesWithAttributeUsage", Scope="type", Target="Microsoft.VisualStudio.Utilities.DisplayNameAttribute")]
[module: SuppressMessage("Microsoft.Design","CA1018:MarkAttributesWithAttributeUsage", Scope="type", Target="Microsoft.VisualStudio.Utilities.NameAttribute")]
[module: SuppressMessage("Microsoft.Design","CA1018:MarkAttributesWithAttributeUsage", Scope="type", Target="Microsoft.VisualStudio.Utilities.OrderAttribute")]

[module: SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Scope = "type", Target = "Microsoft.VisualStudio.Utilities.Range")]

[module: SuppressMessage("Microsoft.Design","CA1006:DoNotNestGenericTypesInMemberSignatures", Scope="member", Target="Microsoft.VisualStudio.Utilities.Orderer.#Order`2(System.Collections.Generic.IEnumerable`1<System.ComponentModel.Composition.Lazy`2<!!0,!!1>>)")]

[module: SuppressMessage("Microsoft.MSInternal","CA908:AvoidTypesThatRequireJitCompilationInPrecompiledAssemblies", Scope="member", Target="Microsoft.VisualStudio.Utilities.PropertyCollection.#get_PropertyList()")]
[module: SuppressMessage("Microsoft.Design","CA1006:DoNotNestGenericTypesInMemberSignatures", Scope="member", Target="Microsoft.VisualStudio.Utilities.PropertyCollection.#PropertyList")]
[module: SuppressMessage("Microsoft.MSInternal","CA908:AvoidTypesThatRequireJitCompilationInPrecompiledAssemblies", Scope="member", Target="Microsoft.VisualStudio.Utilities.PropertyCollection.#PropertyList")]
[module: SuppressMessage("Microsoft.Design","CA1004:GenericMethodsShouldProvideTypeParameter", Scope="member", Target="Microsoft.VisualStudio.Utilities.PropertyCollection.#GetProperty`1(System.Object)")]
[module: SuppressMessage("Microsoft.Naming","CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Scope="type", Target="Microsoft.VisualStudio.Utilities.PropertyCollection")]
[module: SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "orderer", Scope = "member", Target = "Microsoft.VisualStudio.Utilities.Orderer.#RemoveNodeFromList`2(System.Collections.Generic.List`1<Microsoft.VisualStudio.Utilities.Node`1<System.Lazy`2<!!0,!!1>>>,Microsoft.VisualStudio.Utilities.Node`1<System.Lazy`2<!!0,!!1>>)", Justification = "Orderer is the name of the class")]

[module: SuppressMessage("Microsoft.Design", "CA1018:MarkAttributesWithAttributeUsage", Scope = "type", Target = "Microsoft.VisualStudio.Utilities.BaseDefinitionAttribute")]
[module: SuppressMessage("Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments", Scope = "type", Target = "Microsoft.VisualStudio.Utilities.BaseDefinitionAttribute")]

#endif