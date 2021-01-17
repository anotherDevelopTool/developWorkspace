using System;
using System.Drawing;
using System.IO;
using System.Data.SQLite;
using System.Data;
using Microsoft.CSharp;
using System.Collections.Generic;
using DevelopWorkspace.Base;
using DevelopWorkspace.Main;
using Excel = Microsoft.Office.Interop.Excel;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel;
using System;
using Microsoft.CSharp;
using System.Drawing;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Xml;
using System.Windows.Markup;
using DevelopWorkspace.Base;
using Heidesoft.Components.Controls;
using System.Windows.Threading;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Diagnostics;
using System.Threading;
using Xceed.Wpf.AvalonDock.Layout;
using System.Reflection;
using DevelopWorkspace.Base;
using DevelopWorkspace.Base.Model;
using DevelopWorkspace.Base.Utils;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Collections.ObjectModel;
using Microsoft.Office.Interop.Word;
public class Script
{
    public static void Main(string[] args)
    {
        //需要appdomain以shared方式执行
        ContextMenuCommand selectCommand = new ContextMenuCommand("export data to activesheet", "対象テーブルのデータをアクティブシートにエクスポートします。", "export_contextmenu",
                        (p) =>
                        {
                            BackgroundWorker backgroundWorker = new BackgroundWorker();
                            backgroundWorker.DoWork += new DoWorkEventHandler((s, ev) =>
                            {
                                DevelopWorkspace.Main.TableInfo tableinfo = p as DevelopWorkspace.Main.TableInfo;
                                if (tableinfo != null)
                                {
                                    tableinfo.exportToActiveSheet();

                                }
                            });

                            backgroundWorker.RunWorkerAsync();
                        },
                        (p) => { return true; });
        if (!Services.dbsupportContextmenuCommandList.Contains(selectCommand))
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)delegate ()
            {
                Services.dbsupportContextmenuCommandList.Add(selectCommand);
            });
        }


        ContextMenuCommand junitCommand = new ContextMenuCommand("export junit formatted data to activesheet", "対象テーブルのデータをJNITフォーマットでアクティブシートにエクスポートします。", "junit",
                        (p) =>
                        {
                            BackgroundWorker backgroundWorker = new BackgroundWorker();
                            backgroundWorker.DoWork += new DoWorkEventHandler((s, ev) =>
                            {
                                DevelopWorkspace.Main.TableInfo tableinfo = p as DevelopWorkspace.Main.TableInfo;
                                if (tableinfo != null)
                                {
                                    string[,] data = tableinfo.getTableDataWithSchema();
                                    if (data != null)
                                    {
                                        List<List<string>> rowList = data.ToNestedList();
                                        rowList.RemoveAt(4);
                                        rowList.RemoveAt(3);
                                        rowList.RemoveAt(2);
                                        rowList.RemoveAt(0);
                                        if (rowList.Count > 4)
                                        {
                                            rowList.RemoveRange(4, rowList.Count - 4);
                                        }
                                        //rowList.Insert(0, new List<string> { tableinfo.TableName });
                                        rowList.exportToActiveSheetOfExcel(headerHeight: 1, schemaHeight: 0, _startRow: 0, _startCol: 0, _isOverwritten: false);
                                    }
                                }
                            });

                            backgroundWorker.RunWorkerAsync();
                        },
                        (p) => { return true; });
        if (!Services.dbsupportContextmenuCommandList.Contains(junitCommand))
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)delegate ()
            {
                Services.dbsupportContextmenuCommandList.Add(junitCommand);
            });
        }
        
        // 扩张主画面，DB，Script默认嵌入机能的Ribbon
        Services.RibbonQueryMain = (object parent) =>
        {
            Fluent.RibbonGroupBox ribbonGroupBox = new Fluent.RibbonGroupBox();
            ribbonGroupBox.Header = "ScriptEnhanced";
            ribbonGroupBox.Width = 120;
            Fluent.Button button = new Fluent.Button();
            button.LargeIcon = DevelopWorkspace.Base.Utils.Files.GetIconFile("word");
            button.Header = "memo";
            button.Margin = new Thickness(5, 0, 5, 0);
            button.Click += (object sender, RoutedEventArgs e) =>
               {
                   DevelopWorkspace.Base.Services.BusyWorkService(new Action(() =>
                   {
                       try
                       {
                           //System.Diagnostics.Process.Start("https://www.google.com/");
                           
                            string helpfile = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "help", "main.docx");
                            if (File.Exists(helpfile))
                            {
                                Microsoft.Office.Interop.Word.Application app = new Microsoft.Office.Interop.Word.Application();
                                app.Documents.Open(helpfile);
                                app.Visible = true;
                                
                            }
                           
                           
                           
                           
                       }
                       catch (Exception ex)
                       {
                           DevelopWorkspace.Base.Logger.WriteLine(ex.Message, DevelopWorkspace.Base.Level.ERROR);
                       }
                   }));

               };
            ribbonGroupBox.Items.Add(button);
            return ribbonGroupBox;
        };
        Services.RibbonQueryDb = (object parent) =>
        {
            Fluent.RibbonGroupBox ribbonGroupBox = new Fluent.RibbonGroupBox();
            ribbonGroupBox.Header = "ScriptEnhanced";
            ribbonGroupBox.Width = 120;
            Fluent.Button button = new Fluent.Button();
            button.LargeIcon = DevelopWorkspace.Base.Utils.Files.GetIconFile("word");
            button.Header = "memo";
            button.Margin = new Thickness(5, 0, 5, 0);
            button.Click += (object sender, RoutedEventArgs e) =>
               {
                   DevelopWorkspace.Base.Services.BusyWorkService(new Action(() =>
                   {
                       try
                       {
                           System.Diagnostics.Process.Start("https://www.google.com/");
                       }
                       catch (Exception ex)
                       {
                           DevelopWorkspace.Base.Logger.WriteLine(ex.Message, DevelopWorkspace.Base.Level.ERROR);
                       }
                   }));

               };
            ribbonGroupBox.Items.Add(button);
            return ribbonGroupBox;
        };
        Services.RibbonQueryScript = (object parent) =>
        {
            Fluent.RibbonGroupBox ribbonGroupBox = new Fluent.RibbonGroupBox();
            ribbonGroupBox.Header = "ScriptEnhanced";
            ribbonGroupBox.Width = 120;
            Fluent.Button button = new Fluent.Button();
            button.LargeIcon = DevelopWorkspace.Base.Utils.Files.GetIconFile("word");
            button.Header = "memo";
            button.Margin = new Thickness(5, 0, 5, 0);
            button.Click += (object sender, RoutedEventArgs e) =>
               {
                   DevelopWorkspace.Base.Services.BusyWorkService(new Action(() =>
                   {
                       try
                       {
                           System.Diagnostics.Process.Start("https://www.google.com/");
                       }
                       catch (Exception ex)
                       {
                           DevelopWorkspace.Base.Logger.WriteLine(ex.Message, DevelopWorkspace.Base.Level.ERROR);
                       }
                   }));

               };
            ribbonGroupBox.Items.Add(button);
            return ribbonGroupBox;
        };


    }
}

