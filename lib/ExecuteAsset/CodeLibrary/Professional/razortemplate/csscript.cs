using System;
using Microsoft.Office.Interop.Excel;
using System.IO;
using System.Text.RegularExpressions;
using DevelopWorkspace.Base;
using RazorEngine;
using RazorEngine.Templating; // For extension methods.
using DevelopWorkspace.Base;
public class Script
{
    public static void Main(string[] args)
    {

        DevelopWorkspace.Base.Logger.WriteLine("Process called");
        string template = "Hello @Model.Name, welcome to RazorEngine!";
        var result =
            Engine.Razor.RunCompile(template, "templateKey", null, new { Name = "World" });
        DevelopWorkspace.Base.Logger.WriteLine(result);

    }
}
