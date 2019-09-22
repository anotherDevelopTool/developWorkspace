using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevelopWorkspace.Base.Model;
using System.Windows.Controls;

namespace DevelopWorkspace.Base
{
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

        public delegate void BusyWork(Action action);
        public delegate void BusyIndicator(string indicatorMessage);
        static BusyWork _busyWork = (Action action) => { action.Invoke(); };
        //20190315 cancel longtimetask 
        public static Button cancelLongTimeTask = null;
        public static LongTimeTaskState longTimeTaskState = LongTimeTaskState.Continue;
        public static void CancelLongTimeTaskOn() {
            if (cancelLongTimeTask != null) {
                cancelLongTimeTask.Visibility = System.Windows.Visibility.Visible;
            }
        }
        public static void CancelLongTimeTaskOff()
        {
            if (cancelLongTimeTask != null)
            {
                cancelLongTimeTask.Visibility = System.Windows.Visibility.Hidden;
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

        public static void ErrorMessage(string errorMsg)
        {
            System.Windows.Controls.ToolTip tooltip = new System.Windows.Controls.ToolTip();
            tooltip.Content=errorMsg;
            tooltip.Style= System.Windows.Application.Current.MainWindow.FindResource("ErrorMessageToolTip") as System.Windows.Style;
            tooltip.IsOpen = true;
            tooltip.BringIntoView();
        }


    }
}
