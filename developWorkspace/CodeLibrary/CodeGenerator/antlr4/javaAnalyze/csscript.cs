using System;
using System.Drawing;
using System.IO;
using System.Data.SQLite;
using System.Data;
using Microsoft.CSharp;
using System.Collections.Generic;
using DevelopWorkspace.Base;
using Excel = Microsoft.Office.Interop.Excel;
using System.Threading;
using System.Threading.Tasks;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Antlr4.Runtime.Misc;
using Code.Generator;
using System.Collections.Generic;
using System.Linq;

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
public class Script
{
    public static void Main(string[] args)
    {
        string retString = System.DateTime.Now.ToString();

        //var lines = File.ReadAllText("helloworld.java");
        var lines = args[0];
        //IList<InsurancePolicyData> insurancePolicyDataList = new List<InsurancePolicyData>();
        var stream = new AntlrInputStream(lines);
        var lexer = new JavaLexer(stream);
        var tokens = new CommonTokenStream(lexer);
        var parser = new JavaParser(tokens);

        parser.BuildParseTree = true;

        //listener
        //JavaParserCustomListener extractor = new JavaParserCustomListener();
        //ParseTreeWalker parseTreeWalker = new ParseTreeWalker();
        //parseTreeWalker.Walk(extractor, ctx);

        var visitor = new CompilationUnitVisitor();
        var results = visitor.Visit(parser.compilationUnit());
        DevelopWorkspace.Base.Logger.WriteLine(DevelopWorkspace.Base.Dump.ToDump(results), Level.DEBUG);

    }
}