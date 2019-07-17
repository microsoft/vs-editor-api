//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License. See License.txt in the project root for license information.
//
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;

[assembly: InternalsVisibleTo("Microsoft.VisualStudio.Language.Implementation, PublicKey=" + ThisAssembly.PublicKey)]
[assembly: InternalsVisibleTo("Microsoft.VisualStudio.Language.Intellisense.Implementation, PublicKey=" + ThisAssembly.PublicKey)]
[assembly: InternalsVisibleTo("Microsoft.VisualStudio.Language.Intellisense.UnitTestHelper, PublicKey=" + ThisAssembly.PublicKey)]
[assembly: InternalsVisibleTo("Microsoft.VisualStudio.Language.Intellisense.UnitTests, PublicKey=" + ThisAssembly.PublicKey)]
[assembly: InternalsVisibleTo("Microsoft.VisualStudio.UI.Text.Cocoa.View.Implementation, PublicKey=" + ThisAssembly.PublicKey)]
[assembly: InternalsVisibleTo("Microsoft.VisualStudio.Platform.VSEditor, PublicKey=" + ThisAssembly.PublicKey)]

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
//
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: ReliabilityContract(Consistency.MayCorruptProcess, Cer.MayFail)]
