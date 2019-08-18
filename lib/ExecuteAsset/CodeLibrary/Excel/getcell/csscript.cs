using System;
using Microsoft.Office.Interop.Excel;
using System.IO;
using System.Data.SQLite;
using System.Data;
using Microsoft.CSharp;
using System.Collections.Generic;
using DevelopWorkspace.Base;
public class Script
{

   static string ObjectValue(object _object){
    if( _object == null ) return "";
    return _object.ToString();

    }       
    public static void Main(string[] args)
    {

        //DevelopWorkspace.Base.Logger.WriteLine(DevelopWorkspace.Base.Dump.ToDump(M1()));
        //return "";

        DevelopWorkspace.Base.Logger.WriteLine("Process called");
        dynamic xlApp = DevelopWorkspace.Base.Excel.GetLatestActiveExcelRef();
        xlApp.Visible = true;

        var targetSheet = xlApp.ActiveWorkbook.ActiveSheet;
        //读取方式一
        object[,] value2_copy = targetSheet.Range(targetSheet.Cells(1, 1), targetSheet.Cells(targetSheet.UsedRange.Rows.Count, targetSheet.UsedRange.Columns.Count)).Value2;

        DevelopWorkspace.Base.Logger.WriteLine(ObjectValue(value2_copy[7, 3]));
        DevelopWorkspace.Base.Logger.WriteLine(ObjectValue(value2_copy[7, 4]));
        DevelopWorkspace.Base.Logger.WriteLine(ObjectValue(value2_copy[7, 5]));
        DevelopWorkspace.Base.Logger.WriteLine(ObjectValue(value2_copy[7, 6]));
        DevelopWorkspace.Base.Logger.WriteLine(ObjectValue(value2_copy[7, 7]));
 

        DevelopWorkspace.Base.Logger.WriteLine("Process committed");


//读取方式一
        /*
            xlApp.ActiveWorkbook.Worksheets("Sheet1").Range("A1").Value = "A1 ";
            xlApp.ActiveWorkbook.Worksheets("Sheet1").Range("A2").Value = "A2 ";
            xlApp.ActiveWorkbook.Worksheets("Sheet1").Range("A3").Value = "A3 ";
            xlApp.ActiveWorkbook.Worksheets("Sheet1").Range("A4").Value = "A4 ";
            xlApp.ActiveWorkbook.Worksheets("Sheet1").Range("A5").Value = "A5 ";
            xlApp.ActiveWorkbook.Worksheets("Sheet1").Range("B1").Value = "B1 ";
            xlApp.ActiveWorkbook.Worksheets("Sheet1").Range("B2").Value = "B2 ";
            xlApp.ActiveWorkbook.Worksheets("Sheet1").Range("B3").Value = "B3 ";
            xlApp.ActiveWorkbook.Worksheets("Sheet1").Range("B4").Value = "B4 ";
            xlApp.ActiveWorkbook.Worksheets("Sheet1").Range("B5").Value = "B5 ";
            */
        //DevelopWorkspace.Base.Logger.WriteLine(DevelopWorkspace.Base.Dump.ToDump(xlApp.ActiveWorkbook.Worksheets("Sheet1").Range("A1:A5").Value));        
        

//写方式
        //Range selectedRange = xlApp.ActiveWorkbook.Worksheets("Sheet1").Range("A1:C5");
        //foreach( Range range in  selectedRange){
        //if (range.Value == null) continue;
        //	DevelopWorkspace.Base.Logger.WriteLine(range.Value.ToString());
        //}
        //多次元配列
        //object[,] selectedValue2 = xlApp.ActiveWorkbook.Worksheets("Sheet1").Range("A1:B5").Value2;
        //for(int row=1;row<=selectedValue2.GetLength(0);row++){
        //	for(int col=1;col<=selectedValue2.GetLength(1);col++){
        //	selectedValue2[row,col] = "overwritten" +row.ToString() +col.ToString();
        //	}
        //}			
        //xlApp.ActiveWorkbook.Worksheets("Sheet1").Range("A1:B5").Value2 =selectedValue2;

//写性能测试
        //一次写入过少分多次时性能不好，一次过多写入也会造成问题，不同的spec的PC表现不同，自己的手提10000左右是一个理想？值，total10万没有问题
        /*
        int rows=10000,columns=100;
        string [,] ret = new string[rows, columns];
        DevelopWorkspace.Base.Logger.WriteLine("data create start",Level.DEBUG);
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                ret[row, col] = row + "-------" +col;
            }
        }
        DevelopWorkspace.Base.Logger.WriteLine("data create end",Level.DEBUG);

        DevelopWorkspace.Base.Logger.WriteLine("draw start",Level.DEBUG);
        targetSheet.Range(targetSheet.Cells(1, 1), targetSheet.Cells(rows, columns)).Value2=ret;
        targetSheet.Range(targetSheet.Cells(1+rows, 1), targetSheet.Cells(rows*2, columns)).Value2=ret;
        targetSheet.Range(targetSheet.Cells(1+rows*2, 1), targetSheet.Cells(rows*3, columns)).Value2=ret;
        targetSheet.Range(targetSheet.Cells(1+rows*3, 1), targetSheet.Cells(rows*4, columns)).Value2=ret;
        targetSheet.Range(targetSheet.Cells(1+rows*4, 1), targetSheet.Cells(rows*5, columns)).Value2=ret;
        targetSheet.Range(targetSheet.Cells(1+rows*5, 1), targetSheet.Cells(rows*6, columns)).Value2=ret;
        targetSheet.Range(targetSheet.Cells(1+rows*6, 1), targetSheet.Cells(rows*7, columns)).Value2=ret;
        targetSheet.Range(targetSheet.Cells(1+rows*7, 1), targetSheet.Cells(rows*8, columns)).Value2=ret;
        targetSheet.Range(targetSheet.Cells(1+rows*8, 1), targetSheet.Cells(rows*9, columns)).Value2=ret;
        targetSheet.Range(targetSheet.Cells(1+rows*9, 1), targetSheet.Cells(rows*10, columns)).Value2=ret;
        
        DevelopWorkspace.Base.Logger.WriteLine("draw end",Level.DEBUG);
        */

    }
}