using System;
using Microsoft.CSharp;
using System.Drawing;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Xml;
using System.Windows.Markup;
using DevelopWorkspace.Base;
using Heidesoft.Components.Controls;
using System.Windows.Threading;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Diagnostics;
using System.Threading;
using Xceed.Wpf.AvalonDock.Layout;
using Excel = Microsoft.Office.Interop.Excel;
using System.Reflection;
using DevelopWorkspace.Base;
using DevelopWorkspace.Base.Model;
using DevelopWorkspace.Base.Utils;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Collections.ObjectModel;
using CefSharp;
using CefSharp.Wpf;
//css_reference CefSharp.Wpf.dll
public class ProcessMonitorInfo : ViewModelBase
{
    private string _service_type = "rba-bo-api";
    public string service_type
    {
        get { return _service_type; }
        set
        {
            if (_service_type != value)
            {
                _service_type = value;
                RaisePropertyChanged("service_type");
            }
        }
    }
    private string _process_name = "java.exe";
    public string process_name
    {
        get { return _process_name; }
        set
        {
            if (_process_name != value)
            {
                _process_name = value;
                RaisePropertyChanged("process_name");
            }
        }
    }
    private string _logfile = "system.log";
    public string logfile
    {
        get { return _logfile; }
        set
        {
            if (_logfile != value)
            {
                _logfile = value;
                RaisePropertyChanged("logfile");
            }
        }
    }
    private bool _alive = false;
    public bool alive
    {
        get { return _alive; }
        set
        {
            if (_alive != value)
            {
                _alive = value;
                RaisePropertyChanged("alive");
            }
        }
    }
    private bool _registed = false;
    public bool registed
    {
        get { return _registed; }
        set
        {
            if (_registed != value)
            {
                _registed = value;
                RaisePropertyChanged("registed");
            }
        }
    }
    private string _current_dir = "system.log";
    public string current_dir
    {
        get { return _current_dir; }
        set
        {
            if (_current_dir != value)
            {
                _current_dir = value;
                RaisePropertyChanged("current_dir");
            }
        }
    }
}
public class ProcessMonitorSetting
{
    public List<ProcessMonitorInfo> setting;
}
public class Script
{
    //TODO 面向Addin基类化
    [AddinMeta(Name = "embedChrome", Date = "2009-07-20", Description = "monitor utility", LargeIcon = "monitor", Red = 128, Green = 145, Blue = 213)]
    public class ViewModel : DevelopWorkspace.Base.Model.ScriptBaseViewModel
    {
        System.Windows.Controls.ListView listView;
        ICSharpCodeX.AvalonEdit.Edi.EdiTextEditor sqlSource;
        FileSystemEventHandler fileSystemEventHandler;
        string settingFileHash = "";
        string userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name.Substring(System.Security.Principal.WindowsIdentity.GetCurrent().Name.IndexOf(@"\") + 1);
        ObservableCollection<ProcessMonitorInfo> processMonitorInfoList = new ObservableCollection<ProcessMonitorInfo>();


        [MethodMeta(Name = "最新log取得", Date = "2009-07-20", Description = "", LargeIcon = "logger")]
        public void EventHandler1(object sender, RoutedEventArgs e)
        {
            try
            {
                ProcessMonitorInfo ProcessMonitorInfo = (ProcessMonitorInfo)listView.SelectedItem;
                string logfile = ProcessMonitorInfo.logfile.FormatWith(new { userName = userName });
                bool alive = ProcessMonitorInfo.alive;
                string process_name = ProcessMonitorInfo.process_name;

                if (!fileExist(ref logfile))
                {
                    DevelopWorkspace.Base.Logger.WriteLine(logfile + " does not exist...");
                    return;
                }
                bool bMore = false;
                Logger.WriteLine(ReadLastLines(logfile, 0, 150, out bMore).Aggregate((a, b) => a + "\n" + b));
            }
            catch (Exception ex)
            {
                DevelopWorkspace.Base.Logger.WriteLine(ex.ToString());
            }

        }
        [MethodMeta(Name = "最新status取得", Date = "2009-07-20", Description = "", LargeIcon = "monitor")]
        public void EventHandler2(object sender, RoutedEventArgs e)
        {
            try
            {
                Process[] processes = Process.GetProcessesByName("chrome");
                List<ProcessMonitorInfo> removedList = new List<ProcessMonitorInfo>();
                foreach (var processMonitorInfo in processMonitorInfoList)
                {
                    var process = processes.FirstOrDefault(p => p.ProcessName.Equals(processMonitorInfo.process_name) && p.GetCurrentDirectory().Equals(processMonitorInfo.current_dir));
                    if (process != null)
                    {
                        processMonitorInfo.alive = true;
                    }
                    else
                    {
                        if (processMonitorInfo.registed)
                        {
                            processMonitorInfo.alive = false;
                        }
                        else
                        {
                            removedList.Add(processMonitorInfo);
                        }
                    }
                }
                removedList.ForEach(p => processMonitorInfoList.Remove(p));
                foreach (var process in processes)
                {
                    var selected = processMonitorInfoList.FirstOrDefault(p => p.process_name.Equals(process.ProcessName) && process.GetCurrentDirectory().Equals(p.current_dir));
                    if (selected == null)
                    {
                        processMonitorInfoList.Add(new ProcessMonitorInfo
                        {
                            process_name = process.ProcessName,
                            current_dir = process.GetCurrentDirectory(),
                            alive = true
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                DevelopWorkspace.Base.Logger.WriteLine(ex.ToString());
            }
        }
        public bool fileExist(ref string filename)
        {
            if (Regex.IsMatch(filename, "^[a-z]:", RegexOptions.IgnoreCase))
            {
            }
            else
            {
                filename = System.IO.Path.Combine(DevelopWorkspace.Main.StartupSetting.instance.homeDir, filename);
            }
            if (File.Exists(filename))
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        public override UserControl getView(string strXaml)
        {
            StringReader strreader = new StringReader(strXaml);
            XmlTextReader xmlreader = new XmlTextReader(strreader);
            Cef.Initialize(new CefSettings());
            CefSharp.Wpf.ChromiumWebBrowser cefBrowserView = new CefSharp.Wpf.ChromiumWebBrowser("https://github.com/cefsharp/cefsharp");
            UserControl userControl = new UserControl();
            userControl.Content = cefBrowserView;
            return userControl;
        }
        //后期清理处理
        public bool DoClearance(string bookName)
        {
            //
            if (DevelopWorkspace.Main.AppConfig.SysConfig.This.WatchFileSystemActivity)
            {
                (Application.Current.MainWindow as DevelopWorkspace.Main.MainWindow).fileSystemWatcher.Changed -= fileSystemEventHandler;
            }
            return true;
        }

        private void OnProcess(object source, FileSystemEventArgs e)
        {
            try
            {
                // get the file's extension
                if (getResPathByExt("setting.json").Equals(e.FullPath))
                {
                    string json = DevelopWorkspace.Base.Utils.Files.ReadAllText(getResPathByExt("setting.json"), Encoding.UTF8);
                    string lastedSettingFileHash = DevelopWorkspace.Base.Utils.Files.GetSha256Hash(json);
                    if (!settingFileHash.Equals(lastedSettingFileHash))
                    {
                        settingFileHash = lastedSettingFileHash;
                        listView.Dispatcher.BeginInvoke((Action)delegate ()
                        {
                            json = DevelopWorkspace.Base.Utils.Files.ReadAllText(getResPathByExt("setting.json"), Encoding.UTF8);
                            if (!string.IsNullOrWhiteSpace(json))
                            {
                                ProcessMonitorSetting processMonitorSetting = (ProcessMonitorSetting)JsonConvert.DeserializeObject(json, typeof(ProcessMonitorSetting));
                                processMonitorSetting.setting.ForEach(item => processMonitorInfoList.Add(item));
                                listView.DataContext = processMonitorInfoList;
                                listView.SelectedIndex = 0;
                            }
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                DevelopWorkspace.Base.Logger.WriteLine(ex.Message, DevelopWorkspace.Base.Level.DEBUG);
            }
        }

        // Read last lines of a file....
        public List<string> ReadLastLines(string StatisticsFile, int nFromLine, int nNoLines, out bool bMore)
        {
            // Initialise more
            bMore = false;
            try
            {
                char[] buffer = null;
                //lock (strMessages)  Lock something if you need to....
                {
                    if (File.Exists(StatisticsFile))
                    {
                        // Open file
                        using (StreamReader sr = new StreamReader(StatisticsFile))
                        {
                            long FileLength = sr.BaseStream.Length;
                            int c, linescount = 0;
                            long pos = FileLength - 1;
                            long PreviousReturn = FileLength;
                            // Process file
                            while (pos >= 0 && linescount < nFromLine + nNoLines) // Until found correct place
                            {
                                // Read a character from the end
                                c = BufferedGetCharBackwards(sr, pos);
                                if (c == Convert.ToInt32('\n'))
                                {
                                    // Found return character
                                    if (++linescount == nFromLine)
                                        // Found last place
                                        PreviousReturn = pos + 1; // Read to here
                                }
                                // Previous char
                                pos--;
                            }
                            pos++;
                            // Create buffer
                            buffer = new char[PreviousReturn - pos];
                            sr.DiscardBufferedData();
                            // Read all our chars
                            sr.BaseStream.Seek(pos, SeekOrigin.Begin);
                            sr.Read(buffer, (int)0, (int)(PreviousReturn - pos));
                            sr.Close();
                            // Store if more lines available
                            if (pos > 0)
                                // Is there more?
                                bMore = true;
                        }
                        if (buffer != null)
                        {
                            // Get data
                            string strResult = new string(buffer);
                            strResult = strResult.Replace("\r", "");

                            // Store in List
                            List<string> strSort = new List<string>(strResult.Split('\n'));
                            // Reverse order
                            //strSort.Reverse();

                            return strSort;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("ReadLastLines Exception:" + ex.ToString());
            }
            // Lets return a list with no entries
            return new List<string>();
        }

        const int CACHE_BUFFER_SIZE = 1024;
        private long ncachestartbuffer = -1;
        private char[] cachebuffer = null;
        // Cache the file....
        private int BufferedGetCharBackwards(StreamReader sr, long iPosFromBegin)
        {
            // Check for error
            if (iPosFromBegin < 0 || iPosFromBegin >= sr.BaseStream.Length)
                return -1;
            // See if we have the character already
            if (ncachestartbuffer >= 0 && ncachestartbuffer <= iPosFromBegin && ncachestartbuffer + cachebuffer.Length > iPosFromBegin)
            {
                return cachebuffer[iPosFromBegin - ncachestartbuffer];
            }
            // Load into cache
            ncachestartbuffer = (int)Math.Max(0, iPosFromBegin - CACHE_BUFFER_SIZE + 1);
            int nLength = (int)Math.Min(CACHE_BUFFER_SIZE, sr.BaseStream.Length - ncachestartbuffer);
            cachebuffer = new char[nLength];
            sr.DiscardBufferedData();
            sr.BaseStream.Seek(ncachestartbuffer, SeekOrigin.Begin);
            sr.Read(cachebuffer, (int)0, (int)nLength);

            return BufferedGetCharBackwards(sr, iPosFromBegin);
        }

    }

    public class MainWindow : Window
    {
        public MainWindow(string strXaml)
        {
            Width = 600;
            Height = 800;

            Grid grid = new Grid();
            Content = grid;

            StackPanel parent = new StackPanel();
            grid.Children.Add(parent);

            ViewModel model = new ViewModel();

            var methods = model.GetType().GetMethods().Where(m => m.GetCustomAttributes(typeof(MethodMetaAttribute), false).Length > 0).ToList();
            for (int i = 0; i < methods.Count; i++)
            {
                var method = methods[i];
                var methodAttribute = (MethodMetaAttribute)Attribute.GetCustomAttribute(methods[i], typeof(MethodMetaAttribute));
                Button btn = new Button();
                btn.Content = methodAttribute.Name; ;
                parent.Children.Add(btn);
                btn.Click += (obj, subargs) =>
                {
                    method.Invoke(model, new object[] { obj, subargs });
                };
            }

            CefSharp.Wpf.ChromiumWebBrowser cefBrowserView = new CefSharp.Wpf.ChromiumWebBrowser();

            parent.Children.Add(cefBrowserView);

            model.install(strXaml);
        }
    }
    public static void Main(string[] args)
    {
        string strXaml = args[0].ToString();
        MainWindow win = new MainWindow(strXaml);
        win.Show();
    }
}
