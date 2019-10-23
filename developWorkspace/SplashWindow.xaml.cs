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
using System.Windows.Shapes;
using System.Windows.Threading;

namespace DevelopWorkspace.Main
{
     /// <summary>
    /// ConfirmForm.xaml 的交互逻辑
    /// </summary>
    public partial class SplashWindow : Window
    {
         /// <summary>
        /// 
        /// </summary>
        /// <param name="confirmMessage">确认信息</param>
        /// <param name="rowInfoList">选项列表</param>
        /// <param name="MultiSelect">控制单选还是允许复选</param>
        public SplashWindow()
        {
            InitializeComponent();
            this.busy.IsBusyIndicatorShowing = true;
            this.Loaded += new RoutedEventHandler(Splash_Loaded);
        }
        void Splash_Loaded(object sender, RoutedEventArgs e)
        {
            IAsyncResult result = null;

            // This is an anonymous delegate that will be called when the initialization has COMPLETED
            AsyncCallback initCompleted = delegate (IAsyncResult ar)
            {
                try
                {
                    App.Current.ApplicationInitialize.EndInvoke(result);
                }
                catch (Exception ex)
                {
                }
                try { 
                    // Ensure we call close on the UI Thread.
                    Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Invoker)delegate { Close(); });
                }
                catch (Exception ex) {
                }
            };

            // This starts the initialization process on the Application
            result = App.Current.ApplicationInitialize.BeginInvoke(this, initCompleted, null);
        }

        public void SetProgress(double progress)
        {
            // Ensure we update on the UI Thread.
            //Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Invoker)delegate { progBar.Value = progress; });
        }
    }
}