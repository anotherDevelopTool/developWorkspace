using System;
using Microsoft.CSharp;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Xml;
using System.Windows.Markup;
using WPFMediaKit;
using DevelopWorkspace.Base;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Heidesoft.Components.Controls;
using System.Windows.Threading;
using System.Linq;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Diagnostics;
using System.Threading;
using Xceed.Wpf.AvalonDock.Layout;
using System.Windows.Media;
using DevelopWorkspace.Base;
using System.IO;
using System.Linq;
using System.Reflection;
public class Script
{

    //https://stackoverflow.com/questions/248362/how-do-i-build-a-datatemplate-in-c-sharp-code
    //TODO 面向Addin基类化
    [AddinMeta(Name = "elasticSearch", Date = "2009-07-20", Description = "elastic utility",Red =128,Green=145,Blue=213)]
    public class ViewModel : DevelopWorkspace.Base.Model.ScriptBaseViewModel
    {
        System.Windows.Controls.ListView listView;
        [MethodMeta(Name = "load", Date = "2009-07-20", Description = "所有index载入", LargeIcon = "elasticsearch")]
        public void EventHandler1(object sender, RoutedEventArgs e)
        {

            DevelopWorkspace.Base.Logger.WriteLine("Process called");
        }
        [MethodMeta(Name = "import", Date = "2009-07-20", Description = "导入指定index的所有document到EXCEL", LargeIcon = "import")]
        public void EventHandler2(object sender, RoutedEventArgs e)
        {

            DevelopWorkspace.Base.Logger.WriteLine("Process called");
        }
        [MethodMeta(Name = "export", Date = "2009-07-20", Description = "指定EXCEL内的数据反映到Elastic", LargeIcon = "export")]
        public void EventHandler3(object sender, RoutedEventArgs e)
        {

            DevelopWorkspace.Base.Logger.WriteLine("Process called");
        }
        [MethodMeta(Name = "delete", Date = "2009-07-20", Description = "删除指定index的所有document，index本身不删除", LargeIcon = "delete")]
        public void EventHandler4(object sender, RoutedEventArgs e)
        {
            dynamic content = listView.SelectedItem;
            DevelopWorkspace.Base.Logger.WriteLine(content.index);
        }
        public override UserControl getView(string strXaml)
        {
            StringReader strreader = new StringReader(strXaml);
            XmlTextReader xmlreader = new XmlTextReader(strreader);
            UserControl view = XamlReader.Load(xmlreader) as UserControl;
            listView = DevelopWorkspace.Base.Utils.WPF.FindLogicaChild<System.Windows.Controls.ListView>(view, "trvFamilies");
            listView.DataContext = new[] { new { IsNotKey = false, index = "current_sale_20190909", size = 200 }, new { IsNotKey = false, index = "current_sale_20190910", size = 200 } };
            listView.SelectedIndex = 0;
            // (listView.SelectedItem as ListViewItem).Content
            //btnCapurure.Click += (obj, subargs) =>
            //{
            //    DevelopWorkspace.Base.Logger.WriteLine("Process called");
            //};
            return view;
        }
    }

    public class MainWindow : Window
    {
        public MainWindow(string strXaml)
        {
            Width = 600;
            Height = 800;

            Grid grid = new Grid();
            Content = grid;

            StackPanel parent = new StackPanel();
            grid.Children.Add(parent);

            ViewModel model = new ViewModel();

            var methods = model.GetType().GetMethods().Where(m => m.GetCustomAttributes(typeof(MethodMetaAttribute), false).Length > 0).ToList();
            for (int i = 0; i < methods.Count; i++)
            {
                var method = methods[i];
                var methodAttribute = (MethodMetaAttribute)Attribute.GetCustomAttribute(methods[i], typeof(MethodMetaAttribute));
                Button btn = new Button();
                btn.Content = methodAttribute.Name; ;
                parent.Children.Add(btn);
                btn.Click += (obj, subargs) =>
                {
                    method.Invoke(model, new object[] { obj, subargs });
                };
            }

            parent.Children.Add(model.getView(strXaml));

            model.install(strXaml);
        }
    }
    public static void Main(string[] args)
    {
        DevelopWorkspace.Base.Logger.WriteLine("Process called");
        string strXaml = args[0].ToString();
        MainWindow win = new MainWindow(strXaml);
        win.Show();
    }
}