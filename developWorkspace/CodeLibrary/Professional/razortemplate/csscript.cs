using System;
using Microsoft.Office.Interop.Excel;
using System.IO;
using System.Text.RegularExpressions;
using DevelopWorkspace.Base;
using RazorEngine;
using RazorEngine.Templating; // For extension methods.
using DevelopWorkspace.Base;
using System.Collections.Generic;
using System.Linq;
public class Script
{
    public static void Main(string[] args)
    {

        DevelopWorkspace.Base.Logger.WriteLine("Process called");
        
   
        
        
        string template = "Hello @Model.Name, welcome to RazorEngine!";
            var dic = new Dictionary<string, object>();
            dic["name"] = "dafadfad";
            dic["name1"] = "dafadfad";
            dic["name2"] = "dafadfad";

            var column1 = new Dictionary<string, object>();
            column1["isKey"] = "";
            column1["ColumnName"] = "dafadfad";
            column1["Remark"] = "dafadfad";

            var column2 = new Dictionary<string, object>();
            column2["isKey"] = "*";
            column2["ColumnName"] = "dafadfad";
            column2["Remark"] = "dafadfad";

            var column3 = new Dictionary<string, object>();
            column3["isKey"] = "*";
            column3["ColumnName"] = "dafadfad";
            column3["Remark"] = "dafadfad";
            List<Dictionary<string, object>> columns = new List<Dictionary<string, object>>() { column1, column2, column3 };
            dic["columns"] = columns;
            var keyColumns = (dic["columns"] as List<Dictionary<string, object>>).Where(item => item["isKey"].ToString().Equals("*"));
            
        List<string> list = new List<string>(){"1","2"};
        var select = from paire in list where paire == "name" select paire;
        //var result =Razor.Parse(args[0], new { Name = "World", dict = dic.ToExpando() ,god = list});

        
        var config = new RazorEngine.Configuration.TemplateServiceConfiguration();
        config.Debug = true;
        config.EncodedStringFactory = new RazorEngine.Text.RawStringFactory(); // Raw string encoding.
        var service = RazorEngineService.Create(config);
        
        
string commonRule =@"@functions {
  public static bool hilao(string item){
    return true;
  }
}
";       
//        DevelopWorkspace.Base.Logger.WriteLine(result);
 //          var service = Engine.Razor;
            // In this example I'm using the default configuration, but you should choose a different template manager: http://antaris.github.io/RazorEngine/TemplateManager.html
            //通过layout的方式可以输出一些固定的内容
            service.AddTemplate("layout", "author:@Model @RenderBody()");
            //service.AddTemplate("template", @"@{Layout = ""layout"";}my template");
            //通过使用commonRule附加的方式把一些通用的变换规则共用，如代码生成时类型变换规则
            service.AddTemplate("template", commonRule + "\n" + args[0]);
            service.Compile("template");
            //service.Compile("layout");
            var result1 = service.Run("template",null,new { Name = "World", dict = dic.ToExpando() ,god = list});
            //var result1 = service.Run("template");
             DevelopWorkspace.Base.Logger.WriteLine(result1);
        

    }
}
