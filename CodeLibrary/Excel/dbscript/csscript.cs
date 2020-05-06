using System;
using Microsoft.Office.Interop.Excel;
using System.IO;
using System.Data.SQLite;
using System.Data;
using System.Data.Common;
using System.Linq;
using Microsoft.CSharp;
using System.Collections.Generic;
using DevelopWorkspace.Base;
public class Script
{
	const int START_ROW = 1;
	const int START_COL = 2;
	const string SCHEMA_COLUMN_NAME = "ColumnName";
	const string SCHEMA_IS_KEY = "IsKey";
	const string SCHEMA_DATATYPE_NAME = "DataTypeName";
	const string SCHEMA_COLUMN_SIZE = "ColumnSize";

	/// <summary>
	/// 表头行的输出信息定义
	/// </summary>
	static string[] schemaList = new string[] {
		SCHEMA_COLUMN_NAME,
		SCHEMA_IS_KEY,
		SCHEMA_DATATYPE_NAME,
		SCHEMA_COLUMN_SIZE
	};
	/// <summary>
	/// 对象表一览
	/// </summary>
	/// 
	/*
	static string[] selectTableNameList = new string[] {
		"Customers",
		"Orders",
		"Employees"
	};
	*/
	static string[] selectTableNameList = new string[] {
		"highscores"
	};
	static dynamic _xlApp = null;
	/// <summary>
	/// 主处理
	/// </summary>
	/// <param name="view"></param>
	/// <returns></returns>
	public static void Main(string[] args)
	{
		//using (SQLiteConnection con = new SQLiteConnection("Data Source=northwindEF.db;Pooling=true;FailIfMissing=false")) {
		using (SQLiteConnection con = new SQLiteConnection("Data Source=test.db;Pooling=true;FailIfMissing=false")) {
			con.Open();
			LoadDataIntoExcel(Script.selectTableNameList, con.CreateCommand());
			//CreateSqlScriptByActivedSheet(con.CreateCommand());
			con.Close();
		}
	}

	/// <summary>
	/// 取得EXCEL实例
	/// </summary>
	/// <returns></returns>
	public static dynamic xlApp {

		get {
			if (Script._xlApp == null) {
				try {
					Script._xlApp = (Microsoft.Office.Interop.Excel.Application)System.Runtime.InteropServices.Marshal.GetActiveObject("Excel.Application");
				} catch (Exception ex) {
					//如果没有娶到那么创建新的实例
					Script._xlApp = new Microsoft.Office.Interop.Excel.Application();
				}
				//如果没有娶到那么创建新的实例
				if (Script._xlApp == null) {
					Script._xlApp = new Microsoft.Office.Interop.Excel.Application();
					Script._xlApp.Workbooks.Add();
				}
				//如果没有打开的WORKBOOK那么创建一个
				if (Script._xlApp.Workbooks.Count == 0) {
					Script._xlApp.Workbooks.Add();
				}				
			}
			return Script._xlApp;		
		}
	}
	public static void CreateSqlScriptByActivedSheet(DbCommand cmd)
	{
		var targetSheet = xlApp.ActiveWorkbook.ActiveSheet;		
		object[,] value2_copy = targetSheet.Range(targetSheet.Cells(START_ROW, START_COL),
			                        targetSheet.Cells(targetSheet.UsedRange.Rows.Count + START_ROW, targetSheet.UsedRange.Columns.Count + START_COL)).Value2;
		bool bTableTokenHit = false;
		string strTableName = "";
		
		Dictionary<string,List<string>> dicShema = new Dictionary<string, List<string>>();
		foreach (string keyword in Script.schemaList) {
			dicShema.Add(keyword, new List<string>());
		}
		List<string> lstRowData = new List<string>();
		
		for (int iRow = 1; iRow < value2_copy.GetLength(0); iRow++) {
			//如果整行都是空白那么判定其为表数据的结束
			if (bTableTokenHit) {
				for (int iCol = 1; iCol < value2_copy.GetLength(1); iCol++) {
					if (value2_copy[iRow, iCol] != null)
						break;
					if (iCol == value2_copy.GetLength(1) - 1)
						bTableTokenHit = false;
				}
			}
			//find where table begin
			if (bTableTokenHit == false) {
				bTableTokenHit = true;
				//前两个CELL不为空以外有一个为空则认定不是表头的开始
				for (int iCol = 1 + 2; iCol < value2_copy.GetLength(1); iCol++) {
					if (value2_copy[iRow, iCol] != null)
						bTableTokenHit = false;						
				}
				if (value2_copy[iRow, 1] == null || value2_copy[iRow, 2] == null)
					bTableTokenHit = false;
				//如果是表头则开始处理表名以及其余属性行信息
				if (bTableTokenHit) {
					foreach (string keyword in Script.schemaList) {
						dicShema[keyword].Clear();
					}

					strTableName = value2_copy[iRow, 1].ToString();	
					//表属性行定义取得
					foreach (string keyword in Script.schemaList) {
						iRow++;
						for (int iCol = 1; iCol < value2_copy.GetLength(1); iCol++) {
							if (keyword == SCHEMA_COLUMN_NAME) {
								if (value2_copy[iRow, iCol] != null)
									dicShema[keyword].Add(value2_copy[iRow, iCol].ToString());
							} else {
								dicShema[keyword].Add(value2_copy[iRow, iCol] == null ? "" : value2_copy[iRow, iCol].ToString());
							}
						}
					}
					iRow++;
				}
			}
			if (bTableTokenHit) {
				//process data region
				lstRowData.Clear();
				for (int iCol = 1; iCol < dicShema[SCHEMA_COLUMN_NAME].Count + 1; iCol++) {
					if (value2_copy[iRow, iCol] == null) {
						lstRowData.Add("null");
						continue;
					} else {
						switch (dicShema[SCHEMA_DATATYPE_NAME][iCol - 1]) {
							case "char":
							case "nchar":
							case "varchar":
							case "nvarchar":
							case "ntext":
							case "datetime":
								lstRowData.Add("'" + value2_copy[iRow, iCol].ToString() + "'");
								break;
							default:
								lstRowData.Add(value2_copy[iRow, iCol].ToString());
								break;
						}
					}
				}
				DevelopWorkspace.Base.Logger.WriteLine(string.Format("INSERT INTO {0} ({1}) VALUES ({2});",
					strTableName,
					dicShema[SCHEMA_COLUMN_NAME].Aggregate((i, j) => i + "," + j),
					lstRowData.Take(dicShema[SCHEMA_COLUMN_NAME].Count).Aggregate((i, j) => i + "," + j)));

				cmd.CommandText = string.Format("INSERT INTO {0} ({1}) VALUES ({2});",
					strTableName,
					dicShema[SCHEMA_COLUMN_NAME].Aggregate((i, j) => i + "," + j),
					lstRowData.Take(dicShema[SCHEMA_COLUMN_NAME].Count).Aggregate((i, j) => i + "," + j));
				cmd.ExecuteNonQuery(); 
			}
		}
	}
	public static void LoadDataIntoExcel(string[] selectTableNameList, DbCommand cmd)
	{
		try {
			
			xlApp.Visible = true;
			xlApp.ScreenUpdating = false;           
			var targetSheet = xlApp.ActiveWorkbook.ActiveSheet;

			int sartRow = START_ROW;
			int startCol = START_COL;
			Range selected;

			foreach (string tableName in selectTableNameList) {
				//Table属性定义行区域颜色定制
				selected = targetSheet.Range(targetSheet.Cells(sartRow, startCol),
					targetSheet.Cells(sartRow, startCol + 1));
				selected.Interior.Pattern = Microsoft.Office.Interop.Excel.Constants.xlSolid;
				selected.Interior.ThemeColor = Microsoft.Office.Interop.Excel.XlThemeColor.xlThemeColorAccent4;
				selected.Value2 = new string[]{ tableName, tableName };
				Script.DrawBorder(selected);
				sartRow++;
				string[,] value2_copy = GetTableDataWithSchema(tableName, cmd);
				//Table属性定义行区域颜色定制
				selected = targetSheet.Range(targetSheet.Cells(sartRow, startCol),
					targetSheet.Cells(sartRow + Script.schemaList.GetLength(0) - 1, value2_copy.GetLength(1) + startCol - 1));
				selected.Interior.Pattern = Microsoft.Office.Interop.Excel.Constants.xlSolid;
				selected.Interior.ThemeColor = Microsoft.Office.Interop.Excel.XlThemeColor.xlThemeColorAccent5;
				//Data拷贝到指定区域
				selected = targetSheet.Range(targetSheet.Cells(sartRow, startCol),
					targetSheet.Cells(value2_copy.GetLength(0) + sartRow - 1, value2_copy.GetLength(1) + startCol - 1));
				selected.NumberFormat = "@";
				selected.Value2 = value2_copy;
				Script.DrawBorder(selected);
				sartRow = value2_copy.GetLength(0) + sartRow + 2;
			}
			targetSheet.Columns("B:AZ").EntireColumn.AutoFit();

			xlApp.ScreenUpdating = true;
			xlApp.Quit();
		} catch (Exception ex) {
			DevelopWorkspace.Base.Logger.WriteLine(ex.Message);
		}
	}
	
	/// <summary>
	/// 这个的写法参照VBA的关联部分，可以在VBA开发环境中的寻找各种定义
	/// </summary>
	/// <param name="selected"></param>
	static void DrawBorder(Range selected)
	{
		XlBordersIndex[] borderIndexes = new XlBordersIndex[] {
			XlBordersIndex.xlEdgeLeft,
			XlBordersIndex.xlEdgeTop,
			XlBordersIndex.xlEdgeBottom,
			XlBordersIndex.xlEdgeRight,
			XlBordersIndex.xlInsideVertical,
			XlBordersIndex.xlInsideHorizontal
		};
		foreach (XlBordersIndex idx in borderIndexes) {
			selected.Borders[idx].LineStyle = XlLineStyle.xlContinuous;
			selected.Borders[idx].ColorIndex = 0;
			selected.Borders[idx].TintAndShade = 0;
			selected.Borders[idx].Weight = XlBorderWeight.xlThin;
		}
	}
	/// <summary>
	/// 指定表单的Schema情报以及数据取得
	/// </summary>
	/// <param name="tableName"></param>
	/// <returns></returns>
	static string[,] GetTableDataWithSchema(string tableName, DbCommand cmd)
	{
		List<List<string>> linked = new List<List<string>>();
		string[,] ret;
		cmd.CommandText = string.Format("SELECT * FROM {0}", tableName);
		using (DbDataReader rdr = cmd.ExecuteReader()) {
			System.Data.DataTable schemaTable = rdr.GetSchemaTable();
			List<string> schemaLinkedList = null;
			foreach (string keyword in Script.schemaList) {
				schemaLinkedList = new List<string>();
				//字段名取得
				foreach (System.Data.DataRow row in schemaTable.Rows) {
					if (keyword == "IsKey") {
						if (row[keyword].ToString() == "True") {
							schemaLinkedList.Add("*");
						} else {
							schemaLinkedList.Add("");
						}
					} else {
						schemaLinkedList.Add(row[keyword].ToString());
					}
				}
				linked.Add(schemaLinkedList);
			}
			List<string> dataLinkedList = null;
			schemaLinkedList = linked[0];
			while (rdr.Read()) {
				dataLinkedList = new List<string>();
				foreach (string columnName in schemaLinkedList) {
					dataLinkedList.Add(rdr[columnName].ToString());
				}
				linked.Add(dataLinkedList);
			}
			//List内容转换成二维数组
			ret = new string[linked.Count, schemaLinkedList.Count];
			for (int row = 0; row < linked.Count; row++) {
				for (int col = 0; col < schemaLinkedList.Count; col++) {
					ret[row, col] = linked[row][col];
				}
			}
			return ret;
		}
	}
}