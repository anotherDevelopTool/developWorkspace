using System;
using Microsoft.Office.Interop.Excel;
using System.IO;
using System.Data.SQLite;
using System.Data;
using Microsoft.CSharp;
using System.Collections.Generic;
using System.Linq;
using DevelopWorkspace.Base;
public class Script
{
    public static void Main(string[] args)
     {

        try{
            dynamic xlApp = DevelopWorkspace.Base.Excel.GetLatestActiveExcelRef();        
            xlApp.Visible = true;
        	for(int iSheet=1;iSheet<xlApp.ActiveWorkbook.Worksheets.Count;iSheet++)
            {
                var targetSheet = xlApp.ActiveWorkbook.Worksheets(iSheet);
                object[,] value2_copy = targetSheet.Range(targetSheet.Cells(1, 1), targetSheet.Cells(targetSheet.UsedRange.Rows.Count, targetSheet.UsedRange.Columns.Count)).Value2;
                if(value2_copy.GetLength(0) < 8 || value2_copy.GetLength(1) < 8 ) continue;
            	string tableName = ObjectValue(value2_copy[5, 5]);
            	if(tableName == "" ) continue;
            	string tableDesciption = ObjectValue(value2_copy[5, 2]);

                DevelopWorkspace.Base.Logger.WriteLine(string.Format("{0}\t{1}",tableName,tableDesciption));
            	string columnName = "";
            	string dataType = "";
            	string dataLength = "";
            	bool IsRequired = false;
            	bool IsKey = false;
            	List<string> keys = new List<string>();

            	string template = @"DROP TABLE {0};
CREATE TABLE {1}
(
{2}
)
WITH (
  OIDS=FALSE
);
ALTER TABLE {3}
  OWNER TO postgres;
COMMENT ON TABLE {4}
  IS '{5}';";
            	string columnsScript = "";
                for (int iRow = 8; iRow < value2_copy.GetLength(0); iRow++)
                {
                	columnName = ObjectValue(value2_copy[iRow, 3]);
                	dataType = ObjectValue(value2_copy[iRow, 5]);
                	dataLength = ObjectValue(value2_copy[iRow, 6]);
                	IsRequired = ObjectValue(value2_copy[iRow, 7]) == "○"?true:false;
                	IsKey = ObjectValue(value2_copy[iRow, 8]) == "○"?true:false;
                	if(columnName == "" ) break;
                	if(IsKey) keys.Add(columnName);
                    DevelopWorkspace.Base.Logger.WriteLine(string.Format("{0}\t{1}",ObjectValue(value2_copy[iRow, 3]),ObjectValue(value2_copy[iRow, 4])));

        	   }  
            }

        }
        catch(Exception ex){
        	DevelopWorkspace.Base.Logger.WriteLine(DevelopWorkspace.Base.Dump.ToDump(ex));
        }
    }

   static string ObjectValue(object _object){
	if( _object == null ) return "";
	return _object.ToString();

    }
}