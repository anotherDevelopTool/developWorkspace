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
//using System.Windows.Shapes;
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
    [AddinMeta(Name = "hyddd", Date = "2009-07-20", Description = "单体测试数据生成辅助工具插件")]
    public class ViewModel : DevelopWorkspace.Base.Model.ScriptBaseViewModel
    {

        [MethodMeta(Name = "update", Date = "2009-07-20", Description = "单体测试数据生成辅助工具插件",LargeIcon = "update")]
        public void EventHandler1(object sender, RoutedEventArgs e)
        {

            DevelopWorkspace.Base.Logger.WriteLine("Process called");
        }
        [MethodMeta(Name = "delete", Date = "2009-07-20", Description = "单体测试数据生成辅助工具插件",LargeIcon = "import")]
        public void EventHandler2(object sender, RoutedEventArgs e)
        {

            DevelopWorkspace.Base.Logger.WriteLine("Process called");
        }
        [MethodMeta(Name = "import", Date = "2009-07-20", Description = "单体测试数据生成辅助工具插件",LargeIcon = "export")]
        public void EventHandler3(object sender, RoutedEventArgs e)
        {

            DevelopWorkspace.Base.Logger.WriteLine("Process called");
        }
        [MethodMeta(Name = "export", Date = "2009-07-20", Description = "单体测试数据生成辅助工具插件",LargeIcon = "import1")]
        public void EventHandler4(object sender, RoutedEventArgs e)
        {

            DevelopWorkspace.Base.Logger.WriteLine("Process called");
        }
        [MethodMeta(Name = "setting", Date = "2009-07-20", Description = "单体测试数据生成辅助工具插件",LargeIcon = "import")]
        public void EventHandler5(object sender, RoutedEventArgs e)
        {

            DevelopWorkspace.Base.Logger.WriteLine("Process called");
        }

        //主要画面的控件在这里初始化，提供给各个方法共用
        public override UserControl getView(string strXaml){

            StringReader strreader = new StringReader(strXaml);
            XmlTextReader xmlreader = new XmlTextReader(strreader);
            UserControl view = XamlReader.Load(xmlreader) as UserControl;
            System.Windows.Controls.Button btnCapurure = DevelopWorkspace.Base.Utils.WPF.FindLogicaChild<System.Windows.Controls.Button>(view, "button123");            
            btnCapurure.Click += (obj, subargs) =>
        {
                DevelopWorkspace.Base.Logger.WriteLine("Process called");
                };
            
            return view;
            
            
        }


    }
    //下面的代码是固定代码一般情况下无需修改
    public class MainWindow : Window
    {
        private Label label1;

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
 					btn.Content = methodAttribute.Name;;
                	parent.Children.Add(btn);
                	btn.Click += (obj, subargs) =>
                    {
                		method.Invoke(model, new object[] { obj, subargs });
                		};
                }            
    
            parent.Children.Add(model.getView(strXaml));               
            
            
            model.install(strXaml);
            

        }
       void button1_Click(object sender, RoutedEventArgs e)
        {
            label1.Content = "Hello WPF!";
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