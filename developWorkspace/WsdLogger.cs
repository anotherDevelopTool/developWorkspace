using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DevelopWorkspace.Main
{
    public class WsdLogger
    {
        static WsdLogger This = null;
        private static object lockobj = new object();
        private string logDirectory;
        private string loggerDate;
        private string loggerFile;
        private string loggerName;
        private string logThisDirectory;

        //https://www.cnblogs.com/tuqun/p/3889978.html 原封不动拿来主义
        /// <summary>
        ///  创建日志对象
        /// </summary>
        /// <param name="loggerName">日志文件名</param>
        internal WsdLogger()
        {

            this.loggerName = "DefaultLogger";
            //创建程序记录日志文件夹
            this.logDirectory = new FileInfo(Assembly.GetExecutingAssembly().GetName().CodeBase.Replace("file:///", string.Empty)).DirectoryName + @"\logs";
            if (!Directory.Exists(this.logDirectory))
            {
                Directory.CreateDirectory(this.logDirectory);
            }

        }

        /// <summary>
        /// 写入日志内容
        /// </summary>
        /// <param name="line"></param>
        public void Write(string line)
        {
            try
            {
                lock (lockobj)
                {
                    string str = DateTime.Now.ToString("yyyy-MM-dd");
                    if ((this.loggerFile == "") || !str.Equals(this.loggerDate))
                    {
                        this.loggerDate = str;
                        this.logThisDirectory = this.logDirectory + @"\" + this.loggerDate;
                        if (!Directory.Exists(this.logThisDirectory))
                        {
                            Directory.CreateDirectory(this.logThisDirectory);
                        }
                        this.loggerFile = this.logThisDirectory + @"\" + this.loggerName + ".log";
                    }
                    if (File.Exists(this.loggerFile))
                    {
                        //判断如果超过1M就进行文件分割
                        FileInfo file = new FileInfo(this.loggerFile);
                        if (file.Length > 1048576)
                        {
                            file.CopyTo(this.logThisDirectory + @"\" + this.loggerName + DateTime.Now.ToString("hhmmss") + ".log", true);
                            file.Delete();
                        }
                        using (StreamWriter writer = File.AppendText(this.loggerFile))
                            writer.WriteLine(string.Format("{0} {1} {2}", DateTime.Now.ToString("HH:mm:ss:ffff"), Thread.CurrentThread.Name, line));
                    }

                    if (!File.Exists(this.loggerFile))
                        using (StreamWriter writer = File.CreateText(this.loggerFile))
                            writer.WriteLine(string.Format("{0} {1} {2}", DateTime.Now.ToString("HH:mm:ss:ffff"), Thread.CurrentThread.Name, line));
                }
            }
            catch (Exception exception)
            {
                using (StreamWriter writer2 = File.Exists("log.txt") ? File.AppendText("log.txt") : File.CreateText("log.txt"))
                {
                    try
                    {
                        writer2.WriteLine(exception.ToString());
                        writer2.Close();
                    }
                    catch
                    {
                    }
                }
            }
        }

        public static void WriteLine(string line) {
            if (This == null) {
                This = new WsdLogger();
            }
            This.Write(line);
        }
    }
}
