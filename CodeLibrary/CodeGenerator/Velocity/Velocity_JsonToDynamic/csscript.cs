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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class Script
{
    //通过JSON映射成的JOBJECT转换成Dictionary，以方便vecolity使用
    public static object ToCollections(object o)
    {
        var jo = o as JObject;
        if (jo != null) return jo.ToObject<IDictionary<string, object>>().ToDictionary(k => k.Key, v => ToCollections(v.Value));
        var ja = o as JArray;
        if (ja != null) return ja.ToObject<List<object>>().Select(ToCollections).ToList();
        return o;
    }
    public static void Main(string[] args)
    {
        DevelopWorkspace.Base.Logger.WriteLine("Process called");

        VelocityEngine vltEngine = new VelocityEngine();
        vltEngine.Init();

		Newtonsoft.Json.Linq.JObject htmlAttributes = JsonConvert.DeserializeObject<Newtonsoft.Json.Linq.JObject>(args[0]);
		Dictionary<string,object> dic = (Dictionary < string,object>) ToCollections(htmlAttributes);
        
        //如果需要对某些字段进行定制，需要如下增加属性的方式提供给vecolity使用
        //var mappings = (Dictionary<string, object>)dic["mappings"];
        //var fields = (Dictionary<string, object>)mappings["properties"];
        //foreach (var field in fields)
        //{
        //    System.Diagnostics.Debug.WriteLine(field.Key);
        //    var raws = (Dictionary<string, object>)field.Value;
        //    raws["camel"] = field.Key + "???";
        //    foreach (var raw in raws)
        //    {
        //        System.Diagnostics.Debug.WriteLine(raw.Key + "---" + raw.Value);
        //    }
        //}
        
        VelocityContext vltContext = new VelocityContext();
        vltContext.Put("root", dic);

        StringWriter vltWriter = new StringWriter();
		vltEngine.Evaluate(vltContext,vltWriter,"",args[1]);

        DevelopWorkspace.Base.Logger.WriteLine(vltWriter.GetStringBuilder().ToString());

    }
}