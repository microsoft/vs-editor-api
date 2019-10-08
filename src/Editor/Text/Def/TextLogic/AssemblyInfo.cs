//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License. See License.txt in the project root for license information.
//
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.Versioning;
using System.Security.Permissions;

//
// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
//

[assembly: ComponentGuarantees(ComponentGuaranteesOptions.Stable)]


[assembly: AssemblyTrademark ("")]
[assembly: AssemblyCulture ("")]
#pragma warning disable 618
[assembly: SecurityPermission (SecurityAction.RequestMinimum, Flags = SecurityPermissionFlag.Execution)]
#pragma warning restore 618
[assembly: ReliabilityContract(Consistency.MayCorruptProcess, Cer.MayFail)]

[assembly: InternalsVisibleTo("Microsoft.VisualStudio.UI.Text.Commanding.Implementation, PublicKey=" + ThisAssembly.PublicKey)]
[assembly: InternalsVisibleTo("Microsoft.VisualStudio.Platform.VSEditor, PublicKey=" + ThisAssembly.PublicKey)]
[assembly: InternalsVisibleTo("Microsoft.VisualStudio.Text.Implementation, PublicKey=" + ThisAssembly.PublicKey)]
[assembly: InternalsVisibleTo("Microsoft.VisualStudio.Editor.Implementation, PublicKey=" + ThisAssembly.PublicKey)]
[assembly: InternalsVisibleTo("Microsoft.VisualStudio.Text.UI.Utilities, PublicKey=" + ThisAssembly.PublicKey)]
[assembly: InternalsVisibleTo("Microsoft.VisualStudio.Language.Implementation, PublicKey=" + ThisAssembly.PublicKey)]
[assembly: InternalsVisibleTo("Microsoft.VisualStudio.Text.TextViewUnitTestHelper, PublicKey=" + ThisAssembly.PublicKey)]
[assembly: InternalsVisibleTo("Microsoft.VisualStudio.Logic.Text.BufferUndoManager.Implementation, PublicKey=" + ThisAssembly.PublicKey)]
[assembly: InternalsVisibleTo("Microsoft.VisualStudio.Text.EditorOptions.Implementation, PublicKey=" + ThisAssembly.PublicKey)]
[assembly: InternalsVisibleTo("Microsoft.VisualStudio.Text.DifferenceViewer.Implementation, PublicKey=" + ThisAssembly.PublicKey)]
[assembly: InternalsVisibleTo("Microsoft.VisualStudio.UI.Text.Wpf.View.Implementation, PublicKey=" + ThisAssembly.PublicKey)]
[assembly: InternalsVisibleTo("Microsoft.VisualStudio.UI.Text.Cocoa.View.Implementation, PublicKey=" + ThisAssembly.PublicKey)]
[assembly: InternalsVisibleTo("Microsoft.VisualStudio.Logic.Text.Tagging.Aggregator.Implementation, PublicKey=" + ThisAssembly.PublicKey)]
[assembly: InternalsVisibleTo("Microsoft.VisualStudio.Text.UI.WPF.Utilities, PublicKey=" + ThisAssembly.PublicKey)]
[assembly: InternalsVisibleTo("Microsoft.VisualStudio.Language.Intellisense.Implementation, PublicKey=" + ThisAssembly.PublicKey)]
[assembly: InternalsVisibleTo("Microsoft.VisualStudio.Text.DeferCreation.Implementation.UnitTests, PublicKey=" + ThisAssembly.PublicKey)]
[assembly: InternalsVisibleTo("Microsoft.VisualStudio.Logic.Text.Find.Implementation, PublicKey=" + ThisAssembly.PublicKey)]
[assembly: InternalsVisibleTo("Logic.Text.Find.Implementation.UnitTests, PublicKey=" + ThisAssembly.PublicKey)]
[assembly: InternalsVisibleTo("Microsoft.VisualStudio.Text.Outlining.Implementation, PublicKey=" + ThisAssembly.PublicKey)]
[assembly: InternalsVisibleTo("Microsoft.VisualStudio.UI.Text.Wpf.OutliningMargin.Implementation, PublicKey=" + ThisAssembly.PublicKey)]
[assembly: InternalsVisibleTo("Microsoft.VisualStudio.Editor.UnitTests, PublicKey=" + ThisAssembly.PublicKey)]
[assembly: InternalsVisibleTo("Microsoft.VisualStudio.Text.Implementation.StandaloneUndo, PublicKey=" + ThisAssembly.PublicKey)]
[assembly: InternalsVisibleTo("Microsoft.VisualStudio.Logic.Text.BufferUndoManager.UnitTests, PublicKey=" + ThisAssembly.PublicKey)]
[assembly: InternalsVisibleTo("Microsoft.VisualStudio.Text.Internal.UnitTests, PublicKey=" + ThisAssembly.PublicKey)]
[assembly: InternalsVisibleTo("Microsoft.VisualStudio.Text.IndentationManager.Implementation, PublicKey=" + ThisAssembly.PublicKey)]
[assembly: InternalsVisibleTo("Microsoft.VisualStudio.UI.Text.EditorOperations.UnitTests, PublicKey=" + ThisAssembly.PublicKey)]
[assembly: InternalsVisibleTo("Microsoft.VisualStudio.Text.Find.Implementation, PublicKey=" + ThisAssembly.PublicKey)]
