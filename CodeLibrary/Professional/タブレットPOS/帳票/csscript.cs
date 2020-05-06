using System;
using Microsoft.Office.Interop.Excel;
using System.IO;
using System.Data.SQLite;
using System.Data;
using Microsoft.CSharp;
using System.Collections.Generic;
using System;
using System.IO;
using NVelocity;
using NVelocity.App;
using NVelocity.Runtime;
using Microsoft.CSharp;
using System.Collections.Generic;
using System;
using Microsoft.Office.Interop.Excel;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using DevelopWorkspace.Base;
class ReportData{
    public ReportData(){
        sections = new List<SectionData>();
    }
    public FormData formdata {get;set;} 
    public List<SectionData> sections {get;set;}    
}

class FormData{
    public string formprint {get;set;}                                  
    public string name {get;set;}                                                                       
    public string printmode {get;set;}                                                                      
    public string filename   {get;set;}                                                                 
    public string cutflag {get;set;}                                                                        
    public string orientation {get;set;}                                                                        
}
class ColumnData{
    public ColumnData(){
        fontsize = "";
    }
    public string column {get;set;}                                 
    public string align {get;set;}                                                                      
    public string fontsize {get;set;}                                                                       
    public string type {get;set;}                                                                       
    public string value  {get;set;}                                                                 
    public string barcodetype    {get;set;}                                                                 
    public string strdispflag    {get;set;}                                                                 
}
class RowData{
    public RowData(){
        cols = new List<ColumnData>();
        condition = false;
    }
    public string name {get;set;}                                                                       
    public bool condition {get;set;}                                                                        
    public List<ColumnData> cols {get;set;} 
}
class SectionData{
    public SectionData(){
        rows = new List<RowData>();
    }
    public string section {get;set;}                                    
    public string name {get;set;}                                                                       
    public List<RowData> rows {get;set;}    
}
public class Script
{
   static string ObjectValue(object _object){
    if( _object == null ) return "";
    return _object.ToString().Trim();

    }   
    public static void Main(string[] args)
    {  

        //DevelopWorkspace.Base.Logger.WriteLine(DevelopWorkspace.Base.Dump.ToDump(M1()));
        //return "";

        DevelopWorkspace.Base.Logger.WriteLine("Process called");
        //dynamic xlApp = DevelopWorkspace.Main.XlApp.Excel;
        dynamic xlApp = DevelopWorkspace.Base.Excel.GetLatestActiveExcelRef();
        xlApp.Visible = true;

        
        VelocityEngine vltEngine = new VelocityEngine();
        System.IO.File.WriteAllText(@"C:\Users\Public\HelloNVelocity.vm", args[1]);

        vltEngine.SetProperty(RuntimeConstants.RESOURCE_LOADER, "file");
        vltEngine.SetProperty(RuntimeConstants.FILE_RESOURCE_LOADER_PATH, @"C:\Users\Public\");
        vltEngine.Init();

        Template vltTemplate = vltEngine.GetTemplate("HelloNVelocity.vm");

        VelocityContext vltContext = new VelocityContext();
        
        
        
        
        var targetSheet = xlApp.ActiveWorkbook.ActiveSheet;
        object[,] value2_copy = targetSheet.Range(targetSheet.Cells(1, 1), targetSheet.Cells(targetSheet.UsedRange.Rows.Count, targetSheet.UsedRange.Columns.Count)).Value2;
        
        ReportData reportdata = new ReportData();
        FormData formdata = new FormData();
        
        reportdata.formdata = formdata;
        
        //DevelopWorkspace.Base.Logger.WriteLine(DevelopWorkspace.Base.Dump.ToDump(value2_copy));        

        for (int iRow = 9; iRow < value2_copy.GetLength(0); iRow++)
        {
            if(ObjectValue(value2_copy[iRow, 4]) == "formdata")
            {
                //DevelopWorkspace.Base.Logger.WriteLine("formdata finded....");
                iRow++;
                while(ObjectValue(value2_copy[iRow, 5]) != ""){
                    if(ObjectValue(value2_copy[iRow, 5]) == "formprint") formdata.formprint = ObjectValue(value2_copy[iRow, 19]) ;
                    if(ObjectValue(value2_copy[iRow, 5]) == "name") formdata.name    = ObjectValue(value2_copy[iRow, 19]) ;
                    if(ObjectValue(value2_copy[iRow, 5]) == "printmode") formdata.printmode = ObjectValue(value2_copy[iRow, 19]) ;
                    if(ObjectValue(value2_copy[iRow, 5]) == "filename") formdata.filename    = ObjectValue(value2_copy[iRow, 19]) ;
                    if(ObjectValue(value2_copy[iRow, 5]) == "cutflag") formdata.cutflag  = ObjectValue(value2_copy[iRow, 19]) ;
                    if(ObjectValue(value2_copy[iRow, 5]) == "orientation") formdata.orientation = ObjectValue(value2_copy[iRow, 19]) ;
                    iRow++;
                }
            }
            
            if(ObjectValue(value2_copy[iRow, 6]) == "section")
            {
                //DevelopWorkspace.Base.Logger.WriteLine(iRow + ":::row:::" + "section...");
               
                SectionData sectiondata = new SectionData();
                reportdata.sections.Add(sectiondata);
                sectiondata.name = ObjectValue(value2_copy[iRow+1, 19]) ;
                //see whether row data exits
                if(ObjectValue(value2_copy[iRow+2, 7]) == "row"){
                    iRow++;
                    iRow++;
                    while(ObjectValue(value2_copy[iRow, 7]) == "row"){
                        RowData rowdata = new RowData();
                        rowdata.name = ObjectValue(value2_copy[iRow, 19]) ;
                        rowdata.condition = ObjectValue(value2_copy[iRow, 15])  == "△" ? true:false;
                        iRow++;
                        sectiondata.rows.Add(rowdata);
                        ColumnData columndata=null;
                        while(ObjectValue(value2_copy[iRow, 8]) != ""){
                            if(ObjectValue(value2_copy[iRow, 8]) == "column") {
                                //if(columndata != null ) DevelopWorkspace.Base.Logger.WriteLine(DevelopWorkspace.Base.Dump.ToDump(columndata));   
                                columndata = new ColumnData();
                                rowdata.cols.Add(columndata);
                            }
                            if(ObjectValue(value2_copy[iRow, 8]) == "column") columndata.column = ObjectValue(value2_copy[iRow, 19]) ;
                            if(ObjectValue(value2_copy[iRow, 8]) == "align") columndata.align    = ObjectValue(value2_copy[iRow, 19]) ;
                            if(ObjectValue(value2_copy[iRow, 8]) == "fontsize") columndata.fontsize = ObjectValue(value2_copy[iRow, 19]) ;
                            if(ObjectValue(value2_copy[iRow, 8]) == "type") columndata.type  = ObjectValue(value2_copy[iRow, 19]) ;
                            if(ObjectValue(value2_copy[iRow, 8]) == "value") columndata.value    = ObjectValue(value2_copy[iRow, 19]) ;
                            if(ObjectValue(value2_copy[iRow, 8]) == "barcodetype") columndata.barcodetype    = ObjectValue(value2_copy[iRow, 19]) ;
                            if(ObjectValue(value2_copy[iRow, 8]) == "strdispflag") columndata.strdispflag    = ObjectValue(value2_copy[iRow, 19]) ;
                            iRow++;
                        }
                        //rowdata.cols.Add(columndata);
                        //DevelopWorkspace.Base.Logger.WriteLine(DevelopWorkspace.Base.Dump.ToDump(columndata));   
                        //DevelopWorkspace.Base.Logger.WriteLine(DevelopWorkspace.Base.Dump.ToDump(rowdata));        
                    }
                    iRow--;
                }
            }
        }

        //DevelopWorkspace.Base.Logger.WriteLine(DevelopWorkspace.Base.Dump.ToDump(reportdata));        
        vltContext.Put("reportdata", reportdata);
        StringWriter vltWriter = new StringWriter();
        vltTemplate.Merge(vltContext, vltWriter);

        DevelopWorkspace.Base.Logger.WriteLine(vltWriter.GetStringBuilder().ToString());
        System.IO.File.WriteAllText(@"C:\Users\Public\report.xml", vltWriter.GetStringBuilder().ToString());

        DevelopWorkspace.Base.Logger.WriteLine("Process committed");

    }
}