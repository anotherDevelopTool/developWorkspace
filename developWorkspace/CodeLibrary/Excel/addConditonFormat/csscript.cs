using System;
using System.Drawing;
using System.IO;
using System.Data.SQLite;
using System.Data;
using Microsoft.CSharp;
using System.Collections.Generic;
using DevelopWorkspace.Base;
using Excel = Microsoft.Office.Interop.Excel;
public class Script
{
    public static void Main(string[] args)
 {

      dynamic xlApp = DevelopWorkspace.Base.Excel.GetLatestActiveExcelRef(); 
      xlApp.Visible = true;

       var targetSheet = xlApp.ActiveWorkbook.ActiveSheet;
        
    var rangeFormat = targetSheet.Range(targetSheet.Cells(1, 1), targetSheet.Cells(targetSheet.UsedRange.Rows.Count, targetSheet.UsedRange.Columns.Count));
    var fcs = rangeFormat.FormatConditions;
//  var fc = fcs.Add
//      (Excel.XlFormatConditionType.xlExpression, Type.Missing, "=IF($F$1) >= 10", Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
    
    var fc = (Excel.FormatCondition)fcs.Add(
        Type: Excel.XlFormatConditionType.xlExpression,
        Formula1: "=AND($A1="+'"'+"Modified"+'"'+",MOD(COUNTIF($A$1:$A1,"+'"'+"Modified"+'"'+"),2)=1,A1<>A2)"
    );
    var interior = fc.Interior;
    fc.Font.Color = System.Drawing.ColorTranslator.ToOle(Color.Red);    
    fc.StopIfTrue = false;
    //////
    fc = (Excel.FormatCondition)fcs.Add(
        Type: Excel.XlFormatConditionType.xlExpression,
        Formula1: "=AND(ISBLANK(A6),ISBLANK(A5),ISBLANK(A4),ISBLANK(A3),ISBLANK(A2),ISBLANK(A1))"
    );
    interior = fc.Interior;
    interior.Color = System.Drawing.ColorTranslator.ToOle(Color.White); 
    fc.StopIfTrue = false;
    
    ////
    fc = (Excel.FormatCondition)fcs.Add(
        Type: Excel.XlFormatConditionType.xlExpression,
        Formula1: "=$A1=" +'"'+"Deleted"+'"'
    );

    interior = fc.Interior;
    interior.Color = System.Drawing.ColorTranslator.ToOle(Color.Gray);
    fc.StopIfTrue = false;
    ///////
    fc = (Excel.FormatCondition)fcs.Add(
        Type: Excel.XlFormatConditionType.xlExpression,
        Formula1: "=$A1=" +'"'+"Added"+'"'
    );

    interior = fc.Interior;
    interior.Color = System.Drawing.ColorTranslator.ToOle(Color.Red);
    fc.StopIfTrue = false;
    //////
    fc = (Excel.FormatCondition)fcs.Add(
        Type: Excel.XlFormatConditionType.xlExpression,
        Formula1: "=$A1=" +'"'+"Modified"+'"'
    );
    
    interior = fc.Interior;
    interior.Color = System.Drawing.ColorTranslator.ToOle(Color.Azure);
    fc.StopIfTrue = false;
    /////


    interior = null;
    fc = null;
    fcs = null;
    
    
    
    
        DevelopWorkspace.Base.Logger.WriteLine("Process committed");
 }


}