using System;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("NiL.JS")]
[assembly: AssemblyProduct("Elementary.JS")]
[assembly: AssemblyDescription("JavaScript engine for .NET")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyCopyright("(C) NiLProject 2013-2017, (C) Yuuki Wesp 2019")]
[assembly: AssemblyTrademark("NiL.JS")]
[assembly: AssemblyVersion("2.6.1204")]
[assembly: AssemblyFileVersion("2.6.1204")]
[assembly: NeutralResourcesLanguage("en-US")]
[assembly: ComVisible(false)]
[assembly: CLSCompliant(true)]

#if !PORTABLE && !NETCORE
[assembly: Guid("a70afe5a-2b29-49fd-afbf-28794042ea21")]
#endif

internal static class InternalInfo
{
    internal const string Version = "2.5.1070";
}
