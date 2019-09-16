using System;
using Microsoft.Office.Interop.Excel;
using System.IO;
using Laan.Sql.Parser;
using Microsoft.CSharp;
using System.Collections.Generic;
using DevelopWorkspace.Base;
public class Script
{
    public static void Main(string[] args)
 {
     DevelopWorkspace.Base.Logger.WriteLine("Process called");
//DevelopWorkspace.Base.Logger.WriteLine(view[1,1]);
var statements = ParserFactory.Execute( args[0] );
DevelopWorkspace.Base.Logger.WriteLine(DevelopWorkspace.Base.Dump.ToDump(statements));

 }
}
