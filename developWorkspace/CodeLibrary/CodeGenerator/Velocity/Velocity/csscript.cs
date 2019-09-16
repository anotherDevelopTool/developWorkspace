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
        string json = @"{
  'Active': false,
  'Roles': [
    'Expired','a','b','c'
  ]
}";
        //使用下面这个方式可以遍历一个Json对象，但是这个方式在vm里可能无法使用，因为Jarray不是一个通用的类型
        Dictionary<string, object> htmlAttributes = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);



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
        Dictionary<string,string> dict = new Dictionary<string,string>();
        dict.Add("username","1111");
        dict.Add("age","2222");
        vltContext.Put("dict", dict);

        StringWriter vltWriter = new StringWriter();
        vltTemplate.Merge(vltContext, vltWriter);

        DevelopWorkspace.Base.Logger.WriteLine(vltWriter.GetStringBuilder().ToString());



    }
}