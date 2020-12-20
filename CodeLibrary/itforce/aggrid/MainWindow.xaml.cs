using CefSharp;
using CefSharp.Wpf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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

namespace WpfApp8
{
    public class DotNetMessage
    {
        public void Show(string message)
        {
            MessageBox.Show(message);
        }
        public string GetData()
        {
            string data = File.ReadAllText(@"C:\Users\xujingjiang\Source\Repos\developSupportToolls\developWorkspace\bin\Debug\CefSharp\aggrid\olympicWinnersSmall.json");
            return data;

        }
    }
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        CefSharp.Wpf.ChromiumWebBrowser cefBrowserView;

        [Obsolete]
        public MainWindow()
        {
            if (AppDomain.CurrentDomain.GetAssemblies().Where(assembly => assembly.GetName().ToString().StartsWith("CefSharp")).Count() == 0)
            {
                //编译出错时一旦把这个注释掉之后编译通过之后在关闭注释后再编译
                CefSharp.Cef.Initialize(new CefSettings());
            }
            InitializeComponent();
            Cef.Initialize(new CefSettings());
            cefBrowserView = new CefSharp.Wpf.ChromiumWebBrowser("http://localhost:8084/index.html");
            cefBrowserView.JavascriptObjectRepository.Settings.LegacyBindingEnabled = true;
            CefSharpSettings.LegacyJavascriptBindingEnabled = true;
            cefBrowserView.JavascriptObjectRepository.Register("dotNetMessage", new DotNetMessage(), isAsync: false, options: BindingOptions.DefaultBinder);

            cefBrowserView.Width = 1000;
            cefBrowserView.Height = 1000;
            MyBrowser.Children.Add(cefBrowserView);
            cefBrowserView.LoadingStateChanged += CefBrowserView_LoadingStateChanged;
            cefBrowserView.ConsoleMessage += Browser_ConsoleMessage;
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            cefBrowserView.Load(@"C:\Users\xujingjiang\Source\Repos\developSupportToolls\developWorkspace\bin\Debug\CefSharp\aggrid\index.html");
        }
        private void Browser_ConsoleMessage(object sender, ConsoleMessageEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(e.Message);
        }
        private void CefBrowserView_LoadingStateChanged(object sender, LoadingStateChangedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("CefBrowserView_LoadingStateChanged");
            if (!e.IsLoading) { 
                //
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var pageQueryScript = @"
                    (function(){
                        return gridOptions.api.getSelectedNodes()[0].data; 
                    })()";
            var scriptTask = cefBrowserView.EvaluateScriptAsync(pageQueryScript);
            scriptTask.ContinueWith(u =>
            {
                if (u.Result.Success && u.Result.Result != null)
                {
                    dynamic rowdata = u.Result.Result;
                    //System.Diagnostics.Debug.WriteLine(rowdata.athlete);
                }
            });

        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            var pageQueryScript = @"
                    (function(){
                        LoadData();
                        return 0; 
                    })()";
            var scriptTask = cefBrowserView.EvaluateScriptAsync(pageQueryScript);
            scriptTask.ContinueWith(u =>
            {
                if (u.Result.Success && u.Result.Result != null)
                {
                    System.Diagnostics.Debug.WriteLine(u.Result.Result.ToString());
                }
            });

        }
    }
}
