using System;
using Microsoft.Office.Interop.Excel;
using System.IO;
using DevelopWorkspace.Base;
public class Script
{
    public static void Main(string[] args)
 {
     DevelopWorkspace.Base.Logger.WriteLine("Process called");
     Microsoft.Office.Interop.Excel.Application xlApp = new Microsoft.Office.Interop.Excel.Application();
     xlApp.Visible = true;
     System.Windows.MessageBox.Show("pause");

	Script.WalkDirectoryTree(new DirectoryInfo(@"E:\workshop\projects\PCMC\00_管理\Y02_問合管理"));
     DevelopWorkspace.Base.Logger.WriteLine("Process committed");
          xlApp.Quit();
 }


	/// <summary>
	/// 对已打开的BOOK的所有Sheet进行走查，可以追加过滤开关
	/// </summary>
	/// <param name="thisBook"></param>
	static void WalkWorkBook(Workbook thisBook)
    {
		foreach(Worksheet thisSheet in thisBook.Worksheets){
		DevelopWorkspace.Base.Logger.WriteLine(thisSheet.Name);
			Console.WriteLine(thisSheet.Name);
		}
		thisBook.Close(false);
	}
    /// <summary>
    /// 对指定目录进行走查
    /// </summary>
    /// <param name="root"></param>
	static void WalkDirectoryTree(System.IO.DirectoryInfo root)
    {
        System.IO.FileInfo[] files = null;
        System.IO.DirectoryInfo[] subDirs = null;

        // First, process all the files directly under this folder
        try
        {
            files = root.GetFiles("*.xls");
        }
        // This is thrown if even one of the files requires permissions greater
        // than the application provides.
        //catch (UnauthorizedAccessException e)
        //{
            // This code just writes out the message and continues to recurse.
            // You may decide to do something different here. For example, you
            // can try to elevate your privileges and access the file again.
            //log.Add(e.Message);
        //}

        catch (System.IO.DirectoryNotFoundException e)
        {
            //Console.WriteLine(e.Message);
        }

        if (files != null)
        {
            foreach (System.IO.FileInfo fi in files)
            {
                // In this example, we only access the existing FileInfo object. If we
                // want to open, delete or modify the file, then
                // a try-catch block is required here to handle the case
                // where the file has been deleted since the call to TraverseTree().
                //Console.WriteLine(fi.FullName);
                
                 DevelopWorkspace.Base.Logger.WriteLine(fi.FullName);
                 Microsoft.Office.Interop.Excel.Application xlApp =(Microsoft.Office.Interop.Excel.Application)System.Runtime.InteropServices.Marshal.GetActiveObject("Excel.Application");
				WalkWorkBook(xlApp.Workbooks.Open(fi.FullName,false));
            }

            // Now find all the subdirectories under this directory.
            subDirs = root.GetDirectories();

            foreach (System.IO.DirectoryInfo dirInfo in subDirs)
            {
                // Resursive call for each subdirectory.
                WalkDirectoryTree(dirInfo);
            }
        }            
    }    

}