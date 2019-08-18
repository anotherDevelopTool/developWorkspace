namespace DevelopWorkspace.Main.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.IO;
    using System.Windows.Media.Imaging;
    using System.Windows.Media;
    using DevelopWorkspace.Base;
    using DevelopWorkspace.Base.Model;
    //通过这个机制新的appdomain里可以调用默认domain里的UI服务
    public class RemoteLogger : MarshalByRefObject, Ilogger
    {
        OutputToolViewModel model;
        public Level level { get; set; }
        internal RemoteLogger(OutputToolViewModel m)
        {
            model = m;
        }
        //关于【对象“***.rem”已经断开连接或不在服务器上】异常的解决方法
        //对象“/9ca38d87_7f53_49b7_8c81_f2d499239f27/jqgpmhyy_rwcmicplf3j8s0j_1.rem”已经断开连接或不在服务器上。
        //如此可以保证客户端的Callback对象在应用程序的整个生命周期内都是激活的状态，服务端可以随时回调它
        public override object InitializeLifetimeService()
        {
            //Remoting对象 无限生存期
            return null;
        }
        public void WriteLine(string logText, Level level = Level.INFO)
        {
            model.WriteLine(logText);
        }
    }
    class OutputToolViewModel : ToolViewModel, Ilogger
    {
        public Level level { get; set; }
        public OutputToolViewModel()
          : base("Output")
        {
            Workspace.This.ActiveDocumentChanged += new EventHandler(OnActiveDocumentChanged);
            ContentId = ToolContentId;
            //关联上
            //Logger.Inner = this;
            //AppDomain.CurrentDomain.SetData("logger", new RemoteLogger(this));

            ////TODO
            //output = (logtext) =>
            //{
            //};

        }
        public const string ToolContentId = "Output";


        //2019/02/23 通过属性绑定的方式有问题，废弃,换成delegate方式
        public Action<string> output { get; set; }
        public void WriteLine(string logText, Level level = Level.INFO)
        {
            output(logText);
        }

        //public void WriteLine(string logText)
        //{
        //    LogText = logText;
        //}

        private string _logText;
        public string LogText
        {
            get { return _logText; }
            set
            {

                _logText = value;
                RaisePropertyChanged("LogText");
            }
        }


        void OnActiveDocumentChanged(object sender, EventArgs e)
        {
            FileSize = 0;
            LastModified = DateTime.MinValue;

            if (Workspace.This.ActiveDocument != null)
            {
                FileViewModel f = Workspace.This.ActiveDocument as FileViewModel;

                if (f != null)
                {
                    if (f.FilePath != null && File.Exists(f.FilePath))
                    {
                        var fi = new FileInfo(f.FilePath);
                        FileSize = fi.Length;
                        LastModified = fi.LastWriteTime;
                    }

                }
            }
        }

        #region FileSize

        private long _fileSize;
        public long FileSize
        {
            get { return _fileSize; }
            set
            {
                if (_fileSize != value)
                {
                    _fileSize = value;
                    RaisePropertyChanged("FileSize");
                }
            }
        }

        #endregion

        #region LastModified

        private DateTime _lastModified;
        public DateTime LastModified
        {
            get { return _lastModified; }
            set
            {
                if (_lastModified != value)
                {
                    _lastModified = value;
                    RaisePropertyChanged("LastModified");
                }
            }
        }

        #endregion

        public override Uri IconSource
        {
            get
            {
                return new Uri("pack://application:,,,/DevelopWorkspace;component/Images/property-blue.png", UriKind.RelativeOrAbsolute);
            }
        }
    }
}
