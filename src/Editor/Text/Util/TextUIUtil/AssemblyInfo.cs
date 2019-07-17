//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License. See License.txt in the project root for license information.
//
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Security.Permissions;

[assembly: InternalsVisibleTo("Microsoft.VisualStudio.UI.Text.Wpf.View.Implementation, PublicKey=" + ThisAssembly.PublicKey)]
[assembly: InternalsVisibleTo("Microsoft.VisualStudio.UI.Text.Wpf.Input.Implementation, PublicKey=" + ThisAssembly.PublicKey)]
[assembly: InternalsVisibleTo("Microsoft.VisualStudio.UI.Text.Wpf.Classification.Implementation, PublicKey=" + ThisAssembly.PublicKey)]
[assembly: InternalsVisibleTo("Microsoft.VisualStudio.UI.Text.Wpf.DragDrop.Implementation, PublicKey=" + ThisAssembly.PublicKey)]
[assembly: InternalsVisibleTo("Microsoft.VisualStudio.UI.Text.AdornmentLibrary.TextMarker.Implementation, PublicKey=" + ThisAssembly.PublicKey)]
[assembly: InternalsVisibleTo("Microsoft.VisualStudio.UI.Text.AdornmentLibrary.ToolTip.Wpf.Implementation, PublicKey=" + ThisAssembly.PublicKey)]
[assembly: InternalsVisibleTo("Microsoft.VisualStudio.UI.Text.TextMarkerAdornment.UnitTests, PublicKey=" + ThisAssembly.PublicKey)]
[assembly: InternalsVisibleTo("Microsoft.VisualStudio.UI.Text.CurrentLineHighlighter.Implementation, PublicKey=" + ThisAssembly.PublicKey)]
[assembly: InternalsVisibleTo("Microsoft.VisualStudio.UI.Text.Wpf.GlyphMargin.Implementation, PublicKey=" + ThisAssembly.PublicKey)]
[assembly: InternalsVisibleTo("Microsoft.VisualStudio.Text.DifferenceViewer.Implementation, PublicKey=" + ThisAssembly.PublicKey)]
[assembly: InternalsVisibleTo("Microsoft.VisualStudio.Text.OverviewMargin.Implementation, PublicKey=" + ThisAssembly.PublicKey)]
[assembly: InternalsVisibleTo("Microsoft.VisualStudio.UI.Text.Wpf.OverviewMargin.UnitTests, PublicKey=" + ThisAssembly.PublicKey)]
[assembly: InternalsVisibleTo("Microsoft.VisualStudio.Text.UI.Utilities.UnitTests, PublicKey=" + ThisAssembly.PublicKey)]
[assembly: InternalsVisibleTo("Microsoft.VisualStudio.Text.TextViewUnitTestHelper, PublicKey=" + ThisAssembly.PublicKey)]
[assembly: InternalsVisibleTo("Microsoft.VisualStudio.UI.Text.Wpf.View.UnitTests, PublicKey=" + ThisAssembly.PublicKey)]
[assembly: InternalsVisibleTo("Microsoft.VisualStudio.UI.Text.Wpf.Input.UnitTests, PublicKey=" + ThisAssembly.PublicKey)]
[assembly: InternalsVisibleTo("Microsoft.VisualStudio.Text.Differencing.DifferenceViewer.UnitTests, PublicKey=" + ThisAssembly.PublicKey)]
[assembly: InternalsVisibleTo("Microsoft.VisualStudio.Language.Intellisense.Implementation, PublicKey=" + ThisAssembly.PublicKey)]
[assembly: InternalsVisibleTo("Microsoft.VisualStudio.UI.Text.Commanding.Implementation, PublicKey=" + ThisAssembly.PublicKey)]
[assembly: InternalsVisibleTo("Microsoft.VisualStudio.UI.Text.Commanding.Implementation.UnitTests, PublicKey=" + ThisAssembly.PublicKey)]
[assembly: InternalsVisibleTo("Microsoft.VisualStudio.Text.Editor.PrintingService.Implementation, PublicKey=" + ThisAssembly.PublicKey)]
[assembly: InternalsVisibleTo("EditorTestApp, PublicKey=" + ThisAssembly.PublicKey)]
[assembly: InternalsVisibleTo("Microsoft.VisualStudio.UI.Text.Cocoa.View.Implementation, PublicKey=" + ThisAssembly.PublicKey)]
[assembly: InternalsVisibleTo("Microsoft.VisualStudio.Platform.VSEditor, PublicKey=" + ThisAssembly.PublicKey)]

//
// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
//
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, Flags = SecurityPermissionFlag.Execution)]
[assembly: ReliabilityContract(Consistency.MayCorruptProcess, Cer.MayFail)]
