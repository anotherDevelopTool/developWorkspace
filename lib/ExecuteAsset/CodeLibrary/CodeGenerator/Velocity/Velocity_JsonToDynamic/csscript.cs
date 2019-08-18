using System;
using Microsoft.Office.Interop.Excel;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using DevelopWorkspace.Base;
public class Script
{
    public static void Main(string[] args)
 {
     DevelopWorkspace.Base.Logger.WriteLine("Process called");
 string json = @"{
  'Active': false,
  'Roles': [
    'Expired','a','b','c'
  ]
}";
//使用下面这个方式可以遍历一个Json对象，而且这个方式在也可以使用，虽然Jarray不是一个通用的类型
Dictionary<string, object> htmlAttributes = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
//DevelopWorkspace.Base.Logger.WriteLine(DevelopWorkspace.Base.Dump.ToDump(htmlAttributes["Roles"] ));
DevelopWorkspace.Base.Logger.WriteLine((htmlAttributes["Roles"] as Newtonsoft.Json.Linq.JArray)[0].ToString());
DevelopWorkspace.Base.Logger.WriteLine(DevelopWorkspace.Base.Dump.ToDump(htmlAttributes));
 }




}
