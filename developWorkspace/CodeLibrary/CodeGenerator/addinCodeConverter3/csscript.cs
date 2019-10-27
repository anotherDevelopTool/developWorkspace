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

//css_reference unvell.ReoGridEditor.exe;
public class Script
{
    //TODO 面向Addin基类化
    [AddinMeta(Name = "dataConvertTools", Date = "2019-10-14", Description = "代码变换工具")]
    public class ViewModel : DevelopWorkspace.Base.Model.ScriptBaseViewModel
    {
        UserControl view;
        ReoGridControl reogrid = null;
        ICSharpCode.AvalonEdit.Edi.EdiTextEditor convertRule;
        TreeView codeLibraryTreeView = null;
        System.Windows.Forms.Integration.WindowsFormsHost host;
        string currentExt = "ConvertRule";
        string CodeBasePath = "";
        bool filterController;
        bool filterService;
        bool filterLogic;
        bool filterDao;
        JavaProject javaProject = new JavaProject();
        List<JavaClazz> parsedClazzList = new List<JavaClazz>();

        [MethodMeta(Name = "变换", Date = "2009-07-20", Description = "变换", LargeIcon = "convert")]
        public void EventHandler1(object sender, RoutedEventArgs e)
        {
            try
            {
                host.Visibility = System.Windows.Visibility.Hidden;

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
                var sqlKeys = ((List<VelocityDictionary<string, object>>)tableInfo["Columns"])[0].Keys.Where(key => key.IndexOf(":") > 1);
                bool sqloutput = false;
                foreach (var sqlKey in sqlKeys)
                {
                    var sqlItems = sqlKey.Split(':');
                    keyword = sqlItems[0];
                    sqlMethodName = sqlItems[1];
                    //控制当前生成的SQL
                    tableInfo["CurrentSqlKey"] = sqlKey;
                    if (keyword.Equals("SELECT") && currentExt.Equals("5.ConvertRule"))
                    {
                        // SelectSQL
                        vltEngine.Evaluate(vltContext, vltWriter, "", convertRule.Text);
                        DevelopWorkspace.Base.Logger.WriteLine(vltWriter.GetStringBuilder().ToString());
                        sqloutput = true;
                        break;
                    }
                    else if (keyword.Equals("INSERT") && currentExt.Equals("6.ConvertRule"))
                    {
                        // InsertSQL
                        vltEngine.Evaluate(vltContext, vltWriter, "", convertRule.Text);
                        DevelopWorkspace.Base.Logger.WriteLine(vltWriter.GetStringBuilder().ToString());
                        sqloutput = true;
                        break;
                    }
                    else if (keyword.Equals("UPDATE") && currentExt.Equals("7.ConvertRule"))
                    {
                        vltEngine.Evaluate(vltContext, vltWriter, "", convertRule.Text);
                        DevelopWorkspace.Base.Logger.WriteLine(vltWriter.GetStringBuilder().ToString());
                        sqloutput = true;
                        break;
                    }
                    else if (keyword.Equals("DELETE") && currentExt.Equals("8.ConvertRule"))
                    {
                        vltEngine.Evaluate(vltContext, vltWriter, "", convertRule.Text);
                        DevelopWorkspace.Base.Logger.WriteLine(vltWriter.GetStringBuilder().ToString());
                        sqloutput = true;
                        break;
                    }
                }
                if (!sqloutput)
                {
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
            finally {
                host.Visibility = System.Windows.Visibility.Visible;
            }
        }
        [MethodMeta(Name = "文件做成", Date = "2009-07-20", Description = "文件做成", LargeIcon = "convert")]
        public void EventHandler2(object sender, RoutedEventArgs e)
        {
            try
            {
                host.Visibility = System.Windows.Visibility.Hidden;

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
                string codeTempPath = Path.Combine(codeTempBasePath, Setting["CodeTempPath"].ToString());
                string resourceTempPath = Path.Combine(codeTempBasePath, Setting["ResourceTempPath"].ToString());
                string codeBasePath = Setting["CodeBasePath"].ToString();
                string datasource = "core";
                string classname = "classname";
                string WIN_MERGE_PATH = Setting["winmerger"].ToString();

                VelocityDictionary<string, object> tableInfo = dic["TableInfo"] as VelocityDictionary<string, object>;
                if (tableInfo["DataSource"].ToString().EndsWith("core"))
                {
                    datasource = "core";
                }
                else if (tableInfo["DataSource"].ToString().EndsWith("front"))
                {
                    datasource = "front";
                }
                else if (tableInfo["DataSource"].ToString().EndsWith("ics"))
                {
                    datasource = "ics";
                }
                classname = tableInfo["ClassName"].ToString();

                string entityPath = System.IO.Path.Combine(codeTempPath, "db", datasource, "entity");
                Directory.CreateDirectory(entityPath);

                string DaoPath = System.IO.Path.Combine(codeTempPath, "db", datasource, "dao");
                Directory.CreateDirectory(DaoPath);

                entityPath = System.IO.Path.Combine(entityPath, classname + "Entity.java");
                string sqlPath = System.IO.Path.Combine(resourceTempPath, "db", datasource, "dao", classname + "Dao");
                Directory.CreateDirectory(sqlPath);
                string crudSqlPath = System.IO.Path.Combine(sqlPath, classname + "create.sql");

                string modelPath = System.IO.Path.Combine(codeTempPath, "model", "biz");
                if (tableInfo.ContainsKey("functionId"))
                {
                    modelPath = System.IO.Path.Combine(modelPath, tableInfo["functionId"].ToString());
                }

                VelocityContext vltContext = new VelocityContext();
                vltContext.Put("root", dic);

                StringWriter vltWriter = new StringWriter();
                // entity
                vltEngine.Evaluate(vltContext, vltWriter, "", getResByExt("2.ConvertRule"));
                System.IO.File.WriteAllText(entityPath, vltWriter.GetStringBuilder().ToString());

                // model
                vltWriter = new StringWriter();
                vltEngine.Evaluate(vltContext, vltWriter, "", getResByExt("3.ConvertRule"));
                modelPath = System.IO.Path.Combine(modelPath, "req");
                Directory.CreateDirectory(modelPath);
                modelPath = System.IO.Path.Combine(modelPath, classname + "ReqModel.java");
                System.IO.File.WriteAllText(modelPath, vltWriter.GetStringBuilder().ToString());


                //
                string keyword = "";
                string sqlMethodName = "";
                var sqlKeys = ((List<VelocityDictionary<string, object>>)tableInfo["Columns"])[0].Keys.Where(key => key.IndexOf(":") > 1);
                foreach (var sqlKey in sqlKeys)
                {
                    var sqlItems = sqlKey.Split(':');
                    keyword = sqlItems[0];
                    sqlMethodName = sqlItems[1];
                    vltWriter = new StringWriter();
                    //控制当前生成的SQL
                    tableInfo["CurrentSqlKey"] = sqlKey;
                    if (keyword.Equals("SELECT"))
                    {
                        // SelectSQL
                        vltEngine.Evaluate(vltContext, vltWriter, "", getResByExt("5.ConvertRule"));
                    }
                    else if (keyword.Equals("INSERT"))
                    {
                        // InsertSQL
                        vltEngine.Evaluate(vltContext, vltWriter, "", getResByExt("6.ConvertRule"));
                    }
                    else if (keyword.Equals("UPDATE"))
                    {
                        vltEngine.Evaluate(vltContext, vltWriter, "", getResByExt("7.ConvertRule"));
                    }
                    else if (keyword.Equals("DELETE"))
                    {
                        vltEngine.Evaluate(vltContext, vltWriter, "", getResByExt("8.ConvertRule"));
                    }
                    crudSqlPath = System.IO.Path.Combine(sqlPath, sqlMethodName + ".sql");
                    System.IO.File.WriteAllText(crudSqlPath, vltWriter.GetStringBuilder().ToString());
                }

                // DAO
                vltWriter = new StringWriter();
                vltEngine.Evaluate(vltContext, vltWriter, "", getResByExt("9.ConvertRule"));
                DaoPath = System.IO.Path.Combine(DaoPath, classname + "Dao.java");
                System.IO.File.WriteAllText(DaoPath, vltWriter.GetStringBuilder().ToString());

                if (System.IO.File.Exists(WIN_MERGE_PATH))
                {
                    //string WIN_MERGE_PATH = @"C:\Program Files (x86)\WinMerge\WinMergeU.exe";
                    string args = "";
                    args = @" /r /u /wl /wr /dl ""{0}"" /dr ""{1}"" ""{2}"" ""{3}"" ";
                    args = String.Format(args, "generated code", "git", codeTempBasePath, codeBasePath);
                    System.Diagnostics.Process.Start(WIN_MERGE_PATH, args);
                }
                else
                {
                    System.Diagnostics.Process.Start(codeTempBasePath, null);
                }


            }
            catch (Exception ex)
            {
                DevelopWorkspace.Base.Logger.WriteLine(ex.ToString());
            }
            finally
            {
                host.Visibility = System.Windows.Visibility.Visible;
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
        [MethodMeta(Name = "项目信息取得", Category = "junit", Description = "read", LargeIcon = "project")]
        public void EventHandler15(object sender, RoutedEventArgs e)
        {
            try
            {
                host.Visibility = System.Windows.Visibility.Hidden;
                javaProject.javaClazzList.Clear();
                walkDirectoryRecursive(new DirectoryInfo(CodeBasePath));
                DevelopWorkspace.Base.Logger.WriteLine("Process called");
            }
            catch (Exception ex)
            {
                DevelopWorkspace.Base.Logger.WriteLine(ex.ToString());
            }
            finally
            {
                host.Visibility = System.Windows.Visibility.Visible;
            }
        }

        [MethodMeta(Name = "变换", Category = "junit", Date = "2009-07-20", Description = "变换", LargeIcon = "convert")]
        public void EventHandler16(object sender, RoutedEventArgs e)
        {
            try
            {
                host.Visibility = System.Windows.Visibility.Hidden;

                var selectedItem = codeLibraryTreeView.SelectedItem as TreeViewItem;
                if (selectedItem != null)
                {
                    var selectedClazz = javaProject.javaClazzList.Where(clazz => clazz.clazzName.EndsWith(selectedItem.Header.ToString())).FirstOrDefault();
                    DevelopWorkspace.Base.Logger.WriteLine("----------------schema information begin-----------------------------", Level.DEBUG);
                    DevelopWorkspace.Base.Logger.WriteLine(DevelopWorkspace.Base.Dump.ToDump(selectedClazz), Level.DEBUG);
                    DevelopWorkspace.Base.Logger.WriteLine("----------------schema information end-------------------------------", Level.DEBUG);

                    VelocityEngine vltEngine = new VelocityEngine();
                    vltEngine.Init();

                    VelocityContext vltContext = new VelocityContext();
                    vltContext.Put("project", javaProject);
                    vltContext.Put("targetClazz", selectedClazz);
                    StringWriter vltWriter = new StringWriter();

                    vltEngine.Evaluate(vltContext, vltWriter, "", convertRule.Text);
                    DevelopWorkspace.Base.Logger.WriteLine(vltWriter.GetStringBuilder().ToString());
                }
            }
            catch (Exception ex)
            {
                DevelopWorkspace.Base.Logger.WriteLine(ex.ToString());
            }
            finally
            {
                host.Visibility = System.Windows.Visibility.Visible;
            }
        }

        [MethodMeta(Name = "TestCase", Category = "junit", Description = "read", LargeIcon = "template")]
        public void EventHandler17(object sender, RoutedEventArgs e)
        {
            currentExt = "17.ConvertRule";
            convertRule.Text = getResByExt("17.ConvertRule");
            DevelopWorkspace.Base.Logger.WriteLine("Process called");
        }
        [MethodMeta(Name = "Junit格式的Excel变换", Category = "junit", Date = "2009-07-20", Description = "read", LargeIcon = "junit")]
        public void EventHandler18(object sender, RoutedEventArgs e)
        {
            var data = DevelopWorkspace.Base.Excel.GetDataWithSchemaFromActivedSheet();
            if (data != null) DevelopWorkspace.Base.Excel.DrawDataWithSchemaToExcel(data);
            DevelopWorkspace.Base.Logger.WriteLine("Process called");
        }

        public override UserControl getView(string strXaml)
        {
            StringReader strreader = new StringReader(strXaml);
            XmlTextReader xmlreader = new XmlTextReader(strreader);
            view = XamlReader.Load(xmlreader) as UserControl;
            host = DevelopWorkspace.Base.Utils.WPF.FindLogicaChild<System.Windows.Forms.Integration.WindowsFormsHost>(view, "host");
            reogrid = ((unvell.ReoGrid.Editor.ReoGridEditor)host.Child).GridControl;

            ((unvell.ReoGrid.Editor.ReoGridEditor)host.Child).FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            foreach (var control in (((unvell.ReoGrid.Editor.ReoGridEditor)host.Child).Controls))
            {
                if (control is System.Windows.Forms.MenuStrip)
                {
                    ((System.Windows.Forms.MenuStrip)control).Visible = false;
                }
                if (control is System.Windows.Forms.StatusStrip)
                {
                    ((System.Windows.Forms.StatusStrip)control).Visible = false;
                }
            }
            reogrid.SetSettings(unvell.ReoGrid.WorkbookSettings.View_ShowSheetTabControl, false);
            reogrid.CurrentWorksheet.SetSettings(WorksheetSettings.View_ShowGridLine, false);

            codeLibraryTreeView = DevelopWorkspace.Base.Utils.WPF.FindLogicaChild<TreeView>(view, "codeLibraryTreeView");
            codeLibraryTreeView.SelectedItemChanged += (obj, subargs) =>
            {
            };


            convertRule = DevelopWorkspace.Base.Utils.WPF.FindLogicaChild<ICSharpCode.AvalonEdit.Edi.EdiTextEditor>(view, "convertRule");

            EventHandler6(null, null);
            view.SizeChanged += (obj, subargs) =>
            {
                host.Height = subargs.NewSize.Height;
            };

            string userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            userName = userName.Substring(userName.IndexOf(@"\") + 1);

            var projectDropDown = new DropdownListCell();
            reogrid.CurrentWorksheet[3, 3] = projectDropDown;
            reogrid.CurrentWorksheet[4, 7] = new object[] { new CheckBoxCell(), "Controller" };
            reogrid.CurrentWorksheet[5, 7] = new object[] { new CheckBoxCell(), "Service" };
            reogrid.CurrentWorksheet[6, 7] = new object[] { new CheckBoxCell(), "Logic" };
            reogrid.CurrentWorksheet[7, 7] = new object[] { new CheckBoxCell(), "Dao" };

            reogrid.CurrentWorksheet.CellDataChanged += (s, args) =>
            {
                //base folder change...
                if (args.Cell.Position == new CellPosition(1, 3))
                {
                    if (System.IO.Directory.Exists(args.Cell.Data.ToString()))
                    {
                        string defaultProject = "";
                        projectDropDown.Items.Clear();
                        List<string> projectList = new List<string>();
                        var projectDirs = new DirectoryInfo(args.Cell.Data.ToString()).GetDirectories();
                        foreach (System.IO.DirectoryInfo dirInfo in projectDirs)
                        {
                            projectList.Add(dirInfo.Name);
                            defaultProject = dirInfo.Name;

                        }
                        projectDropDown.Items.AddRange(projectList.ToArray());
                        reogrid.CurrentWorksheet[3, 3] = defaultProject;
                    }

                }
                //project change
                else if (args.Cell.Position == new CellPosition(3, 3))
                {
                    string CodeTempBasePath = objectString(reogrid.CurrentWorksheet[1, 3]);
                    CodeTempBasePath = CodeTempBasePath.Substring(0, CodeTempBasePath.LastIndexOf(@"\"));
                    CodeTempBasePath = System.IO.Path.Combine(CodeTempBasePath, "code", objectString(reogrid.CurrentWorksheet[3, 3]), "src", "main");
                    string CodeTempPath = System.IO.Path.Combine("java", @"jp\co\rakuten\brandavenue\backend\bo\api");
                    string ResourceTempPath = System.IO.Path.Combine("resources", @"jp\co\rakuten\brandavenue\backend\bo\api");

                    CodeBasePath = System.IO.Path.Combine(objectString(reogrid.CurrentWorksheet[1, 3]), objectString(reogrid.CurrentWorksheet[3, 3]), "src", "main");
                    string RootPackage = "jp.co.rakuten.brandavenue.backend.bo.api";
                    reogrid.CurrentWorksheet[4, 3] = CodeTempBasePath;
                    reogrid.CurrentWorksheet[5, 3] = CodeTempPath;
                    reogrid.CurrentWorksheet[6, 3] = ResourceTempPath;
                    reogrid.CurrentWorksheet[7, 3] = CodeBasePath;
                    reogrid.CurrentWorksheet[8, 3] = RootPackage;
                }
                else if (args.Cell.Position == new CellPosition(4, 7) || 
                         args.Cell.Position == new CellPosition(5, 7) || 
                         args.Cell.Position == new CellPosition(6, 7) || 
                         args.Cell.Position == new CellPosition(7, 7))
                {
                    filterController = (args.Cell.Worksheet[4, 7] as bool?) ?? false;
                    filterService = (args.Cell.Worksheet[5,7] as bool?) ?? false;
                    filterLogic = (args.Cell.Worksheet[6,7] as bool?) ?? false;
                    filterDao = (args.Cell.Worksheet[7,7] as bool?) ?? false;
                }
            };
            string favoriteRootPath = System.IO.Path.Combine(@"D:\Users\", userName, "git");
            string rootPath = objectString(reogrid.CurrentWorksheet[1, 3]);
            if (string.IsNullOrEmpty(rootPath))
            {
                reogrid.CurrentWorksheet[1, 3] = favoriteRootPath;
            }
            else {
                reogrid.CurrentWorksheet[1, 3] = rootPath;
            }




            return view;
        }
        //help方法
        public string objectString(object origin)
        {
            if (origin == null) return "";
            return origin.ToString();
        }
        //help类，为了在velocity内取值方便
        public class VelocityDictionary<K, V> : Dictionary<K, V>
        {
            public string getValue(K key)
            {
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
                        VelocityDictionary<string, object> parent = new VelocityDictionary<string, object>();
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
                            else
                            {
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
            if (typeof(IDictionary<string, object>).IsAssignableFrom(schemmaRange.parent.GetType()))
            {

                ((IDictionary<string, object>)schemmaRange.parent)[schemmaRange.key] = schemmaRange.current;
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
            for (subRow = schemmaRange.row; subRow < selectedRange.Rows - 1; subRow++)
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
            string javafile = filepath.Substring(filepath.LastIndexOf(@"\") + 1);
            Services.BusyWorkIndicatorService(string.Format("{0}:{1}", "parser", javafile));
            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate { }));

            var lines = File.ReadAllText(filepath);
            var results = Java.Code.SourcefileParser.GetJavaClazzInformation(lines);
            //对Methodcall的类型进行复原处理
            foreach (JavaClazz javaClazz in results)
            {
                javaClazz.filePath = filepath;
                foreach (ClazzMethod clazzMethod in javaClazz.methodList)
                {
                    List<MethodCall> realMethodCallList = new List<MethodCall>();

                    foreach (MethodCall methodCall in clazzMethod.methodCallList)
                    {
                        string realCalleeType = "";
                        if ("this".Equals(methodCall.calleeName))
                        {
                            realCalleeType = javaClazz.clazzName;
                        }
                        else
                        {
                            var localvariable = clazzMethod.localVariableList.Where(sourceProperty => sourceProperty.propertyName.Equals(methodCall.calleeName)).FirstOrDefault();
                            if (localvariable == null)
                            {
                                var callparameter = clazzMethod.parametereList.Where(parameter => parameter.parameterName.Equals(methodCall.calleeName)).FirstOrDefault();
                                if (callparameter == null)
                                {
                                    var property = javaClazz.propertyList.Where(sourceProperty => sourceProperty.propertyName.Equals(methodCall.calleeName)).FirstOrDefault();
                                    if (property != null)
                                    {
                                        realCalleeType = property.propertyType;
                                    }
                                }
                                else
                                {
                                    realCalleeType = callparameter.pararameterType;
                                }
                            }
                            else
                            {
                                realCalleeType = localvariable.propertyType;
                            }
                        }
                        if (!string.IsNullOrEmpty(realCalleeType))
                        {
                            realMethodCallList.Add(new MethodCall() { calleeName = realCalleeType, methodName = methodCall.methodName });
                        }
                    }
                    //替换
                    clazzMethod.methodCallList = realMethodCallList;

                }
                //所有的类信息缓存
                javaProject.javaClazzList.Add(javaClazz);
            }
            //提示给使用者的类别
            foreach (JavaClazz javaClazz in results)
            {
                if (filterController && javaClazz.clazzName.EndsWith("Controller") ||
                    filterService && javaClazz.clazzName.EndsWith("Service") ||
                    filterLogic && javaClazz.clazzName.EndsWith("Logic") ||
                    filterDao && javaClazz.clazzName.EndsWith("Dao"))
                {
                    var clazzViewItem = new TreeViewItem();
                    clazzViewItem.Header = javaClazz.clazzName;
                    codeLibraryTreeView.Items.Add(clazzViewItem);

                    foreach (ClazzMethod clazzMethod in javaClazz.methodList)
                    {
                        var clazzMethodViewItem = new TreeViewItem();
                        clazzMethodViewItem.Header = clazzMethod.methodName;
                        clazzViewItem.Items.Add(clazzMethodViewItem);
                    }
                }
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
    public class JavaProject
    {
        public JavaClazz findJavaClazzByName(string classname)
        {
            JavaClazz defaultClazz = new JavaClazz() { clazzName = "dummyclazz" };
            var findedClazz = javaClazzList.FirstOrDefault(clazz => clazz.clazzName.Equals(classname));
            if (findedClazz != null) return findedClazz;
            return defaultClazz;
        }
        public ClazzMethod findClazzMethodByName(string classname, string methodname)
        {
            ClazzMethod defaultClazzMethod = new ClazzMethod() { methodName = "dummymethod" };
            var findedClazz = javaClazzList.FirstOrDefault(clazz => clazz.clazzName.Equals(classname));
            if (findedClazz == null) return defaultClazzMethod;
            var findedClazzMethod = findedClazz.methodList.FirstOrDefault(method => method.methodName.Equals(methodname));
            if (findedClazzMethod == null) return defaultClazzMethod;
            return findedClazzMethod;
        }
        List<JavaClazz> _javaClazzList = new List<JavaClazz>();
        public List<JavaClazz> javaClazzList 
        { 
            get
            {
                return _javaClazzList;
            }
            set
            {
                _javaClazzList = value;
            }
        }
    }
}