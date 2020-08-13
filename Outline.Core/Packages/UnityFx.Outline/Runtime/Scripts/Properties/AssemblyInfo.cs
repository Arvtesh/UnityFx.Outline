// Copyright (C) 2019-2020 Alexander Bogarsukov. All rights reserved.
// See the LICENSE.md file in the project root for more information.

using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("UnityFx.Outline")]
[assembly: AssemblyProduct("UnityFx.Outline")]
[assembly: AssemblyDescription("Screen-space outlines for Unity3d.")]
#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif
[assembly: AssemblyCompany("")]
[assembly: AssemblyCopyright("Copyright © Alexander Bogarsukov 2019-2020")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// Make internals visible to the editor assembly.
[assembly: InternalsVisibleTo("UnityFx.Outline.Editor")]
[assembly: InternalsVisibleTo("UnityFx.Outline.URP")]
