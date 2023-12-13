
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Resources;
using System;
using System.IO;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Text;


#if DEBUG
[assembly: AssemblyConfiguration("DEBUG")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif

// General Information
[assembly: AssemblyTitle("LockScreenBGInfo")]
[assembly: AssemblyDescription("Set image for Windows  lockscreen for all users")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("BGInfo")]
[assembly: AssemblyCopyright("Skorik (c) 2020")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Version informationr(
[assembly: AssemblyVersion("1.1.46.0")]
[assembly: AssemblyFileVersion("1.1.46.0")]


