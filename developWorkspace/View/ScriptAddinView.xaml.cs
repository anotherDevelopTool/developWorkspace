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
using ICSharpCodeX.AvalonEdit.Highlighting;
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
        private static bool CanLoadResource(Uri uri)
        {
            try
            {
                Application.GetResourceStream(uri);
                return true;
            }
            catch (IOException)
            {
                return false;
            }
        }
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
                string defaultCategoryKey = ribbonTabTool.Header.ToString();
                Fluent.Button btn_1 = Base.Utils.WPF.FindLogicaChild<Fluent.Button>(ribbonTabTool, "btn_1");
                List<System.Windows.Controls.Control> buttonList = new List<System.Windows.Controls.Control>();
                buttonList.Add(btn_1);
                var methods = model.GetType().GetMethods().Where(m => m.GetCustomAttributes(typeof(MethodMetaAttribute), false).Length > 0).ToList();


                List<MethodMetaAttribute> methodMetaAttributeList = new List<MethodMetaAttribute>();
                Dictionary<string, Fluent.RibbonTabItem> tabItemList = new Dictionary<string, RibbonTabItem>();
                List<Fluent.RibbonTabItem> tabItemSortedList = new List<RibbonTabItem>();
                tabItemList[defaultCategoryKey] = ribbonTabTool;
                tabItemSortedList.Add(ribbonTabTool);
                ///
                for (int i = 0; i < methods.Count; i++)
                {
                    var method = methods[i];
                    var methodAttribute = (MethodMetaAttribute)Attribute.GetCustomAttribute(methods[i], typeof(MethodMetaAttribute));
                    methodMetaAttributeList.Add(methodAttribute);
                }
                var categories = methodMetaAttributeList.GroupBy(attribute => attribute.Category);
                bool hasDefaultCatetory = false;
                foreach (var category in categories)
                {
                    if (string.IsNullOrEmpty(category.Key))
                    {
                        hasDefaultCatetory = true;
                    }
                }
                bool firstIdx = true;
                foreach (var category in categories) {
                    if (string.IsNullOrEmpty(category.Key))
                    {
                    }
                    else {
                        Fluent.RibbonTabItem ribbonTabItem;
                        if (firstIdx && !hasDefaultCatetory)
                        {
                            ribbonTabItem = ribbonTabTool;
                            ribbonTabItem.Header = category.Key;
                            tabItemList[category.Key] = tabItemList[defaultCategoryKey];
                            tabItemList.Remove(defaultCategoryKey);
                            firstIdx = !firstIdx;
                        }
                        else
                        {
                            ribbonTabItem = new Fluent.RibbonTabItem();
                            ribbonTabItem.Header = category.Key;
                            RibbonGroupBox groupBox = new RibbonGroupBox();
                            groupBox.Header = category.Key;
                            ribbonTabItem.Groups.Add(groupBox);
                            tabItemList[category.Key] = ribbonTabItem;
                            tabItemSortedList.Add(ribbonTabItem);
                        }
                    }
                }
                for (int i = 1; i < methods.Count; i++)
                {
                    string category = methodMetaAttributeList[i].Category;
                    string controlType = string.IsNullOrEmpty(methodMetaAttributeList[i].Control) ? "button" : methodMetaAttributeList[i].Control;
                    if ("button".Equals(controlType))
                    {
                        var button = new Fluent.Button();
                        category = string.IsNullOrEmpty(category) ? defaultCategoryKey : category;
                        var ribbonTabItem = tabItemList.Where(groubox => groubox.Key.Equals(category)).Select(dic => dic.Value).First();
                        ribbonTabItem.Groups[0].Items.Add(button);
                        buttonList.Add(button);
                    }
                    else if ("combobox".Equals(controlType))
                    {
                        var combobox = new Fluent.ComboBox();
                        combobox.Name = methodMetaAttributeList[i].Name;
                        category = string.IsNullOrEmpty(category) ? defaultCategoryKey : category;
                        var ribbonTabItem = tabItemList.Where(groubox => groubox.Key.Equals(category)).Select(dic => dic.Value).First();
                        ribbonTabItem.Groups[0].Items.Add(combobox);
                        buttonList.Add(combobox);

                        var initFunc = model.GetType().GetMethods().Where(m => m.Name.Equals(methodMetaAttributeList[i].Init)).First();
                        var dataList = initFunc.Invoke(model, new object[] { });
                        combobox.ItemsSource = (List<string>)dataList;
                        combobox.SelectedIndex = 0;
                        combobox.MinWidth = 100;
                        combobox.Margin = new Thickness(5,10,5,10);
                    }
                }
                for (int i = 0; i < methods.Count; i++)
                {
                    var method = methods[i];
                    var methodAttribute = (MethodMetaAttribute)Attribute.GetCustomAttribute(methods[i], typeof(MethodMetaAttribute));
                    if (buttonList[i] is Fluent.Button)
                    {
                        var button = ((Fluent.Button)buttonList[i]);

                        button.Header = methodAttribute.Name;
                        button.ToolTip = methodAttribute.Description;
                        string iconfile = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "addins", methodAttribute.LargeIcon + ".png");
                        if (File.Exists(iconfile))
                        {
                            var uri = new Uri(iconfile);
                            button.LargeIcon = new BitmapImage(uri);
                        }
                        else
                        {
                            var resourceString = "/DevelopWorkspace;component/Images/" + (string.IsNullOrEmpty(methodAttribute.LargeIcon) ? "plugin" : methodAttribute.LargeIcon) + ".png";
                            if (CanLoadResource(new Uri(resourceString, UriKind.Relative)))
                            {
                                button.LargeIcon = new BitmapImage(new Uri(resourceString, UriKind.Relative));
                            }
                            else
                            {
                                button.LargeIcon = new BitmapImage(new Uri("/DevelopWorkspace;component/Images/plugin.png", UriKind.Relative));
                            }
                        }

                        button.Click += (obj, subargs) =>
                        {

                            Base.Services.BusyWorkService(new Action(() =>
                            {
                                try
                                {
                                    method.Invoke(model, new object[] { obj, subargs });
                                }
                                catch (Exception ex)
                                {
                                    DevelopWorkspace.Base.Logger.WriteLine(ex.Message, Base.Level.ERROR);
                                }
                            }));

                        };
                    }
                    if (buttonList[i] is Fluent.ComboBox)
                    {
                        var combobox = (Fluent.ComboBox)buttonList[i];
                        combobox.SelectionChanged += (obj, subargs) =>
                        {
                            Base.Services.BusyWorkService(new Action(() =>
                            {
                                try
                                {
                                    method.Invoke(model, new object[] { obj,combobox.SelectedItem });
                                }
                                catch (Exception ex)
                                {
                                    DevelopWorkspace.Base.Logger.WriteLine(ex.Message, Base.Level.ERROR);
                                }
                            }));

                        };
                    }

                }
                // 自定义Ribbon，提供更强力接口
                var ribbonGroupBoxList = model.GetType().GetMethods().Where(m => m.ReturnType == typeof(Fluent.RibbonGroupBox)).ToList();
                for (int i = 0; i < ribbonGroupBoxList.Count; i++)
                {
                    var method = ribbonGroupBoxList[i];
                    var groupBox = method.Invoke(model, new object[] { }) as Fluent.RibbonGroupBox;
                    if(groupBox != null) ribbonTabTool.Groups.Add(groupBox);
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
                tabItemSortedList.ForEach(tabItem => {
                    ribbon.Tabs.Add(tabItem);
                });
                ribbon.SelectedTabIndex = ribbon.Tabs.Count - tabItemSortedList.Count();
                Base.Services.ActiveModel.RibbonTabIndex = ribbon.SelectedTabIndex;
                Base.Services.RegRibbon(this.DataContext as Base.Model.PaneViewModel, tabItemSortedList.ToList<object>());

                Base.Services.ActiveModel = this.DataContext as Base.Model.PaneViewModel;

            }));
        }

    }
}
