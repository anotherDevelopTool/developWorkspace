using System;
using Microsoft.CSharp;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
//using System.Windows.Shapes;
using System.Xml;
using System.Windows.Markup;
using WPFMediaKit;
using DevelopWorkspace.Base;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Heidesoft.Components.Controls;
using System.Windows.Threading;
using System.Linq;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Diagnostics;
using System.Threading;
using Xceed.Wpf.AvalonDock.Layout;
using System.Windows.Media;
using DevelopWorkspace.Base;
using System.Xml;
using System.Xml.Xsl;
using System.Xml.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Linq;
using System.Reflection;
using NVelocity;
using NVelocity.App;
using NVelocity.Runtime;
using Microsoft.CSharp;
using System.Text.RegularExpressions;
using ICSharpCode.AvalonEdit.Highlighting;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.FileIO;
class ConvertUtil
{
    public static int DataSourceType(string dataSource)
    {
        int dataSourceType = 0;
        string XML_PATTERN = @"^\s{0,}<";
        Regex regex = new Regex(XML_PATTERN, RegexOptions.IgnoreCase);
        var result = regex.Match(dataSource);
        if (result.Success)
        {
            dataSourceType = 1;
        }
        else
        {
            string JSON_PATTERN = @"^\s{0,}{";
            regex = new Regex(JSON_PATTERN, RegexOptions.IgnoreCase);
            result = regex.Match(dataSource);
            if (result.Success)
            {
                dataSourceType = 2;
            }
        }
        return dataSourceType;
    }

    //通过JSON映射成的JOBJECT转换成Dictionary，以方便vecolity使用
    public static object ToCollections(object o)
    {
        var jo = o as JObject;
        if (jo != null) return jo.ToObject<IDictionary<string, object>>().ToDictionary(k => k.Key, v => ToCollections(v.Value));
        var ja = o as JArray;
        if (ja != null) return ja.ToObject<List<object>>().Select(ToCollections).ToList();
        return o;
    }
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

    public static Dictionary<string, object> ReadCsvFileTextFieldParser(string csvText, string delimiter = "\t")
    {

        var list = new List<Dictionary<string, string>>();
        var fieldDict = new Dictionary<int, string>();

        using (TextFieldParser parser = new TextFieldParser(new StringReader(csvText)))
        {
            parser.SetDelimiters(delimiter);

            bool headerParsed = false;

            while (!parser.EndOfData)
            {
                //Processing row
                string[] rowFields = parser.ReadFields();
                if (!headerParsed)
                {
                    for (int i = 0; i < rowFields.Length; i++)
                    {
                        fieldDict.Add(i, rowFields[i]);
                    }
                    headerParsed = true;
                }
                else
                {
                    var rowData = new Dictionary<string, string>();
                    for (int i = 0; i < rowFields.Length; i++)
                    {
                        rowData[fieldDict[i]] = rowFields[i];
                    }
                    list.Add(rowData);
                }
            }
        }
        Dictionary<string, object> ret = new Dictionary<string, object>();
        ret["rowdata"] = list;
        return ret;
    }


}

public class Script
{


    //https://stackoverflow.com/questions/248362/how-do-i-build-a-datatemplate-in-c-sharp-code
    //TODO 面向Addin基类化
    [AddinMeta(Name = "dataConvertTools", Date = "2009-07-20", Description = "代码变换工具")]
    public class ViewModel : DevelopWorkspace.Base.Model.ScriptBaseViewModel
    {
        UserControl view;
        ICSharpCode.AvalonEdit.Edi.EdiTextEditor dataSource;
        ICSharpCode.AvalonEdit.Edi.EdiTextEditor convertRule;
        string currentExt = "ConvertRule";

        [MethodMeta(Name = "变换", Date = "2009-07-20", Description = "变换", LargeIcon = "convert")]
        public void EventHandler2(object sender, RoutedEventArgs e)
        {
            try
            {
                DevelopWorkspace.Base.Logger.WriteLine(convertRule.Text);


                VelocityEngine vltEngine = new VelocityEngine();
                vltEngine.Init();

                dynamic dic = new Dictionary<string, object>();
                //XML type
                if (ConvertUtil.DataSourceType(dataSource.Text) == 1)
                {

                    //这个是演示XmlToGenericObject如何使用，需要结合VM模板一起使用
                    System.IO.File.WriteAllText(@"C:\Users\Public\contacts1.xml", dataSource.Text);
                    var xDoc = XDocument.Load(new StreamReader(@"C:\Users\Public\contacts1.xml"));
                    ConvertUtil.Parse(dic, xDoc.Elements().First());
                }
                //JSON type
                else if (ConvertUtil.DataSourceType(dataSource.Text) == 2)
                {
                    Newtonsoft.Json.Linq.JObject htmlAttributes = JsonConvert.DeserializeObject<Newtonsoft.Json.Linq.JObject>(dataSource.Text);
                    dic = (Dictionary<string, object>)ConvertUtil.ToCollections(htmlAttributes);

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
                }
                //CSV format with tab delimiter
                else {
                    dic = ConvertUtil.ReadCsvFileTextFieldParser(dataSource.Text);
                }

                DevelopWorkspace.Base.Logger.WriteLine(DevelopWorkspace.Base.Dump.ToDump(dic), Level.DEBUG);

                VelocityContext vltContext = new VelocityContext();
                vltContext.Put("root", dic);

                StringWriter vltWriter = new StringWriter();
                vltEngine.Evaluate(vltContext, vltWriter, "", convertRule.Text);
                DevelopWorkspace.Base.Logger.WriteLine(vltWriter.GetStringBuilder().ToString());
            }
            catch (Exception ex)
            {
                DevelopWorkspace.Base.Logger.WriteLine(ex.ToString());
            }
        }

        [MethodMeta(Name = "保存", Date = "2009-07-20", Description = "保存", LargeIcon = "save")]
        public void EventHandler4(object sender, RoutedEventArgs e)
        {
            saveResByExt(dataSource.Text, "dataSource");
            saveResByExt(convertRule.Text, currentExt);
            DevelopWorkspace.Base.Logger.WriteLine("Process called");
        }
        [MethodMeta(Name = "elastic mapping", Date = "2009-07-20", Description = "read", LargeIcon = "template")]
        public void EventHandler5(object sender, RoutedEventArgs e)
        {
            currentExt = "ConvertRule";
            convertRule.Text = getResByExt("ConvertRule");
            DevelopWorkspace.Base.Logger.WriteLine("Process called");
        }
        [MethodMeta(Name = "entity", Date = "2009-07-20", Description = "read", LargeIcon = "template")]
        public void EventHandler6(object sender, RoutedEventArgs e)
        {
            currentExt = "2.ConvertRule";
            convertRule.Text = getResByExt("2.ConvertRule");
            DevelopWorkspace.Base.Logger.WriteLine("Process called");
        }
        [MethodMeta(Name = "model", Date = "2009-07-20", Description = "read", LargeIcon = "template")]
        public void EventHandler7(object sender, RoutedEventArgs e)
        {
            currentExt = "3.ConvertRule";
            convertRule.Text = getResByExt("3.ConvertRule");
            DevelopWorkspace.Base.Logger.WriteLine("Process called");
        }
        [MethodMeta(Name = "builder", Date = "2009-07-20", Description = "read", LargeIcon = "template")]
        public void EventHandler8(object sender, RoutedEventArgs e)
        {
            currentExt = "4.ConvertRule";
            convertRule.Text = getResByExt("4.ConvertRule");
            DevelopWorkspace.Base.Logger.WriteLine("Process called");
        }
        [MethodMeta(Name = "TableSchemeCsv", Date = "2009-07-20", Description = "read", LargeIcon = "template")]
        public void EventHandler9(object sender, RoutedEventArgs e)
        {
            currentExt = "9.ConvertRule";
            convertRule.Text = getResByExt("9.ConvertRule");
            DevelopWorkspace.Base.Logger.WriteLine("Process called");
        }
        public override UserControl getView(string strXaml)
        {
            StringReader strreader = new StringReader(strXaml);
            XmlTextReader xmlreader = new XmlTextReader(strreader);
            view = XamlReader.Load(xmlreader) as UserControl;
            dataSource = DevelopWorkspace.Base.Utils.WPF.FindLogicaChild<ICSharpCode.AvalonEdit.Edi.EdiTextEditor>(view, "dataSource");
            convertRule = DevelopWorkspace.Base.Utils.WPF.FindLogicaChild<ICSharpCode.AvalonEdit.Edi.EdiTextEditor>(view, "convertRule");
            dataSource.Text = getResByExt("dataSource");
            convertRule.Text = getResByExt("ConvertRule");

            var typeConverter = new HighlightingDefinitionTypeConverter();
            var csSyntaxHighlighter = (IHighlightingDefinition)typeConverter.ConvertFrom("VTL");
            //JavaScript,SQL,Ruby,XML,ASP/XHTML
            this.convertRule.SyntaxHighlighting = csSyntaxHighlighter;

            dataSource.TextChanged += (obj, subargs) =>
            {
                //XML type
                if (ConvertUtil.DataSourceType(dataSource.Text) == 1)
                {
                    typeConverter = new HighlightingDefinitionTypeConverter();
                    csSyntaxHighlighter = (IHighlightingDefinition)typeConverter.ConvertFrom("XML");
                    this.dataSource.SyntaxHighlighting = csSyntaxHighlighter;
                }
                //JSON etc
                else if (ConvertUtil.DataSourceType(dataSource.Text) == 2)
                {
                    typeConverter = new HighlightingDefinitionTypeConverter();
                    csSyntaxHighlighter = (IHighlightingDefinition)typeConverter.ConvertFrom("C#");
                    this.dataSource.SyntaxHighlighting = csSyntaxHighlighter;
                }
            };
            return view;
        }
    }

    public class MainWindow : Window
    {
        private Label label1;

        public MainWindow(string strXaml)
        {
            Width = 1000;
            Height = 800;

            Grid grid = new Grid();
            Content = grid;

            StackPanel parent = new StackPanel();
            grid.Children.Add(parent);

            ViewModel model = new ViewModel();

            var methods = model.GetType().GetMethods().Where(m => m.GetCustomAttributes(typeof(MethodMetaAttribute), false).Length > 0).ToList();
            for (int i = 0; i < methods.Count; i++)
            {
                var method = methods[i];
                var methodAttribute = (MethodMetaAttribute)Attribute.GetCustomAttribute(methods[i], typeof(MethodMetaAttribute));
                Button btn = new Button();
                btn.Content = methodAttribute.Name; ;
                parent.Children.Add(btn);
                btn.Click += (obj, subargs) =>
                {
                    method.Invoke(model, new object[] { obj, subargs });
                };
            }

            parent.Children.Add(model.getView(strXaml));


            model.install(strXaml);


        }
        void button1_Click(object sender, RoutedEventArgs e)
        {
            label1.Content = "Hello WPF!";
        }
    }
    public static void Main(string[] args)
    {
        DevelopWorkspace.Base.Logger.WriteLine("Process called");

        string strXaml = args[0].ToString();


        MainWindow win = new MainWindow(strXaml);


        win.Show();


    }


}