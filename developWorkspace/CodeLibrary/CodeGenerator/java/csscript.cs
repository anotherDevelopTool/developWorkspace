package com.wishwzp.poi;

import java.io.FileOutputStream;
import java.util.Date;

import org.apache.poi.hssf.usermodel.HSSFCell;
import org.apache.poi.hssf.usermodel.HSSFCellStyle;
import org.apache.poi.hssf.usermodel.HSSFRichTextString;
import org.apache.poi.hssf.usermodel.HSSFWorkbook;
import org.apache.poi.ss.usermodel.Cell;
import org.apache.poi.ss.usermodel.CellStyle;
import org.apache.poi.ss.usermodel.Row;
import org.apache.poi.ss.usermodel.Sheet;
import org.apache.poi.ss.usermodel.Workbook;

public class HelloWorld {

    public static void main(String[] args) throws Exception{
        Workbook wb=new HSSFWorkbook(); 
        Sheet sheet=wb.createSheet("Sheet"); 
        Row row=sheet.createRow(2);
        row.setHeightInPoints(30);
        
        
        FileOutputStream fileOut=new FileOutputStream("test.xls");
        wb.write(fileOut);
        fileOut.close();
    }
    

    private static void createCell(Workbook wb,Row row,short column,short halign,short valign){
        Cell cell=row.createCell(column); 
        cell.setCellValue(new HSSFRichTextString("Align It")); 
        
        CellStyle cellStyle=wb.createCellStyle();
        
       
        cell.setCellStyle(cellStyle); 
    }
    

}