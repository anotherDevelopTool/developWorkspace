using System;
using System.Drawing;
using System.IO;
using System.Data.SQLite;
using System.Data;
using Microsoft.CSharp;
using System.Collections.Generic;
using DevelopWorkspace.Base;
using DevelopWorkspace.Main;
using Excel = Microsoft.Office.Interop.Excel;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel;
public class Script
{
    public static void Main(string[] args)
    {
        //需要appdomain以shared方式执行
        ContextMenuCommand selectCommand = new ContextMenuCommand("export data to activesheet", "対象テーブルのデータをアクティブシートにエクスポートします。", "export_contextmenu",
                        (p) =>
                        {
                            BackgroundWorker backgroundWorker = new BackgroundWorker();
                            backgroundWorker.DoWork += new DoWorkEventHandler((s, ev) =>
                            {
                                DevelopWorkspace.Main.TableInfo tableinfo = p as DevelopWorkspace.Main.TableInfo;
                                if (tableinfo != null)
                                {
                                    tableinfo.exportToActiveSheet();

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


        ContextMenuCommand junitCommand = new ContextMenuCommand("export junit formatted data to activesheet", "対象テーブルのデータをJNITフォーマットでアクティブシートにエクスポートします。", "junit",
                        (p) =>
                        {
                            BackgroundWorker backgroundWorker = new BackgroundWorker();
                            backgroundWorker.DoWork += new DoWorkEventHandler((s, ev) =>
                            {
                                DevelopWorkspace.Main.TableInfo tableinfo = p as DevelopWorkspace.Main.TableInfo;
                                if (tableinfo != null)
                                {
                                    string[,] data = tableinfo.getTableDataWithSchema();
                                    if (data != null)
                                    {
                                        List<List<string>> rowList = data.ToNestedList();
                                        rowList.RemoveAt(4);
                                        rowList.RemoveAt(3);
                                        rowList.RemoveAt(2);
                                        rowList.RemoveAt(0);
                                        if (rowList.Count > 4)
                                        {
                                            rowList.RemoveRange(4, rowList.Count - 4);
                                        }
                                        //rowList.Insert(0, new List<string> { tableinfo.TableName });
                                        rowList.exportToActiveSheetOfExcel(headerHeight: 1, schemaHeight: 0, _startRow: 0, _startCol: 0, _isOverwritten: false);
                                    }
                                }
                            });

                            backgroundWorker.RunWorkerAsync();
                        },
                        (p) => { return true; });
        if (!Services.dbsupportContextmenuCommandList.Contains(junitCommand))
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)delegate ()
            {
                Services.dbsupportContextmenuCommandList.Add(junitCommand);
            });
        }


    }
}

