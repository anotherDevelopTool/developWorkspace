using System;
using System.IO;
using NVelocity;
using NVelocity.App;
using NVelocity.Runtime;
using Microsoft.CSharp;
using System.Collections.Generic;
using System;
using System.IO;
using Microsoft.CSharp;
using System.Collections.Generic;
using System.Dynamic;
using System.Xml;
using System.Xml.Xsl;
using System.Xml.Linq;
using System.Linq;
using DevelopWorkspace.Base;
/// <summary>
/// 这个类是通过Dictionary以及LIST把XML的各个要素装载进来，以供Velocity使用
/// </summary>
class XmlToGenericObject
{
    public static void Parse(object parent, XElement node)
    {
        if (node.HasElements)
        {
            if (node.Elements(node.Elements().First().Name.LocalName).Count() > 1)
            {
                //list
                //var item = new ExpandoObject();
                //var list = new List<object>();
                var item = new Dictionary<string, object>();
                var list = new List<object>();
                foreach (var element in node.Elements())
                {
                    Parse(list, element);
                }

                //AddProperty(item, node.Elements().First().Name.LocalName, list);
                AddProperty(parent, node.Name.ToString(), list);
            }
            else
            {
                var item = new Dictionary<string, object>();

                foreach (var attribute in node.Attributes())
                {
                    AddProperty(item, attribute.Name.ToString(), attribute.Value.Trim());
                }

                //element
                foreach (var element in node.Elements())
                {
                    Parse(item, element);
                }

                AddProperty(parent, node.Name.ToString(), item);
            }
        }
        else
        {
            AddProperty(parent, node.Name.ToString(), node.Value.Trim());
        }
    }

    private static void AddProperty(dynamic parent, string name, object value)
    {
        if (parent is List<object>)
        {
            (parent as List<object>).Add(value);
        }
        else
        {
            (parent as IDictionary<String, object>)[name] = value;
        }
    }

}

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

        Template vltTemplate = vltEngine.GetTemplate("HelloNVelocity.vm");

        VelocityContext vltContext = new VelocityContext();
        //Test1
        List<string> list = new List<string> { "a", "b", "c", "d" };
        vltContext.Put("list", list);

        //Test2
        Table table = new Table { TableName = "user", Remark = "yu-za" };
        vltContext.Put("user", table);

        //test3     
        vltContext.Put("owner", "Unmi");
        vltContext.Put("bill", "1000");
        vltContext.Put("type", "报销单");
        vltContext.Put("date", DateTime.Now.ToLongDateString());
        //test4
        //可以利用这个方法把Excel抽出来的数据做成数据结构供Velocity使用
        Dictionary<string, string> dict = new Dictionary<string, string>();
        dict.Add("username", "1111");
        dict.Add("age", "2222");
        vltContext.Put("dict", dict);

        //这个是演示XmlToGenericObject如何使用，需要结合VM模板一起使用
        System.IO.File.WriteAllText(@"C:\Users\Public\contacts1.xml", view[1, 1]);
        var xDoc = XDocument.Load(new StreamReader(@"C:\Users\Public\contacts1.xml"));
        dynamic root = new Dictionary<string, object>();

        XmlToGenericObject.Parse(root, xDoc.Elements().First());
        vltContext.Put("root", root);
        DevelopWorkspace.Base.Logger.WriteLine(DevelopWorkspace.Base.Dump.ToDump(root));
        StringWriter vltWriter = new StringWriter();
        vltTemplate.Merge(vltContext, vltWriter);
        //DevelopWorkspace.Base.Logger.WriteLine(root[0]["TableName"]);

        DevelopWorkspace.Base.Logger.WriteLine(vltWriter.GetStringBuilder().ToString());



    }
}