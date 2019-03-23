using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Security.Permissions;

[assembly: InternalsVisibleTo ("Microsoft.VisualStudio.Language.Intellisense.UnitTests, PublicKey=" + ThisAssembly.PublicKey )]
[assembly: InternalsVisibleTo ("Microsoft.VisualStudio.Language.Intellisense.IntegrationTests, PublicKey=" + ThisAssembly.PublicKey )]
[assembly: InternalsVisibleTo ("Microsoft.VisualStudio.Language.Intellisense.UnitTestHelper, PublicKey=" + ThisAssembly.PublicKey )]
[assembly: InternalsVisibleTo ("Microsoft.VisualStudio.Language.Intellisense.Implementation, PublicKey=" + ThisAssembly.PublicKey)]
[assembly: InternalsVisibleTo ("Microsoft.VisualStudio.Language.Intellisense.CompletionIntegrationTests, PublicKey=" + ThisAssembly.PublicKey)]

//
// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
//
[assembly: AssemblyTrademark ("")]
[assembly: AssemblyCulture ("")]
#pragma warning disable 618
[assembly: SecurityPermission (SecurityAction.RequestMinimum, Flags = SecurityPermissionFlag.Execution)]
#pragma warning restore 618
[assembly: ReliabilityContract(Consistency.MayCorruptProcess, Cer.MayFail)]
