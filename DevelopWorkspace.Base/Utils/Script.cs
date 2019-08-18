using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DevelopWorkspace.Base.Utils
{
    public class Script
    {
        public static void getPatternFilesByTraverseTree(string pattern, string searchpath, List<string> pathlist,List<Regex> exceptlist)
        {
            System.IO.FileInfo[] files = null;
            System.IO.DirectoryInfo[] subDirs = null;
            DirectoryInfo root = new DirectoryInfo(searchpath);
            // First, process all the files directly under this folder
            try
            {
                files = root.GetFiles(pattern);
            }
            catch (System.IO.DirectoryNotFoundException e)
            {
                //Console.WriteLine(e.Message);
            }

            if (files != null)
            {
                foreach (var file in files)
                {
                    bool includeflg = true;
                    foreach (var regex in exceptlist) {
                        if (regex.Match(file.Name).Success) {
                            includeflg = false;
                            break;
                        }
                    }
                    if(includeflg) pathlist.Add(file.FullName);
                }
                subDirs = root.GetDirectories();
                foreach (System.IO.DirectoryInfo dirInfo in subDirs)
                {
                    getPatternFilesByTraverseTree(pattern, dirInfo.FullName, pathlist, exceptlist);
                }
            }
        }
        public static string executeExternCommand(string cliCommand)
        {
            ProcessStartInfo start = new ProcessStartInfo("cmd.exe");
            start.FileName = "cmd.exe";            // 设定程序名
            start.Arguments = " /c " + cliCommand;
            start.CreateNoWindow = true;           // 不显示dos 窗口
            start.UseShellExecute = false;         // 是否指定操作系统外壳进程启动程序，没有这行，调试时编译器会通知你加上的...orz
            start.RedirectStandardInput = true;
            start.RedirectStandardOutput = true;   // 重新定向标准输入、输出流

            start.RedirectStandardError = true;    // 重新定向标准输入、输出流

            Process p = Process.Start(start);
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
}
