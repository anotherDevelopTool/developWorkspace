using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace DevelopWorkspace.Main
{
    internal delegate void Invoker();
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    //计算主程序集需要的依存程序集的检索目录集和以及配置文件的位置，他们需要作为单一完整的目录，这块需要遵守默认规则原则
    public class StartupSetting : MarshalByRefObject
    {
        string[] _searchDirs = null;
        string _homeDir = null;
        internal StartupSetting()
        {
            AppDomain.CurrentDomain.SetData("StartupSetting", this);
        }
        public static StartupSetting instance {
            get
            {
                StartupSetting startup = AppDomain.CurrentDomain.GetData("StartupSetting") as StartupSetting;
                return startup;
            }
        }
        public override object InitializeLifetimeService()
        {
            //Remoting对象 无限生存期
            return null;
        }
        public string[] searchDirs
        {
            get
            {
                if (_searchDirs == null)
                {
                    string searchDirBase = homeDir;
                    //关联目录的根目录作为默认搜索位置，即app.config的所在目录
                    //Dependency
                    //    |-------app.config
                    //    libs
                    //       |-----antlr4
                    //       |-----loaded_assemblies
                    //       |-----code_assemblies

                    List<string> paths = new List<string>();
                    //主程序集目录作为默认搜索位置
                    paths.Add(AppDomain.CurrentDomain.BaseDirectory);
                    paths.Add(searchDirBase);
                    App.WalkDirectoryTree("*.dll", searchDirBase, paths);
                    _searchDirs =paths.ToArray();
                }
                return _searchDirs;

            }
        }
        public string homeDir
        {
            get
            {
                if (_homeDir == null)
                {
                    //通过app.config的位置倒推出homedir，注意这块的逻辑一定要在第三方程序集装载之前执行，也就是说只依赖主程序集（GAC除外），否则就做不到主程序可以单独放到任何位子执行的效果
                    string configPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DevelopWorkspace.exe.config");
                    if (System.IO.File.Exists(configPath))
                    {
                        _homeDir = AppDomain.CurrentDomain.BaseDirectory;
                    }
                    else
                    {
                        var finded = App.FindFileInPath("DevelopWorkspace.exe.config", AppDomain.CurrentDomain.BaseDirectory);
                        if (!string.IsNullOrEmpty(finded))
                        {
                            _homeDir=Path.GetDirectoryName(finded);
                        }
                        else
                        {
                            string iniPath = System.IO.Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DevelopWorkspace.exe.ini");
                            if (System.IO.File.Exists(iniPath))
                            {
                                string writtenPath = File.ReadAllLines(iniPath)[0];
                                string trickedDir = System.IO.Path.Combine(writtenPath, "DevelopWorkspace.exe.config");
                                if (System.IO.File.Exists(trickedDir))
                                {
                                    _homeDir = writtenPath;
                                }
                                else {
                                    Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
                                    ofd.DefaultExt = ".config";
                                    ofd.Filter = "config file|DevelopWorkspace.exe.config";
                                    if (ofd.ShowDialog() == true)
                                    {
                                        _homeDir = ofd.FileName.Substring(0, ofd.FileName.Length - "DevelopWorkspace.exe.config".Length);
                                        File.WriteAllLines(iniPath, new string[] { _homeDir });
                                    }
                                    else
                                    {
                                        Application.Current.Shutdown();
                                    }
                                }
                            }
                            else
                            {
                                Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
                                ofd.DefaultExt = ".config";
                                ofd.Filter = "config file|DevelopWorkspace.exe.config";
                                if (ofd.ShowDialog() == true)
                                {
                                    _homeDir = ofd.FileName.Substring(0, ofd.FileName.Length - "DevelopWorkspace.exe.config".Length);
                                    File.WriteAllLines(iniPath, new string[] { _homeDir });
                                }
                                else {
                                    Application.Current.Shutdown();
                                }
                            }
                        }
                    }
                }
                return _homeDir;

            }
        }
    }

    public partial class App : Application
    {
        static App()
        {

            //计算主程序集需要的依存程序集的检索目录集和以及配置文件的位置，他们需要作为单一完整的目录，这块需要遵守默认规则原则
            StartupSetting startup = new StartupSetting();

            AppDomain.CurrentDomain.SetData("APP_CONFIG_FILE", System.IO.Path.Combine(startup.homeDir, "DevelopWorkspace.exe.config"));


            #region 为了可以exe可以放到任意位置...下面这对实验代码程序集和主程序集在同一个目录或者主程序集的子目录时才有效，结果方案采用AppDomain.CurrentDomain.AssemblyResolve的方式
            //先默认路径下是否有config,如果没有去user临时路径是否有DevelopWorkspace.Main.exe.ini，有的话取出它里面的内容时为基本目录，否则弹出指定路径的对话框..
            //https://www.cnblogs.com/zhesong/p/pbpcf.html
            //AppDomain.CurrentDomain.SetData("DataDirectory", AppDomain.CurrentDomain.BaseDirectory);
            //AppDomain.CurrentDomain.SetData("PRIVATE_BINPATH", startup.searchDirs.Aggregate((total, next) => total + ";" + next));
            //AppDomain.CurrentDomain.SetData("BINPATH_PROBE_ONLY", startup.searchDirs.Aggregate((total, next) => total + ";" + next));
            //var m = typeof(AppDomainSetup).GetMethod("UpdateContextProperty", BindingFlags.NonPublic | BindingFlags.Static);
            //var funsion = typeof(AppDomain).GetMethod("GetFusionContext", BindingFlags.NonPublic | BindingFlags.Instance);
            //m.Invoke(null, new object[] { funsion.Invoke(AppDomain.CurrentDomain, null), "PRIVATE_BINPATH", startup.searchDirs.Aggregate((total, next) => total + ";" + next) });
            #endregion
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);


        }
        public static void WriteLineWrapper(string logtext) {
            //如果不使用反射会依存developworkspace.base.dll这个程序集，但是在启动伊始这个程序集可能实在未知未知，不能明示依存这个库
            //即使没有实际调用也会提前装载
            if (AppDomain.CurrentDomain.GetData("logger") != null)
            {
                var logger = AppDomain.CurrentDomain.GetData("logger");
                MethodInfo method = logger.GetType().GetMethod("WriteLine");
                object[] parameters = new object[] { logtext, 3 };
                method.Invoke(logger, parameters);
            }
            else {
                //在APP启动初期，正式的logger还没有就绪时如果出现例外使用下面的方式输出到本地文件中
                WsdLogger.WriteLine(logtext);
            }

        }
        public static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            // Ignore missing resources
            if (args.Name.Contains(".resources"))
                return null;

            // check for assemblies already loaded
            Assembly assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.FullName == args.Name);
            if (assembly != null)
                return assembly;




            // Try to load by filename - split out the filename of the full assembly name
            // and append the base path of the original assembly (ie. look in the same dir)
            string dllname = args.Name.Split(',')[0] + ".dll".ToLower();
            StartupSetting startup = AppDomain.CurrentDomain.GetData("StartupSetting") as StartupSetting;
            string asmFile = FindFileInPath(dllname, startup.homeDir);
            if (!string.IsNullOrEmpty(asmFile))
            {
                try
                {
                    return Assembly.LoadFrom(asmFile);
                }
                catch(Exception ex)
                {
                    WriteLineWrapper($"{ex.Message} failure");
                    return null;
                }
            }
            WriteLineWrapper($"can't load assembly: { dllname },please copy it to {startup.homeDir} or it's subdirectory");
            // FAIL - not found
            return null;
        }
        public static string FindFileInPath(string filename, string path)
        {
            filename = filename.ToLower();

            foreach (var fullFile in Directory.GetFiles(path))
            {
                var file = Path.GetFileName(fullFile).ToLower();
                if (file == filename)
                    return fullFile;

            }
            foreach (var dir in Directory.GetDirectories(path))
            {
                var file = FindFileInPath(filename, dir);
                if (!string.IsNullOrEmpty(file))
                    return file;
            }

            return null;
        }
        public static void WalkDirectoryTree(string pattern, string searchpath, List<string> pathlist)
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
                if (files.Count() > 0)
                {
                    pathlist.Add(root.FullName);
                }

                subDirs = root.GetDirectories();
                foreach (System.IO.DirectoryInfo dirInfo in subDirs)
                {
                    WalkDirectoryTree(pattern, dirInfo.FullName, pathlist);
                }
            }
        }

        public App()
        {
            ApplicationInitialize = _applicationInitialize;
        }
        public static new App Current
        {
            get { return Application.Current as App; }
        }
        internal delegate void ApplicationInitializeDelegate(SplashWindow splashWindow);
        internal ApplicationInitializeDelegate ApplicationInitialize;
        private void _applicationInitialize(SplashWindow splashWindow)
        {

            // Create the main window, but on the UI thread.
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Invoker)delegate
            {
                //string passFile = System.IO.Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DevelopWorkspace.exe.password");
                //string trialInfo = System.IO.Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DevelopWorkspace.exe.trial");

                //SoftwareLocker.TrialMaker t = new SoftwareLocker.TrialMaker("DevelopWorkspace", passFile,
                //    trialInfo,
                //    "Wechat:catsamurai\nMobile: CN +86-13664256548\ne-mail:xujingjiang@outlook.com",
                //    90, 210, "745");

                //byte[] MyOwnKey = { 97, 250, 1, 5, 84, 21, 7, 63,
                //4, 54, 87, 56, 123, 10, 3, 62,
                //7, 9, 20, 36, 37, 21, 101, 57};
                //t.TripleDESKey = MyOwnKey;

                //t.CheckRegister();

                MainWindow mainWindow = new MainWindow();
                Application.Current.MainWindow = mainWindow;
                mainWindow.Show();
            });
        }
    }
}