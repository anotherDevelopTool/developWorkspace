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
public class Script
{
    public static void Main(string[] args)
    {
        //需要appdomain以shared方式执行
        ContextMenuCommand selectCommand = new ContextMenuCommand("export data to activesheet", "対象テーブルのデータをアクティブシートにエクスポートします。", "load",
                        (p) =>
                        {
                            p.Dump();
                            DevelopWorkspace.Main.TableInfo tableinfo = p as DevelopWorkspace.Main.TableInfo;
                            if (tableinfo != null)
                            {
                                tableinfo.XLAppRef.DbConnection.Open();
                                tableinfo.XLAppRef.DbConnection.CreateCommand();
                                tableinfo.XLAppRef.GetTableDataWithSchema(tableinfo, tableinfo.XLAppRef.DbConnection.CreateCommand()).ToListWithList().exportToActiveSheetOfExcel(0,5,0,0,true);
                                tableinfo.XLAppRef.DbConnection.Close();

                            }
                        },
                        (p) => { return true; });
        if (!Services.dbsupportContextmenuCommandList.Contains(selectCommand))
        {
            "insert".Dump();
            Services.dbsupportContextmenuCommandList.Add(selectCommand);
        }
    }
}

