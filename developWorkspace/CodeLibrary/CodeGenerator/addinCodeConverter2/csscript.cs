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
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Antlr4.Runtime.Misc;
using Java.Code;
using System.Linq;
public class Script
{
    //TODO 面向Addin基类化
    [AddinMeta(Name = "dataConvertTools2", Date = "2019-10-14", Description = "代码变换工具")]
    public class ViewModel : DevelopWorkspace.Base.Model.ScriptBaseViewModel
    {
        UserControl view;
        ReoGridControl reogrid = null;
        TreeView codeLibraryTreeView = null;
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
                var dic = getSchemaDictionary(reogrid);

				VelocityDictionary<string, object> Setting = dic["Setting"] as VelocityDictionary<string, object>;
				VelocityDictionary<string, object> tableInfo = dic["TableInfo"] as VelocityDictionary<string, object>;

				VelocityContext vltContext = new VelocityContext();
                vltContext.Put("root", dic);
				StringWriter vltWriter = new StringWriter();
				//
				string keyword = "";
				string sqlMethodName = "";
				var sqlKeys = ((List < VelocityDictionary<string, object> >) tableInfo["Columns"])[0].Keys.Where(key => key.IndexOf(":") > 1);
				bool sqloutput = false;
				foreach (var sqlKey in sqlKeys) {
					var sqlItems = sqlKey.Split(':');
					keyword = sqlItems[0];
					sqlMethodName = sqlItems[1];
					//控制当前生成的SQL
					tableInfo["CurrentSqlKey"] = sqlKey;
					if (keyword.Equals("SELECT") && currentExt.Equals("5.ConvertRule")) {
						// SelectSQL
						vltEngine.Evaluate(vltContext, vltWriter, "", convertRule.Text);
		                DevelopWorkspace.Base.Logger.WriteLine(vltWriter.GetStringBuilder().ToString());
						sqloutput = true;
		                break;
					}
					else if(keyword.Equals("INSERT") && currentExt.Equals("6.ConvertRule")) {
						// InsertSQL
						vltEngine.Evaluate(vltContext, vltWriter, "", convertRule.Text);
		                DevelopWorkspace.Base.Logger.WriteLine(vltWriter.GetStringBuilder().ToString());
						sqloutput = true;
		                break;
					}
					else if(keyword.Equals("UPDATE") && currentExt.Equals("7.ConvertRule")) {
						vltEngine.Evaluate(vltContext, vltWriter, "", convertRule.Text);
		                DevelopWorkspace.Base.Logger.WriteLine(vltWriter.GetStringBuilder().ToString());
						sqloutput = true;
		                break;
					}
					else if(keyword.Equals("DELETE") && currentExt.Equals("8.ConvertRule")) {
						vltEngine.Evaluate(vltContext, vltWriter, "", convertRule.Text);
		                DevelopWorkspace.Base.Logger.WriteLine(vltWriter.GetStringBuilder().ToString());
						sqloutput = true;
		                break;
					}
				}				
				if( !sqloutput ){
					vltEngine.Evaluate(vltContext, vltWriter, "", convertRule.Text);
					DevelopWorkspace.Base.Logger.WriteLine(vltWriter.GetStringBuilder().ToString());
				}
                DevelopWorkspace.Base.Logger.WriteLine("----------------schema information begin-----------------------------", Level.DEBUG);
                DevelopWorkspace.Base.Logger.WriteLine(DevelopWorkspace.Base.Dump.ToDump(dic), Level.DEBUG);
                DevelopWorkspace.Base.Logger.WriteLine("----------------schema information end-------------------------------", Level.DEBUG);
                
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
                var dic = getSchemaDictionary(reogrid);
                DevelopWorkspace.Base.Logger.WriteLine("----------------schema information begin-----------------------------", Level.DEBUG);
                DevelopWorkspace.Base.Logger.WriteLine(DevelopWorkspace.Base.Dump.ToDump(dic), Level.DEBUG);
                DevelopWorkspace.Base.Logger.WriteLine("----------------schema information end-------------------------------", Level.DEBUG);
				
				VelocityDictionary<string, object> Setting = dic["Setting"] as VelocityDictionary<string, object>;
                string project = Setting["Project"].ToString();
                string codeTempBasePath = Setting["CodeTempBasePath"].ToString();
                string codeTempPath = Setting["CodeTempPath"].ToString();
                string resourceTempPath = Setting["ResourceTempPath"].ToString();
                string codeBasePath = Setting["CodeBasePath"].ToString();
				string datasource = "core";
				string classname = "classname";
                string WIN_MERGE_PATH = Setting["winmerger"].ToString();

				VelocityDictionary<string, object> tableInfo = dic["TableInfo"] as VelocityDictionary<string, object>;
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
				
				string entityPath = System.IO.Path.Combine(codeTempPath,"db",datasource,"entity");
				Directory.CreateDirectory(entityPath);

				string DaoPath = System.IO.Path.Combine(codeTempPath,"db",datasource,"dao");
				Directory.CreateDirectory(DaoPath);

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
				modelPath = System.IO.Path.Combine(modelPath,"req");
				Directory.CreateDirectory(modelPath);
				modelPath = System.IO.Path.Combine(modelPath,classname + "ReqModel.java");
                System.IO.File.WriteAllText(modelPath,vltWriter.GetStringBuilder().ToString());

				
				//
				string keyword = "";
				string sqlMethodName = "";
				var sqlKeys = ((List < VelocityDictionary<string, object> >) tableInfo["Columns"])[0].Keys.Where(key => key.IndexOf(":") > 1);
				foreach (var sqlKey in sqlKeys) {
					var sqlItems = sqlKey.Split(':');
					keyword = sqlItems[0];
					sqlMethodName = sqlItems[1];
					vltWriter = new StringWriter();
					//控制当前生成的SQL
					tableInfo["CurrentSqlKey"] = sqlKey;
					if (keyword.Equals("SELECT")) {
						// SelectSQL
						vltEngine.Evaluate(vltContext, vltWriter, "", getResByExt("5.ConvertRule"));
					}
					else if(keyword.Equals("INSERT")) {
						// InsertSQL
						vltEngine.Evaluate(vltContext, vltWriter, "", getResByExt("6.ConvertRule"));
					}
					else if(keyword.Equals("UPDATE")) {
						vltEngine.Evaluate(vltContext, vltWriter, "", getResByExt("7.ConvertRule"));
					}
					else if(keyword.Equals("DELETE")) {
						vltEngine.Evaluate(vltContext, vltWriter, "", getResByExt("8.ConvertRule"));
					}
					crudSqlPath = System.IO.Path.Combine(sqlPath,sqlMethodName +".sql");
					System.IO.File.WriteAllText(crudSqlPath,vltWriter.GetStringBuilder().ToString());
				}				
				
				// DAO
				vltWriter = new StringWriter();
                vltEngine.Evaluate(vltContext, vltWriter, "", getResByExt("9.ConvertRule"));
				DaoPath = System.IO.Path.Combine(DaoPath,classname + "Dao.java");
                System.IO.File.WriteAllText(DaoPath,vltWriter.GetStringBuilder().ToString());

                if (System.IO.File.Exists(WIN_MERGE_PATH)){
					//string WIN_MERGE_PATH = @"C:\Program Files (x86)\WinMerge\WinMergeU.exe";
					string args = "";
					args = @" /r /u /wl /wr /dl ""{0}"" /dr ""{1}"" ""{2}"" ""{3}"" ";
					args = String.Format(args, "generated code", "git", codeTempBasePath, codeBasePath);
					System.Diagnostics.Process.Start(WIN_MERGE_PATH, args);
				}
                else{
                	System.Diagnostics.Process.Start(codeTempBasePath, null);
                }


			}
            catch (Exception ex)
            {
                DevelopWorkspace.Base.Logger.WriteLine(ex.ToString());
            }
        }

        [MethodMeta(Name = "保存", Date = "2009-07-20", Description = "保存", LargeIcon = "save")]
        public void EventHandler4(object sender, RoutedEventArgs e)
        {
            //reogrid.Save(getResPathByExt("xlsx"));
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
            currentExt = "3.ConvertRule";
            convertRule.Text = getResByExt("3.ConvertRule");
            DevelopWorkspace.Base.Logger.WriteLine("Process called");
        }
        [MethodMeta(Name = "model(client)", Date = "2009-07-20", Description = "read", LargeIcon = "template")]
        public void EventHandler8(object sender, RoutedEventArgs e)
        {
            currentExt = "4.ConvertRule";
            convertRule.Text = getResByExt("4.ConvertRule");
            DevelopWorkspace.Base.Logger.WriteLine("Process called");
        }
        [MethodMeta(Name = "SelectSQL", Date = "2009-07-20", Description = "read", LargeIcon = "template")]
        public void EventHandler9(object sender, RoutedEventArgs e)
        {
            currentExt = "5.ConvertRule";
            convertRule.Text = getResByExt("5.ConvertRule");
            DevelopWorkspace.Base.Logger.WriteLine("Process called");
        }
        [MethodMeta(Name = "InsertSQL", Date = "2009-07-20", Description = "read", LargeIcon = "template")]
        public void EventHandler10(object sender, RoutedEventArgs e)
        {
            currentExt = "6.ConvertRule";
            convertRule.Text = getResByExt("6.ConvertRule");
            DevelopWorkspace.Base.Logger.WriteLine("Process called");
        }
        [MethodMeta(Name = "UpdateSQL", Date = "2009-07-20", Description = "read", LargeIcon = "template")]
        public void EventHandler11(object sender, RoutedEventArgs e)
        {
            currentExt = "7.ConvertRule";
            convertRule.Text = getResByExt("7.ConvertRule");
            DevelopWorkspace.Base.Logger.WriteLine("Process called");
        }
        [MethodMeta(Name = "DeleteSQL", Date = "2009-07-20", Description = "read", LargeIcon = "template")]
        public void EventHandler12(object sender, RoutedEventArgs e)
        {
            currentExt = "8.ConvertRule";
            convertRule.Text = getResByExt("8.ConvertRule");
            DevelopWorkspace.Base.Logger.WriteLine("Process called");
        }
        [MethodMeta(Name = "Dao", Date = "2009-07-20", Description = "read", LargeIcon = "template")]
        public void EventHandler13(object sender, RoutedEventArgs e)
        {
            currentExt = "9.ConvertRule";
            convertRule.Text = getResByExt("9.ConvertRule");
            DevelopWorkspace.Base.Logger.WriteLine("Process called");
        }

        [MethodMeta(Name = "projectlist", Category = "junit", Control = "combobox", Init= "getProjectList", Description = "read", LargeIcon = "template")]
        public void EventHandler14(object sender, RoutedEventArgs e)
        {
            DevelopWorkspace.Base.Logger.WriteLine("Process called");
        }
        public List<string> getProjectList()
        {
            return new List<string>() { "rba-bo-api","rba-common","rba-backend-api"};
        }

        [MethodMeta(Name = "junit", Category = "junit", Description = "read", LargeIcon = "template")]
        public void EventHandler15(object sender, RoutedEventArgs e)
        {
            walkDirectoryRecursive(new DirectoryInfo(@"C:\wbc_sam\workspace\wbc-sam\src\main\java\com\water_biz_c\sam\controller"));
            DevelopWorkspace.Base.Logger.WriteLine("Process called");
        }

        public override UserControl getView(string strXaml)
        {
            StringReader strreader = new StringReader(strXaml);
            XmlTextReader xmlreader = new XmlTextReader(strreader);
            view = XamlReader.Load(xmlreader) as UserControl;
 			System.Windows.Forms.Integration.WindowsFormsHost host = DevelopWorkspace.Base.Utils.WPF.FindLogicaChild<System.Windows.Forms.Integration.WindowsFormsHost>(view, "host");
            reogrid = (ReoGridControl)host.Child;
			reogrid.SetSettings(unvell.ReoGrid.WorkbookSettings.View_ShowSheetTabControl, false);
            reogrid.CurrentWorksheet.SetSettings(WorksheetSettings.View_ShowGridLine, false);

            codeLibraryTreeView = DevelopWorkspace.Base.Utils.WPF.FindLogicaChild<TreeView>(view, "codeLibraryTreeView");
            codeLibraryTreeView.SelectedItemChanged += (obj, subargs) =>
            {
            };
            //codeLibraryTreeView.Loaded += (obj, subargs) =>
            //{
            //    codeLibraryTreeView.Items.Clear();
            //    var treeViewItem1 = new TreeViewItem();
            //    treeViewItem1.Header = "controller1";
            //    codeLibraryTreeView.Items.Add(treeViewItem1);

            //    var subTreeViewItem1 = new TreeViewItem();
            //    subTreeViewItem1.Header = "service1";
            //    treeViewItem1.Items.Add(subTreeViewItem1);

            //    var treeViewItem2 = new TreeViewItem();
            //    treeViewItem2.Header = "controller2";
            //    codeLibraryTreeView.Items.Add(treeViewItem2);

            //};


            convertRule = DevelopWorkspace.Base.Utils.WPF.FindLogicaChild<ICSharpCode.AvalonEdit.Edi.EdiTextEditor>(view, "convertRule");
            
            EventHandler6(null,null);
            view.SizeChanged += (obj, subargs) =>
            {
                host.Height = subargs.NewSize.Height;
            };

            string userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
			userName=userName.Substring(userName.IndexOf(@"\") + 1);
			string originString;
			for(int i=4;i<12;i++){
				originString = objectString(reogrid.CurrentWorksheet.Ranges["A1:CV200"].Cells[i,3].Data);
				reogrid.CurrentWorksheet.Ranges["A1:CV200"].Cells[i,3].Data = originString.Replace("os-jiangjiang.xu",userName);
			}
            return view;
        }
        //help方法
		public string objectString(object origin){
			if(origin == null ) return "";
			return origin.ToString();
        }
        //help类，为了在velocity内取值方便
        public class VelocityDictionary<K, V> : Dictionary<K, V> {
            public string getValue(K key) {
                V defaultValue;
                TryGetValue(key, out defaultValue);
                return objectString(defaultValue);
            }
            string objectString(V origin)
            {
                if (origin == null) return "";
                return origin.ToString();
            }
        }
        public VelocityDictionary<string, object> getSchemaDictionary(ReoGridControl reogrid)
        {
            //Directory.CreateDirectory(@"C:\workspace\csharp\WPF Extended DataGrid 2015\1\2");
            var worksheet = reogrid.GetWorksheetByName("sheet1");
            VelocityDictionary<string, object> retDictonary = new VelocityDictionary<string, object>();
            // fill data into worksheet
            var selectedRange = worksheet.Ranges["A1:CV200"];

            for (int row = 0; row < selectedRange.Rows - 1; row++)
            {
                for (int col = 0; col < selectedRange.Cols - 1; col++)
                {
                    if (selectedRange.Cells[row, col].Data != null && selectedRange.Cells[row, col].Data.ToString().EndsWith("{}"))
                    {
                        string nameCellString = selectedRange.Cells[row, col].Data.ToString();
                        VelocityDictionary<string, object>  parent = new VelocityDictionary<string, object>();
                        retDictonary[nameCellString.Substring(0, nameCellString.Length - 2)] = parent;
                        col++;
                        row++;
                        int subRow;
                        for (subRow = row; subRow < selectedRange.Rows - 1; subRow++)
                        {
                            //如果碰到sibling则跳出
                            if (selectedRange.Cells[subRow, col - 1].Data != null && selectedRange.Cells[subRow, col - 1].Data.ToString() != "") break;
                            if (selectedRange.Cells[subRow, col].Data != null)
                            {
                                string keyCellString = selectedRange.Cells[subRow, col].Data.ToString();
                                if (keyCellString.EndsWith("[]"))
                                {
                                    SchemaRange schemmaRange = new SchemaRange
                                    {
                                        parent = parent,
                                        key = keyCellString.Substring(0, keyCellString.Length - 2),
                                        row = subRow,
                                        col = col
                                    };
                                    reverseListObject(selectedRange, schemmaRange);
                                    subRow = schemmaRange.row;
                                }
                                else
                                {
                                    parent[keyCellString] = selectedRange.Cells[subRow, col + 1].Data.ToString();
                                }
                            }
                            else {
                                //进入下一个结构判断
                                break;
                            }
                        }

                        row = subRow;
                        col = 0;

                    }

                }
            }

            return retDictonary;

        }

        private void reverseListObject(ReferenceRange selectedRange, SchemaRange schemmaRange)
        {
            schemmaRange.current = new List<VelocityDictionary<string, object>>();
            if (typeof(IDictionary<string,object>).IsAssignableFrom(schemmaRange.parent.GetType()))
            {

                ((IDictionary<string,object>)schemmaRange.parent)[schemmaRange.key] = schemmaRange.current;
            }
            else
            {
            }

            schemmaRange.col++;
            List<string> schemaList = new List<string>();
            for (int idx = schemmaRange.col; idx < selectedRange.Cols - 1; idx++)
            {
                if (selectedRange.Cells[schemmaRange.row, idx].Data != null)
                {
                    schemaList.Add(selectedRange.Cells[schemmaRange.row, idx].Data.ToString());
                }
            }
            schemmaRange.row++;
            int subRow, subCol;
            for (subRow = schemmaRange.row; subRow < selectedRange.Rows -1 ; subRow++)
            {
                if (selectedRange.Cells[subRow, schemmaRange.col - 1].Data != null && selectedRange.Cells[subRow, schemmaRange.col - 1].Data.ToString() != "") break;
                VelocityDictionary<string, object> column = new VelocityDictionary<string, object>();
                bool isEmptyRow = true;
                for (subCol = schemmaRange.col; subCol < schemmaRange.col + schemaList.Count; subCol++)
                {
                    if (selectedRange.Cells[subRow, subCol].Data != null)
                    {
                        System.Diagnostics.Debug.WriteLine(selectedRange.Cells[subRow, subCol].Data);
                        column[schemaList[subCol - schemmaRange.col]] = selectedRange.Cells[subRow, subCol].Data.ToString();
                        isEmptyRow = false;
                    }
                    else
                    {
                        column[schemaList[subCol - schemmaRange.col]] = "";
                    }

                }
                if (isEmptyRow) break;
                else
                    ((List<VelocityDictionary<string, object>>)schemmaRange.current).Add(column);
            }

            schemmaRange.row = subRow;
            schemmaRange.row--;


        }

        public string aggregateString(IEnumerable<string> listString)
        {
            if (listString.Count() == 0)
            {
                return "";
            }
            return listString.Aggregate((total, next) => total + "\t" + next);
        }

        public void parseSourceFile(string filepath)
        {
            var lines = File.ReadAllText(filepath);
            var results = Java.Code.SourcefileParser.GetJavaClazzInformation(lines);
            foreach (JavaClazz javaClazz in results)
            {

                var clazzViewItem = new TreeViewItem();
                clazzViewItem.Header = javaClazz.clazzName;
                codeLibraryTreeView.Items.Add(clazzViewItem);

                string outputString = "";
                //outputString += javaClazz.clazzName;
                //outputString += "\n";
                //outputString += "annotationList\t" + aggregateString(javaClazz.annotationList.Select(annotation => annotation.qualifiedName));
                //outputString += "\n";
                //outputString += "modifierList\t" + aggregateString(javaClazz.modifierList);
                //outputString += "\n";
                //outputString += "propertyList\t" + aggregateString(javaClazz.propertyList.Select(property => property.propertyName));
                //outputString += "\n";
                //outputString += "methodCallList";
                //outputString += "\n";
                foreach (ClazzMethod clazzMethod in javaClazz.methodList)
                {
                    var clazzMethodViewItem = new TreeViewItem();
                    clazzMethodViewItem.Header = clazzMethod.methodName;
                    clazzViewItem.Items.Add(clazzMethodViewItem);

                    //outputString += "\t" + clazzMethod.methodType;
                    //outputString += "\n";
                    //outputString += "\t" + clazzMethod.methodName;
                    //outputString += "\n";
                    //outputString += "\t\t" + "annotationList\t" + aggregateString(clazzMethod.annotationList.Select(annotation => annotation.qualifiedName));
                    //outputString += "\n";
                    //outputString += "\t\t" + "modifierList\t" + aggregateString(clazzMethod.modifierList);
                    //outputString += "\n";
                    //outputString += "\t\t" + "parametereList\t" + aggregateString(clazzMethod.parametereList.Select(parameter => parameter.parameterName));
                    //outputString += "\n";
                    //outputString += "\t\t" + "localVariableList\t" + aggregateString(clazzMethod.localVariableList.Select(property => property.propertyName));
                    //outputString += "\n";
                    //outputString += "\t\t" + "methodCallList\t" + aggregateString(clazzMethod.methodCallList.Select(methodcall => methodcall.calleeName + "." + methodcall.methodName + "()"));
                    //outputString += "\n";

                }
                //DevelopWorkspace.Base.Logger.WriteLine(outputString);
            }
        }
        void walkDirectoryRecursive(System.IO.DirectoryInfo root)
        {
            System.IO.FileInfo[] files = null;
            System.IO.DirectoryInfo[] subDirs = null;

            // First, process all the files directly under this folder
            try
            {
                files = root.GetFiles("*.java");
            }
            catch (System.IO.DirectoryNotFoundException e)
            {
            }

            if (files != null)
            {
                foreach (System.IO.FileInfo fi in files)
                {
                    Console.WriteLine(fi.FullName);
                    parseSourceFile(fi.FullName);
                }

                // Now find all the subdirectories under this directory.
                subDirs = root.GetDirectories();

                foreach (System.IO.DirectoryInfo dirInfo in subDirs)
                {
                    // Resursive call for each subdirectory.
                    walkDirectoryRecursive(dirInfo);
                }
            }
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
