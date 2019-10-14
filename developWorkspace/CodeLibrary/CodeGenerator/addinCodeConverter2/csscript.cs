using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using Antlr4.Runtime;
using Java.Code;
using DevelopWorkspace.Base;
using DevelopWorkspace.Base.Codec;
using Heidesoft.Components.Controls;
using ICSharpCode.AvalonEdit.Highlighting;
using Microsoft.CSharp;
using Microsoft.VisualBasic.FileIO;
using Microsoft.VisualBasic;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using NVelocity.App;
using NVelocity.Runtime;
using NVelocity;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Text;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Threading;
using System.Windows;
using System.Xml.Linq;
using System.Xml.Xsl;
using System.Xml;
using System;
using Xceed.Wpf.AvalonDock.Layout;
using unvell.ReoGrid;
using unvell.ReoGrid.CellTypes;
using unvell.ReoGrid.Chart;
using unvell.ReoGrid.Drawing.Shapes;
public class Script
{
    //TODO 面向Addin基类化
    [AddinMeta(Name = "dataConvertTools2", Date = "2019-10-14", Description = "代码变换工具")]
    public class ViewModel : DevelopWorkspace.Base.Model.ScriptBaseViewModel
    {
        UserControl view;
        ReoGridControl reogrid = null;
        ICSharpCode.AvalonEdit.Edi.EdiTextEditor convertRule;

        string currentExt = "ConvertRule";

        [MethodMeta(Name = "变换", Date = "2009-07-20", Description = "变换", LargeIcon = "convert")]
        public void EventHandler2(object sender, RoutedEventArgs e)
        {
            try
            {
                VelocityEngine vltEngine = new VelocityEngine();
                vltEngine.Init();

                //CSV format with tab delimiter
                var dic = DevelopWorkspace.Base.Codec.CodeSupport.getSchemaDictionary(reogrid);
                DevelopWorkspace.Base.Logger.WriteLine("----------------schema information begin-----------------------------", Level.DEBUG);
                DevelopWorkspace.Base.Logger.WriteLine(DevelopWorkspace.Base.Dump.ToDump(dic), Level.DEBUG);
                DevelopWorkspace.Base.Logger.WriteLine("----------------schema information end-------------------------------", Level.DEBUG);


                //string projectPath = System.IO.Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "rba-bo-api");
                //Directory.CreateDirectory(@"C:\workspace\csharp\WPF Extended DataGrid 2015\1\2");
                //System.IO.File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "addins", classAttribute.Name + "." + ext), strXaml);

                VelocityContext vltContext = new VelocityContext();
                vltContext.Put("root", dic);

                StringWriter vltWriter = new StringWriter();
                vltEngine.Evaluate(vltContext, vltWriter, "", convertRule.Text);
                DevelopWorkspace.Base.Logger.WriteLine(vltWriter.GetStringBuilder().ToString());
            }
            catch (Exception ex)
            {
                DevelopWorkspace.Base.Logger.WriteLine(ex.ToString());
            }
        }

        [MethodMeta(Name = "保存", Date = "2009-07-20", Description = "保存", LargeIcon = "save")]
        public void EventHandler4(object sender, RoutedEventArgs e)
        {
            reogrid.Save(getResPathByExt("xlsx"));
            saveResByExt(convertRule.Text, currentExt);

            DevelopWorkspace.Base.Logger.WriteLine("Process called");
        }
        [MethodMeta(Name = "elastic mapping", Date = "2009-07-20", Description = "read", LargeIcon = "template")]
        public void EventHandler5(object sender, RoutedEventArgs e)
        {
            reogrid.Load(getResPathByExt("xlsx"), unvell.ReoGrid.IO.FileFormat.Excel2007);
            currentExt = "ConvertRule";
            convertRule.Text = getResByExt("ConvertRule");
            DevelopWorkspace.Base.Logger.WriteLine("Process called");
        }
        [MethodMeta(Name = "entity", Date = "2009-07-20", Description = "read", LargeIcon = "template")]
        public void EventHandler6(object sender, RoutedEventArgs e)
        {
            reogrid.Load(getResPathByExt("xlsx"), unvell.ReoGrid.IO.FileFormat.Excel2007);
            currentExt = "2.ConvertRule";
            convertRule.Text = getResByExt("2.ConvertRule");
            DevelopWorkspace.Base.Logger.WriteLine("Process called");
        }
        [MethodMeta(Name = "model", Date = "2009-07-20", Description = "read", LargeIcon = "template")]
        public void EventHandler7(object sender, RoutedEventArgs e)
        {
            reogrid.Load(getResPathByExt("xlsx"), unvell.ReoGrid.IO.FileFormat.Excel2007);
            currentExt = "3.ConvertRule";
            convertRule.Text = getResByExt("3.ConvertRule");
            DevelopWorkspace.Base.Logger.WriteLine("Process called");
        }
        [MethodMeta(Name = "builder", Date = "2009-07-20", Description = "read", LargeIcon = "template")]
        public void EventHandler8(object sender, RoutedEventArgs e)
        {
            reogrid.Load(getResPathByExt("xlsx"), unvell.ReoGrid.IO.FileFormat.Excel2007);
            currentExt = "4.ConvertRule";
            convertRule.Text = getResByExt("4.ConvertRule");
            DevelopWorkspace.Base.Logger.WriteLine("Process called");
        }
        [MethodMeta(Name = "TableSchemeCsv", Date = "2009-07-20", Description = "read", LargeIcon = "template")]
        public void EventHandler9(object sender, RoutedEventArgs e)
        {
            reogrid.Load(getResPathByExt("xlsx"), unvell.ReoGrid.IO.FileFormat.Excel2007);
            currentExt = "9.ConvertRule";
            convertRule.Text = getResByExt("9.ConvertRule");
            DevelopWorkspace.Base.Logger.WriteLine("Process called");
        }
        public override UserControl getView(string strXaml)
        {
            StringReader strreader = new StringReader(strXaml);
            XmlTextReader xmlreader = new XmlTextReader(strreader);
            view = XamlReader.Load(xmlreader) as UserControl;
            reogrid = DevelopWorkspace.Base.Utils.WPF.FindLogicaChild<ReoGridControl>(view, "grid");
            reogrid.SetSettings(unvell.ReoGrid.WorkbookSettings.View_ShowSheetTabControl, false);
            convertRule = DevelopWorkspace.Base.Utils.WPF.FindLogicaChild<ICSharpCode.AvalonEdit.Edi.EdiTextEditor>(view, "convertRule");
            convertRule.Text = getResByExt("ConvertRule");
            return view;
        }



    }

    public class MainWindow : Window
    {
        private Label label1;

        public MainWindow(string strXaml)
        {
            Width = 1000;
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
