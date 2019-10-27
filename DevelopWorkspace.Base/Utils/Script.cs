using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
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
            StringBuilder output = new StringBuilder();
            StringBuilder error = new StringBuilder();

            using (Process process = new Process())
            {
                process.StartInfo.FileName = "cmd.exe";            // 设定程序名;
                process.StartInfo.Arguments = " /c " + cliCommand;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.CreateNoWindow = true;           // 不显示dos 窗口


                using (AutoResetEvent outputWaitHandle = new AutoResetEvent(false))
                using (AutoResetEvent errorWaitHandle = new AutoResetEvent(false))
                {
                    process.OutputDataReceived += (sender, e) => {
                        if (e.Data == null)
                        {
                            try
                            {
                                outputWaitHandle.Set();
                            }
                            catch (Exception ex) { }
                        }
                        else
                        {
                            output.AppendLine(e.Data);
                        }
                    };
                    process.ErrorDataReceived += (sender, e) =>
                    {
                        if (e.Data == null)
                        {
                            try
                            {
                                errorWaitHandle.Set();
                            }
                            catch (Exception ex) { }
                        }
                        else
                        {
                            error.AppendLine(e.Data);
                        }
                    };

                    process.Start();

                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    if (process.WaitForExit(5000) &&
                        outputWaitHandle.WaitOne(5000) &&
                        errorWaitHandle.WaitOne(5000))
                    {
                    }
                    else
                    {
                        // Timed out.
                    }
                }
            }
            DevelopWorkspace.Base.Logger.WriteLine(output.ToString());
            DevelopWorkspace.Base.Logger.WriteLine(error.ToString());
            
            return output.Append(error).ToString();
        }
    }
}
