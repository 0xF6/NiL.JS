﻿using System;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;

#if DEV
[assembly: AssemblyTitleAttribute("NiL.JS for Developers")]
[assembly: AssemblyProductAttribute("NiL.JS Dev")]
#else
[assembly: AssemblyTitleAttribute("NiL.JS")]
[assembly: AssemblyProductAttribute("NiL.JS")]
#endif
[assembly: AssemblyDescriptionAttribute("JavaScript engine for .NET")]
[assembly: AssemblyCompanyAttribute("NiLProject")]
[assembly: AssemblyCopyrightAttribute("Copyright © NiLProject 2014")]
[assembly: AssemblyTrademarkAttribute("NiL.JS")]
[assembly: AssemblyVersionAttribute("1.1.610")]
[assembly: AssemblyFileVersionAttribute("1.1.610")]
[assembly: NeutralResourcesLanguageAttribute("en-US")]
[assembly: ComVisibleAttribute(false)]
[assembly: GuidAttribute("a70afe5a-2b29-49fd-afbf-28794042ea21")]
[assembly: CLSCompliant(true)]