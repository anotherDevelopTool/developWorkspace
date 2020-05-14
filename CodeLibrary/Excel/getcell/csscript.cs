using System;
using Microsoft.Office.Interop.Excel;
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
using static System.ValueTuple;
//css_reference System.ValueTuple.dll

public class Script
{

    static string ObjectValue(object _object)
    {
        if (_object == null) return "";
        return _object.ToString();

    }
    public static void Main(string[] args)
    {
        WriteWorkAsync().Wait();
        XlApp.getDataFromActiveSheet().Dump();
        XlApp.getDataFromActiveSheet().To2dArray().Dump();
        XlApp.getDataFromActiveWorkbook().Dump(); //data in all of sheets and names,return values is ValueTuple
    }

    static async Task WriteWorkAsync()
    {
        await Task.Run(() =>
        {
            List<List<string>> rowList = new List<List<string>>{
                new List<string> { "h1","h2" },
                new List<string> { "saledate","sum","avg","max" },
                new List<string> { "date","int","int","int" },
                new List<string> { "h1","h2","h3","h1" },
                new List<string> { "h1","h2","h3","h2" },
                new List<string> { "h1","h2","h3","h3" },
                new List<string> { "h1","h2","h3","h4" },
                new List<string> { "h1","h2","h3","h5" }
            };

            rowList.exportToActiveSheetOfExcel(1, 1);

        }).ConfigureAwait(false);

    }
	
	
	
	
	
/*	
using System;
using System.Drawing;
using System.IO;
using System.Data.SQLite;
using System.Data;
using Microsoft.CSharp;
using System.Collections.Generic;
using DevelopWorkspace.Base;
using Excel = Microsoft.Office.Interop.Excel;
using System.Threading;
using System.Threading.Tasks;
public class Script
{
    public static void Main(string[] args)
    {
        //需要appdomain以shared方式执行
        ContextMenuCommand selectCommand = new ContextMenuCommand("copy select sqltext to clipboard", "対象テーブルの取得SQL文をシステムのクリップボードにコピーします。", "load",
                        (p) =>
                        {
                            p.Dump();
                        },
                        (p) => { return true; });
        if (!Services.dbsupportContextmenuCommandList.Contains(selectCommand))
        {
            "insert".Dump();
            Services.dbsupportContextmenuCommandList.Add(selectCommand);
        }
    }
}	
*/	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
}