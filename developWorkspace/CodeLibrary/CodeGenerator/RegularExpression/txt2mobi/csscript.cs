using System;
using Microsoft.Office.Interop.Excel;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using DevelopWorkspace.Base;
public class Script
{
    public static void Main(string[] args)
    {
        try
        {

            Script.WalkDirectoryTree(new DirectoryInfo(@"C:\workspace\My Book\test1"));
        }
        catch (Exception e)
        {
            DevelopWorkspace.Base.Logger.WriteLine(e.Message);
        }
    }


    /// <summary>
    /// 对已打开的BOOK的所有Sheet进行走查，可以追加过滤开关
    /// </summary>
    /// <param name="thisBook"></param>
    static void WalkWorkBook(System.IO.FileInfo fileInfo)
    {
        //DevelopWorkspace.Base.Logger.WriteLine(fileInfo.Name);
        //DevelopWorkspace.Base.Logger.WriteLine(fileInfo.Extension);
        //按行读取为字符串数组
        string[] lines = System.IO.File.ReadAllLines(fileInfo.FullName);

        //begin 目　次
        //end ［＃改ページ］ or empty line 
        int bMokuji = 0;
        Regex startTokenMokuji = new Regex("(^　*目　*次　*$)|(^　*もくじ　*$)");
        Regex stopTokenNextPage = new Regex("^　*［＃改ページ］　*$");
        Regex stopTokenEmptyLine = new Regex("^$");

        Regex charpterToken = new Regex("^　*第.*[章|話]　*[^　]*$");
        Regex charSectToken = new Regex("^　*([一二三四五六七八九十]+)　*[^　、。]{0,15}$");
        Regex numSectToken = new Regex("^　*([１２３４５６７８９０]+)　*[^　、。]{0,15}$");

        //如果存在目录内容那么取出它留作后面替换时判断用
        List<string> mokujiList = new List<string>();
        int iMokujiStopLine = 0;
        foreach (string line in lines)
        {
            iMokujiStopLine++;
            if (bMokuji == 0)
            {
                if (startTokenMokuji.IsMatch(line)) { bMokuji = 1; continue; };
            }
            if (bMokuji == 1)
            {
                if (!stopTokenEmptyLine.IsMatch(line)) { bMokuji = 2; };
            }
            if (bMokuji == 2)
            {
                if (stopTokenEmptyLine.IsMatch(line) || stopTokenNextPage.IsMatch(line)) { bMokuji = 0; break; };
                mokujiList.Add("(［＃見出し］　*)?"+line.Trim('　')+"(［＃.*］)?");
            }
        }
        mokujiList.Add("dummy");
        Regex regMokuji = new Regex(mokujiList.Aggregate((a, b) => "^　*" + a + "　*$" + "|" + "^　*" + b + "　*$"));
        int iCurrentLine = 0;
        int iLevel1 = 0, iLevel2 = 0, iLevel3 = 0, iLevel4 = 0;
        List<string> copyLines = new List<string>();
        foreach (string line in lines)
        {
            iCurrentLine++;
            if (regMokuji.IsMatch(line))
            {
                if (iCurrentLine > iMokujiStopLine)
                {
                    iLevel1 = 1;
                    copyLines.Add("<h1>" + line + "</h1>");
                    //DevelopWorkspace.Base.Logger.WriteLine("1");
                   //DevelopWorkspace.Base.Logger.WriteLine(line);
                    
                }
                else
                    copyLines.Add(line + "<br>");
            }
            else if (charpterToken.IsMatch(line))
            {
                iLevel2 = 1;
                copyLines.Add(string.Format("<h{0}>{1}</h{2}>", iLevel1 + iLevel2, line, iLevel1 + iLevel2));
                                    //DevelopWorkspace.Base.Logger.WriteLine("2");
                    //DevelopWorkspace.Base.Logger.WriteLine(line);
            }
            else if (numSectToken.IsMatch(line) || charSectToken.IsMatch(line))
            {
                iLevel3 = 1;
                copyLines.Add(string.Format("<h{0}>{1}</h{2}>", iLevel1 + iLevel2 + iLevel3, line, iLevel1 + iLevel2 + iLevel3));
                                    //DevelopWorkspace.Base.Logger.WriteLine("3");
                    //DevelopWorkspace.Base.Logger.WriteLine(line);
            }
            else
            {
                copyLines.Add(line + "<br>");
            }

        }
        if (iLevel1 + iLevel2 + iLevel3 == 0)
            DevelopWorkspace.Base.Logger.WriteLine(fileInfo.FullName);
        else
        {
            fileInfo.Delete();
            System.IO.File.WriteAllLines(@"C:\workspace\My Book\result\" + fileInfo.Name.Substring(0, fileInfo.Name.Length - ".txt".Length) + ".html", copyLines, System.Text.Encoding.UTF8);
        }

    }
    /// <summary>
    /// 对指定目录进行走查
    /// </summary>
    /// <param name="root"></param>
	static void WalkDirectoryTree(System.IO.DirectoryInfo root)
    {
        System.IO.FileInfo[] files = null;
        System.IO.DirectoryInfo[] subDirs = null;

        try
        {
            files = root.GetFiles("*.txt");
        }
        catch (System.IO.DirectoryNotFoundException e)
        {
            DevelopWorkspace.Base.Logger.WriteLine(e.Message);
        }

        if (files != null)
        {
            foreach (System.IO.FileInfo fi in files)
            {
                WalkWorkBook(fi);
            }
            // Now find all the subdirectories under this directory.
            subDirs = root.GetDirectories();
            foreach (System.IO.DirectoryInfo dirInfo in subDirs)
            {
                // Resursive call for each subdirectory.
                WalkDirectoryTree(dirInfo);
            }
        }
    }
}
