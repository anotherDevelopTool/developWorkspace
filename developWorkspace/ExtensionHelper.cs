using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevelopWorkspace.Base;
namespace DevelopWorkspace.Main
{
    public static class ExtensionHelper
    {
        public static void exportToActiveSheetOfExcel(this List<List<string>> rowdataList,  int headerHeight=1, int schemaHeight=2, int _startRow = 1, int _startCol = 1, bool _isOverwritten=false,bool _isFormatted=true)
        {
            DevelopWorkspace.Main.XlApp.loadDataIntoActiveSheet(_startRow, _startCol,headerHeight, schemaHeight, _isOverwritten, _isFormatted,new List<List<List<string>>> { rowdataList });

        }
        public static void exportToActiveSheet(this TableInfo tableInfo)
        {
            string[,] data = tableInfo.getTableDataWithSchema();
            if (data != null) {
                List<List<string>> rowList = data.ToNestedList();
                rowList.RemoveAt(4);
                rowList.RemoveAt(2);
                rowList.RemoveAt(0);
                rowList.Insert(0, new List<string> { tableInfo.TableName });
                rowList.exportToActiveSheetOfExcel(headerHeight: 1, schemaHeight: 2, _startRow: 0, _startCol: 0, _isOverwritten: false);
            }
        }
        public static string[,] getTableDataWithSchema(this TableInfo tableInfo)
        {
            if (tableInfo != null)
            {
                try
                {
                    tableInfo.XLAppRef.DbConnection.Open();
                    tableInfo.XLAppRef.DbConnection.CreateCommand();
                    return tableInfo.XLAppRef.GetTableDataWithSchema(tableInfo, tableInfo.XLAppRef.DbConnection.CreateCommand());
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    tableInfo.XLAppRef.DbConnection.Close();
                }
            }
            return null;
        }
    }
}
