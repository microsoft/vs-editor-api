//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License. See License.txt in the project root for license information.
//
// This file contain internal APIs that are subject to change without notice.
// Use at your own risk.
//
using System.Reflection;
using System.Runtime.ConstrainedExecution;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Windows;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;

[assembly: InternalsVisibleTo("Microsoft.VisualStudio.Language.Intellisense.Implementation, PublicKey=" + ThisAssembly.PublicKey)]
[assembly: InternalsVisibleTo("Microsoft.VisualStudio.Platform.VSEditor, PublicKey=" + ThisAssembly.PublicKey)]
[assembly: InternalsVisibleTo("Microsoft.VisualStudio.Editor.Implementation, PublicKey=" + ThisAssembly.PublicKey)]
[assembly: InternalsVisibleTo("Microsoft.VisualStudio.Language.Intellisense.UnitTestHelper, PublicKey=" + ThisAssembly.PublicKey)]
[assembly: InternalsVisibleTo("Microsoft.Test.Apex.VisualStudio, PublicKey=" + ThisAssembly.PublicKey)]

//
// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
//

[assembly: ComponentGuarantees(ComponentGuaranteesOptions.None)]


[assembly: AssemblyTrademark ("")]
[assembly: AssemblyCulture ("")]
[assembly: ReliabilityContract(Consistency.MayCorruptProcess, Cer.MayFail)]

[assembly: ThemeInfo(
    ResourceDictionaryLocation.None,
    ResourceDictionaryLocation.SourceAssembly)]

// Peek types forwarded to Microsoft.VisualStudio.Language.Intellisense.dll
[assembly: TypeForwardedTo(typeof(IFocusableIntellisensePresenter))]
[assembly: TypeForwardedTo(typeof(IAccurateClassifier))]
[assembly: TypeForwardedTo(typeof(IAccurateTagger<>))]
[assembly: TypeForwardedTo(typeof(IAccurateTagAggregator<>))]
[assembly: TypeForwardedTo(typeof(ITelemetryDiagnosticID<>))]
