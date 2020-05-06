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
using Java.Code;
using System.Linq;

public class Script
{
    public static void Main(string[] args)
    {
        walkDirectoryRecursive(new DirectoryInfo(@"C:\wbc_sam\workspace\wbc-sam\src\main\java\com\water_biz_c\sam\controller"));

    }
    public static string aggregateString(IEnumerable<string> listString)
    {
        if (listString.Count() == 0)
        {
            return "";
        }
        return listString.Aggregate((total, next) => total + "\t" + next);
    }

    static void parseSourceFile(string filepath)
    {
        var lines = File.ReadAllText(filepath);
        var results = Java.Code.SourcefileParser.GetJavaClazzInformation(lines);
        foreach (JavaClazz javaClazz in results)
        {
            string outputString = "";
            outputString += javaClazz.clazzName;
            outputString += "\n";
            outputString += "annotationList\t" + aggregateString(javaClazz.annotationList.Select(annotation => annotation.qualifiedName));
            outputString += "\n";
            outputString += "modifierList\t" + aggregateString(javaClazz.modifierList);
            outputString += "\n";
            outputString += "propertyList\t" + aggregateString(javaClazz.propertyList.Select(property => property.propertyName));
            outputString += "\n";
            outputString += "methodCallList";
            outputString += "\n";
            foreach (ClazzMethod clazzMethod in javaClazz.methodList)
            {
                outputString += "\t" + clazzMethod.methodType;
                outputString += "\n";
                outputString += "\t" + clazzMethod.methodName;
                outputString += "\n";
                outputString += "\t\t" + "annotationList\t" + aggregateString(clazzMethod.annotationList.Select(annotation => annotation.qualifiedName));
                outputString += "\n";
                outputString += "\t\t" + "modifierList\t" + aggregateString(clazzMethod.modifierList);
                outputString += "\n";
                outputString += "\t\t" + "parametereList\t" + aggregateString(clazzMethod.parametereList.Select(parameter => parameter.parameterName));
                outputString += "\n";
                outputString += "\t\t" + "localVariableList\t" + aggregateString(clazzMethod.localVariableList.Select(property => property.propertyName));
                outputString += "\n";
                outputString += "\t\t" + "methodCallList\t" + aggregateString(clazzMethod.methodCallList.Select(methodcall => methodcall.calleeName + "." + methodcall.methodName + "()"));
                outputString += "\n";

            }
            DevelopWorkspace.Base.Logger.WriteLine(outputString);
        }
    }
    static void walkDirectoryRecursive(System.IO.DirectoryInfo root)
    {
        System.IO.FileInfo[] files = null;
        System.IO.DirectoryInfo[] subDirs = null;

        // First, process all the files directly under this folder
        try
        {
            files = root.GetFiles("*.java");
        }
        catch (System.IO.DirectoryNotFoundException e)
        {
        }

        if (files != null)
        {
            foreach (System.IO.FileInfo fi in files)
            {
                Console.WriteLine(fi.FullName);
                parseSourceFile(fi.FullName);
            }

            // Now find all the subdirectories under this directory.
            subDirs = root.GetDirectories();

            foreach (System.IO.DirectoryInfo dirInfo in subDirs)
            {
                // Resursive call for each subdirectory.
                walkDirectoryRecursive(dirInfo);
            }
        }
    }



}