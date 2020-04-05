using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using Antlr4.Runtime;
using Java.Code;
using DevelopWorkspace.Base;
using DevelopWorkspace.Base.Codec;
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
using Xceed.Wpf.Toolkit.PropertyGrid;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
public class Script
{
    //--------------------------rule setting begin ------------------------------
    static Func<string, string> cameralConvert = (originalString) => ConvertFunc.CONVERT_FIELD(ConvertFunc.CONVERT_LOWERCASE(originalString));
    static Func<string, string> classnameConvert = (originalString) => ConvertFunc.CONVERT_PROPERTY(ConvertFunc.CONVERT_LOWERCASE(originalString));
    public static List<ConvertRule> convertRules = new List<ConvertRule>()
    {
        new ConvertRule(){ SchemaKey ="name",        SchemaAlias ="alias1",ConvertFunc = ConvertFunc.CONVERT_FIELD },
        new ConvertRule(){ SchemaKey ="DataTypeName",SchemaAlias ="alias2",ConvertFunc = ConvertFunc.CONVERT_PROPERTY },
        new ConvertRule(){ SchemaKey ="ColumnName",  SchemaAlias ="alias2",ConvertFunc = cameralConvert },
        new ConvertRule(){ SchemaKey ="ColumnName",  SchemaAlias ="alias3",ConvertFunc = ConvertFunc.CONVERT_LOWERCASE },
        new ConvertRule(){ SchemaKey ="TableName",  SchemaAlias ="alias2",ConvertFunc = classnameConvert },
        new ConvertRule(){ SchemaKey ="name",        SchemaAlias ="alias3",ConvertFunc = ConvertFunc.CONVERT_UPPERCASE },
        new ConvertRule(){ SchemaKey ="name",        SchemaAlias ="alias4",ConvertFunc = ConvertFunc.CONVERT_LOWERCASE }
    };

    public static List<RenameRule> renameRules = new List<RenameRule>()
    {
        new RenameRule(){ SchemaKey ="type",SchemaAlias ="typeString" },
        new RenameRule(){ SchemaKey ="項目名",SchemaAlias ="ColumnName" }

    };
    //--------------------------rule setting end ------------------------------

    //TODO 面向Addin基类化
    [AddinMeta(Name = "dataConvertTools", Date = "2009-07-20", Description = "代码变换工具")]
    public class ViewModel : DevelopWorkspace.Base.Model.ScriptBaseViewModel
    {
        UserControl view;
        ICSharpCode.AvalonEdit.Edi.EdiTextEditor dataSource;
        ICSharpCode.AvalonEdit.Edi.EdiTextEditor convertRule;
        PropertyGrid propertygrid1;

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
                    TextReader tr = new StringReader(dataSource.Text);
                    var xDoc = XDocument.Load(tr);
                    CodeSupport.Parse(dic, xDoc.Elements().First());
                }
                //JSON type
                else if (ConvertUtil.DataSourceType(dataSource.Text) == 2)
                {
                    Newtonsoft.Json.Linq.JObject htmlAttributes = JsonConvert.DeserializeObject<Newtonsoft.Json.Linq.JObject>(dataSource.Text);
                    dic = (Dictionary<string, object>)CodeSupport.ToCollections(htmlAttributes);
                }
                //JAVA type
                else if (ConvertUtil.DataSourceType(dataSource.Text) == 3)
                {
                    var stream = new AntlrInputStream(dataSource.Text);
                    var lexer = new JavaLexer(stream);
                    var tokens = new CommonTokenStream(lexer);
                    var parser = new JavaParser(tokens);

                    parser.BuildParseTree = true;

                    var visitor = new JavaCodeUtility.CompilationUnitVisitor();
                    var results = visitor.Visit(parser.compilationUnit());
                    DevelopWorkspace.Base.Logger.WriteLine(DevelopWorkspace.Base.Dump.ToDump(results), Level.DEBUG);
                    dic = CodeSupport.ConvertObjectToDictionary(new { classes = results });
                }
                //CSV format with tab delimiter
                else
                {
                    dic = CodeSupport.ReadCsvFileTextFieldParser(dataSource.Text);
                }

                //Func<string, string> CONVERT_LOWERCASE = (originalString) => originalString.ToLower();

                var convert = CodeSupport.ApplyRuleToDictionary(dic, Script.convertRules, Script.renameRules);
                convert["Setting"] = propertygrid1.SelectedObject;
                DevelopWorkspace.Base.Logger.WriteLine("----------------schema information begin-----------------------------", Level.DEBUG);
                DevelopWorkspace.Base.Logger.WriteLine(DevelopWorkspace.Base.Dump.ToDump(convert), Level.DEBUG);
                DevelopWorkspace.Base.Logger.WriteLine("----------------schema information end-------------------------------", Level.DEBUG);

                VelocityContext vltContext = new VelocityContext();
                vltContext.Put("root", convert);

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
            DevelopWorkspace.Base.JsonConfig<CodeConfig>.flush(propertygrid1.SelectedObject as CodeConfig);

            DevelopWorkspace.Base.Logger.WriteLine("Process called");
        }
        [MethodMeta(Name = "elastic mapping", Date = "2009-07-20", Description = "read", LargeIcon = "template")]
        public void EventHandler5(object sender, RoutedEventArgs e)
        {
            currentExt = "ConvertRule";
            convertRule.Text = getResByExt("ConvertRule");
            DevelopWorkspace.Base.Logger.WriteLine("Process called");
        }
        [MethodMeta(Name = "entity(Boot)", Date = "2009-07-20", Description = "read", LargeIcon = "template")]
        public void EventHandler6(object sender, RoutedEventArgs e)
        {
            currentExt = "2.ConvertRule";
            convertRule.Text = getResByExt("2.ConvertRule");
            DevelopWorkspace.Base.Logger.WriteLine("Process called");
        }
        [MethodMeta(Name = "model(Boot)", Date = "2009-07-20", Description = "read", LargeIcon = "template")]
        public void EventHandler7(object sender, RoutedEventArgs e)
        {
            currentExt = "3.ConvertRule";
            convertRule.Text = getResByExt("3.ConvertRule");
            DevelopWorkspace.Base.Logger.WriteLine("Process called");
        }
        [MethodMeta(Name = "model(typescript)", Date = "2009-07-20", Description = "read", LargeIcon = "template")]
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
            propertygrid1 = DevelopWorkspace.Base.Utils.WPF.FindLogicaChild<PropertyGrid>(view, "propertygrid1");

            dataSource.Text = getResByExt("dataSource");
            convertRule.Text = getResByExt("ConvertRule");

            propertygrid1.SelectedObject = DevelopWorkspace.Base.JsonConfig<CodeConfig>.loadByFullPath(getResPathByExt("Setting"));

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

class JavaCodeUtility
{
    public class JavaClassInfo
    {
        public String ClassName { set; get; }
        public List<JavaMethodInfo> methods { set; get; }
        public List<JavaPropertyInfo> properties { set; get; }
    }

    public class JavaMethodInfo
    {
        public String ReturnType { set; get; }
        public String MethodName { set; get; }
        public List<JavaParameterInfo> ParamList { set; get; }
    }
    public class JavaPropertyInfo
    {
        public String PropertyType { set; get; }
        public String PropertyName { set; get; }
    }
    public class JavaParameterInfo
    {
        public String ParamType { set; get; }
        public String ParamName { set; get; }
    }

    // 2019/09/29
    // java parser visitor
    public class CompilationUnitVisitor : JavaParserBaseVisitor<List<JavaClassInfo>>
    {
        public override List<JavaClassInfo> VisitCompilationUnit([NotNull] JavaParser.CompilationUnitContext context)
        {
            //context和typeDeclaration是1对多的关系，下面这样的语句不能正确取回值对象，需要获取路径要明确才可以
            //var retClazz = context.Accept(new TypeDeclarationVisitor());
            var visitor = new TypeDeclarationVisitor();
            //var retClazz = context.typeDeclaration()[0].Accept(visitor);
            List<JavaClassInfo> clazzes = new List<JavaClassInfo>();
            context.typeDeclaration().ToList().ForEach(ctx => clazzes.Add(ctx.Accept(visitor)));

            return clazzes;
        }
    }
    public class TypeDeclarationVisitor : JavaParserBaseVisitor<JavaClassInfo>
    {
        public override JavaClassInfo VisitTypeDeclaration([NotNull] JavaParser.TypeDeclarationContext context)
        {
            var retClazz = context.classDeclaration().Accept(new ClassVisitor());
            return retClazz;
        }
    }
    public class ClassVisitor : JavaParserBaseVisitor<JavaClassInfo>
    {
        public override JavaClassInfo VisitClassDeclaration([NotNull] JavaParser.ClassDeclarationContext context)
        {

            List<JavaMethodInfo> methods = new List<JavaMethodInfo>();
            List<JavaPropertyInfo> properties = new List<JavaPropertyInfo>();
            var memberVisitor = new MemberVisitor();

            context.classBody().classBodyDeclaration().ToList().ForEach(
                ctx =>
                {
                    var member = ctx.Accept(memberVisitor);
                    if (member != null)
                    {
                        if (member.GetType().IsAssignableFrom(typeof(JavaMethodInfo)))
                        {
                            methods.Add((JavaMethodInfo)member);
                        }
                        else if (member.GetType().IsAssignableFrom(typeof(JavaPropertyInfo)))
                        {
                            properties.Add((JavaPropertyInfo)member);
                        }
                    }
                }
            );
            return new JavaClassInfo() { ClassName = context.IDENTIFIER().ToString(), methods = methods, properties = properties };
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

    public class MethodVisitor : JavaParserBaseVisitor<JavaMethodInfo>
    {
        public override JavaMethodInfo VisitMethodDeclaration([NotNull] JavaParser.MethodDeclarationContext context)
        {
            var typeString = context.typeTypeOrVoid().Accept(new TypeTypeOrVoidVisitor());
            var parameteres = context.formalParameters().Accept(new FormalParametersVisitor());
            return new JavaMethodInfo() { ReturnType = typeString, MethodName = context.IDENTIFIER().GetText().ToString(), ParamList = parameteres };
        }
    }
    public class PropertyVisitor : JavaParserBaseVisitor<JavaPropertyInfo>
    {
        public override JavaPropertyInfo VisitFieldDeclaration([NotNull] JavaParser.FieldDeclarationContext context)
        {
            var typeString = context.typeType().Accept(new TypeVisitor());
            return new JavaPropertyInfo() { PropertyType = typeString, PropertyName = context.variableDeclarators().GetText().ToString() };
        }
    }

    public class FormalParametersVisitor : JavaParserBaseVisitor<List<JavaParameterInfo>>
    {
        public override List<JavaParameterInfo> VisitFormalParameters([NotNull] JavaParser.FormalParametersContext context)
        {
            List<JavaParameterInfo> parameteres = new List<JavaParameterInfo>();
            if (context.formalParameterList() != null) context.formalParameterList().formalParameter().ToList().ForEach(
                ctx => parameteres.Add(
                new JavaParameterInfo()
                {
                    //这里1对一的关系，直接取对象的子context,
                    ParamType = ctx.typeType().GetText().ToString(),
                    ParamName = ctx.variableDeclaratorId().GetText().ToString()
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
}
public enum JpaSource { CORE, FRONT, ICS }
public class RenameRuleSetting
{
    public string SchemaKey { set; get; }
    public string SchemaAlias { set; get; }
}
public class CodeConfig : ConfigBase
{
    [System.ComponentModel.DataAnnotations.Schema.Column(Order = 1)]
    [Category("common")]
    [Editor(typeof(Xceed.Wpf.Toolkit.PropertyGrid.Editors.EnumComboBoxEditor), typeof(Xceed.Wpf.Toolkit.PropertyGrid.Editors.EnumComboBoxEditor))]
    public JpaSource SelectedJpaSource { get; set; }
    [Category("common")]
    public string ClassName { get; set; }

    [Category("common")]
    [Editor(typeof(Xceed.Wpf.Toolkit.PropertyGrid.Editors.ComboBoxEditor), typeof(Xceed.Wpf.Toolkit.PropertyGrid.Editors.ComboBoxEditor))]
    public List<RenameRule> renameRules { get; set; }

    public CodeConfig()
    {
    }
    public CodeConfig(int create)
    {

    }
}
