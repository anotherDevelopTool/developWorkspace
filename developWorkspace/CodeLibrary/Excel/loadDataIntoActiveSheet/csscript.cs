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

    	/*// demo for get data from active sheet
		List<List<string>> rowDataList = DevelopWorkspace.Main.XlApp.loadDataIntoActiveSheet();
		*/

    	/*// demo for export data to active sheet
		List<List<string>> headerList = new List<List<string>>{
		    new List<string> { "h1","h2" }
		};
		List<List<string>> schemarList = new List<List<string>>{
		    new List<string> { "saledate","sum","avg","max" },
		    new List<string> { "date","int","int","int" }
		};
    	List<List<string>> rowList = new List<List<string>>{
		    new List<string> { "h1","h2","h3","h1" },
		    new List<string> { "h1","h2","h3","h2" },
		    new List<string> { "h1","h2","h3","h3" },
		    new List<string> { "h1","h2","h3","h4" },
		    new List<string> { "h1","h2","h3","h5" }
		};
    	List<List<string>> rowList2 = new List<List<string>>{
		    new List<string> { "h13333333333333" },
		    new List<string> { "h1","h2","h3","s1" },
		    new List<string> { "h1","h2","h3","s2" },
		    new List<string> { "h1","h2","h3","r1" },
		    new List<string> { "h1","h2","h3","r2" },
		    new List<string> { "h1","h2","h3","r3" },
		    new List<string> { "h1","h2","h3","r4" }
		};
		DevelopWorkspace.Main.XlApp.loadDataIntoActiveSheet(headerList, schemarList, rowList,rowList,rowList2,rowList2);
		*/

    	/*// demo for export data to active sheet
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
        List<List<string>> rowList2 = new List<List<string>>{
            new List<string> { "h13333333333333" },
            new List<string> { "h1","h2","h3","s1" },
            new List<string> { "h1","h2","h3","s2" }

        };
        List<List<List<string>>> alltables = new List<List<List<string>>> { rowList, rowList2 };
        //表名所在行的高度，结构的高度，所有表的表名，结构，数据的列表的集合
        DevelopWorkspace.Main.XlApp.loadDataIntoActiveSheet(1, 0, alltables);

    }
}