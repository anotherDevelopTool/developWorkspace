using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevelopWorkspace.Base.Model;
using System.Windows.Controls;
using Newtonsoft.Json;
using System.ComponentModel;
using System.IO;
using System.Windows.Threading;
using System.Threading;
using System.Collections.ObjectModel;
using static DevelopWorkspace.Base.Services;

namespace DevelopWorkspace.Base
{
    public enum eDatabaseTranOperation
    {
        ROLLBACK,
        COMMIT
    }

    public class license {
        public static bool IsTrialLicense { get; set; }
        public static string DaysToEnd { get; set; }
        public static string Runed { get; set; }
    }

    public enum LongTimeTaskState { Cancel,Continue};
    public class RibbonSelectionChangeEventArgs : System.EventArgs
    {
        public int SelectedIndex { get; set; }
        public RibbonSelectionChangeEventArgs(int selectedIndex)
        {
            SelectedIndex = selectedIndex;
        }
    }
    public class WorksheetActiveChangeEventArgs : System.EventArgs
    {
        public string TableName { get; set; }
        public WorksheetActiveChangeEventArgs(string tableName)
        {
            TableName = tableName;
        }
    }
    public class AddinInstalledEventArgs : System.EventArgs
    {
        public AddinMetaAttribute MetaAttriute { get; set; }
        public AddinInstalledEventArgs(AddinMetaAttribute attribute)
        {
            MetaAttriute = attribute;
        }
    }
    public delegate void RibbonSelectionChangeEventHandler(object sender, RibbonSelectionChangeEventArgs e);
    public delegate void WorksheetActiveChangeEventHandler(object sender, WorksheetActiveChangeEventArgs e);
    public delegate void AddinInstalledEventHandler(object sender, AddinInstalledEventArgs e);

    public abstract class Services
    {

        public delegate void BusyWork(Action action, Boolean hasContinuedAction = false);
        public delegate void SimpleAroundWork(object target, string actionName,object context,Action action);
        public delegate void BusyIndicator(string indicatorMessage);

        public delegate object RibbonQuery(object parent);

        static BusyWork _busyWork = (Action action, Boolean hasContinuedAction) => { action.Invoke(); };
        static SimpleAroundWork _aroundWork = (object target,string actionName, object context, Action action ) => { action.Invoke(); };
        //20190315 cancel longtimetask 
        public static Button cancelLongTimeTask = null;
        public static LongTimeTaskState longTimeTaskState = LongTimeTaskState.Continue;
        public static ObservableCollection<ContextMenuCommand> dbsupportContextmenuCommandList = new ObservableCollection<ContextMenuCommand>();
        public static ObservableCollection<ContextMenuCommand> dbsupportSqlContextmenuCommandList = new ObservableCollection<ContextMenuCommand>();
        
        public static void CancelLongTimeTaskOn() {
            if (cancelLongTimeTask != null) {
                cancelLongTimeTask.Dispatcher.BeginInvoke((Action)delegate () {
                    cancelLongTimeTask.Visibility = System.Windows.Visibility.Visible;

                });
            }
        }
        public static void CancelLongTimeTaskOff()
        {
            if (cancelLongTimeTask != null)
            {
                cancelLongTimeTask.Dispatcher.BeginInvoke((Action)delegate () {
                    cancelLongTimeTask.Visibility = System.Windows.Visibility.Hidden;

                });
            }
            longTimeTaskState = LongTimeTaskState.Continue;
        }

        public static BusyWork BusyWorkService
        {
            get { return Services._busyWork; }
            set { Services._busyWork = value; }
        }
        static BusyIndicator _busyWorkIndicator = (string indicatorMessage) => {};
        public static BusyIndicator BusyWorkIndicatorService
        {
            get { return Services._busyWorkIndicator; }
            set { Services._busyWorkIndicator = value; }
        }

        public static SimpleAroundWork SimpleAroundCallService
        {
            get { return Services._aroundWork; }
            set { Services._aroundWork = value; }
        }
        

        //主窗体用的扩张用
        static RibbonQuery _ribbonQueryMain = (object parent) => { return null; };
        public static RibbonQuery RibbonQueryMain
        {
            get { return Services._ribbonQueryMain; }
            set { Services._ribbonQueryMain = value; }
        }
        //DB机能用的扩张用
        static RibbonQuery _ribbonQueryDb = (object parent) => { return null; };
        public static RibbonQuery RibbonQueryDb
        {
            get { return Services._ribbonQueryDb; }
            set { Services._ribbonQueryDb = value; }
        }
        //DB机能用的扩张用
        static RibbonQuery _ribbonQueryScript = (object parent) => { return null; };
        public static RibbonQuery RibbonQueryScript
        {
            get { return Services._ribbonQueryScript; }
            set { Services._ribbonQueryScript = value; }
        }


        static Dictionary<Model.PaneViewModel, List<object>> dicRibbonCache = new Dictionary<Model.PaneViewModel, List<object>>();
        public static void RegRibbon(Model.PaneViewModel model, List<object> tabs)
        {
           dicRibbonCache[model] = tabs;
        }
        public static List<object> GetRibbon(Model.PaneViewModel model)
        {
            if(dicRibbonCache.ContainsKey(model))
                return dicRibbonCache[model] ;
            return new List<object>();
        }
        static Model.PaneViewModel activeModel = null;
        public static PaneViewModel ActiveModel
        {
            get
            {
                return activeModel;
            }

            set
            {
                activeModel = value;
            }
        }
        // 数据库连接过长时画面freeze防止
        public static void executeWithBackgroundAction(Action action)
        {
            Exception exception = null;
            Task backgroundJob = new Task(() => {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    exception = ex;
                }

            });
            backgroundJob.Start();

            while (!backgroundJob.Wait(100))
            {
                System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate { }));

            }
            if (exception != null)
            {
                throw exception;
            }
        }
        public static void ErrorMessage(string errorMsg)
        {
            System.Windows.Controls.ToolTip tooltip = new System.Windows.Controls.ToolTip();
            tooltip.Content=errorMsg;
            tooltip.Style= System.Windows.Application.Current.MainWindow.FindResource("ErrorMessageToolTip") as System.Windows.Style;
            tooltip.IsOpen = true;
            tooltip.BringIntoView();
        }
        static Func<string, string> _mappingColumnName = (originalString) => "";
        public static Func<string, string> mappingColumnName
        {
            get { return _mappingColumnName; }
            set { _mappingColumnName = value; }
        }

    }


    public class ConfigBase : INotifyPropertyChanged
    {
        [ReadOnly(true)]
        public string typeName { get; set; }
        [ReadOnly(true)]
        public string jsonfile { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    public class JsonConfig<T> where T : ConfigBase, new()
    {
        public static T load(string path)
        {
            string jsonfile = System.IO.Path.Combine(path, $"{ typeof(T).Name}.json");
            T t = internalLoad(jsonfile);
            t.jsonfile = jsonfile;
            return t;
        }
        public static T loadByFullPath(string path)
        {
            string jsonfile = path;
            T t = internalLoad(jsonfile);
            t.jsonfile = jsonfile;
            return t;
        }
        static T internalLoad(string filepath)
        {
            T t;
            if (System.IO.File.Exists(filepath))
            {
                string json = File.ReadAllText(filepath, Encoding.UTF8);
                t = (T)JsonConvert.DeserializeObject(json, typeof(T));
                t.jsonfile = filepath;
                if (t.typeName == null) t.typeName = typeof(T).Name;
                if (!typeof(T).Name.Equals(t.typeName))
                {
                    DevelopWorkspace.Base.Logger.WriteLine($"can't load json:{filepath} correctly with typename incorresponding");
                    throw new Exception($"can't load json:{filepath} correctly with typename incorresponding");
                }
            }
            else
            {
                //JsonConvert.DeserializeObject的动作式样是先实例化T，之后对它的属性进行覆盖，如果属性是集合，则进行追加
                //这样默认的构造体里就不能进行赋值行为，另建一个带参构造体通过它进行实例化
                t = (T)Activator.CreateInstance(typeof(T), 0);
 
            }
            return t;
        }
        public static void flush(T userSetting)
        {
            //在反序列化时会激活属性set方法，这个时候jsonfile可能还么有来得及设定
            if (!string.IsNullOrEmpty(userSetting.jsonfile))
            {
                try
                {
                    string json = JsonConvert.SerializeObject(userSetting, Newtonsoft.Json.Formatting.Indented);
                    File.WriteAllText(userSetting.jsonfile, json, Encoding.UTF8);
                }
                catch (Exception ex)
                {
                    DevelopWorkspace.Base.Logger.WriteLine(ex.Message);
                }
            }
        }
    }



}
