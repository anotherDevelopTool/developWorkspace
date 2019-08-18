using System;
using Microsoft.Office.Interop.Excel;
using System.IO;
using System.Xml;
using System.Xml.Xsl;
using Microsoft.CSharp;
using DevelopWorkspace.Base;
public class Script
{
    public static void Main(string[] args)
 {
     DevelopWorkspace.Base.Logger.WriteLine("Process called");

     XmlDocument doc = new XmlDocument();
     doc.LoadXml(args[0]);
		System.IO.File.WriteAllText(@"C:\Users\Public\WriteText.txt", args[1]);
     XslCompiledTransform transform = new XslCompiledTransform();
     transform.Load(@"C:\Users\Public\WriteText.txt");
     TextWriter writer = new StringWriter();
     transform.Transform(doc, null, writer);

     DevelopWorkspace.Base.Logger.WriteLine(writer.ToString());


 }
}