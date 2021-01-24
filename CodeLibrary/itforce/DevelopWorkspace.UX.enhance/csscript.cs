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
            Word = AppDomain.CurrentDomain.BaseDirectory + @"Microsoft.Office.Interop.Word.dll",
            Fluent = AppDomain.CurrentDomain.BaseDirectory + @"Fluent.dll",
            Xaml = @"C:\Windows\Microsoft.NET\assembly\GAC_MSIL\System.Xaml\v4.0_4.0.0.0__b77a5c561934e089\System.Xaml.dll",
            WindowsBase = @"C:\Windows\Microsoft.NET\assembly\GAC_MSIL\WindowsBase\v4.0_4.0.0.0__31bf3856ad364e35\WindowsBase.dll",
            PresentationCore = @"C:\Windows\assembly\NativeImages_v4.0.30319_32\PresentationCore\ffb4b0ce558b3c31c9af6094a95b9e7c\PresentationCore.ni.dll",
            PresentationFramework = @"C:\Windows\Microsoft.NET\assembly\GAC_MSIL\PresentationFramework\v4.0_4.0.0.0__31bf3856ad364e35\PresentationFramework.dll",
            csc = @"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc",
            dest = AppDomain.CurrentDomain.BaseDirectory + @"compiled\",
            grammar = "DevelopWorkspace.UX.enhance",
            vistor = AppDomain.CurrentDomain.BaseDirectory + @"compiled\UX.cs"
        };

        System.IO.File.WriteAllText(@"{vistor}".FormatWith(setting), args[0]);

        string compileCommand = @"{csc} /r:""{Word}""  /r:""{Fluent}""  /r:""{Xaml}"" /r:""{WindowsBase}""  /r:""{PresentationCore}"" /r:""{PresentationFramework}"" /r:DevelopWorkspace.Base.dll /target:library /out:.\compiled\{grammar}.dll /warn:0 /nologo /debug ""{dest}*.cs""".FormatWith(setting);
        DevelopWorkspace.Base.Logger.WriteLine(compileCommand);
        DevelopWorkspace.Base.Utils.Script.executeExternCommand(compileCommand);
    }
}