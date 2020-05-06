using System;
using System.IO;
using NVelocity;
using NVelocity.App;
using NVelocity.Runtime;
using Microsoft.CSharp;
using System.Collections.Generic;
using DevelopWorkspace.Base;
public class Script
{
    class Table
    {
        public string TableName { get; set; }
        public string Remark { get; set; }
        //public string getTableName(){return TableName;}
    };
    public static void Main(string[] args)
    {
        DevelopWorkspace.Base.Logger.WriteLine("Process called");

        VelocityEngine vltEngine = new VelocityEngine();
        System.IO.File.WriteAllText(@"C:\Users\Public\HelloNVelocity.vm", args[1]);

        vltEngine.SetProperty(RuntimeConstants.RESOURCE_LOADER, "file");
        vltEngine.SetProperty(RuntimeConstants.FILE_RESOURCE_LOADER_PATH, @"C:\Users\Public\");
        vltEngine.Init();


        System.IO.File.WriteAllText(@"C:\Users\Public\TableInfo.xml", args[0]);
        DevelopWorkspace.Main.TableInfo myData1 = new DevelopWorkspace.Main.TableInfo();
        System.IO.FileStream fileStream1 = new System.IO.FileStream(@"C:\Users\Public\TableInfo.xml", System.IO.FileMode.Open);

        System.Xml.Serialization.XmlSerializer xml1 = new System.Xml.Serialization.XmlSerializer(typeof(DevelopWorkspace.Main.TableInfo));

        myData1 = (DevelopWorkspace.Main.TableInfo)xml1.Deserialize(fileStream1);
        myData1.TableName = myData1.TableName.CamelCase(true);
        foreach (DevelopWorkspace.Main.ColumnInfo ci in myData1.Columns)
        {
            ci.ColumnName = ci.ColumnName.CamelCase();
        }
        fileStream1.Close();




        Template vltTemplate = vltEngine.GetTemplate("HelloNVelocity.vm");

        VelocityContext vltContext = new VelocityContext();
        //Test1
        List<string> list = new List<string> { "a", "b", "c", "d" };
        vltContext.Put("list", list);

        //Test2
        Table table = new Table { TableName = "user", Remark = "yu-za" };
        vltContext.Put("user", table);



        vltContext.Put("tableinfo", myData1);

        //test3     
        vltContext.Put("owner", "Unmi");
        vltContext.Put("bill", "1000");
        vltContext.Put("type", "报销单");
        vltContext.Put("date", DateTime.Now.ToLongDateString());

        StringWriter vltWriter = new StringWriter();
        vltTemplate.Merge(vltContext, vltWriter);

        DevelopWorkspace.Base.Logger.WriteLine(vltWriter.GetStringBuilder().ToString());



    }
}