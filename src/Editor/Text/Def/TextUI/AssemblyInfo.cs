//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License. See License.txt in the project root for license information.
//
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.Versioning;
using System.Security.Permissions;

[assembly: InternalsVisibleTo("Microsoft.VisualStudio.Text.MultiCaret.Implementation, PublicKey=" + ThisAssembly.PublicKey)]
[assembly: InternalsVisibleTo("Microsoft.VisualStudio.Platform.VSEditor, PublicKey=" + ThisAssembly.PublicKey)]
[assembly: InternalsVisibleTo("Microsoft.VisualStudio.Editor.Implementation, PublicKey=" + ThisAssembly.PublicKey)]
[assembly: InternalsVisibleTo("Microsoft.VisualStudio.Language.Implementation, PublicKey=" + ThisAssembly.PublicKey)]
[assembly: InternalsVisibleTo("Microsoft.VisualStudio.UI.Text.Commanding.Implementation, PublicKey=" + ThisAssembly.PublicKey)]
[assembly: InternalsVisibleTo("Microsoft.VisualStudio.UI.Text.Commanding.Implementation.UnitTests, PublicKey=" + ThisAssembly.PublicKey)]
[assembly: InternalsVisibleTo("Microsoft.VisualStudio.Text.UI.Utilities, PublicKey=" + ThisAssembly.PublicKey)]
[assembly: InternalsVisibleTo("Microsoft.VisualStudio.Platform.VSEditor, PublicKey=" + ThisAssembly.PublicKey)]
[assembly: InternalsVisibleTo("Microsoft.VisualStudio.Text.DifferenceViewer.Implementation, PublicKey=" + ThisAssembly.PublicKey)]
[assembly: InternalsVisibleTo("Microsoft.VisualStudio.Text.Outlining.Implementation, PublicKey=" + ThisAssembly.PublicKey)]
[assembly: InternalsVisibleTo("Microsoft.VisualStudio.UI.Text.EditorOperations.Implementation, PublicKey=" + ThisAssembly.PublicKey)]

// InternalsVisibleTo for VS for Mac implementation assembly:
[assembly: InternalsVisibleTo("Microsoft.VisualStudio.Text.Implementation, PublicKey=0024000004800000940000000602000000240000525341310004000001000100e57febc1f220077550a65e338d3d15d7cbd189cf4f62f7c3829dcb2f8441a6c40631d172e3deb4dc0bb7237b44ec9daeb9bd7d72c3d64c4f52b968795443cb58bc341583c29440345b8c35f72f6a31aecb2903376136f8fc35779bb422eb643f8668fa6605c697bff927e3bb10745328ff878bd1b7e42bbcb839f04baa8460bd")]
[assembly: InternalsVisibleTo("Microsoft.VisualStudio.UI.Text.Cocoa.View.Implementation, PublicKey=" + ThisAssembly.PublicKey)]
[assembly: InternalsVisibleTo("Microsoft.VisualStudio.UI.Text.AdornmentLibrary.VisibleWhitespace.Implementation, PublicKey=" + ThisAssembly.PublicKey)]
[assembly: InternalsVisibleTo("Microsoft.VisualStudio.Text.UI.Cocoa.Utilities, PublicKey=" + ThisAssembly.PublicKey)]
[assembly: InternalsVisibleTo("MonoDevelop.TextEditor, PublicKey=002400000c800000940000000602000000240000525341310004000001000100e1290d741888d13312c0cd1f72bb843236573c80158a286f11bb98de5ee8acc3142c9c97b472684e521ae45125d7414558f2e70ac56504f3e8fe80830da2cdb1cda8504e8d196150d05a214609234694ec0ebf4b37fc7537e09d877c3e65000f7467fa3adb6e62c82b10ada1af4a83651556c7d949959817fed97480839dd39b")]


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
