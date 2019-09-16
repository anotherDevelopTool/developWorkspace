using System;
using Microsoft.Office.Interop.Excel;
using System.IO;
using System.Xml;
using System.Xml.Xsl;
using System.Linq;
using System.Xml.Linq;
using Microsoft.CSharp;
using DevelopWorkspace.Base;
public class Script
{
    public static void Main(string[] args)
    {
        DevelopWorkspace.Base.Logger.WriteLine("Process called");
        //LINQ to XML 编程基础
        //http://www.cnblogs.com/luckdv/articles/1728088.html
        XElement root = XElement.Parse(args[0]);
        DevelopWorkspace.Base.Logger.WriteLine(root.ToString());
        var text = from t in root.Descendants("Column")
                   select new
                   {
                       TableSchemaID = t.Element("ColumnName").Value
                   };

        foreach (var a in text)
        {
            DevelopWorkspace.Base.Logger.WriteLine(a.TableSchemaID);
        }


        var allelements = from t in root.Descendants("Column") select t;
        foreach (var element in allelements)
        {
            string columnName = element.Element("ColumnName").Value;

            //ToDo
            //在这里你可以定制你的编辑方法，如从数据库里取出的字段需要去掉下划线以及转换为驼峰标记，就可以在这里
            //做做手脚

            element.Element("ColumnName").ReplaceWith(new XElement("ID", "2"));
            //root.Element("Category").SetElementValue("CategoryName", "test data");
        }
        DevelopWorkspace.Base.Logger.WriteLine(root.ToString());

        /*
                    TableInfo ti = (((System.Windows.FrameworkElement)e.OriginalSource).DataContext as TableInfo);
                    XElement root = new XElement("Table");
                    XAttribute xTableName = new XAttribute("TableName", ti.Name);
                    root.Add(xTableName);
                    ti.Columns.ToList<ColumnInfo>().ForEach(delegate (ColumnInfo ci)
                    {
                        XElement line = new XElement("Column");
                        for (int i = 0; i < DbXlsUtil.schemaList.Count(); i++)
                        {
                            line.Add(new XElement(DbXlsUtil.schemaList[i], ci.Schemas[i]));
                        }
                        root.Add(line);
                    });
                    this.txtOutput.Text = root.ToString();

           */
        /*


        var xDoc = new XDocument(new XElement( "root",
                 new XElement("dog",
                     new XText("dog said black is a beautify color"),
                     new XAttribute("color", "black")),
                 new XElement("cat"),
                 new XElement("pig", "pig is great")));

             //xDoc输出xml的encoding是系统默认编码，对于简体中文操作系统是gb2312
             //默认是缩进格式化的xml，而无须格式化设置
             xDoc.Save(Console.Out);
      // 上面代码将输出如下Xml：
     //<?xml version="1.0" encoding="gb2312"?>
     //<root>
     //  <dog color="black">dog said black is a beautify color</dog>
     //  <cat />
     //  <pig>pig is great</pig>
     //</root>
             Console.WriteLine();
        var query = from item in xDoc.Element( "root").Elements()
                         select new
                         {
                             TypeName    = item.Name,
                             Saying      = item.Value,
                             Color       = item.Attribute("color") == null?(string)null:item.Attribute("color").Value
                         };


             foreach (var item in query)
             {
                 Console.WriteLine("{0} 's color is {1},{0} said {2}",item.TypeName,item.Color??"Unknown",item.Saying??"nothing");
             }

             Console.Read();
        */
        //DevelopWorkspace.Base.Logger.WriteLine();

    }
}