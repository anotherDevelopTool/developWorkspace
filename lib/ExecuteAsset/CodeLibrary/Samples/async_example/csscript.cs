
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
public class Script
{
    public static void Main(string[] args)
    {
        DevelopWorkspace.Base.Logger.WriteLine("Main#1:"+Thread.CurrentThread.ManagedThreadId);
         string retString="main finished";
         
        Action<bool> onCompleteCallback = (flg) => {
        DevelopWorkspace.Base.Logger.WriteLine("onCompleteCallback#1:"+Thread.CurrentThread.ManagedThreadId);
        DevelopWorkspace.Base.Logger.WriteLine("任务结束#callback");
         };
        DoWorkAsync(onCompleteCallback);
         
        DevelopWorkspace.Base.Logger.WriteLine(retString);
    }

    static async void DoWorkAsync(Action<bool> onCompleteCallback)
    {
     DevelopWorkspace.Base.Logger.WriteLine("DoWorkAsync#1:"+Thread.CurrentThread.ManagedThreadId);

        await Task.Run(() => {
            DevelopWorkspace.Base.Logger.WriteLine("DoWorkAsync#2:"+Thread.CurrentThread.ManagedThreadId);

            //模拟其他任务
            Thread.Sleep(2000);
        });
        DevelopWorkspace.Base.Logger.WriteLine("DoWorkAsync#3:"+Thread.CurrentThread.ManagedThreadId);
        onCompleteCallback(true); // Will automatically be called on the original context
        DevelopWorkspace.Base.Logger.WriteLine("任务结束");
    }

}
