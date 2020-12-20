using System;
using Microsoft.CSharp;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using System.Text;
using System.Xml;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;
using System.Reflection;
using DevelopWorkspace.Base;
using System.Threading;
public class Script
{

    public static void Main(string[] args)
    {
        var setting = new
        {
            CefSharpWpf = AppDomain.CurrentDomain.BaseDirectory + @"CefSharp.Wpf.dll",
            CefSharp = AppDomain.CurrentDomain.BaseDirectory + @"CefSharp\" + "CefSharp.dll",
            CefSharpCore = AppDomain.CurrentDomain.BaseDirectory + @"CefSharp\" + "CefSharp.Core.dll",
            csc = @"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc",
            dest = AppDomain.CurrentDomain.BaseDirectory + @"compiled\",
            grammar = "DevelopWorkspace.Chrome",
            vistor = AppDomain.CurrentDomain.BaseDirectory + @"compiled\CefSharpUtil.cs"
        };

        System.IO.File.WriteAllText(@"{vistor}".FormatWith(setting), args[0]);

        string compileCommand = @"{csc} /r:""{CefSharp}""  /r:""{CefSharpCore}""  /r:""{CefSharpWpf}"" /r:DevelopWorkspace.Base.dll /target:library /out:.\compiled\{grammar}.dll /warn:0 /nologo /debug ""{dest}*.cs""".FormatWith(setting);
        DevelopWorkspace.Base.Logger.WriteLine(compileCommand);
        DevelopWorkspace.Base.Utils.Script.executeExternCommand(compileCommand);
    }
}