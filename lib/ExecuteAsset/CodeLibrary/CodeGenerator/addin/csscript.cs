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
using Fluent;
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

        [MethodMeta(Name = "update", Date = "2009-07-20", Description = "单体测试数据生成辅助工具插件")]
        public void EventHandler1(object sender, RoutedEventArgs e)
        {

            DevelopWorkspace.Base.Logger.WriteLine("Process called");
        }
        [MethodMeta(Name = "update", Date = "2009-07-20", Description = "单体测试数据生成辅助工具插件")]
        public void EventHandler2(object sender, RoutedEventArgs e)
        {

            DevelopWorkspace.Base.Logger.WriteLine("Process called");
        }
        [MethodMeta(Name = "update", Date = "2009-07-20", Description = "单体测试数据生成辅助工具插件")]
        public void EventHandler3(object sender, RoutedEventArgs e)
        {

            DevelopWorkspace.Base.Logger.WriteLine("Process called");
        }
        [MethodMeta(Name = "update", Date = "2009-07-20", Description = "单体测试数据生成辅助工具插件")]
        public void EventHandler4(object sender, RoutedEventArgs e)
        {

            DevelopWorkspace.Base.Logger.WriteLine("Process called");
        }
        [MethodMeta(Name = "update", Date = "2009-07-20", Description = "单体测试数据生成辅助工具插件")]
        public void EventHandler5(object sender, RoutedEventArgs e)
        {

            DevelopWorkspace.Base.Logger.WriteLine("Process called");
        }


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
  
    public class MainWindow : RibbonWindow
    {
        private Label label1;

        public MainWindow(string strXaml)
        {
            Width = 300;
            Height = 300;

            Grid grid = new Grid();
            Content = grid;


//            var wsettings = new System.Xaml.XamlObjectWriterSettings ();
//            wsettings.RootObjectInstance = new Script.UserControl1();
            ViewModel model = new ViewModel();
            grid.Children.Add(model.getView(strXaml));   
            
            
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