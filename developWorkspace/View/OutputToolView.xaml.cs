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
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Rendering;
using ICSharpCode.AvalonEdit.Search;
using System.Windows.Threading;
using System.Threading;
using DevelopWorkspace.Base;

namespace DevelopWorkspace.Main.View
{
    /// <summary>
    /// OutputToolView.xaml 的交互逻辑
    /// </summary>
    public partial class OutputToolView : UserControl
    {
        #region LogText property
        public string LogText
        {
            get
            {
                return (string)GetValue(LogTextProperty);
            }
            set
            {
                SetValue(LogTextProperty, value);
            }
        }
        /// <summary>
        /// null是为了规避如果相同的值时不激活回调方法，每次变更之后把最新值设为null，以确保回调方法被调用
        /// </summary>
        public static DependencyProperty LogTextProperty = DependencyProperty.Register(
                "LogText",
                typeof(string),
                typeof(OutputToolView),
                new PropertyMetadata(null, new PropertyChangedCallback(OnLogTextChanged)));

        public static readonly RoutedEvent DateTimeChangedEvent =
            EventManager.RegisterRoutedEvent("DateTimeChanged", RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<string>), typeof(OutputToolView));

        protected virtual void OnLogTextChanged(string oldValue, string newValue)
        {
            RoutedPropertyChangedEventArgs<string> args = new RoutedPropertyChangedEventArgs<string>(oldValue, newValue);
            args.RoutedEvent = OutputToolView.DateTimeChangedEvent;
            RaiseEvent(args);
        }

        private static void OnLogTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            OutputToolView logView = (OutputToolView)d;

            string oldValue = (string)e.OldValue;
            string newValue = (string)e.NewValue;
            //null是为了规避如果相同的值时不激活回调方法，每次变更之后把最新值设为null，以确保回调方法被调用
            if (newValue == null) return;
            logView.LogText = null;
            //LOG出力，在这里
            System.Diagnostics.Debug.Print("###" + newValue);

            //http://www.codeproject.com/Articles/271598/Application-DoEvents-in-WPF
            //For VB programmers out there, you may be thinking that you simply call Application.DoEvents() to free up the message loop and unfortunately WPF doesn’t provide the same API.However, there is a way to do this in WPF.By pushing a nested message loop, we can cause this nested message loop to be processed immediately, allowing the window’s content to be rendered and our thumbnail to be generated.I wouldn’t recommend using nested message loops and neither would many other people, but in this case, it works for me.
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background,new ThreadStart(delegate { }));
            logView.LogViewTextEditor.AppendText(newValue + "\n");
            logView.LogViewTextEditor.ScrollToEnd();

            logView.OnLogTextChanged(oldValue, newValue);
        }
        #endregion
        public OutputToolView()
        {
            InitializeComponent();
            //SearchPanel.Install(this.LogViewTextEditor);

            //TODO 2019/02/23 不再使用model绑定的方式输出日志
            Base.Services.BusyWorkService(new Action(() =>
            {
                RemoteLogger remoteLogger = new RemoteLogger();
                remoteLogger.level = AppConfig.SysConfig.This.logLevel;
                remoteLogger.output = (logtext) =>
                {
                    Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate { }));
                    //this.LogViewTextEditor.AppendText(logtext + "\n");
                    //this.LogViewTextEditor.ScrollToEnd();
                    //2019/03/04 后台线程调用时对应 
                    this.LogViewTextEditor.Dispatcher.BeginInvoke((Action)delegate ()
                    {
                        this.LogViewTextEditor.AppendText(logtext + "\n");
                        this.LogViewTextEditor.ScrollToEnd();

                    });

                };
            }));
        }
    }
}
