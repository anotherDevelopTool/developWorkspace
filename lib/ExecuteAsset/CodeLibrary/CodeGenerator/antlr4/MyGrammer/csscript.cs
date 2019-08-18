using System;
using Microsoft.CSharp;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using System.Text;
using System.Xml;
using  System.Diagnostics;
using System.IO;
using Newtonsoft.Json;
using Antlr4.Runtime;
using System.Reflection;
using DevelopWorkspace.Base;
public class Script
{

    public static void Main(string[] args)
    {
        var setting = new {
            java="java.exe",
            target=AppDomain.CurrentDomain.BaseDirectory +@"antlr4\"+ "antlr4-csharp-4.6.1-complete.jar",
            runtime=AppDomain.CurrentDomain.BaseDirectory +@"antlr4\"+ "Antlr4.Runtime.dll",
            csc=@"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc",
            name_space="ConsoleApplication1",
            dest=AppDomain.CurrentDomain.BaseDirectory +@"compiled\",
            start_rule="program",
            grammar="MyGrammar",
            g4=@".\compiled\MyGrammar.g4"
        };
    
        System.IO.File.WriteAllText(@"{g4}".FormatWith(setting), args[1]);
        string input = args[0];

        string genetorCommand = @"{java} -cp ""{target}"" org.antlr.v4.CSharpTool -o . -encoding UTF-8 -no-listener -Dlanguage=CSharp -package {name_space}  {g4}".FormatWith(setting);
        DevelopWorkspace.Base.Logger.WriteLine(genetorCommand);
        executeJavaCommand(genetorCommand);
        
        string compileCommand = @"{csc} /r:""{runtime}""  /r:DevelopWorkspace.Base.dll /target:library /out:.\compiled\{grammar}.dll /warn:0 /nologo /debug ""{dest}*.cs""".FormatWith(setting);
        DevelopWorkspace.Base.Logger.WriteLine(compileCommand);
        executeJavaCommand(compileCommand);

        var stream = new AntlrInputStream(input);
        
        System.Reflection.Assembly addinAssembly = System.Reflection.Assembly.LoadFrom(@"{dest}{grammar}.dll".FormatWith(setting));
        Type lexerType = addinAssembly.GetType("{name_space}.{grammar}Lexer".FormatWith(setting));
        Type parserType = addinAssembly.GetType("{name_space}.{grammar}Parser".FormatWith(setting));
        System.Reflection.ConstructorInfo lexerConstructor = lexerType.GetConstructor(new Type[]{typeof(ICharStream)});
        Lexer lexer = lexerConstructor.Invoke(new Object[] { stream}) as Lexer;
        var tokens = new CommonTokenStream(lexer);
        System.Reflection.ConstructorInfo parserTypeConstructor = parserType.GetConstructor(new Type[]{typeof(ITokenStream)});
        Parser parser = parserTypeConstructor.Invoke(new Object[] { tokens }) as Parser;
        MethodInfo  method = parserType.GetMethod("{start_rule}".FormatWith(setting),BindingFlags.Public | BindingFlags.Instance,null,new Type[] { },null);            
        dynamic tree = method.Invoke( parser, new Object[] { } );
        DevelopWorkspace.Base.Logger.WriteLine(tree.ToStringTree(parser));
        
    }
    public static string executeJavaCommand(string cliCommand)
    {
        ProcessStartInfo start = new ProcessStartInfo ("cmd.exe" );
        start.FileName = "cmd.exe" ;            // 设定程序名
        start.Arguments = " /c " + cliCommand;
        start.CreateNoWindow = true ;           // 不显示dos 窗口
        start.UseShellExecute = false ;         // 是否指定操作系统外壳进程启动程序，没有这行，调试时编译器会通知你加上的...orz
        start.RedirectStandardInput = true ;
        start.RedirectStandardOutput = true ;   // 重新定向标准输入、输出流
        
        start.RedirectStandardError = true ;    // 重新定向标准输入、输出流
        
        Process p = Process .Start(start);
        StreamReader reader = p.StandardOutput;         // 截取输出流
        StreamReader readerError = p.StandardError;     // 截取输出流
        string line = reader.ReadLine();                // 每次读一行
        while (!reader.EndOfStream)                     // 不为空则读取
        {
            DevelopWorkspace.Base.Logger.WriteLine(line);
            line = reader.ReadLine();
        }
        DevelopWorkspace.Base.Logger.WriteLine(line);
        line = readerError.ReadLine();                  // 每次读一行
        while (!readerError.EndOfStream)                // 不为空则读取
        {
            DevelopWorkspace.Base.Logger.WriteLine(line);
            line = readerError.ReadLine();
        }
        DevelopWorkspace.Base.Logger.WriteLine(line);
               
        p.WaitForExit();    // 等待程序执行完退出进程
        p.Close();          // 关闭进程
        reader.Close();     // 关闭流
        
        return "";
    }    
}