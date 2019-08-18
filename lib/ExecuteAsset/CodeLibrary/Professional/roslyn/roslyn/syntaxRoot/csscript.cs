using System;
using System.Runtime;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Threading.Tasks;
using DevelopWorkspace.Base;
//css_reference System.Collections.Immutable.dll
//css_reference System.IO.dll
//css_reference System.IO.FileSystem.dll
//css_reference System.ValueTuple.dll
//css_reference System.Text.Encoding.dll
public class Script
{
    public static void Main(string[] args)
    {
        DevelopWorkspace.Base.Logger.WriteLine("Process called");
        #region 03 SyntaxTree
        var tree = CSharpSyntaxTree.ParseText(@"
        public class MyClass
        {
            public int FnSum(int x, int y)
            {
                return x + y;
            }
        }");

        var syntaxRoot = tree.GetRoot();
        var MyClass = syntaxRoot.DescendantNodes().OfType<ClassDeclarationSyntax>().First();
        var MyMethod = syntaxRoot.DescendantNodes().OfType<MethodDeclarationSyntax>().First();

        DevelopWorkspace.Base.Logger.WriteLine(MyClass.Identifier.ToString());
        DevelopWorkspace.Base.Logger.WriteLine(MyMethod.Identifier.ToString());
        DevelopWorkspace.Base.Logger.WriteLine(MyMethod.ParameterList.ToString());
        #endregion
    }
}