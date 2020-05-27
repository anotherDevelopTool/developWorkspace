using Microsoft.CSharp;
using System.Drawing;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
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
using DevelopWorkspace.Base;
using DevelopWorkspace.Main;
using System.Windows.Threading;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Diagnostics;
using System.Threading;
using Excel = Microsoft.Office.Interop.Excel;
using System.Reflection;
using System.Text;
using System;
using RazorEngine;
using RazorEngine.Templating; // For extension methods.
using System.Security.Cryptography;
using SqlParser = DevelopWorkspace.Base.Utils.SqlParserWrapper;
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
public class Script
{
    static FileWatcher fileWatcher;
    static IRazorEngineService service;
    public static void Main(string[] args)
    {
        ContextMenuCommand selectCommand = new ContextMenuCommand("code genetator...", "対象テーブルの情報をコードジェネレーターにエクスポートします。", "code_contextmenu",
                        (p) =>
                        {

                            BackgroundWorker backgroundWorker = new BackgroundWorker();
                            backgroundWorker.DoWork += new DoWorkEventHandler((s, ev) =>
                            {
                                try
                                {
                                    DevelopWorkspace.Main.TableInfo tableinfo = p as DevelopWorkspace.Main.TableInfo;
                                    if (tableinfo != null)
                                    {
                                        string excelfilePath = Path.Combine(StartupSetting.instance.homeDir, "addins", "dataConvertTools.xlsm");
                                        string eventfilePath = Path.Combine(StartupSetting.instance.homeDir, "addins", "dataConvertTools.eventfile");
                                        XlApp.activateNamedSheet(excelfilePath, "viewer");

                                        if (fileWatcher == null) fileWatcher = new FileWatcher(eventfilePath, (filename => { ReadFile(filename); }));

                                        if (service == null)
                                        {
                                            var config = new RazorEngine.Configuration.TemplateServiceConfiguration();
                                            //config.Debug = true;
                                            config.DisableTempFileLocking = true;
                                            config.EncodedStringFactory = new RazorEngine.Text.RawStringFactory(); // Raw string encoding.
                                            service = RazorEngineService.Create(config);
                                        }

                                        List<List<string>> headerNestedList = new List<List<string>>();
                                        List<string> headerList = new List<string>();
                                        headerList.Add(tableinfo.XLAppRef.ConnectionHistory.ConnectionHistoryName);
                                        headerList.Add(tableinfo.TableName);
                                        headerNestedList.Add(headerList);
                                        headerNestedList.exportToActiveSheetOfExcel(headerHeight: 0, schemaHeight: 0, _startRow: 9, _startCol: 2, _isOverwritten: true, _isFormatted: false);
                                        string[,] data = tableinfo.getTableDataWithSchema();

                                        if (data != null)
                                        {
                                            List<List<string>> rowList = data.ToNestedList();

                                            if (rowList.Count > 7)
                                            {
                                                rowList.RemoveRange(7, rowList.Count - 7);
                                            }
                                            List<List<string>> xychangeList = new List<List<string>>();
                                            for (int i = 0; i < 150; i++)
                                            {
                                                List<string> yList = new List<string>();
                                                for (int j = 0; j < rowList.Count - 1; j++)
                                                {
                                                    if (i < rowList[0].Count - 1)
                                                    {
                                                        yList.Add(rowList[j][i]);
                                                    }
                                                    else
                                                    {
                                                        yList.Add("");
                                                    }
                                                }
                                                xychangeList.Add(yList);
                                            }
                                            xychangeList.exportToActiveSheetOfExcel(headerHeight: 0, schemaHeight: 0, _startRow: 11, _startCol: 2, _isOverwritten: true, _isFormatted: false);

                                        }

                                    }
                                }
                                catch (Exception ex)
                                {
                                    DevelopWorkspace.Base.Logger.WriteLine(ex.Message, Level.ERROR);
                                    //2019/3/16 InnerException perhaps is null
                                    if (ex.InnerException != null) DevelopWorkspace.Base.Logger.WriteLine(ex.InnerException.Message, Level.DEBUG);
                                    if (ex.StackTrace != null) DevelopWorkspace.Base.Logger.WriteLine(ex.StackTrace, Level.DEBUG);
                                }
                            });

                            backgroundWorker.RunWorkerAsync();
                        },
                        (p) => { return true; });
        if (!Services.dbsupportContextmenuCommandList.Contains(selectCommand))
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)delegate ()
            {
                Services.dbsupportContextmenuCommandList.Add(selectCommand);
            });
        }

        ContextMenuCommand sqltextCommand = new ContextMenuCommand("code genetator...", "対象テーブルの情報をコードジェネレーターにエクスポートします。", "code_contextmenu",
                        (p) =>
                        {
                            analyzeSql(p);
                        },
                        (p) => { return true; });


        if (!Services.dbsupportSqlContextmenuCommandList.Contains(sqltextCommand))
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)delegate ()
            {
                Services.dbsupportSqlContextmenuCommandList.Add(sqltextCommand);
            });
        }

    }
    private static void analyzeSql(object rawobject)
    {
        object[] values = rawobject as object[];
        List<TableInfo> tableList = values[0] as List<TableInfo>;
        string sqltext = (values[1] as ICSharpCodeX.AvalonEdit.Edi.EdiTextEditor).Text;

        BackgroundWorker backgroundWorker = new BackgroundWorker();
        backgroundWorker.DoWork += new DoWorkEventHandler((s, ev) =>
        {
            try
            {
                DevelopWorkspace.Base.Utils.SqlParserWrapper wrapper = new DevelopWorkspace.Base.Utils.SqlParserWrapper();
                wrapper.Parse(sqltext, false);

                var SelectColumnList = wrapper.SelectColumnList();
                if (SelectColumnList.Count() == 0)
                {
                    Logger.WriteLine($"can't parse correctly,please confirm your SQL", Level.WARNING);
                    return;
                }
                string guessedTableName = SelectColumnList.Select(selectColumn => selectColumn.TableName).FirstOrDefault(tablename => !string.IsNullOrEmpty(tablename));
                TableInfo gussedTableInfo = tableList.FirstOrDefault(tableinfo => tableinfo.TableName.Equals(guessedTableName));
                if (gussedTableInfo == null)
                {

                    return;
                }

                string excelfilePath = Path.Combine(StartupSetting.instance.homeDir, "addins", "dataConvertTools.xlsm");
                string eventfilePath = Path.Combine(StartupSetting.instance.homeDir, "addins", "dataConvertTools.eventfile");
                XlApp.activateNamedSheet(excelfilePath, "viewer");
                if (fileWatcher == null) fileWatcher = new FileWatcher(eventfilePath, (filename => { ReadFile(filename); }));

                if (service == null)
                {
                    var config = new RazorEngine.Configuration.TemplateServiceConfiguration();
                    //config.Debug = true;
                    config.DisableTempFileLocking = true;
                    config.EncodedStringFactory = new RazorEngine.Text.RawStringFactory(); // Raw string encoding.
                    service = RazorEngineService.Create(config);
                }
                List<List<string>> headerNestedList = new List<List<string>>();
                List<string> headerList = new List<string>();
                headerList.Add(gussedTableInfo.XLAppRef.ConnectionHistory.ConnectionHistoryName);
                headerList.Add(gussedTableInfo.TableName);
                headerNestedList.Add(headerList);
                headerNestedList.exportToActiveSheetOfExcel(headerHeight: 0, schemaHeight: 0, _startRow: 9, _startCol: 2, _isOverwritten: true, _isFormatted: false);
                headerNestedList.Dump();

                List<List<string>> schemaNestedList = new List<List<string>>();
                //类型，别名统一适配
                SelectColumnList.ForEach(column =>
                {
                    string isKey = "";
                    string isSelect = "";
                    string isWhere = "";
                    string columnName = column.FieldName;
                    string remark = "";
                    string dataTypeName = column.DataType;
                    string columnSize = "";
                    if (column.SelectOrWhereClause == SqlParser.SelectOrWhereClauseEnum.SELECT_ONLY)
                    {
                        isSelect = "*";
                    }
                    else if (column.SelectOrWhereClause == SqlParser.SelectOrWhereClauseEnum.WHERE_ONLY)
                    {
                        isWhere = "*";
                    }
                    else if (column.SelectOrWhereClause == SqlParser.SelectOrWhereClauseEnum.ALL)
                    {
                        isSelect = "*";
                        isWhere = "*";
                    }

                    if (!string.IsNullOrEmpty(column.TableName) && !string.IsNullOrEmpty(column.FieldName))
                    {
                        var tableInfo = tableList.FirstOrDefault(tableinfo => tableinfo.TableName.Equals(column.TableName));
                        if (tableInfo != null)
                        {
                            var columnInfo = tableInfo.Columns.FirstOrDefault(columninfo => columninfo.ColumnName.Equals(column.FieldName));
                            if (columnInfo != null)
                            {
                                isKey = "*" == columnInfo.Schemas[0] ? "*" : "";
                                remark = columnInfo.Schemas[2];
                                dataTypeName = columnInfo.Schemas[3];
                                columnSize = columnInfo.Schemas[4];
                            }
                        }
                    }
                    if (!string.IsNullOrEmpty(column.AliasName))
                    {
                        columnName = column.AliasName;
                    }
                    if (string.IsNullOrEmpty(dataTypeName))
                    {
                        //推测 datatype为默认string类型
                        dataTypeName = "varchar";
                    }
                    List<string> schemaList = new List<string>() { isKey, columnName, remark, dataTypeName, columnSize, "", "", isWhere };
                    schemaNestedList.Add(schemaList);

                });
                schemaNestedList.exportToActiveSheetOfExcel(headerHeight: 0, schemaHeight: 0, _startRow: 11, _startCol: 2, _isOverwritten: true, _isFormatted: false);
            }
            catch (Exception ex)
            {
                DevelopWorkspace.Base.Logger.WriteLine(ex.Message, Level.ERROR);
                //2019/3/16 InnerException perhaps is null
                if (ex.InnerException != null) DevelopWorkspace.Base.Logger.WriteLine(ex.InnerException.Message, Level.DEBUG);
                if (ex.StackTrace != null) DevelopWorkspace.Base.Logger.WriteLine(ex.StackTrace, Level.DEBUG);
            }

        });

        backgroundWorker.RunWorkerAsync();

    }

    static VelocityDictionary<string, object> getData()
    {
        VelocityDictionary<string, object> dicObj = new VelocityDictionary<string, object>();
        List<List<string>> sheetData = XlApp.getDataFromActiveSheet();
        dicObj.Add("TableInfo",
            new VelocityDictionary<string, object>()
            {
              {"TableName", sheetData[8][2]},
              {"ClassName", sheetData[8][4]},
              {"MemberName", sheetData[8][5]},
              {"DbKind", sheetData[8][6]}
            });
        List<VelocityDictionary<string, object>> selectColumns = new List<VelocityDictionary<string, object>>();
        for (int i = 10; i < sheetData.Count - 1; i++)
        {
            if (!string.IsNullOrWhiteSpace(sheetData[i][2]))
            {
                selectColumns.Add(new VelocityDictionary<string, object>()
                {
                  {"isKey", sheetData[i][1]},
                  {"ColumnName", sheetData[i][2]},
                  {"Remark", sheetData[i][3]},
                  {"SampleData", sheetData[i][6]},
                  {"IsSelectKey", sheetData[i][8]},
                  {"IsSelectColumn", sheetData[i][9]},
                  {"IsUpdateKey", sheetData[i][10]},
                  {"IsUpdateColumn", sheetData[i][11]},
                  {"IsInsertKey", sheetData[i][12]},
                  {"IsInsertColumn", sheetData[i][13]},
                  {"CameralProperty", sheetData[i][17]},
                  {"CameralVariable", sheetData[i][18]},
                  {"JavaType", sheetData[i][19]}
                });
            }
        }
        dicObj.Add("SelectColumns", selectColumns);
        return dicObj;
    }
    static void ReadFile(string file)
    {
        try
        {
            // get the file's extension
            string eventstring = DevelopWorkspace.Base.Utils.Files.ReadAllText(file, System.Text.Encoding.UTF8);
            string convertRuleFilePath = "";
            if ("control_name:CommandButton1".Equals(eventstring))
            {
                convertRuleFilePath = Path.Combine(StartupSetting.instance.homeDir, "addins", "dataConvertTools", "entity.ConvertRule");
            }
            else if ("control_name:CommandButton2".Equals(eventstring))
            {
                convertRuleFilePath = Path.Combine(StartupSetting.instance.homeDir, "addins", "dataConvertTools", "model.ConvertRule");
            }
            else if ("control_name:CommandButton3".Equals(eventstring))
            {
                convertRuleFilePath = Path.Combine(StartupSetting.instance.homeDir, "addins", "dataConvertTools", "select_sql.ConvertRule");
            }
            else if ("control_name:CommandButton4".Equals(eventstring))
            {
                convertRuleFilePath = Path.Combine(StartupSetting.instance.homeDir, "addins", "dataConvertTools", "update_sql.ConvertRule");
            }
            else if ("control_name:CommandButton5".Equals(eventstring))
            {
                convertRuleFilePath = Path.Combine(StartupSetting.instance.homeDir, "addins", "dataConvertTools", "insert_sql.ConvertRule");
            }

            // entity
            string convertRuleString = DevelopWorkspace.Base.Utils.Files.ReadAllText(convertRuleFilePath, System.Text.Encoding.UTF8);
            string templatekey = GetMd5Hash(convertRuleString);
            service.AddTemplate(templatekey, convertRuleString);
            service.Compile(templatekey);
            var resultString = service.Run(templatekey, null, getData().ToExpando());
            DevelopWorkspace.Base.Logger.WriteLine(resultString);

        }
        catch (Exception exp)
        {
            exp.Dump();
        }
    }
    static string GetMd5Hash(string input)
    {
        var md5 = MD5.Create();
        var inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
        var hash = md5.ComputeHash(inputBytes);
        var sb = new StringBuilder();
        foreach (byte t in hash)
        {
            sb.Append(t.ToString("X2"));
        }
        return sb.ToString();
    }
}