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
        public void EventHandler1(object sender, RoutedEventArgs e)
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
        [MethodMeta(Name = "文件做成", Date = "2009-07-20", Description = "文件做成", LargeIcon = "convert")]
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
				
				Dictionary<string, object> Setting = dic["Setting"] as Dictionary<string, object>;
                string project = Setting["Project"].ToString();
                string codeTempBasePath = Setting["CodeTempBasePath"].ToString();
                string codeTempPath = Setting["CodeTempPath"].ToString();
                string resourceTempPath = Setting["ResourceTempPath"].ToString();
                string codeBasePath = Setting["CodeBasePath"].ToString();
                string codePath = Setting["CodePath"].ToString();
                string resourcePath = Setting["ResourcePath"].ToString();
				string datasource = "core";
				string classname = "classname";

				DevelopWorkspace.Base.Logger.WriteLine(codeTempPath);
				DevelopWorkspace.Base.Logger.WriteLine(resourceTempPath);

				Dictionary<string, object> tableInfo = dic["TableInfo"] as Dictionary<string, object>;
                if(tableInfo["DataSource"].ToString().EndsWith("core")){
					datasource = "core";
				}
				else if(tableInfo["DataSource"].ToString().EndsWith("front")){
					datasource = "front";
				}
				else if(tableInfo["DataSource"].ToString().EndsWith("ics")){
					datasource = "ics";
				}
				classname = tableInfo["ClassName"].ToString();
				DevelopWorkspace.Base.Logger.WriteLine(classname);
				
				string entityPath = System.IO.Path.Combine(codeTempPath,"db",datasource,"entity");
				Directory.CreateDirectory(entityPath);
				entityPath = System.IO.Path.Combine(entityPath,classname + "Entity.java");
				string sqlPath = System.IO.Path.Combine(resourceTempPath,"db",datasource,"dao",classname + "Dao");
				Directory.CreateDirectory(sqlPath);
				string crudSqlPath = System.IO.Path.Combine(sqlPath,classname + "create.sql");
				
				string modelPath = System.IO.Path.Combine(codeTempPath,"model","biz");
				if(tableInfo.ContainsKey("functionId")){
					modelPath = System.IO.Path.Combine(modelPath,tableInfo["functionId"].ToString());
				}

				
                VelocityContext vltContext = new VelocityContext();
                vltContext.Put("root", dic);

                StringWriter vltWriter = new StringWriter();
				// entity
                vltEngine.Evaluate(vltContext, vltWriter, "", getResByExt("2.ConvertRule"));
                System.IO.File.WriteAllText(entityPath,vltWriter.GetStringBuilder().ToString());
				
				// model
				vltWriter = new StringWriter();
                vltEngine.Evaluate(vltContext, vltWriter, "", getResByExt("3.ConvertRule"));
				modelPath = System.IO.Path.Combine(modelPath,"req",classname + "ReqModel.java");
                System.IO.File.WriteAllText(modelPath,vltWriter.GetStringBuilder().ToString());

				// SelectSQL
				vltWriter = new StringWriter();
                vltEngine.Evaluate(vltContext, vltWriter, "", getResByExt("5.ConvertRule"));
				crudSqlPath = System.IO.Path.Combine(sqlPath,"selectByPrimaryKey.sql");
                System.IO.File.WriteAllText(crudSqlPath,vltWriter.GetStringBuilder().ToString());

				// InsertSQL
				vltWriter = new StringWriter();
                vltEngine.Evaluate(vltContext, vltWriter, "", getResByExt("6.ConvertRule"));
				crudSqlPath = System.IO.Path.Combine(sqlPath,"insert.sql");
                System.IO.File.WriteAllText(crudSqlPath,vltWriter.GetStringBuilder().ToString());

				// UpdateSQL
				vltWriter = new StringWriter();
                vltEngine.Evaluate(vltContext, vltWriter, "", getResByExt("7.ConvertRule"));
				crudSqlPath = System.IO.Path.Combine(sqlPath,"updateByPrimaryKey.sql");
                System.IO.File.WriteAllText(crudSqlPath,vltWriter.GetStringBuilder().ToString());

				// DeleteSQL
				vltWriter = new StringWriter();
                vltEngine.Evaluate(vltContext, vltWriter, "", getResByExt("8.ConvertRule"));
				crudSqlPath = System.IO.Path.Combine(sqlPath,"deleteByPrimaryKey.sql");
                System.IO.File.WriteAllText(crudSqlPath,vltWriter.GetStringBuilder().ToString());
				
				string WIN_MERGE_PATH = @"C:\Program Files (x86)\WinMerge\WinMergeU.exe";
				string args = "";
				args = @" /r /u /wl /wr /dl ""{0}"" /dr ""{1}"" ""{2}"" ""{3}"" ";
				args = String.Format(args, "generated code", "git", codeTempBasePath, codeBasePath);
				System.Diagnostics.Process.Start(WIN_MERGE_PATH, args);


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
        [MethodMeta(Name = "model(client)", Date = "2009-07-20", Description = "read", LargeIcon = "template")]
        public void EventHandler8(object sender, RoutedEventArgs e)
        {
            reogrid.Load(getResPathByExt("xlsx"), unvell.ReoGrid.IO.FileFormat.Excel2007);
            currentExt = "4.ConvertRule";
            convertRule.Text = getResByExt("4.ConvertRule");
            DevelopWorkspace.Base.Logger.WriteLine("Process called");
        }
        [MethodMeta(Name = "SelectSQL", Date = "2009-07-20", Description = "read", LargeIcon = "template")]
        public void EventHandler9(object sender, RoutedEventArgs e)
        {
            reogrid.Load(getResPathByExt("xlsx"), unvell.ReoGrid.IO.FileFormat.Excel2007);
            currentExt = "5.ConvertRule";
            convertRule.Text = getResByExt("5.ConvertRule");
            DevelopWorkspace.Base.Logger.WriteLine("Process called");
        }
        [MethodMeta(Name = "InsertSQL", Date = "2009-07-20", Description = "read", LargeIcon = "template")]
        public void EventHandler10(object sender, RoutedEventArgs e)
        {
            reogrid.Load(getResPathByExt("xlsx"), unvell.ReoGrid.IO.FileFormat.Excel2007);
            currentExt = "6.ConvertRule";
            convertRule.Text = getResByExt("6.ConvertRule");
            DevelopWorkspace.Base.Logger.WriteLine("Process called");
        }
        [MethodMeta(Name = "UpdateSQL", Date = "2009-07-20", Description = "read", LargeIcon = "template")]
        public void EventHandler11(object sender, RoutedEventArgs e)
        {
            reogrid.Load(getResPathByExt("xlsx"), unvell.ReoGrid.IO.FileFormat.Excel2007);
            currentExt = "7.ConvertRule";
            convertRule.Text = getResByExt("7.ConvertRule");
            DevelopWorkspace.Base.Logger.WriteLine("Process called");
        }
        [MethodMeta(Name = "DeleteSQL", Date = "2009-07-20", Description = "read", LargeIcon = "template")]
        public void EventHandler12(object sender, RoutedEventArgs e)
        {
            reogrid.Load(getResPathByExt("xlsx"), unvell.ReoGrid.IO.FileFormat.Excel2007);
            currentExt = "8.ConvertRule";
            convertRule.Text = getResByExt("8.ConvertRule");
            DevelopWorkspace.Base.Logger.WriteLine("Process called");
        }
        public override UserControl getView(string strXaml)
        {
            StringReader strreader = new StringReader(strXaml);
            XmlTextReader xmlreader = new XmlTextReader(strreader);
            view = XamlReader.Load(xmlreader) as UserControl;
            reogrid = DevelopWorkspace.Base.Utils.WPF.FindLogicaChild<ReoGridControl>(view, "grid");
            reogrid.SetSettings(unvell.ReoGrid.WorkbookSettings.View_ShowSheetTabControl, false);
            reogrid.CurrentWorksheet.SetSettings(WorksheetSettings.View_ShowGridLine, false);

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
