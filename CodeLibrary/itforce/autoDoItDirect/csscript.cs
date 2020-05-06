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
using DevelopWorkspace.Main;
public class Script
{

    public static void Main(string[] args)
    {
    	MainWindow.ShellExecute(IntPtr.Zero, "open", @"C:\workspace\tools\autoit\SciTE4AutoIt3_Portable\SciTE.exe", @"C:\workspace\tools\autoit\SciTE4AutoIt3_Portable\properties\au3.properties", "", MainWindow.ShowWindowStyles.SW_SHOWNORMAL);
    	MainWindow.ShellExecute(IntPtr.Zero, "open", @"C:\workspace\tools\autoit\SciTE4AutoIt3_Portable\SciTE.exe", @"C:\workspace\tools\autoit\SciTE4AutoIt3_Portable\properties\baan.properties", "", MainWindow.ShowWindowStyles.SW_SHOWNORMAL);
    	//ShellExecute(IntPtr.Zero, "open", System.IO.Path.Combine(DevelopWorkspace.Main.StartupSetting.instance.homeDir, "help.htm"), "", "", ShowWindowStyles.SW_SHOWNORMAL);
    	
        var setting = new {
            AutoIt3 = @"C:\workspace\tools\autoit\autoit-v3\install\AutoIt3_x64.exe",
            au3file=@".\compiled\snippet.au3"
        };
    
        System.IO.File.WriteAllText(@"{au3file}".FormatWith(setting), args[0]);

        
        string compileCommand = @"{AutoIt3} {au3file} ".FormatWith(setting);
        DevelopWorkspace.Base.Logger.WriteLine(compileCommand);
        executeJavaCommand(compileCommand);



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