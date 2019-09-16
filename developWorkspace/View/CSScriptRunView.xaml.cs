using CSScriptLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Data;
using System.Windows.Threading;
using System.Threading;
using Heidesoft.Components.Controls;
using System.Reflection;
using System.Security;
using System.Security.Policy;
using System.Security.Permissions;
using System.ComponentModel;
using Xceed.Wpf.Toolkit.PropertyGrid;
using Newtonsoft.Json;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
using System.Diagnostics;
using DevelopWorkspace.Base;
using System.Text.RegularExpressions;
using DevelopWorkspace.Base.Utils;
using ICSharpCode.AvalonEdit.Highlighting;
using static DevelopWorkspace.Main.AppConfig;
using Fluent;
using DevelopWorkspace.Base.Model;

namespace DevelopWorkspace.Main.View
{

    public class ScriptExecutor : MarshalByRefObject
    {
        static Dictionary<int, Assembly> assemblyCache = new Dictionary<int, Assembly>();
        public void executeScript(string script, object view)
        {
            //通过设置debugBuild可以控制是否编译生成调试版本，测试时结合System.Diagnostics.Debugger.Break()来进行调试
            //https://github.com/oleg-shilo/cs-script/wiki/Choosing-Compiler-Engine
            //cs-script已经有缓存机制无需再...
            Assembly compiled;
            if (assemblyCache.ContainsKey(script.GetHashCode()))
            {
                compiled = assemblyCache[script.GetHashCode()] as Assembly;
            }
            else
            {
                //CSScript.GlobalSettings.SearchDirs = App.searchDirs.Aggregate((total, next) => total + ";" + next);
                StartupSetting startup = AppDomain.CurrentDomain.GetData("StartupSetting") as StartupSetting;
                CSScript.GlobalSettings.SearchDirs = startup.searchDirs.Aggregate((total, next) => total + ";" + next);
                foreach (var dir in startup.searchDirs)
                {
                    CSScript.GlobalSettings.AddSearchDir(dir);
                }
                CSScript.GlobalSettings.DefaultRefAssemblies = CSScript.GlobalSettings.DefaultRefAssemblies + "PresentationFramework;System.Xaml;PresentationFramework;";
                //TODO
                compiled = CSScript.LoadCode(script, null, true, new string[] { "WindowsBase", "PresentationCore", "PresentationFramework", "System.Xaml", "PresentationFramework" });
            }
            AsmHelper scriptAsm = new AsmHelper(compiled);
            scriptAsm.Invoke("Script.Main", view);
            if (!assemblyCache.ContainsKey(script.GetHashCode()))
            {
                assemblyCache.Add(script.GetHashCode(), compiled);
            }
        }

    }

    /// <summary>
    /// CSScriptRunView.xaml 的交互逻辑
    /// </summary>
    [Serializable()]
    public partial class CSScriptRunView : UserControl
    {
        static bool IsAppDomainInited = false;
        static AppDomain singleAppDomain;
        TreeView codeLibraryTreeView;
        PropertyGrid propertygrid1;
        TwoLineLabel basicInfoLabel;
        ScriptConfig scriptConfig;
        PaneViewModel model;
        public CSScriptRunView()
        {
            //BusyWorkServiceの外側で処理を入れる場合、this.DataContextがうまく取得できない場合があるので要注意
            Base.Services.BusyWorkService(new Action(() =>
            {
                InitializeComponent();
                //ribbon工具条注意resource定义在usercontrol内这样click等事件直接可以和view代码绑定
                Fluent.Ribbon ribbon = Base.Utils.WPF.FindChild<Fluent.Ribbon>(Application.Current.MainWindow, "ribbon");
                //之前的active内容关联的tab需要隐藏
                if (Base.Services.ActiveModel != null)
                {
                    foreach (object tab in Base.Services.GetRibbon(Base.Services.ActiveModel))
                    {
                        ribbon.Tabs.Remove(tab as Fluent.RibbonTabItem);
                    }
                }
                //从control.resources里面取出ribbontabitem的xaml定义同时实例化
                //    DevelopWorkspace.Base.Logger.WriteLine("Process committed");
                var ribbonTabTool = FindResource("RibbonTabTool") as Fluent.RibbonTabItem;
                ribbon.Tabs.Add(ribbonTabTool);
                ribbon.SelectedTabIndex = ribbon.Tabs.Count - 1;
                Base.Services.ActiveModel.RibbonTabIndex = ribbon.SelectedTabIndex;
                Base.Services.RegRibbon(this.DataContext as Base.Model.PaneViewModel, new List<object> { ribbonTabTool });
                model = this.DataContext as Base.Model.PaneViewModel;
                Base.Services.ActiveModel = this.DataContext as Base.Model.PaneViewModel;
                codeLibraryTreeView = Base.Utils.WPF.FindLogicaChild<TreeView>(ribbonTabTool, "codeLibraryTreeView");
                StartupSetting startup = AppDomain.CurrentDomain.GetData("StartupSetting") as StartupSetting;
                DirectoryInfo codeLibrary = new DirectoryInfo(System.IO.Path.Combine(startup.homeDir, "CodeLibrary"));
                if (codeLibrary.Exists)
                {
                    loadCodeLibraryTree(codeLibrary, this.codeLibraryTreeView as ItemsControl, true);
                }
                else {
                    DevelopWorkspace.Base.Services.ErrorMessage($"please register your script into script library:{codeLibrary.FullName}");
                }

                propertygrid1 = Base.Utils.WPF.FindLogicaChild<PropertyGrid>(ribbonTabTool, "propertygrid1");
                //basicInfoLabel = Base.Utils.WPF.FindLogicaChild<TwoLineLabel>(ribbonTabTool, "basicInfoLabel");

                scriptConfig = JsonConfig<ScriptConfig>.load();
                propertygrid1.SelectedObject = scriptConfig;


                //this.ScriptContent.TextArea.Options = new ICSharpCode.AvalonEdit.TextEditorOptions();


                //TODO
                (this.DataContext as Base.Model.ViewModelBase).clearance= new Func<string, bool>(DoClearance);


                SolidColorBrush brush = new SolidColorBrush(Color.FromArgb((byte)255, (byte)204, (byte)236, (byte)255));
                (this.DataContext as PaneViewModel).ThemeColorBrush = brush;




                ScriptContent.Text = @"using System;
using System.Drawing;
using System.IO;
using System.Data.SQLite;
using System.Data;
using Microsoft.CSharp;
using System.Collections.Generic;
using DevelopWorkspace.Base;
using Excel = Microsoft.Office.Interop.Excel;
using System.Threading;
using System.Threading.Tasks;
public class Script
{
    public static void Main(string[] args)
    {
    	string retString =System.DateTime.Now.ToString();
        DevelopWorkspace.Base.Logger.WriteLine(args[0]);
        DevelopWorkspace.Base.Logger.WriteLine(args[1]);
    }
}";

                if (!IsAppDomainInited)
                {
                    IsAppDomainInited = true;
                    //利用appdoman的特性来隔离脚本的执行环境，防止主appdomain的程序及随着执行脚本是不断在如程序集但是得不到及时地卸载等问题
                    //隔离后主appdomain内的对象在新的appdomain都不能直接使用 
                    //脚本内打断点可以使用主动调用System.Diagnostics.Debugger.Break();
                    AppDomainSetup setup = new AppDomainSetup();
                    setup.ApplicationName = "ApplicationLoader";
                    setup.ApplicationBase = AppDomain.CurrentDomain.BaseDirectory;
                    setup.PrivateBinPath = startup.searchDirs.Aggregate((total, next) => total + ";" + next);
                    setup.CachePath = setup.ApplicationBase;
                    singleAppDomain = AppDomain.CreateDomain("ScriptExecuteAppDomain", null, setup);
                    //为了全局的logger对象可以在所有的appdomain内都可以参照到使用setdata/getdata,注意这个对象需要继承MarshalByRefObject
                    singleAppDomain.SetData("logger", AppDomain.CurrentDomain.GetData("logger"));
                    singleAppDomain.SetData("StartupSetting", AppDomain.CurrentDomain.GetData("StartupSetting"));
                    singleAppDomain.AssemblyResolve += new ResolveEventHandler(App.CurrentDomain_AssemblyResolve);
                }
            }));
        }

        public bool DoClearance(string bookName)
        {
            if (System.Windows.MessageBox.Show("close this tab?", "Confirm Message", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel) return false;


            return true;
        }

        /// <summary>
        /// 提供简单访问各个文本内容的方法
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <returns></returns>
        public string this[int row, int col]
        {
            get
            {
                if (row == 1 && col == 1) return this.Cell_1_1.Text;
                else if (row == 1 && col == 2) return this.Cell_1_2.Text;
                else return this.ScriptContent.Text;
            }
            set
            {
                if (row == 1 && col == 1) this.Cell_1_1.Text = value;
                else if (row == 1 && col == 2) this.Cell_1_2.Text = value;
                else this.ScriptContent.Text = value;
            }
        }
        public string ScriptContentText
        {
            get
            {
                return this.ScriptContent.Text;
            }

            set
            {
                this.ScriptContent.Text = value;
            }
        }


        #region C#执行引擎
        Action actionCSharpScript { get
            {
                return new Action(() =>
                {

                    if (scriptConfig.AppDomain == EngineDomain.single)
                    {
                        //2017.5.27 appdomain对应
                        Object obj = singleAppDomain.CreateInstanceAndUnwrap(typeof(ScriptExecutor).Assembly.FullName, typeof(ScriptExecutor).FullName);
                        Type type = obj.GetType();
                        MethodInfo method = type.GetMethod("executeScript");
                        string[] inputs = new string[] { this[1, 1], this[1, 2] };
                        try
                        {
                            method.Invoke(obj, new object[] { ScriptContent.Text, inputs });
                        }
                        catch (Exception ex)
                        {
                            DevelopWorkspace.Base.Logger.WriteLine(ex.Message,Level.ERROR);
                            //2019/3/16 InnerException perhaps is null
                            if(ex.InnerException != null) DevelopWorkspace.Base.Logger.WriteLine(ex.InnerException.Message, Level.ERROR);
                            if (ex.StackTrace != null) DevelopWorkspace.Base.Logger.WriteLine(ex.StackTrace, Level.ERROR);
                        }
                        finally
                        {
                            //卸载appdomain，什么时候更合适需要斟酌
                            //AppDomain.Unload(MySampleDomain);
                        }
                    }
                    else if (scriptConfig.AppDomain == EngineDomain.required)
                    {
                        //利用appdoman的特性来隔离脚本的执行环境，防止主appdomain的程序及随着执行脚本是不断在如程序集但是得不到及时地卸载等问题
                        //隔离后主appdomain内的对象在新的appdomain都不能直接使用 
                        //脚本内打断点可以使用主动调用System.Diagnostics.Debugger.Break();
                        AppDomainSetup setup = new AppDomainSetup();
                        setup.ApplicationName = "ApplicationLoader";
                        setup.ApplicationBase = AppDomain.CurrentDomain.BaseDirectory;
                        StartupSetting startup = AppDomain.CurrentDomain.GetData("StartupSetting") as StartupSetting;

                        setup.PrivateBinPath = startup.searchDirs.Aggregate((total, next) => total + ";" + next);
                        setup.CachePath = setup.ApplicationBase;
                        //setup.ShadowCopyFiles = "true";
                        //setup.ShadowCopyDirectories = setup.ApplicationBase;
                        //AppDomain.CurrentDomain.SetShadowCopyFiles();
                        AppDomain requestedDomain = AppDomain.CreateDomain("requestedDomain", new System.Security.Policy.Evidence(), setup);

                        //为了全局的logger对象可以在所有的appdomain内都可以参照到使用setdata/getdata,注意这个对象需要继承MarshalByRefObject
                        requestedDomain.SetData("logger", AppDomain.CurrentDomain.GetData("logger"));
                        requestedDomain.SetData("StartupSetting", AppDomain.CurrentDomain.GetData("StartupSetting"));
                        requestedDomain.AssemblyResolve += new ResolveEventHandler(App.CurrentDomain_AssemblyResolve);
                        //2017.5.27 appdomain对应
                        Object obj = requestedDomain.CreateInstanceAndUnwrap(typeof(ScriptExecutor).Assembly.FullName, typeof(ScriptExecutor).FullName);

                        Type type = obj.GetType();
                        MethodInfo method = type.GetMethod("executeScript");
                        string[] inputs = new string[] { this[1, 1], this[1, 2] };
                        try
                        {
                            method.Invoke(obj, new object[] { ScriptContent.Text, inputs });
                        }
                        catch (Exception ex)
                        {
                            DevelopWorkspace.Base.Logger.WriteLine(ex.Message, Level.ERROR);
                            //2019/3/16 InnerException perhaps is null
                            if (ex.InnerException != null) DevelopWorkspace.Base.Logger.WriteLine(ex.InnerException.Message, Level.ERROR);
                            if (ex.StackTrace != null) DevelopWorkspace.Base.Logger.WriteLine(ex.StackTrace, Level.ERROR);
                        }
                        finally
                        {
                            //卸载appdomain，什么时候更合适需要斟酌
                            AppDomain.Unload(requestedDomain);
                        }
                    }
                    else//(scriptConfig.AppDomain == EngineDomain.shared)
                    {

                        ScriptExecutor executor = new ScriptExecutor();
                        
                        string[] inputs = new string[] { this[1, 1], this[1, 2] };
                        try
                        {
                            executor.executeScript(ScriptContent.Text, inputs);
                        }
                        catch (Exception ex)
                        {
                            DevelopWorkspace.Base.Logger.WriteLine(ex.Message, Level.ERROR);
                            //2019/3/16 InnerException perhaps is null
                            if (ex.InnerException != null) DevelopWorkspace.Base.Logger.WriteLine(ex.InnerException.Message, Level.ERROR);
                            if (ex.StackTrace != null) DevelopWorkspace.Base.Logger.WriteLine(ex.StackTrace, Level.ERROR);
                        }
                    }
                    busy.IsBusyIndicatorShowing = false;
                    busy.ClearValue(BusyDecorator.FadeTimeProperty);

                });
            }
        }
        #endregion
        Action actionJavaScript
        {
            get
            {
                return new Action(() =>
                {
                    try
                    {
                        List<string> paths = new List<string>();
                        //主程序集目录作为默认搜索位置
                        DevelopWorkspace.Base.Utils.Script.getPatternFilesByTraverseTree("*.jar", StartupSetting.instance.homeDir, paths,new List<Regex>() {new Regex("antlr4-csharp.*.jar") });
                        //TODO com...以c打头时c没有作为候补，得出的结果时om.换成d,1等没有问题
                        Match packageMatch = new Regex(@"^[]?package[ ]+?(?<package>[\w\.]+)[ ]*?;", RegexOptions.Multiline).Match(this.ScriptContent.Text);
                        string packagename=packageMatch.Groups["package"].Value;
                    
                        Match classMatch = new Regex(@"^[]?public[ ]+?class[ ]+?(?<classname>[\w]+)[ ]?", RegexOptions.Multiline).Match(this.ScriptContent.Text);
                        string classname=classMatch.Groups["classname"].Value;

                        string workdir = System.IO.Path.Combine(StartupSetting.instance.homeDir, "compiled");
                        //如果目录不存在则作成
                        if (!Directory.Exists(workdir))
                        {
                            Directory.CreateDirectory(workdir);
                        }
                        var setting = new
                        {
                            javac = "javac.exe",
                            java = "java.exe",
                            target = AppDomain.CurrentDomain.BaseDirectory + @"antlr4\" + "antlr4-csharp-4.6.1-complete.jar",
                            runtime = AppDomain.CurrentDomain.BaseDirectory + @"antlr4\" + "Antlr4.Runtime.dll",
                            csc = @"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc",
                            name_space = "ConsoleApplication1",
                            dest = AppDomain.CurrentDomain.BaseDirectory + @"compiled\",
                            start_rule = "file",
                            grammar = "CSV",
                            cp = workdir + ";" + (paths.Count==0 ?"": paths.Aggregate((total, next) => total + ";" + next)),
                            compiled = $"{packagename}.{classname}",
                            g4 = System.IO.Path.Combine(workdir, classname + ".java"),
                            compiledClass = System.IO.Path.Combine(workdir, classname + ".class")
                        };
                        //javac - cp.; D:\ochadoop4.0.1\hive - 0.13.1 - cdh5.2.1 - och4.0.1\user_lib\hive--jdbc - 0.13.1 - cdh5.2.1.jar HiveJdbcClient.java
                        //java - cp.; D:\ochadoop4.0.1\hive - 0.13.1 - cdh5.2.1 - och4.0.1\user_lib\hive - jdbc - 0.13.1 - cdh5.2.1.jar HiveJdbcClient
                        //package com;
                        //public class HelloWorld {
                        //string RcvPath = "D:\\DT930\\rec\\";

                        System.IO.File.WriteAllText(@"{g4}".FormatWith(setting), this.ScriptContent.Text);
                        //string input = this[1, 1];

                        string compileCommand = @"{javac} -cp {cp} {g4}".FormatWith(setting);
                        DevelopWorkspace.Base.Logger.WriteLine(compileCommand,Level.DEBUG);
                        //类没有成功编译则退出
                        File.Delete(setting.compiledClass);

                        Script.executeExternCommand(compileCommand);
                        //类没有成功编译则退出
                        if (File.Exists(setting.compiledClass))
                        {

                            if (!string.IsNullOrEmpty(packagename))
                            {
                                string realclasspath = System.IO.Path.Combine(workdir, packagename.Replace('.', '\\'));
                                if (!Directory.Exists(realclasspath))
                                {
                                    Directory.CreateDirectory(realclasspath);
                                }
                                System.IO.File.Copy(setting.compiledClass, System.IO.Path.Combine(realclasspath, classname + ".class"), true);
                            }

                            string javaCommand = @"{java} -cp {cp} {compiled}".FormatWith(setting);
                            DevelopWorkspace.Base.Logger.WriteLine(javaCommand, Level.DEBUG);
                            Script.executeExternCommand(javaCommand);
                        }
                     }
                    catch (Exception ex)
                    {
                        DevelopWorkspace.Base.Logger.WriteLine(ex.Message, Level.ERROR);
                    }
                    busy.IsBusyIndicatorShowing = false;
                    busy.ClearValue(BusyDecorator.FadeTimeProperty);
                });
            }
        }

        Action actionNodeJsScript
        {
            get
            {
                return new Action(() =>
                {
                    try
                    {

                        var setting = new
                        {
                            nodejs = "node.exe",
                            jsfile = @".\compiled\task.js",
                            javac = "javac.exe",
                            java = "java.exe",
                            target = AppDomain.CurrentDomain.BaseDirectory + @"antlr4\" + "antlr4-csharp-4.6.1-complete.jar",
                            runtime = AppDomain.CurrentDomain.BaseDirectory + @"antlr4\" + "Antlr4.Runtime.dll",
                            csc = @"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc",
                            name_space = "ConsoleApplication1",
                            dest = AppDomain.CurrentDomain.BaseDirectory + @"compiled\",
                            start_rule = "file",
                            grammar = "CSV",
                            cp = @"C:\workspace\developWorkspace\developWorkspace\bin\Debug\compiled",
                            compiled = @"HelloWorld",
                            g4 = @".\compiled\HelloWorld.java"

                        };

                        //javac - cp.; D:\ochadoop4.0.1\hive - 0.13.1 - cdh5.2.1 - och4.0.1\user_lib\hive--jdbc - 0.13.1 - cdh5.2.1.jar HiveJdbcClient.java
                        //java - cp.; D:\ochadoop4.0.1\hive - 0.13.1 - cdh5.2.1 - och4.0.1\user_lib\hive - jdbc - 0.13.1 - cdh5.2.1.jar HiveJdbcClient
                        System.IO.File.WriteAllText(@"{jsfile}".FormatWith(setting), this.ScriptContent.Text);
                        //string input = this[1, 1];

                        string compileCommand = @"{nodejs} {jsfile}".FormatWith(setting);
                        DevelopWorkspace.Base.Logger.WriteLine(compileCommand);
                        Script.executeExternCommand(compileCommand);
                        }
                    catch (Exception ex)
                    {
                        DevelopWorkspace.Base.Logger.WriteLine(ex.Message, Level.ERROR);
                    }
                    busy.IsBusyIndicatorShowing = false;
                    busy.ClearValue(BusyDecorator.FadeTimeProperty);
               });
            }
        }
        //脚本执行
        //C#,java支持(TODO)
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                busy.FadeTime = TimeSpan.Zero;
                busy.IsBusyIndicatorShowing = true;
                // in order for setting the opacity to take effect, you have to delay the task slightly to ensure WPF has time to process the updated visual
                #region
                //Dispatcher.BeginInvoke(new Action(() =>
                //{
                //    if (appdomainMode == "SINGLE")
                //    {
                //        //2017.5.27 appdomain对应
                //        Object obj = singleAppDomain.CreateInstanceAndUnwrap(typeof(ScriptExecutor).Assembly.FullName, typeof(ScriptExecutor).FullName);
                //        Type type = obj.GetType();
                //        MethodInfo method = type.GetMethod("executeScript");
                //        string[,] inputs = new string[2, 3];
                //        inputs[1, 1] = this[1, 1];
                //        inputs[1, 2] = this[1, 2];
                //        try
                //        {
                //            method.Invoke(obj, new object[] { ScriptContent.Text, inputs });
                //        }
                //        catch (Exception ex)
                //        {
                //            DevelopWorkspace.Base.Logger.WriteLine(ex.Message);
                //            DevelopWorkspace.Base.Logger.WriteLine(ex.InnerException.Message);
                //        }
                //        finally
                //        {
                //            //卸载appdomain，什么时候更合适需要斟酌
                //            //AppDomain.Unload(MySampleDomain);
                //        }
                //    }
                //    else if (appdomainMode == "REQUESTED")
                //    {
                //        //利用appdoman的特性来隔离脚本的执行环境，防止主appdomain的程序及随着执行脚本是不断在如程序集但是得不到及时地卸载等问题
                //        //隔离后主appdomain内的对象在新的appdomain都不能直接使用 
                //        //脚本内打断点可以使用主动调用System.Diagnostics.Debugger.Break();
                //        AppDomainSetup setup = new AppDomainSetup();
                //        setup.ApplicationName = "ApplicationLoader";
                //        setup.ApplicationBase = AppDomain.CurrentDomain.BaseDirectory;
                //        StartupSetting startup = AppDomain.CurrentDomain.GetData("StartupSetting") as StartupSetting;

                //        setup.PrivateBinPath = startup.searchDirs.Aggregate((total, next) => total + ";" + next);
                //        setup.CachePath = setup.ApplicationBase;
                //        //setup.ShadowCopyFiles = "true";
                //        //setup.ShadowCopyDirectories = setup.ApplicationBase;
                //        //AppDomain.CurrentDomain.SetShadowCopyFiles();
                //        AppDomain requestedDomain = AppDomain.CreateDomain("requestedDomain", new System.Security.Policy.Evidence(), setup);

                //        //为了全局的logger对象可以在所有的appdomain内都可以参照到使用setdata/getdata,注意这个对象需要继承MarshalByRefObject
                //        requestedDomain.SetData("logger", AppDomain.CurrentDomain.GetData("logger"));
                //        requestedDomain.SetData("StartupSetting", AppDomain.CurrentDomain.GetData("StartupSetting"));
                //        requestedDomain.AssemblyResolve += new ResolveEventHandler(App.CurrentDomain_AssemblyResolve);
                //        //2017.5.27 appdomain对应
                //        Object obj = requestedDomain.CreateInstanceAndUnwrap(typeof(ScriptExecutor).Assembly.FullName, typeof(ScriptExecutor).FullName);

                //        Type type = obj.GetType();
                //        MethodInfo method = type.GetMethod("executeScript");
                //        string[,] inputs = new string[2, 3];
                //        inputs[1, 1] = this[1, 1];
                //        inputs[1, 2] = this[1, 2];
                //        try
                //        {
                //            method.Invoke(obj, new object[] { ScriptContent.Text, inputs });
                //        }
                //        catch (Exception ex)
                //        {
                //            DevelopWorkspace.Base.Logger.WriteLine(ex.Message);
                //            DevelopWorkspace.Base.Logger.WriteLine(ex.InnerException.Message);
                //        }
                //        finally
                //        {
                //            //卸载appdomain，什么时候更合适需要斟酌
                //            AppDomain.Unload(requestedDomain);
                //        }
                //    }
                //    else//(appdomainMode == "SHARED")
                //    {
                //        try
                //        {
                //            StartupSetting startup = AppDomain.CurrentDomain.GetData("StartupSetting") as StartupSetting;

                //            CSScript.GlobalSettings.SearchDirs = startup.searchDirs.Aggregate((total, next) => total + ";" + next);

                //            foreach (var dir in startup.searchDirs)
                //            {
                //                CSScript.GlobalSettings.AddSearchDir(dir);
                //            }

                //            AsmHelper scriptAsm = new AsmHelper(CSScript.LoadCode(ScriptContent.Text, null, false));
                //            scriptAsm.Invoke("Script.Main", this);
                //        }
                //        catch (Exception ex)
                //        {
                //            DevelopWorkspace.Base.Logger.WriteLine(ex.Message);
                //        }
                //    }

                //    busy.IsBusyIndicatorShowing = false;
                //    busy.ClearValue(BusyDecorator.FadeTimeProperty);
                //}), DispatcherPriority.Background);
                #endregion
                ScriptConfig latestConfig = propertygrid1.SelectedObject as ScriptConfig;
                if(latestConfig.ScriptLanguage == Main.Language.csharp){
                    Dispatcher.BeginInvoke(actionCSharpScript, DispatcherPriority.Background);
                }
                else if (latestConfig.ScriptLanguage == Main.Language.java)
                {
                    Dispatcher.BeginInvoke(actionJavaScript, DispatcherPriority.Background);
                }
                else if (latestConfig.ScriptLanguage == Main.Language.javascript)
                {
                    Dispatcher.BeginInvoke(actionNodeJsScript, DispatcherPriority.Background);
                }

            }
            catch (Exception ex)
            {
                DevelopWorkspace.Base.Logger.WriteLine(ex.Message);
                DevelopWorkspace.Base.Logger.WriteLine(ex.InnerException.Message, Level.ERROR);
            }
        }

        private void run_MouseEnter(object sender, MouseEventArgs e)
        {
            //popLink.IsOpen = true;
        }

        private void lnk_Click(object sender, RoutedEventArgs e)
        {

        }
        private void loadCodeLibraryTree(System.IO.DirectoryInfo root, ItemsControl treeView, bool bRoot)
        {
            ItemsControl treeViewItem = null;
            if (bRoot == true)
            {
                treeViewItem = treeView;
            }
            else
            {
                treeViewItem = new TreeViewItem();
                (treeViewItem as TreeViewItem).Header = root.Name;
                treeView.Items.Add(treeViewItem);
            }

            System.IO.FileInfo[] files = null;
            System.IO.DirectoryInfo[] subDirs = null;

            // First, process all the files directly under this folder
            try
            {
                files = root.GetFiles("setting.xml");
            }
            // This is thrown if even one of the files requires permissions greater
            // than the application provides.
            //catch (UnauthorizedAccessException e)
            //{
            // This code just writes out the message and continues to recurse.
            // You may decide to do something different here. For example, you
            // can try to elevate your privileges and access the file again.
            //log.Add(e.Message);
            //}

            catch (System.IO.DirectoryNotFoundException e)
            {
                //Console.WriteLine(e.Message);
            }

            foreach (System.IO.FileInfo fi in files)
            {
                //2019/02/26暂时ScriptConfig和setting.xml共存，日后需要统一成ScriptConfig,废弃掉xml
                ScriptConfig config = JsonConfig<ScriptConfig>.load(System.IO.Path.GetDirectoryName(fi.FullName));

                System.IO.StringReader xmlReader = new System.IO.StringReader(System.IO.File.ReadAllText(fi.FullName));
                DataSet dataSet = new DataSet();
                dataSet.ReadXml(xmlReader);

                //TreeViewItem childTreeViewItem = new TreeViewItem();
                //childTreeViewItem.Header = dataSet.Tables["run"].Rows[0]["title"];
                //childTreeViewItem.Tag = fi;
                //treeViewItem.Items.Add(childTreeViewItem);
                (treeViewItem as TreeViewItem).Header = config.Title;
                treeViewItem.Tag = fi;

                // In this example, we only access the existing FileInfo object. If we
                // want to open, delete or modify the file, then
                // a try-catch block is required here to handle the case
                // where the file has been deleted since the call to TraverseTree().
                //Console.WriteLine(fi.FullName);
            }

            // Now find all the subdirectories under this directory.
            subDirs = root.GetDirectories();

            foreach (System.IO.DirectoryInfo dirInfo in subDirs)
            {
                // Resursive call for each subdirectory.
                loadCodeLibraryTree(dirInfo, treeViewItem, false);
            }
        }

        private void TreeView_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var tree = sender as TreeView;
            if (tree.SelectedItem is TreeViewItem)
            {
                var item = tree.SelectedItem as TreeViewItem;
                if (item.Tag == null) return;

                //TODO
                //在共有配置的基础上进一步覆盖自己的属性
                ScriptConfig latestConfig = propertygrid1.SelectedObject as ScriptConfig;
                JsonConfig<ScriptConfig>.flush(latestConfig);

                ScriptConfig config = JsonConfig<ScriptConfig>.load((item.Tag as FileInfo).Directory.ToString());
                config.Path = (item.Tag as FileInfo).Directory.ToString();
                //Load Script
                System.IO.StringReader xmlReader = new System.IO.StringReader(System.IO.File.ReadAllText((item.Tag as FileInfo).FullName));
                DataSet dataSet = new DataSet();
                dataSet.ReadXml(xmlReader);
                //if (dataSet.Tables["run"].Columns.Contains("appdomain"))
                //{
                //    appdomainMode = dataSet.Tables["run"].Rows[0]["appdomain"].ToString();
                //    if(Enum.IsDefined(typeof(Main.EngineDomain), appdomainMode)){
                //        config.AppDomain = (Main.EngineDomain)Enum.Parse(typeof(Main.EngineDomain), appdomainMode);
                //    }
                //    else{
                //        config.AppDomain = Main.EngineDomain.shared;
                //    }
                //}
                foreach (System.Data.DataRow codeBlk in dataSet.Tables["codeBlock"].Rows)
                {
                    this[Convert.ToInt16(codeBlk["row"]), Convert.ToInt16(codeBlk["col"])] = System.IO.File.ReadAllText((item.Tag as FileInfo).Directory + @"\" + codeBlk["file"].ToString());
                }
                if (config.ScriptLanguage == Main.Language.csharp)
                {
                    var typeConverter = new HighlightingDefinitionTypeConverter();
                    var csSyntaxHighlighter = (IHighlightingDefinition)typeConverter.ConvertFrom("C#");
                    //JavaScript,SQL,Ruby,XML,ASP/XHTML
                    this.ScriptContent.SyntaxHighlighting = csSyntaxHighlighter;
                }
                else if (config.ScriptLanguage == Main.Language.java)
                {
                    var typeConverter = new HighlightingDefinitionTypeConverter();
                    var csSyntaxHighlighter = (IHighlightingDefinition)typeConverter.ConvertFrom("Java");
                    this.ScriptContent.SyntaxHighlighting = csSyntaxHighlighter;
                }

                //config.Title = dataSet.Tables["run"].Rows[0]["title"].ToString();
                //config.Description = dataSet.Tables["run"].Rows[0]["description"].ToString();
                //config.FirstInputType = dataSet.Tables["codeBlock"].Rows[0]["type"].ToString();
                //if (dataSet.Tables["codeBlock"].Rows.Count > 1) config.SecondInputType = dataSet.Tables["codeBlock"].Rows[1]["type"].ToString();
                //if (dataSet.Tables["codeBlock"].Rows.Count > 2) config.ScriptLanguage = dataSet.Tables["codeBlock"].Rows[2]["type"].ToString();


                propertygrid1.SelectedObject = config;
                scriptConfig = config;
                //basicInfoLabel.Text = config.Title;
                model.Title = $"Script[{config.Title}]";
            }
        }



    }
}
