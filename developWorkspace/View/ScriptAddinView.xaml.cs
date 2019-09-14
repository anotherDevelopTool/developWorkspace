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
using System.Reflection;
using System.Security;
using System.Security.Policy;
using System.Security.Permissions;
using System.ComponentModel;
using Xceed.Wpf.Toolkit.PropertyGrid;
using Newtonsoft.Json;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
using System.Diagnostics;
using DevelopWorkspace.Base;
using System.Text.RegularExpressions;
using DevelopWorkspace.Base.Utils;
using ICSharpCode.AvalonEdit.Highlighting;
using static DevelopWorkspace.Main.AppConfig;
using Fluent;
using DevelopWorkspace.Base.Model;
using System.Drawing;

namespace DevelopWorkspace.Main.View
{

    /// <summary>
    /// CSScriptRunView.xaml 的交互逻辑
    /// </summary>
    [Serializable()]
    public partial class ScriptAddinView : UserControl
    {
        static bool IsAppDomainInited = false;
        static AppDomain singleAppDomain;
        TreeView codeLibraryTreeView;
        PropertyGrid propertygrid1;
        TwoLineLabel basicInfoLabel;
        ScriptConfig scriptConfig;
        ScriptBaseViewModel model;
        public ScriptAddinView()
        {
            //BusyWorkServiceの外側で処理を入れる場合、this.DataContextがうまく取得できない場合があるので要注意
            Base.Services.BusyWorkService(new Action(() =>
            {
                InitializeComponent();

                model = this.DataContext as Base.Model.ScriptBaseViewModel;
                this.ViewContainer.Children.Add(model.getView()); ;
                //ribbon工具条注意resource定义在usercontrol内这样click等事件直接可以和view代码绑定
                Fluent.Ribbon ribbon = Base.Utils.WPF.FindChild<Fluent.Ribbon>(Application.Current.MainWindow, "ribbon");


                var ribbonTabTool = FindResource("RibbonTabTool") as Fluent.RibbonTabItem;
                RibbonGroupBox ribbonGroupBox = Base.Utils.WPF.FindLogicaChild<Fluent.RibbonGroupBox>(ribbonTabTool, "ribbonGroupBox");
                Fluent.Button btn_1 = Base.Utils.WPF.FindLogicaChild<Fluent.Button>(ribbonTabTool, "btn_1");
                List<Fluent.Button> buttonList = new List<Fluent.Button>();
                buttonList.Add(btn_1);
                var methods = model.GetType().GetMethods().Where(m => m.GetCustomAttributes(typeof(MethodMetaAttribute), false).Length > 0).ToList();
                for (int i = 1; i < methods.Count; i++)
                {
                    Fluent.Button button = new Fluent.Button();
                    ribbonGroupBox.Items.Add(button);
                    buttonList.Add(button);
                }
                for (int i = 0; i < methods.Count; i++)
                {
                    var method = methods[i];
                    var methodAttribute = (MethodMetaAttribute)Attribute.GetCustomAttribute(methods[i], typeof(MethodMetaAttribute));
                    buttonList[i].Header = methodAttribute.Name;
                    buttonList[i].ToolTip = methodAttribute.Description;
                    string iconfile = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "addins", methodAttribute.Name + ".png");
                    if (File.Exists(iconfile))
                    {
                        var uri = new Uri(iconfile);
                        buttonList[i].LargeIcon = new BitmapImage(uri);
                    }
                    

                    buttonList[i].Click += (obj, subargs) =>
                    {
                        method.Invoke(model, new object[] { obj,subargs});
                    };
                }

                //之前的active内容关联的tab需要隐藏
                if (Base.Services.ActiveModel != null)
                {
                    foreach (object tab in Base.Services.GetRibbon(Base.Services.ActiveModel))
                    {
                        ribbon.Tabs.Remove(tab as Fluent.RibbonTabItem);
                    }
                }
                //从control.resources里面取出ribbontabitem的xaml定义同时实例化
                //    DevelopWorkspace.Base.Logger.WriteLine("Process committed");
                ribbon.Tabs.Add(ribbonTabTool);
                ribbon.SelectedTabIndex = ribbon.Tabs.Count - 1;
                Base.Services.ActiveModel.RibbonTabIndex = ribbon.SelectedTabIndex;
                Base.Services.RegRibbon(this.DataContext as Base.Model.PaneViewModel, new List<object> { ribbonTabTool });

                Base.Services.ActiveModel = this.DataContext as Base.Model.PaneViewModel;

            }));
        }




    }
}
