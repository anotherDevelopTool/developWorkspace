using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using Antlr4.Runtime;
using Java.Code;
using DevelopWorkspace.Base;
using Heidesoft.Components.Controls;
using ICSharpCode.AvalonEdit.Highlighting;
using Microsoft.CSharp;
using Microsoft.VisualBasic.FileIO;
using Microsoft.VisualBasic;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using NVelocity.App;
using NVelocity.Runtime;
using NVelocity;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Text;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Threading;
using System.Windows;
using System.Xml.Linq;
using System.Xml.Xsl;
using System.Xml;
using System;
using WPFMediaKit;
using Xceed.Wpf.AvalonDock.Layout;

class JavaCodeUtility
{

    public class Clazz
    {
        public String name;
        public List<Method> methods;
        public List<Property> properties;
    }

    public class Method
    {
        public String type;
        public String name;
        public List<Parameter> parameteres;
    }
    public class Property
    {
        public String type;
        public String name;
    }
    public class Parameter
    {
        public String type;
        public String name;
    }
    public class Instruction
    {
        public String name;
    }

    // 2019/09/29
    // java parser visitor
    public class CompilationUnitVisitor : JavaParserBaseVisitor<List<Clazz>>
    {
        public override List<Clazz> VisitCompilationUnit([NotNull] JavaParser.CompilationUnitContext context)
        {
            //context和typeDeclaration是1对多的关系，下面这样的语句不能正确取回值对象，需要获取路径要明确才可以
            //var retClazz = context.Accept(new TypeDeclarationVisitor());
            var visitor = new TypeDeclarationVisitor();
            //var retClazz = context.typeDeclaration()[0].Accept(visitor);
            List<Clazz> clazzes = new List<Clazz>();
            context.typeDeclaration().ToList().ForEach(ctx => clazzes.Add(ctx.Accept(visitor)));

            return clazzes;
        }
    }
    public class TypeDeclarationVisitor : JavaParserBaseVisitor<Clazz>
    {
        public override Clazz VisitTypeDeclaration([NotNull] JavaParser.TypeDeclarationContext context)
        {
            var retClazz = context.classDeclaration().Accept(new ClassVisitor());
            return retClazz;
        }
    }
    public class ClassVisitor : JavaParserBaseVisitor<Clazz>
    {
        public override Clazz VisitClassDeclaration([NotNull] JavaParser.ClassDeclarationContext context)
        {

            List<Method> methods = new List<Method>();
            List<Property> properties = new List<Property>();
            var memberVisitor = new MemberVisitor();

            context.classBody().classBodyDeclaration().ToList().ForEach(
                ctx =>
                {
                    var member = ctx.Accept(memberVisitor);
                    if (member != null)
                    {
                        if (member.GetType().IsAssignableFrom(typeof(Method)))
                        {
                            methods.Add((Method)member);
                        }
                        else if (member.GetType().IsAssignableFrom(typeof(Property)))
                        {
                            properties.Add((Property)member);
                        }
                    }
                }
            );
            return new Clazz() { name = context.IDENTIFIER().ToString(), methods = methods, properties = properties };
        }
    }

    public class MemberVisitor : JavaParserBaseVisitor<object>
    {
        public override object VisitMemberDeclaration([NotNull] JavaParser.MemberDeclarationContext context)
        {
            object returnObject = null;
            //分歧处理
            //memberDeclaration
            //    : methodDeclaration
            //    | genericMethodDeclaration
            //    | fieldDeclaration
            //    | constructorDeclaration
            //    | genericConstructorDeclaration
            //    | interfaceDeclaration
            //    | annotationTypeDeclaration
            //    | classDeclaration
            //    | enumDeclaration
            if (context.fieldDeclaration() != null)
            {
                returnObject = context.fieldDeclaration().Accept(new PropertyVisitor());
            }
            else if (context.methodDeclaration() != null)
            {
                returnObject = context.methodDeclaration().Accept(new MethodVisitor());
            }
            return returnObject;
        }
    }

    public class MethodVisitor : JavaParserBaseVisitor<Method>
    {
        public override Method VisitMethodDeclaration([NotNull] JavaParser.MethodDeclarationContext context)
        {
            var typeString = context.typeTypeOrVoid().Accept(new TypeTypeOrVoidVisitor());
            var parameteres = context.formalParameters().Accept(new FormalParametersVisitor());
            return new Method() { type = typeString, name = context.IDENTIFIER().GetText().ToString(), parameteres = parameteres };
        }
    }
    public class PropertyVisitor : JavaParserBaseVisitor<Property>
    {
        public override Property VisitFieldDeclaration([NotNull] JavaParser.FieldDeclarationContext context)
        {
            var typeString = context.typeType().Accept(new TypeVisitor());
            return new Property() { type = typeString, name = context.variableDeclarators().GetText().ToString() };
        }
    }

    public class FormalParametersVisitor : JavaParserBaseVisitor<List<Parameter>>
    {
        public override List<Parameter> VisitFormalParameters([NotNull] JavaParser.FormalParametersContext context)
        {
            List<Parameter> parameteres = new List<Parameter>();
            if (context.formalParameterList() != null) context.formalParameterList().formalParameter().ToList().ForEach(
                ctx => parameteres.Add(
                new Parameter()
                {
                    //这里1对一的关系，直接取对象的子context,
                    type = ctx.typeType().GetText().ToString(),
                    name = ctx.variableDeclaratorId().GetText().ToString()
                }
                )
            );

            return parameteres;

        }
    }

    public class TypeTypeOrVoidVisitor : JavaParserBaseVisitor<string>
    {
        public override string VisitTypeTypeOrVoid([NotNull] JavaParser.TypeTypeOrVoidContext context)
        {

            return context.GetText();
        }
    }
    public class TypeVisitor : JavaParserBaseVisitor<string>
    {
        public override string VisitTypeType([NotNull] JavaParser.TypeTypeContext context)
        {

            return context.GetText();
        }
    }

}
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
            else
            {
                string JAVA_PATTERN = @"^\s{0,}public[ ]+?class";
                regex = new Regex(JAVA_PATTERN, RegexOptions.Multiline);
                result = regex.Match(dataSource);
                if (result.Success)
                {
                    dataSourceType = 3;
                }
            }
        }
        DevelopWorkspace.Base.Logger.WriteLine(dataSourceType.ToString(), Level.DEBUG);
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
                //JAVA type
                else if (ConvertUtil.DataSourceType(dataSource.Text) == 3)
                {
                    //IList<InsurancePolicyData> insurancePolicyDataList = new List<InsurancePolicyData>();
                    var stream = new AntlrInputStream(dataSource.Text);
                    var lexer = new JavaLexer(stream);
                    var tokens = new CommonTokenStream(lexer);
                    var parser = new JavaParser(tokens);

                    parser.BuildParseTree = true;

                    var visitor = new JavaCodeUtility.CompilationUnitVisitor();
                    var results = visitor.Visit(parser.compilationUnit());
                    DevelopWorkspace.Base.Logger.WriteLine(DevelopWorkspace.Base.Dump.ToDump(results), Level.DEBUG);
                }
                //CSV format with tab delimiter
                else
                {
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
                //Java etc
                else if (ConvertUtil.DataSourceType(dataSource.Text) == 3)
                {
                    typeConverter = new HighlightingDefinitionTypeConverter();
                    csSyntaxHighlighter = (IHighlightingDefinition)typeConverter.ConvertFrom("Java");
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