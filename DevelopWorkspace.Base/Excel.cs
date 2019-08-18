using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DevelopWorkspace.Base
{
    public class MessageFilter : IOleMessageFilter
    {
        //
        // Class containing the IOleMessageFilter
        // thread error-handling functions.

        // Start the filter.
        public static void Register()
        {
            IOleMessageFilter newFilter = new MessageFilter();
            IOleMessageFilter oldFilter = null;
            CoRegisterMessageFilter(newFilter, out oldFilter);
        }

        // Done with the filter, close it.
        public static void Revoke()
        {
            IOleMessageFilter oldFilter = null;
            CoRegisterMessageFilter(null, out oldFilter);
        }

        //
        // IOleMessageFilter functions.
        // Handle incoming thread requests.
        int IOleMessageFilter.HandleInComingCall(int dwCallType,
          System.IntPtr hTaskCaller, int dwTickCount, System.IntPtr
          lpInterfaceInfo)
        {
            //Return the flag SERVERCALL_ISHANDLED.
            return 0;
        }

        // Thread call was rejected, so try again.
        int IOleMessageFilter.RetryRejectedCall(System.IntPtr
          hTaskCallee, int dwTickCount, int dwRejectType)
        {
            if (dwRejectType == 2)
            // flag = SERVERCALL_RETRYLATER.
            {
                // Retry the thread call immediately if return >=0 & 
                // <100.
                return 99;
            }
            // Too busy; cancel call.
            return -1;
        }

        int IOleMessageFilter.MessagePending(System.IntPtr hTaskCallee,
          int dwTickCount, int dwPendingType)
        {
            //Return the flag PENDINGMSG_WAITDEFPROCESS.
            return 2;
        }

        // Implement the IOleMessageFilter interface.
        [DllImport("Ole32.dll")]
        private static extern int
          CoRegisterMessageFilter(IOleMessageFilter newFilter, out
          IOleMessageFilter oldFilter);
    }

    [ComImport(), Guid("00000016-0000-0000-C000-000000000046"),
    InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    interface IOleMessageFilter
    {
        [PreserveSig]
        int HandleInComingCall(
            int dwCallType,
            IntPtr hTaskCaller,
            int dwTickCount,
            IntPtr lpInterfaceInfo);

        [PreserveSig]
        int RetryRejectedCall(
            IntPtr hTaskCallee,
            int dwTickCount,
            int dwRejectType);

        [PreserveSig]
        int MessagePending(
            IntPtr hTaskCallee,
            int dwTickCount,
            int dwPendingType);
    }


    public class Excel
    {
        private const int SW_SHOWNORMAL = 1;
        static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
        const UInt32 SWP_NOSIZE = 0x0001;
        const UInt32 SWP_NOMOVE = 0x0002;
        const UInt32 SWP_SHOWWINDOW = 0x0040;
        private delegate bool EnumWindowsCallback(IntPtr hwnd, /*ref*/ IntPtr param);
        private delegate bool EnumThreadWindowsCallback(IntPtr hwnd, /*ref*/ IntPtr param);


        [DllImport("user32.dll")]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
        [DllImport("user32.dll")]
        private static extern int EnumWindows(EnumWindowsCallback callback, /*ref*/ IntPtr param);
        [DllImport("user32.dll")]
        private static extern bool EnumThreadWindows(uint dwThreadId, EnumThreadWindowsCallback callback, /*ref*/ IntPtr param);
        [DllImport("user32.dll")]
        private static extern IntPtr GetParent(IntPtr hwnd);
        [DllImport("user32.dll")]
        private static extern bool EnumChildWindows(IntPtr hWndParent, EnumWindowsCallback callback, /*ref*/ IntPtr param);
        [DllImport("user32.dll")]
        private static extern int GetClassNameW(IntPtr hwnd, [MarshalAs(UnmanagedType.LPWStr)] StringBuilder buf, int nMaxCount);
        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowTextW(IntPtr hwnd, [MarshalAs(UnmanagedType.LPWStr)] StringBuilder buf, int nMaxCount);
        [DllImport("Oleacc.dll")]
        private static extern int AccessibleObjectFromWindow(
              IntPtr hwnd, uint dwObjectID, byte[] riid,
              ref IntPtr ptr /*ppUnk*/);
        // Use the overload that's convenient when we don't need the ProcessId - pass IntPtr.Zero for the second parameter
        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, /*out uint */ IntPtr refProcessId);
        [DllImport("Kernel32")]
        private static extern uint GetCurrentThreadId();
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);
        enum GetWindow_Cmd : uint
        {
            GW_HWNDFIRST = 0,
            GW_HWNDLAST = 1,
            GW_HWNDNEXT = 2,
            GW_HWNDPREV = 3,
            GW_OWNER = 4,
            GW_CHILD = 5,
            GW_ENABLEDPOPUP = 6
        }
        public enum GWL
        {
            GWL_WNDPROC = (-4),
            GWL_HINSTANCE = (-6),
            GWL_HWNDPARENT = (-8),
            GWL_STYLE = (-16),
            GWL_EXSTYLE = (-20),
            GWL_USERDATA = (-21),
            GWL_ID = (-12)
        }
        [Flags]
        enum WindowStyles : uint
        {
            WS_OVERLAPPED = 0x00000000,
            WS_POPUP = 0x80000000,
            WS_CHILD = 0x40000000,
            WS_MINIMIZE = 0x20000000,
            WS_VISIBLE = 0x10000000,
            WS_DISABLED = 0x08000000,
            WS_CLIPSIBLINGS = 0x04000000,
            WS_CLIPCHILDREN = 0x02000000,
            WS_MAXIMIZE = 0x01000000,
            WS_BORDER = 0x00800000,
            WS_DLGFRAME = 0x00400000,
            WS_VSCROLL = 0x00200000,
            WS_HSCROLL = 0x00100000,
            WS_SYSMENU = 0x00080000,
            WS_THICKFRAME = 0x00040000,
            WS_GROUP = 0x00020000,
            WS_TABSTOP = 0x00010000,

            WS_MINIMIZEBOX = 0x00020000,
            WS_MAXIMIZEBOX = 0x00010000,

            WS_CAPTION = WS_BORDER | WS_DLGFRAME,
            WS_TILED = WS_OVERLAPPED,
            WS_ICONIC = WS_MINIMIZE,
            WS_SIZEBOX = WS_THICKFRAME,
            WS_TILEDWINDOW = WS_OVERLAPPEDWINDOW,

            WS_OVERLAPPEDWINDOW = WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX,
            WS_POPUPWINDOW = WS_POPUP | WS_BORDER | WS_SYSMENU,
            WS_CHILDWINDOW = WS_CHILD,

            //Extended Window Styles

            WS_EX_DLGMODALFRAME = 0x00000001,
            WS_EX_NOPARENTNOTIFY = 0x00000004,
            WS_EX_TOPMOST = 0x00000008,
            WS_EX_ACCEPTFILES = 0x00000010,
            WS_EX_TRANSPARENT = 0x00000020,

            //#if(WINVER >= 0x0400)

            WS_EX_MDICHILD = 0x00000040,
            WS_EX_TOOLWINDOW = 0x00000080,
            WS_EX_WINDOWEDGE = 0x00000100,
            WS_EX_CLIENTEDGE = 0x00000200,
            WS_EX_CONTEXTHELP = 0x00000400,

            WS_EX_RIGHT = 0x00001000,
            WS_EX_LEFT = 0x00000000,
            WS_EX_RTLREADING = 0x00002000,
            WS_EX_LTRREADING = 0x00000000,
            WS_EX_LEFTSCROLLBAR = 0x00004000,
            WS_EX_RIGHTSCROLLBAR = 0x00000000,

            WS_EX_CONTROLPARENT = 0x00010000,
            WS_EX_STATICEDGE = 0x00020000,
            WS_EX_APPWINDOW = 0x00040000,

            WS_EX_OVERLAPPEDWINDOW = (WS_EX_WINDOWEDGE | WS_EX_CLIENTEDGE),
            WS_EX_PALETTEWINDOW = (WS_EX_WINDOWEDGE | WS_EX_TOOLWINDOW | WS_EX_TOPMOST),
            //#endif /* WINVER >= 0x0400 */

            //#if(WIN32WINNT >= 0x0500)

            WS_EX_LAYERED = 0x00080000,
            //#endif /* WIN32WINNT >= 0x0500 */

            //#if(WINVER >= 0x0500)

            WS_EX_NOINHERITLAYOUT = 0x00100000, // Disable inheritence of mirroring by children
            WS_EX_LAYOUTRTL = 0x00400000, // Right to left mirroring
                                          //#endif /* WINVER >= 0x0500 */

            //#if(WIN32WINNT >= 0x0500)

            WS_EX_COMPOSITED = 0x02000000,
            WS_EX_NOACTIVATE = 0x08000000
            //#endif /* WIN32WINNT >= 0x0500 */

        }
        [DllImport("user32.dll")]
        static extern IntPtr GetTopWindow(IntPtr hWnd);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll")]
        public static extern bool IsWindowVisible(IntPtr hWnd);
        [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
        static extern IntPtr GetWindowLongPtr32(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
        private static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern int GetWindowThreadProcessId(IntPtr hwnd, out int ID);
        [DllImport("user32.dll")]
        private static extern Boolean ShowWindow(IntPtr hWnd, Int32 nCmdShow);
        [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
        public static extern int SetWindowLongPtr32(IntPtr hWnd, int nIndex, int dwNewLong);
        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
        private static extern int SetWindowLongPtr64(IntPtr hWnd, int nIndex, int dwNewLong);
        private const uint OBJID_NATIVEOM = 0xFFFFFFF0;
        private static readonly byte[] IID_IDispatchBytes = new Guid("{00020400-0000-0000-C000-000000000046}").ToByteArray();
        private static FileVersionInfo _excelExecutableInfo = null;
        // This static method is required because Win32 does not support
        // GetWindowLongPtr directly
        public static IntPtr GetWindowLongWrapper(IntPtr hWnd, int nIndex)
        {
            if (IntPtr.Size == 8)
                return GetWindowLongPtr64(hWnd, nIndex);
            else
                return GetWindowLongPtr32(hWnd, nIndex);
        }
        public static int SetWindowLongWrapper(IntPtr hWnd, int nIndex, int dwNewLong)
        {
            if (IntPtr.Size == 8)
                return SetWindowLongPtr64(hWnd, nIndex, dwNewLong);
            else
                return SetWindowLongPtr32(hWnd, nIndex, dwNewLong);
        }
        internal static FileVersionInfo ExcelExecutableInfo
        {
            get
            {
                if (_excelExecutableInfo == null)
                {
                    ProcessModule excel = Process.GetCurrentProcess().MainModule;
                    _excelExecutableInfo = FileVersionInfo.GetVersionInfo(excel.FileName);
                }
                return _excelExecutableInfo;
            }
        }
        internal static bool SafeIsExcelVersionPre15
        {
            get
            {
                return ExcelExecutableInfo.FileMajorPart < 15;
            }
        }
        //static object GetApplicationFromWindows()
        //{
        //    if (SafeIsExcelVersionPre15)
        //    {
        //        return GetApplicationFromWindow(WindowHandle);
        //    }

        //    return GetApplicationFromWindows15();
        //}
        public static object GetApplicationFromWindow(IntPtr hWndMain)
        {
            // This is Andrew Whitechapel's plan for getting the Application object.
            // It does not work when there are no Workbooks open.
            IntPtr hWndChild = IntPtr.Zero;
            StringBuilder cname = new StringBuilder(256);
            EnumChildWindows(hWndMain, delegate (IntPtr hWndEnum, IntPtr param)
            {
                // Check the window class
                GetClassNameW(hWndEnum, cname, cname.Capacity);
                if (cname.ToString() == "EXCEL7" || cname.ToString() == "XLMAIN")
                {
                    hWndChild = hWndEnum;
                    return false;	// Stop enumerating
                }
                return true;	// Continue enumerating
            }, IntPtr.Zero);
            if (hWndChild != IntPtr.Zero)
            {
                IntPtr pUnk = IntPtr.Zero;
                object obj = null;
                object app = null;
                int hr = AccessibleObjectFromWindow(
                        hWndChild, OBJID_NATIVEOM,
                        IID_IDispatchBytes, ref pUnk);
                if (hr >= 0)
                {
                    try
                    {
                        // Marshal to .NET, then call .Application
                        obj = Marshal.GetObjectForIUnknown(pUnk);

                        app = obj.GetType().InvokeMember("Application", System.Reflection.BindingFlags.GetProperty, null, obj, null, new CultureInfo(1033));

                        //   object ver = app.GetType().InvokeMember("Version", System.Reflection.BindingFlags.GetProperty, null, app, null);
                        return app;
                    }
                    catch (Exception ex)
                    {
                        DevelopWorkspace.Base.Logger.WriteLine(ex.Message,Level.ERROR);
                        return null;
                    }
                    finally {
                        if(pUnk != IntPtr.Zero) Marshal.Release(pUnk);
                        if (obj != null) Marshal.ReleaseComObject(obj);
                    }
                }
            }
            return null;
        }
        // Enumerate through all top-level windows of the main thread,
        // and for those of class XLMAIN, dig down by calling GetApplicationFromWindow.
        //static object GetApplicationFromWindows15()
        //{
        //    object application = null;
        //    StringBuilder buffer = new StringBuilder(256);
        //    EnumThreadWindows(_mainNativeThreadId, delegate (IntPtr hWndEnum, IntPtr param)
        //    {
        //        // Check the window class
        //        if (IsAnExcelWindow(hWndEnum, buffer))
        //        {
        //            application = GetApplicationFromWindow(hWndEnum);
        //            if (application != null)
        //            {
        //                return false;	// Stop enumerating
        //            }
        //            return true;
        //        }
        //        return true;	// Continue enumerating
        //    }, IntPtr.Zero);
        //    return application; // May or may not be null
        //}
        // Check if hWnd refers to a Window of class "XLMAIN" indicating an Excel top-level window.
        static bool IsAnExcelWindow(IntPtr hWnd, StringBuilder buffer)
        {
            buffer.Length = 0;
            GetClassNameW(hWnd, buffer, buffer.Capacity);
            return buffer.ToString() == "XLMAIN";
        }


        //取得最后一个活跃的excel的实例（如果这个实例没有打开的workbook,那么会继续寻找下一个实例）
        //使用侧代码需要使用完后释放这个参照:Marshal.ReleaseComObject(obj);
        public static dynamic GetLatestActiveExcelRef(bool createWhenNoActive = false)
        {
            //TODO https://docs.microsoft.com/zh-cn/previous-versions/ms228772(v=vs.120)
            //解决应用程序正忙 (RPC_E_CALL_REJECTED 0x80010001)
            //被调用者拒绝了调用 (RPC_E_SERVERCALL_RETRYLATER 0x8001010A)
            //MessageFilter.Register();

            //2019/03/16 连续取得时Excel可能在主窗口之上的情况，这个时候就不能正确获取excel实例，这里在处理开始时先设置主窗口为最上方
            SetWindowPos(Process.GetCurrentProcess().MainWindowHandle, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);

            IntPtr lastWindowHandle = GetWindow(Process.GetCurrentProcess().MainWindowHandle, (uint)GetWindow_Cmd.GW_HWNDNEXT);
            IntPtr startWindowHandle = lastWindowHandle;
            dynamic app = null;
            while (!startWindowHandle.Equals(IntPtr.Zero))
            {
                startWindowHandle = GetWindow(startWindowHandle, (uint)GetWindow_Cmd.GW_HWNDNEXT);
                StringBuilder cname = new StringBuilder(256);
                GetClassNameW(startWindowHandle, cname, cname.Capacity);
                if (cname.ToString().Equals("XLMAIN") || cname.ToString().Equals("EXCEL7"))
                {
                    int processId;
                    //获取进程ID  
                    GetWindowThreadProcessId(startWindowHandle, out processId);

                    app = Excel.GetApplicationFromWindow(startWindowHandle);
                    if (app != null && app.ActiveWorkbook != null)
                    {
                        //////int style = (int)GetWindowLongWrapper(Process.GetCurrentProcess().MainWindowHandle,(int)GWL.GWL_STYLE);
                        //2019/03/16 连续取得时Excel可能在主窗口之上的情况，这个时候就不能正确获取excel实例
                        //SetWindowPos(Process.GetCurrentProcess().MainWindowHandle, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
                        //ShowWindow(startWindowHandle, SW_SHOWNORMAL);
                        int style = (int)GetWindowLongWrapper(startWindowHandle, (int)GWL.GWL_STYLE);
                        if ((style & (int)WindowStyles.WS_MINIMIZE) == (int)WindowStyles.WS_MINIMIZE || !IsWindowVisible(startWindowHandle))
                        {
                            ShowWindow(startWindowHandle, SW_SHOWNORMAL);
                        }
                        else
                        {
                            SetForegroundWindow(startWindowHandle);
                        }
                        SetWindowPos(Process.GetCurrentProcess().MainWindowHandle, HWND_NOTOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
                        SetForegroundWindow(Process.GetCurrentProcess().MainWindowHandle);
                        //////SetWindowLongWrapper(Process.GetCurrentProcess().MainWindowHandle, (int)GWL.GWL_STYLE, style & ~(int)HWND_TOPMOST);
                        //SetForegroundWindow(Process.GetCurrentProcess().MainWindowHandle);
                        //}
                        return app;
                    }
                }
            }
            if (createWhenNoActive)
            {
                SetWindowPos(Process.GetCurrentProcess().MainWindowHandle, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
                //2019/3/11 没有必要再去尝试获取一次了，直接生成excel实例
                //try
                //{
                //    app = Marshal.GetActiveObject("Excel.Application");
                //}
                //catch (Exception ex)
                //{
                //    app = new Microsoft.Office.Interop.Excel.Application();
                //}
                app = new Microsoft.Office.Interop.Excel.Application();
                //如果没有打开的WORKBOOK那么创建一个
                app.Visible = true;//防止实例不可见，无法使用
                app.ScreenUpdating = true;
                if (app.Workbooks.Count == 0)
                {
                    app.Workbooks.Add();
                }
            }
            SetWindowPos(Process.GetCurrentProcess().MainWindowHandle, HWND_NOTOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
            SetForegroundWindow(Process.GetCurrentProcess().MainWindowHandle);

            return app;
        }



        public static dynamic GetLatestActiveExcelRefOnWatch(out int procossIdOfExcel)
        {
            IntPtr lastWindowHandle = GetTopWindow((IntPtr)null); ;
            IntPtr startWindowHandle = lastWindowHandle;
            dynamic app = null;
            while (!startWindowHandle.Equals(IntPtr.Zero))
            {
                startWindowHandle = GetWindow(startWindowHandle, (uint)GetWindow_Cmd.GW_HWNDNEXT);
                StringBuilder cname = new StringBuilder(256);
                GetClassNameW(startWindowHandle, cname, cname.Capacity);
                if (cname.ToString().Equals("XLMAIN") || cname.ToString().Equals("EXCEL7"))
                {
                    int processId;
                    //获取进程ID  
                    GetWindowThreadProcessId(startWindowHandle, out processId);

                    app = Excel.GetApplicationFromWindow(startWindowHandle);
                    if (app != null && app.ActiveWorkbook != null)
                    {
                        procossIdOfExcel = processId;
                        return app;
                    }
                }
            }
            procossIdOfExcel = 0;
            return app;
        }

    }

}
