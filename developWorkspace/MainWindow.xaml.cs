using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Heidesoft.Components.Controls;
using System.Windows.Threading;
using System.Linq;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Diagnostics;
using System.Threading;
using Xceed.Wpf.AvalonDock.Layout;
using System.Windows.Media;
using DevelopWorkspace.Base;
using Fluent;

using Button = Fluent.Button;
using System.Linq.Expressions;
using System.IO;
using System.Windows.Media.Imaging;
using DevelopWorkspace.Base.Model;
using System.Collections.Generic;
using DevelopWorkspace.Main.Model;
using System.Windows.Controls.Primitives;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using RoslynPad.Editor;
using RoslynPad.Roslyn;
using System.Reflection;
using Workspace = DevelopWorkspace.Main.Model.Workspace;
using System.Threading.Tasks;

namespace DevelopWorkspace.Main
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : RibbonWindow
    {
        [DllImport("user32.dll")]
        private static extern int SetParent(IntPtr hWndChild, IntPtr hWndParent);

        [DllImport("user32.dll")]
        private static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool PostMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);

        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        private static extern bool SetWindowPos(IntPtr hWnd, int hWndInsertAfter,
                    int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern uint SetWindowLong(IntPtr hwnd, int nIndex, uint newLong);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern uint GetWindowLong(IntPtr hwnd, int nIndex);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool ShowWindow(IntPtr hWnd, short State);
        [DllImport("user32")]
        private static extern int SetForegroundWindow(IntPtr hwnd);

        private const int HWND_TOP = 0x0;
        private const int WM_COMMAND = 0x0112;
        private const int WM_QT_PAINT = 0xC2DC;
        private const int WM_PAINT = 0x000F;
        private const int WM_SIZE = 0x0005;
        private const int SWP_FRAMECHANGED = 0x0020;
        public enum ShowWindowStyles : short
        {
            SW_HIDE = 0,
            SW_SHOWNORMAL = 1,
            SW_NORMAL = 1,
            SW_SHOWMINIMIZED = 2,
            SW_SHOWMAXIMIZED = 3,
            SW_MAXIMIZE = 3,
            SW_SHOWNOACTIVATE = 4,
            SW_SHOW = 5,
            SW_MINIMIZE = 6,
            SW_SHOWMINNOACTIVE = 7,
            SW_SHOWNA = 8,
            SW_RESTORE = 9,
            SW_SHOWDEFAULT = 10,
            SW_FORCEMINIMIZE = 11,
            SW_MAX = 11
        }
        [DllImport("shell32.dll")]
        public static extern IntPtr ShellExecute(
            IntPtr hwnd,
            string lpOperation,
            string lpFile,
            string lpParameters,
            string lpDirectory,
            ShowWindowStyles nShowCmd);

        public event RibbonSelectionChangeEventHandler ribbonSelectionChangeEvent;
        public event WorksheetActiveChangeEventHandler WorksheetActiveChangeEvent;

        //
        public FileSystemWatcher fileSystemWatcher = new FileSystemWatcher();

        //监控excel的行为以达到和developworkspace进行配合的目的
        System.Timers.Timer excelWatchTimer = null;
        readonly object syncLock = new object();
        readonly object syncStopWatchLock = new object();

        Microsoft.Office.Interop.Excel.Application excelRefOnWatch;
        string currentTableNameOnWatch = "";
        bool isUninstalled = false;
        int procossIdOfExcel = 0;
        RoslynHost host;

        public void Application_SheetActivate(object sh)
        {
            Microsoft.Office.Interop.Excel.Worksheet targetSheet = null;
            try
            {
                targetSheet = sh as Microsoft.Office.Interop.Excel.Worksheet;
                if (targetSheet == null || targetSheet.UsedRange.Rows.Count < 6 && targetSheet.UsedRange.Rows.Count < 3)
                {
                    WorksheetActiveChangeEvent(targetSheet, new WorksheetActiveChangeEventArgs(null));
                    currentTableNameOnWatch = null;
                }
                else
                {
                    if (string.IsNullOrEmpty(targetSheet.Cells[5, 2].value))
                    {
                        WorksheetActiveChangeEvent(targetSheet, new WorksheetActiveChangeEventArgs(null));
                        currentTableNameOnWatch = null;
                    }
                    else
                    {
                        currentTableNameOnWatch = targetSheet.Cells[1, 2].value;
                        WorksheetActiveChangeEvent(targetSheet, new WorksheetActiveChangeEventArgs(currentTableNameOnWatch));
                    }
                }
            }
            finally
            {
                if (targetSheet != null)
                {
                    Marshal.ReleaseComObject(targetSheet);
                    targetSheet = null;
                }
            }
        }
        private void ExcelRefOnWatch_WorkbookActivate(Microsoft.Office.Interop.Excel.Workbook Wb)
        {
            Microsoft.Office.Interop.Excel.Worksheet targetSheet = null;
            try
            {
                targetSheet = Wb.ActiveSheet as Microsoft.Office.Interop.Excel.Worksheet;

                if (targetSheet == null || targetSheet.UsedRange.Rows.Count < 6 && targetSheet.UsedRange.Rows.Count < 3)
                {
                    currentTableNameOnWatch = null;
                    WorksheetActiveChangeEvent(targetSheet, new WorksheetActiveChangeEventArgs(currentTableNameOnWatch));
                }
                else
                {
                    if (string.IsNullOrEmpty(targetSheet.Cells[5, 2].value))
                    {
                        WorksheetActiveChangeEvent(targetSheet, new WorksheetActiveChangeEventArgs(null));
                        currentTableNameOnWatch = null;
                    }
                    else
                    {
                        currentTableNameOnWatch = targetSheet.Cells[1, 2].value;
                        WorksheetActiveChangeEvent(targetSheet, new WorksheetActiveChangeEventArgs(currentTableNameOnWatch));
                    }
                }
            }
            finally
            {
                if (targetSheet != null)
                {
                    Marshal.ReleaseComObject(targetSheet);
                    Marshal.ReleaseComObject(Wb);
                    targetSheet = null;
                    Wb = null;
                }
            }
        }
        //call from DataExcelUtilView        
        public void TriggerExcelWatchEvent(string tableName)
        {
            if (!AppConfig.SysConfig.This.WatchExcelActivity) return;

            currentTableNameOnWatch = tableName;
            WorksheetActiveChangeEvent(null, new WorksheetActiveChangeEventArgs(currentTableNameOnWatch));
        }
        public void TriggerExcelWatchEvent()
        {
            if (!AppConfig.SysConfig.This.WatchExcelActivity) return;

            WorksheetActiveChangeEvent(null, new WorksheetActiveChangeEventArgs(currentTableNameOnWatch));

        }
        public void ClearExcelWatchEvent(Microsoft.Office.Interop.Excel.Application xlApp)
        {
            if (xlApp != null)
            {
                xlApp.SheetActivate -= new Microsoft.Office.Interop.Excel.AppEvents_SheetActivateEventHandler(Application_SheetActivate);
                xlApp.WorkbookActivate -= ExcelRefOnWatch_WorkbookActivate;
                Marshal.ReleaseComObject(xlApp);
                xlApp = null;

                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }
        public void AppendExcelWatchEvent(Microsoft.Office.Interop.Excel.Application xlApp)
        {
            if (xlApp != null)
            {
                xlApp.SheetActivate += new Microsoft.Office.Interop.Excel.AppEvents_SheetActivateEventHandler(Application_SheetActivate);
                xlApp.WorkbookActivate += ExcelRefOnWatch_WorkbookActivate;
                Application_SheetActivate(xlApp.ActiveWorkbook.ActiveSheet);
            }
        }

        public void UnInstallExcelWatch()
        {
            //Bug 当前worksheet有内容需要自动追加一个新的worksheet时会引发例外：Microsoft Excel 正在等待某个应用程序以完成对象链接与嵌入操作
            if (!AppConfig.SysConfig.This.WatchExcelActivity) return;

            lock (syncLock)
            {
                ClearExcelWatchEvent(excelRefOnWatch);
                isUninstalled = true;
            }
        }
        public void InstallExcelWatch()
        {
            if (!AppConfig.SysConfig.This.WatchExcelActivity) return;
            if (isUninstalled)
            {
                lock (syncLock)
                {
                    excelRefOnWatch = Excel.GetLatestActiveExcelRefOnWatch(out procossIdOfExcel);
                    AppendExcelWatchEvent(excelRefOnWatch);
                    isUninstalled = false;
                }
            }
        }

        private void OnExelActivityTimedEvent(Object source, System.Timers.ElapsedEventArgs e)
        {
            lock (syncLock)
            {
                // when application terminated,uninstall...
                if (isUninstalled) return;
                // 当程序不在活跃状态下超过30秒的话则临时把监视excel的机能清除掉以防止影响excel的整体性能
                if (IsIdlingOverSomeSeconds)
                {
                    ClearExcelWatchEvent(excelRefOnWatch);
                    procossIdOfExcel = 0;
                    return;
                }

                int procossIdOfTopMost;
                Microsoft.Office.Interop.Excel.Application excelRefTopMost = Excel.GetLatestActiveExcelRefOnWatch(out procossIdOfTopMost);
                if (excelRefTopMost == null)
                {
                    ClearExcelWatchEvent(excelRefOnWatch);
                    return;
                }
                else if (procossIdOfTopMost != procossIdOfExcel)
                {
                    //Bug 当前worksheet有内容需要自动追加一个新的worksheet时会引发例外：Microsoft Excel 正在等待某个应用程序以完成对象链接与嵌入操作
                    ClearExcelWatchEvent(excelRefOnWatch);

                    DevelopWorkspace.Base.Logger.WriteLine($"OnExelActivityTimedEvent current processid:{procossIdOfTopMost} previous processid:{procossIdOfExcel}", Base.Level.TRACE);

                    excelRefOnWatch = excelRefTopMost;
                    procossIdOfExcel = procossIdOfTopMost;

                    AppendExcelWatchEvent(excelRefOnWatch);
                }
                else
                {
                    Marshal.ReleaseComObject(excelRefTopMost);
                }
            }
        }

        public MainWindow()
        {
            //ICSharpCode.AvalonEdit.Edi.HighlightingExtension.RegisterCustomHighlightingPatterns(StartupSetting.instance.homeDir, null);
            InitializeComponent();
            this.propertygrid.SelectedObject = JsonConfig<AppConfig.SysConfig>.load(StartupSetting.instance.homeDir);
            this.DataContext = Model.Workspace.This;
            //对COMMAND的具体动作进行绑定
            Model.Workspace.This.InitCommandBinding(this);
            if (System.Windows.Forms.Screen.AllScreens.Count() == 1)
            {
                this.Top = (AppConfig.SysConfig.This.Top > SystemParameters.PrimaryScreenHeight || AppConfig.SysConfig.This.Top < 0) ? 100 : AppConfig.SysConfig.This.Top;
                this.Left = (AppConfig.SysConfig.This.Left > SystemParameters.PrimaryScreenWidth || AppConfig.SysConfig.This.Left < 0) ? 100 : AppConfig.SysConfig.This.Left;
                this.Height = AppConfig.SysConfig.This.Height;
                this.Width = AppConfig.SysConfig.This.Width;

            }
            else
            {
                this.Top = AppConfig.SysConfig.This.Top;
                this.Left = AppConfig.SysConfig.This.Left;
                this.Height = AppConfig.SysConfig.This.Height;
                this.Width = AppConfig.SysConfig.This.Width;
            }


            ribbonSelectionChangeEvent += new RibbonSelectionChangeEventHandler(RibbonSelectionChangeEventFunc);
            WorksheetActiveChangeEvent += MainWindow_WorksheetActiveChangeEvent;
            ScriptBaseViewModel.AddinInstalledEvent += MainWindow_AddinInstalledEvent;

            //
            RegisterCustomHighlightingPatterns();


            //Excel切换监视
            if (AppConfig.SysConfig.This.WatchExcelActivity)
            {
                // Create a timer and set a two second interval.
                excelWatchTimer = new System.Timers.Timer();
                excelWatchTimer.Interval = 5 * 1000;

                // Hook up the Elapsed event for the timer.
                excelWatchTimer.Elapsed += OnExelActivityTimedEvent;

                // Have the timer fire repeated events (true is the default)
                excelWatchTimer.AutoReset = true;

                // Start the timer
                excelWatchTimer.Enabled = true;

                InstallExcelWatch();
            }

            this.Activated += MainWindow_Activated;
            this.Deactivated += MainWindow_Deactivated;

            Base.Services.BusyWorkService = new Base.Services.BusyWork(doBusyWork);
            Base.Services.BusyWorkIndicatorService = (string indicatorMessage) =>
            {
                busy.Dispatcher.BeginInvoke((Action)delegate ()
                {
                    busy.SetIndicator(indicatorMessage);
                });

            };

            string language = AppConfig.SysConfig.This.Language;
            if (string.IsNullOrWhiteSpace(language))
            {
                language = System.Threading.Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName;
            }

            var messages = (from message in DbSettingEngine.GetEngine(true).Messages
                            where message.Language.Equals(language)
                            select message).ToArray<Message>();
            foreach (Message message in messages)
            {
                Application.Current.Resources.Remove(message.MessageId);
                Application.Current.Resources.Add(message.MessageId, message.Content.Replace("\\n", "\n"));
            }

            // addins service
            List<AddinMetaAttribute> addinsAttribute = ScriptBaseViewModel.ScanAddinsJson();
            foreach (var attribute in addinsAttribute)
            {
                Button addin = new Button();
                addin.Header = attribute.Name;
                addin.ToolTip = attribute.Description;
                string iconfile = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "addins", string.IsNullOrEmpty(attribute.LargeIcon) ? "plugin" : attribute.LargeIcon + ".png");
                if (File.Exists(iconfile))
                {
                    var uri = new Uri(iconfile);
                    addin.LargeIcon = new BitmapImage(uri);
                }
                else
                {
                    try
                    {
                        var resourceString = "/DevelopWorkspace;component/Images/" + (string.IsNullOrEmpty(attribute.LargeIcon) ? "plugin" : attribute.LargeIcon) + ".png";
                        addin.LargeIcon = new BitmapImage(new Uri(resourceString, UriKind.Relative));
                    }
                    catch (Exception ex)
                    {
                    }
                }

                this.tools.Items.Add(addin);

                addin.Command = (this.DataContext as Workspace).LoadAddinCommand;
                //This sets a binding that binds the 'Name' property in PersonViewModel
                //Leave constructor parameter emtpy to bind to the object itself i.e. new Binding() { Source = Person }; will bind to person
                //var binding = new Binding(nameof(PersonViewModel.Name)) { Source = person };
                var binding = new Binding() { Source = attribute };
                //This sets the binding to the button and button CommandParameterProperty
                var bindingExpression = BindingOperations.SetBinding(addin, ButtonBase.CommandParameterProperty, binding);
            }

            //2019/03/15
            Base.Services.cancelLongTimeTask = this.cancelLongTimeTask;

            BackgroundWorker backgroundWorker = new BackgroundWorker(); ;
            backgroundWorker.DoWork += new DoWorkEventHandler((s, e) =>
            {
                //设定DB的初始化比较花时间，把这个移到主画面以至于后面的子画面的打开事件会比较快
                //将来可以把这个动作做得更用户友善比如load中等...
                var items = (from history in DbSettingEngine.GetEngine(true).ConnectionHistories
                             select history).ToArray<ConnectionHistory>();

            });

            backgroundWorker.RunWorkerAsync();

            ////todo trial version
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

            //SoftwareLocker.TrialMaker.RunTypes RT = t.ShowDialog();
            //bool is_trial;
            //if (RT == SoftwareLocker.TrialMaker.RunTypes.Expired)
            //{
            //    System.Windows.Application.Current.Shutdown();
            //}



            //file
            if (AppConfig.SysConfig.This.WatchFileSystemActivity)
            {
                fileSystemWatcher.Path = StartupSetting.instance.homeDir;
                fileSystemWatcher.IncludeSubdirectories = true;   //设置监控C盘文件夹下的全部子文件夹
                fileSystemWatcher.Filter = "*.*";   //设置监控文件的类型;
                fileSystemWatcher.NotifyFilter = NotifyFilters.LastWrite;
                DevelopWorkspace.Base.Logger.WriteLine($"monitor change of files in {StartupSetting.instance.homeDir}", Level.DEBUG);
            }
        }
        Stopwatch stopWatch = new Stopwatch();
        private void MainWindow_Deactivated(object sender, EventArgs e)
        {
            DevelopWorkspace.Base.Logger.WriteLine($"MainWindow_Deactivated", Base.Level.TRACE);
            lock (syncStopWatchLock)
            {
                stopWatch.Start();
            }
        }

        private void MainWindow_Activated(object sender, EventArgs e)
        {
            DevelopWorkspace.Base.Logger.WriteLine($"MainWindow_Activated", Base.Level.TRACE);
            lock (syncStopWatchLock)
            {
                stopWatch.Stop();
                stopWatch = new Stopwatch();
            }
        }
        private bool IsIdlingOverSomeSeconds
        {
            get
            {
                double eclapsedSeconds = 0;
                lock (syncStopWatchLock)
                {
                    eclapsedSeconds = stopWatch.Elapsed.TotalSeconds;
                }
                DevelopWorkspace.Base.Logger.WriteLine($"IsIdlingOverSomeSeconds:{eclapsedSeconds}", Base.Level.TRACE);
                return eclapsedSeconds > 30;
            }
        }
        private static bool CanLoadResource(Uri uri)
        {
            try
            {
                Application.GetResourceStream(uri);
                return true;
            }
            catch (IOException)
            {
                return false;
            }
        }
        private void MainWindow_AddinInstalledEvent(object sender, AddinInstalledEventArgs e)
        {
            AddinMetaAttribute attribute = e.MetaAttriute;
            Button selectedButton = null;
            foreach (Fluent.Button button in this.tools.Items)
            {
                if (button.Header.Equals(attribute.Name))
                {
                    selectedButton = button;
                    break;
                }
            }
            if (selectedButton == null)
            {
                selectedButton = new Button();
                this.tools.Items.Add(selectedButton);
            }
            selectedButton.Header = attribute.Name;
            selectedButton.ToolTip = attribute.Description;
            string iconfile = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "addins", attribute.LargeIcon + ".png");
            if (!string.IsNullOrEmpty(attribute.LargeIcon) && File.Exists(iconfile))
            {
                var uri = new Uri(iconfile);
                selectedButton.LargeIcon = new BitmapImage(uri);
            }
            else
            {
                try
                {
                    var resourceString = "/DevelopWorkspace;component/Images/" + (string.IsNullOrEmpty(attribute.LargeIcon) ? "plugin" : attribute.LargeIcon) + ".png";
                    if (CanLoadResource(new Uri(resourceString, UriKind.Relative)))
                    {
                        selectedButton.LargeIcon = new BitmapImage(new Uri(resourceString, UriKind.Relative));
                    }
                    else
                    {
                        selectedButton.LargeIcon = new BitmapImage(new Uri("/DevelopWorkspace;component/Images/plugin.png", UriKind.Relative));
                    }
                }
                catch (Exception ex)
                {
                }
            }
            selectedButton.Command = (this.DataContext as Workspace).LoadAddinCommand;
            var binding = new Binding() { Source = attribute };
            var bindingExpression = BindingOperations.SetBinding(selectedButton, ButtonBase.CommandParameterProperty, binding);
        }

        private void MainWindow_WorksheetActiveChangeEvent(object sender, WorksheetActiveChangeEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"MainWindow_WorksheetActiveChangeEvent: {e.TableName}");
        }

        private void RibbonSelectionChangeEventFunc(object sender, RibbonSelectionChangeEventArgs e)
        {
            var tc = sender as Fluent.Ribbon; //The sender is a type of TabControl...

            if (tc != null)
                System.Diagnostics.Debug.WriteLine($"RibbonSelectionChangeEventFunc: {e.SelectedIndex}");
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            //TODO LOGGER
            //Base.Logger.WriteLine(e.ToString());
            //System.Diagnostics.Debug.Print(e.ToString());
        }

        private void doBusyWork(Action action)
        {
            try
            {

                busy.FadeTime = TimeSpan.Zero;
                busy.IsBusyIndicatorShowing = true;
                // in order for setting the opacity to take effect, you have to delay the task slightly to ensure WPF has time to process the updated visual
                //2019/02/25 通过Dispatcher.BeginInvoke这个方式实际上对执行代码进行了一个滞后执行的设定，在这个WPF框架内使用了dockmanager，它的model先实例化之后通过数据绑定
                //的机制实例化view，这个流程不容易介入，这里实现的就是等view完全实例化后再执行用户代码的机制
                //本质上就是进行主线程的消息队列排队
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    try
                    {
                        action();
                    }
                    catch (Exception ex)
                    {
                        DevelopWorkspace.Base.Services.ErrorMessage(ex.Message);
                        DevelopWorkspace.Base.Logger.WriteLine(ex.Message, Base.Level.ERROR);
                    }
                    busy.IsBusyIndicatorShowing = false;
                    busy.ClearValue(BusyDecorator.FadeTimeProperty);
                }), DispatcherPriority.Background);
            }
            catch (Exception ex)
            {
                DevelopWorkspace.Base.Services.ErrorMessage(ex.Message);
                DevelopWorkspace.Base.Logger.WriteLine(ex.Message);
            }
        }

        private void OpenExpresso(object sender, RoutedEventArgs e)
        {
            //Todo
            //如何把既存的应用程序嵌入到主窗体？目前这段代码只是一个思路，还做不出效果
            WindowInteropHelper wndHelp = new WindowInteropHelper(this);
            Process p = new Process();
            //需要启动的程序
            p.StartInfo.FileName = @"tools\Expresso\Expresso.exe";
            //为了美观,启动的时候最小化程序
            p.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
            //启动
            p.Start();

            //这里必须等待,否则启动程序的句柄还没有创建,不能控制程序
            Thread.Sleep(1500);
            //最大化启动的程序
            //ShowWindow(p.MainWindowHandle, (short)ShowWindowStyles.SW_MAXIMIZE);
            //设置被绑架程序的父窗口
            SetParent(p.MainWindowHandle, wndHelp.Handle);
            SetForegroundWindow(p.MainWindowHandle);
            // 改变尺寸
            ResizeControl(p);

        }
        // 控制嵌入程序的位置和尺寸
        private void ResizeControl(Process p)
        {
            SendMessage(p.MainWindowHandle, WM_COMMAND, WM_PAINT, 0);
            PostMessage(p.MainWindowHandle, WM_QT_PAINT, 0, 0);
            SetWindowPos(
                p.MainWindowHandle,
                HWND_TOP,
                0,  // 设置偏移量,把原来窗口的菜单遮住
                0,
                (int)this.Width,
                (int)this.Height,
                SWP_FRAMECHANGED);
            SendMessage(p.MainWindowHandle, WM_COMMAND, WM_SIZE, 0);
        }

        private void WindowsFormsHost_Loaded(object sender, RoutedEventArgs e)
        {
            //Todo
            //如何把既存的应用程序嵌入到主窗体？目前这段代码只是一个思路，还做不出效果
            WindowInteropHelper wndHelp = new WindowInteropHelper(this);
            Process p = new Process();
            //需要启动的程序
            p.StartInfo.FileName = @"tools\Expresso\Expresso.exe";
            //为了美观,启动的时候最小化程序
            p.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
            //启动
            p.Start();

            //这里必须等待,否则启动程序的句柄还没有创建,不能控制程序
            Thread.Sleep(1500);
            //最大化启动的程序
            //ShowWindow(p.MainWindowHandle, (short)ShowWindowStyles.SW_MAXIMIZE);
            //设置被绑架程序的父窗口
            //SetParent(p.MainWindowHandle, this.video.Handle);
            SetForegroundWindow(p.MainWindowHandle);
            // 改变尺寸
            ResizeControl(p);

        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            //wcf这个方式针对一个典型的WEB应用的话显得我所是从
            //这里引入httpserver这个开源的webserver作为一个代替手段
            //RestfulService.RestfulService.Start(new string[]{ });
            //           DevelopWorkspace.WebServer.EmbeddedWebServer.Start(new string[] { });
        }
        private void about_Click(object sender, RoutedEventArgs e)
        {
            DevelopWorkspace.Main.AboutDialog aboutDialog = new DevelopWorkspace.Main.AboutDialog();
            aboutDialog.Owner = DevelopWorkspace.Base.Utils.WPF.GetTopWindow(this);
            Point position = ((Fluent.Button)sender).PointToScreen(new Point(0d, 0d));

            aboutDialog.Top = position.Y;
            aboutDialog.Left = position.X - aboutDialog.Width - ((Fluent.Button)sender).ActualWidth - 10;

            aboutDialog.ShowDialog();

        }
        private void help_Click(object sender, RoutedEventArgs e)
        {
            ShellExecute(IntPtr.Zero, "open", System.IO.Path.Combine(StartupSetting.instance.homeDir, "help.htm"), "", "", ShowWindowStyles.SW_SHOWNORMAL);
        }

        private void dockManager_ActiveContentChanged(object sender, EventArgs e)
        {
            if (dockManager.ActiveContent is DevelopWorkspace.Main.Model.ToolViewModel) return;
            if (dockManager.ActiveContent == Base.Services.ActiveModel) return;

            Fluent.Ribbon ribbon = Base.Utils.WPF.FindChild<Fluent.Ribbon>(Application.Current.MainWindow, "ribbon");
            int SelectedTabIndex = -1;
            //切换前的ActiveModel的ribbon需要隐藏
            if (Base.Services.ActiveModel != null)
            {
                //2019/03/07 remember last postion about tabitem
                if (ribbon.SelectedTabIndex != 0)
                    Base.Services.ActiveModel.RibbonTabIndex = ribbon.SelectedTabIndex;

                foreach (object tab in Base.Services.GetRibbon(Base.Services.ActiveModel))
                {
                    ribbon.Tabs.Remove(tab as Fluent.RibbonTabItem);
                }
            }
            if (dockManager.ActiveContent != null)
            {
                Base.Services.ActiveModel = dockManager.ActiveContent as Base.Model.PaneViewModel;
                SelectedTabIndex = Base.Services.ActiveModel.RibbonTabIndex;
                foreach (object tab in Base.Services.GetRibbon(Base.Services.ActiveModel))
                {
                    ribbon.Tabs.Add(tab as Fluent.RibbonTabItem);
                }
            }
            //切换前的ActiveModel的ribbon需要隐藏
            if (SelectedTabIndex == -1)
                ribbon.SelectedTabIndex = ribbon.Tabs.Count - 1;
            else if (ribbon.Tabs.Count >= SelectedTabIndex)
            {
                ribbon.SelectedTabIndex = SelectedTabIndex;
            }
        }

        public T FindChild<T>(DependencyObject parent, string childName)
           where T : DependencyObject
        {
            // Confirm parent and childName are valid. 
            if (parent == null) return null;

            T foundChild = null;

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                // If the child is not of the request child type child
                Debug.Print(string.Format("findchild ---{0}", child.ToString()));
                T childType = child as T;
                if (childType == null)
                {
                    // recursively drill down the tree
                    foundChild = FindChild<T>(child, childName);

                    // If the child is found, break so we do not overwrite the found child. 
                    if (foundChild != null) break;
                }
                else if (!string.IsNullOrEmpty(childName))
                {
                    var frameworkElement = child as FrameworkElement;
                    // If the child's name is set for search
                    if (frameworkElement != null && frameworkElement.Name == childName)
                    {
                        // if the child's name is of the request name
                        foundChild = (T)child;
                        break;
                    }
                }
                else
                {
                    // child element found.
                    foundChild = (T)child;
                    break;
                }
            }

            return foundChild;
        }

        private void dockManager_Loaded(object sender, RoutedEventArgs e)
        {
            var userview = FindChild<View.CSScriptRunView>(dockManager, "tabview_usercontrol");
            Debug.Print(string.Format("debug ---{0}", "dockManager_LayoutChanged"));

        }

        private void dockManager_Initialized(object sender, EventArgs e)
        {
            Debug.Print(string.Format("debug ---{0}", "dockManager_Initialized"));

        }

        private void dockManager_LayoutChanged(object sender, EventArgs e)
        {
            Debug.Print(string.Format("debug ---{0}", "dockManager_LayoutChanged"));

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DevelopWorkspace.Base.Logger.WriteLine("Cancel is clicked", Base.Level.DEBUG);
            System.Diagnostics.Debug.WriteLine("Cancel is clicked");
        }

        private void cancelLongTimeTask_Click(object sender, RoutedEventArgs e)
        {
            Base.Services.longTimeTaskState = Base.LongTimeTaskState.Cancel;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            try
            {
                AppConfig.SysConfig.This.Top = this.Top;
                AppConfig.SysConfig.This.Left = this.Left;
                AppConfig.SysConfig.This.Height = this.Height;
                AppConfig.SysConfig.This.Width = this.Width;
                AppConfig.SysConfig.This.Maximized = false;
                JsonConfig<AppConfig.SysConfig>.flush(AppConfig.SysConfig.This);
                if (AppConfig.DatabaseConfig.This != null) JsonConfig<AppConfig.DatabaseConfig>.flush(AppConfig.DatabaseConfig.This);

                //
                if (excelWatchTimer != null) excelWatchTimer.Enabled = false;
                UnInstallExcelWatch();

                this.busy.Shutdown();
                //Dispatcher.InvokeShutdown();
            }
            catch (Exception ex)
            {

            }
        }

        private void ribbon_SelectedTabChanged(object sender, SelectionChangedEventArgs e)
        {
            var ribbon = sender as Ribbon;
            if (ribbon != null)
                ribbonSelectionChangeEvent(dockManager.ActiveContent, new RibbonSelectionChangeEventArgs(ribbon.SelectedTabIndex));
        }
        private SolidColorBrush _solidColorBrush = new SolidColorBrush(Color.FromArgb((byte)255, (byte)0, (byte)255, (byte)0));

        private void dockManager_DocumentClosed(object sender, Xceed.Wpf.AvalonDock.DocumentClosedEventArgs e)
        {
            Workspace.This.Close(e.Document.Content as PaneViewModel);
        }

        private void dockManager_DocumentClosing(object sender, Xceed.Wpf.AvalonDock.DocumentClosingEventArgs e)
        {

        }
        public static async void RegisterCustomHighlightingPatterns()
        {
            await Task.Run(() => {
                ICSharpCodeX.AvalonEdit.Edi.HighlightingExtension.RegisterCustomHighlightingPatterns(StartupSetting.instance.homeDir);
            }).ConfigureAwait(false);
        }
    }

    public class DebugBindingConverter : IValueConverter
    {

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }

        #endregion
    }




}
