//********************************************************************************
// Copyright (c) Microsoft Corporation. All rights reserved
//********************************************************************************

using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Security.Permissions;

#pragma warning disable CS0436 // ThisAssembly is internal, yet InternalsVisibleTo conflates them across assemblies
[assembly: InternalsVisibleTo("Microsoft.VisualStudio.Text.PatternMatching.UnitTests, PublicKey=" + ThisAssembly.PublicKey)]
#pragma warning restore CS0436

//
// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
//
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
#pragma warning disable 618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, Flags = SecurityPermissionFlag.Execution)]
#pragma warning restore 618
[assembly: ReliabilityContract(Consistency.MayCorruptProcess, Cer.MayFail)]
