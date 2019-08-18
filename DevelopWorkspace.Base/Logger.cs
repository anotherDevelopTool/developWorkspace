using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevelopWorkspace.Base
{

    //通过这个机制新的appdomain里可以调用默认domain里的UI服务
    public class RemoteLogger : MarshalByRefObject, Ilogger
    {
        public Action<string> output {
            get;
            set; }

        public Level level { get; set; }

        public void WriteLine(string logText, Level _level = Level.INFO)
        {

            if (_level >= level)
            {
                if (_level != Level.INFO)
                {
                    StackFrame sf = (new StackTrace()).GetFrame(2);
                    //using (StringReader sr = new StringReader(logText))
                    //{
                    //    string line;
                    //    while ((line = sr.ReadLine()) != null)
                    //    {
                    //        output($"{Enum.GetName(typeof(Level), _level)}...{DateTime.Now.ToString()}>[{sf.GetMethod().Name}]:{line}");
                    //    }
                    //}
                    output($"{Enum.GetName(typeof(Level), _level)}...{DateTime.Now.ToString()}>[{sf.GetMethod().Name}]:{logText}");
                }
                else
                {
                    output(logText);
                }
            }
        }
        //关联上
        public RemoteLogger()
        {
            level = Level.INFO;
            //TODO
            output = (logtext) =>
            {
            };
            Logger.Inner = this;
            AppDomain.CurrentDomain.SetData("logger", this);
        }
        //关于【对象“***.rem”已经断开连接或不在服务器上】异常的解决方法
        //对象“/9ca38d87_7f53_49b7_8c81_f2d499239f27/jqgpmhyy_rwcmicplf3j8s0j_1.rem”已经断开连接或不在服务器上。
        //如此可以保证客户端的Callback对象在应用程序的整个生命周期内都是激活的状态，服务端可以随时回调它
        public override object InitializeLifetimeService()
        {
            //Remoting对象 无限生存期
            return null;
        }
    }
    public abstract class Logger
    {
        public static void setLevel(Level level) {
            Ilogger logger = AppDomain.CurrentDomain.GetData("logger") as Ilogger;
            if (logger == null) return;
            logger.level = level;
        }

        private static Ilogger _inner = null;

        public static Ilogger Inner
        {
            get { return Logger._inner; }
            set { Logger._inner = value; }
        }
        public static void WriteLine(string logText,Level level = Level.INFO)
        {
            Inner=AppDomain.CurrentDomain.GetData("logger") as Ilogger;
            if (Inner == null) return;
            Inner.WriteLine(logText, level);
        }

    }

}
