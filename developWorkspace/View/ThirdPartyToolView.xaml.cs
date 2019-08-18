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
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Diagnostics;
namespace DevelopWorkspace.Main.View
{

    /// <summary>
    /// ThirdPartyToolView.xaml 的交互逻辑
    /// </summary>
    public partial class ThirdPartyToolView : UserControl
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

        [DllImport("user32.dll", EntryPoint = "SendMessage")]
        private static extern int SendMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);

        //Sets window attributes
        [DllImport("USER32.DLL")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        //Gets window attributes
        [DllImport("USER32.DLL")]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool ShowWindow(IntPtr hWnd, short State);
        [DllImport("user32")]
        private static extern int SetForegroundWindow(IntPtr hwnd);

        //assorted constants needed
        public static int GWL_STYLE = -16;
        public static int WS_CHILD = 0x40000000; //child window
        public static int WS_BORDER = 0x00800000; //window with border
        public static int WS_DLGFRAME = 0x00400000; //window with double border but no title
        public static int WS_CAPTION = WS_BORDER | WS_DLGFRAME; //window with a title bar

        private const int HWND_TOP = 0x0;
        private const int WM_COMMAND = 0x0112;
        private const int WM_QT_PAINT = 0xC2DC;
        private const int WM_PAINT = 0x000F;
        private const int WM_SIZE = 0x0005;
        private const int SWP_FRAMECHANGED = 0x0020;
        public const uint WM_SYSCOMMAND = 0x0112;
        public const int SC_MONITORPOWER = 61808;
        public const int SC_MAXIMIZE = 0xF030;
        public const int SC_MINIMIZE = 0xF020;
        public const int WM_CLOSE = 0x10;
        public const int SC_NOMAL = 0xF120;
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
        Boolean IsAlreadyLoaded = false;
        Process process;
        //使用绑定的方式来达到MODEL在VIEW中可以看到，可能不是一个Smart的方式或者是违反了MVVM模式设计
        //但有时候在View中可以看到Model的数据会带来极大地便利
        //注意View在Loaded时间这个时点你可以拿到绑定的内容，在构造阶段你是拿不到的，这一点需要注意
        #region ThirdPartyID property
        public string ThirdPartyID
        {
            get
            {
                return (string)GetValue(ThirdPartyIDProperty);
            }
            set
            {
                SetValue(ThirdPartyIDProperty, value);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public static DependencyProperty ThirdPartyIDProperty = DependencyProperty.Register(
                "ThirdPartyID",
                typeof(string),
                typeof(ThirdPartyToolView),
                new PropertyMetadata(null, new PropertyChangedCallback(OnThirdPartyIDChanged)));

        public static readonly RoutedEvent DateTimeChangedEvent =
            EventManager.RegisterRoutedEvent("DateTimeChanged", RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<string>), typeof(ThirdPartyToolView));

        protected virtual void OnThirdPartyIDChanged(string oldValue, string newValue)
        {
            RoutedPropertyChangedEventArgs<string> args = new RoutedPropertyChangedEventArgs<string>(oldValue, newValue);
            args.RoutedEvent = ThirdPartyToolView.DateTimeChangedEvent;
            RaiseEvent(args);
        }

        private static void OnThirdPartyIDChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }
        #endregion


        public ThirdPartyToolView()
        {
            InitializeComponent();
        }

        private void OpenExpresso(Model.ThirdPartyToolModel model)
        {
            //Todo
            //如何把既存的应用程序嵌入到主窗体？目前这段代码只是一个思路，还做不出效果
            //WindowInteropHelper wndHelp = new WindowInteropHelper(this);
            process = new Process();
            //需要启动的程序
            process.StartInfo.FileName = model.ThirdPartyTool.ExeFilePath;
            //p.StartInfo.FileName = @"excel.exe";
            //为了美观,启动的时候最小化程序
            process.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
            //启动
            process.Start();

            //这里必须等待,否则启动程序的句柄还没有创建,不能控制程序
            //这个时间拿捏得好否则效果出来，还有现在TAB键切换时显示不正TODO
            Thread.Sleep(2500);
            //设置被绑架程序的父窗口
            SetParent(process.MainWindowHandle, this.video.Handle);
            int style = GetWindowLong(process.MainWindowHandle, GWL_STYLE);
            //take current window style and remove WS_CAPTION from it
            SetWindowLong(process.MainWindowHandle, GWL_STYLE, (style & ~WS_CAPTION));
            ShowWindow(process.MainWindowHandle, (short)ShowWindowStyles.SW_MAXIMIZE);
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
                (int)this.video.Width,
                (int)this.video.Height,
                SWP_FRAMECHANGED);
            SendMessage(p.MainWindowHandle, WM_COMMAND, WM_SIZE, 0);
        }
        public static void ToggleTitleBar(IntPtr hwnd, bool showTitle)
        {
            long style = GetWindowLong(hwnd, -16);
            if (showTitle)
                style |= 0xc00000L;
            else
                style &= -12582913L;
            //SetWindowLong(hwnd.ToInt64(), -16, style);
            SetWindowPos(hwnd, 0, 0, 0, 0, 0, 0x27);
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (!IsAlreadyLoaded)
            {
                IsAlreadyLoaded = true;
                //知识点
                //这个方法在构造体里调用的话会返回空指针，那个时点数据绑定还没有开始，需要在这个时间的时点来做才可以
                Model.ThirdPartyToolModel model = this.GetBindingExpression(ThirdPartyIDProperty).DataItem as Model.ThirdPartyToolModel;
                OpenExpresso(model);
            }

        }

        private void WindowsFormsHost_SizeChanged(object sender, SizeChangedEventArgs e)
        {
        }

        private void video_SizeChanged(object sender, EventArgs e)
        {
            if (process != null)
            {
                ResizeControl(process);

            }
        }
    }
}
