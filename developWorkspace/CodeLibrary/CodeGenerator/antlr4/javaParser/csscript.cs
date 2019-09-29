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
using Antlr4.Runtime;
using System.Reflection;
using DevelopWorkspace.Base;
using System.Threading;
public class Script
{

    public static void Main(string[] args)
    {
        var setting = new
        {
            java = "java.exe",
            target = AppDomain.CurrentDomain.BaseDirectory + @"antlr4\" + "antlr4-csharp-4.6.6-complete.jar",
            runtime = AppDomain.CurrentDomain.BaseDirectory + @"antlr4\" + "Antlr4.Runtime.dll",
            csc = @"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc",
            name_space = "Java.Code",
            dest = AppDomain.CurrentDomain.BaseDirectory + @"compiled\",
            start_rule = "file",
            grammar = "Java.Code",
            lexer = AppDomain.CurrentDomain.BaseDirectory + @"compiled\JavaLexer.g4",
            parser = AppDomain.CurrentDomain.BaseDirectory + @"compiled\JavaParser.g4"
        };

        System.IO.File.WriteAllText(@"{lexer}".FormatWith(setting), args[1]);
        System.IO.File.WriteAllText(@"{parser}".FormatWith(setting), args[2]);

        string genetorCommand = @"{java} -cp ""{target}"" org.antlr.v4.CSharpTool -o {dest} -encoding UTF-8 -Dlanguage=CSharp -package {name_space}  {lexer}".FormatWith(setting);
        DevelopWorkspace.Base.Logger.WriteLine(genetorCommand);
        DevelopWorkspace.Base.Utils.Script.executeExternCommand(genetorCommand);

        genetorCommand = @"{java} -cp ""{target}"" org.antlr.v4.CSharpTool -o {dest} -encoding UTF-8 -Dlanguage=CSharp -visitor -package {name_space}  {parser}".FormatWith(setting);
        DevelopWorkspace.Base.Logger.WriteLine(genetorCommand);
        DevelopWorkspace.Base.Utils.Script.executeExternCommand(genetorCommand);

        string compileCommand = @"{csc} /r:""{runtime}""  /r:DevelopWorkspace.Base.dll /target:library /out:.\compiled\{grammar}.dll /warn:0 /nologo /debug ""{dest}*.cs""".FormatWith(setting);
        DevelopWorkspace.Base.Logger.WriteLine(compileCommand);
        DevelopWorkspace.Base.Utils.Script.executeExternCommand(compileCommand);
    }
}