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
using Heidesoft.Components.Controls;
using System.Windows.Threading;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Diagnostics;
using Xceed.Wpf.AvalonDock.Layout;
using System.Reflection;
using DevelopWorkspace.Base.Model;
using DevelopWorkspace.Base.Utils;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;
using Microsoft.Office.Interop.Word;
using AutoIt;
//css_reference AutoItX3.Assembly.dll
public class Script
{
    [DllImport("User32.dll")]
    private static extern bool SetForegroundWindow(IntPtr handle);

    [DllImport("User32.dll")]
    private static extern bool ShowWindow(IntPtr handle, int nCmdShow);

    private const int SW_SHOWNORMAL = 1;
    private const int SW_SHOWMAXIMIZED = 3;

    public static void BringToFront(IntPtr handle)
    {
        if (handle == IntPtr.Zero)
            return;

        // Maximize window
        ShowWindow(handle, SW_SHOWNORMAL);

        SetForegroundWindow(handle);
    }
    public static void OpenMemoFile(string filename)
    {
        string helpfile = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "help", filename);
        if (File.Exists(helpfile))
        {
            Microsoft.Office.Interop.Word.Application app = new Microsoft.Office.Interop.Word.Application();
            app.Documents.Open(helpfile);
            app.Visible = true;
            string processName = "WINWORD";
            Process[] processes = Process.GetProcessesByName(processName);
            if (processes.Length == 0) // Process not running
            { }
            else // Process running
            {
                BringToFront(processes[0].MainWindowHandle);
            }

        }
    }
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


        ContextMenuCommand indexInfoCommand = new ContextMenuCommand("output index information", "対象テーブルのインデクス情報を出力します。", "indexinfo",
                        (p) =>
                        {
                            BackgroundWorker backgroundWorker = new BackgroundWorker();
                            backgroundWorker.DoWork += new DoWorkEventHandler((s, ev) =>
                            {
                                DevelopWorkspace.Main.TableInfo tableinfo = p as DevelopWorkspace.Main.TableInfo;
                                if (tableinfo == null)
                                {
                                    return;

                                }

                                try
                                {
                                    tableinfo.XLAppRef.DbConnection.Open();

                                    var cmd = tableinfo.XLAppRef.DbConnection.CreateCommand();

                                    Regex regex = new Regex(@"^\s{0,}select\b", RegexOptions.IgnoreCase);

                                    string commandText = "";
                                    string SelectedText = @"select 
b.uniqueness, a.index_name, a.table_name, a.column_name
from all_ind_columns a, all_indexes b
where a.index_name=b.index_name 
and a.table_name = upper('" + tableinfo.TableName + @"')
order by a.table_name, a.index_name, a.column_position";

                                    string mysqlSelectedText = @"
select index_schema,
       index_name,
       group_concat(column_name order by seq_in_index) as index_columns,
       index_type,
       case non_unique
            when 1 then 'Not Unique'
            else 'Unique'
            end as is_unique,
        table_name
from information_schema.statistics
where table_schema not in ('information_schema', 'mysql',
                           'performance_schema', 'sys')
                           and table_name='" + tableinfo.TableName + @"'
group by index_schema,
         index_name,
         index_type,
         non_unique,
         table_name
order by index_schema,
         index_name";

                                    if (tableinfo.XLAppRef.Provider.ProviderID == 3)
                                    {
                                        SelectedText = mysqlSelectedText;
                                    }

                                    if (regex.Match(SelectedText).Success)
                                        commandText = tableinfo.XLAppRef.Provider.LimitCondition.FormatWith(new { RawSQL = SelectedText, MaxRecord = AppConfig.DatabaseConfig.This.maxRecordCount });
                                    else
                                        commandText = SelectedText;

                                    cmd.CommandText = commandText;
                                    DevelopWorkspace.Base.Logger.WriteLine(cmd.CommandText, DevelopWorkspace.Base.Level.DEBUG);

                                    List<string> titleList = new List<string>();
                                    List<int> columnPadSizeList = new List<int>();
                                    List<List<string>> dataListList = new List<List<string>>();
                                    bool titleInitial = false;
                                    Services.executeWithBackgroundAction(() =>
                {
                    using (var rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            if (!titleInitial)
                            {
                                titleInitial = true;
                                for (int idx = 0; idx < rdr.FieldCount; idx++)
                                {
                                    titleList.Add(rdr.GetName(idx).ToString());
                                    columnPadSizeList.Add(rdr.GetName(idx).ToString().Length);
                                }
                            }
                            List<string> dataList = new List<string>();
                            for (int idx = 0; idx < rdr.FieldCount; idx++)
                            {
                                string data = rdr[idx] == null ? "" : rdr[idx].ToString();
                                dataList.Add(data);
                                if (columnPadSizeList[idx] < data.Length) columnPadSizeList[idx] = data.Length;
                            }
                            dataListList.Add(dataList);
                        }
                    }
                    if (dataListList.Count > 0)
                    {
                        string titleOutput = "";
                        for (int idx = 0; idx < titleList.Count; idx++)
                        {
                            titleOutput += string.Format("{0," + (0 - columnPadSizeList[idx] - 4) + "}", titleList[idx]);
                        }
                        DevelopWorkspace.Base.Logger.WriteLine(titleOutput);
                        int outputLimit = dataListList.Count;
                        if (outputLimit > AppConfig.DatabaseConfig.This.maxRecordCount) outputLimit = AppConfig.DatabaseConfig.This.maxRecordCount;
                        for (int idx = 0; idx < dataListList.Count; idx++)
                        {
                            string dataOutput = "";
                            for (int jdx = 0; jdx < dataListList[idx].Count; jdx++)
                            {
                                dataOutput += string.Format("{0," + (0 - columnPadSizeList[jdx] - 4) + "}", dataListList[idx][jdx]);
                            }
                            DevelopWorkspace.Base.Logger.WriteLine(dataOutput);
                        }
                    }
                });

                                }
                                catch (Exception ex)
                                {
                                    DevelopWorkspace.Base.Logger.WriteLine(ex.Message, DevelopWorkspace.Base.Level.ERROR);
                                }
                                finally
                                {
                                    tableinfo.XLAppRef.DbConnection.Close();
                                }
                            });

                            backgroundWorker.RunWorkerAsync();
                        },
                        (p) => { return true; });
        if (!Services.dbsupportContextmenuCommandList.Contains(indexInfoCommand))
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)delegate ()
            {
                Services.dbsupportContextmenuCommandList.Add(indexInfoCommand);
            });
        }


        // 扩张主画面，DB，Script默认嵌入机能的Ribbon
        Services.RibbonQueryMain = (object parent) =>
        {
            Fluent.RibbonGroupBox ribbonGroupBox = new Fluent.RibbonGroupBox();
            ribbonGroupBox.Header = "その他";

            {
                Fluent.Button button = new Fluent.Button();
                button.LargeIcon = DevelopWorkspace.Base.Utils.Files.GetIconFile("word");
                button.Header = "メモ";
                button.Margin = new Thickness(1, 0, 1, 0);
                button.Click += (object sender, RoutedEventArgs e) =>
                   {
                       DevelopWorkspace.Base.Services.BusyWorkService(new Action(() =>
                       {
                           try
                           {
                               OpenMemoFile("main.docx");
                           }
                           catch (Exception ex)
                           {
                               DevelopWorkspace.Base.Logger.WriteLine(ex.Message, DevelopWorkspace.Base.Level.ERROR);
                           }
                       }));

                   };
                ribbonGroupBox.Items.Add(button);
            }
            {
                Fluent.Button button = new Fluent.Button();
                button.LargeIcon = DevelopWorkspace.Base.Utils.Files.GetIconFile("confluence");
                button.Header = "Confluence";
                button.Margin = new Thickness(1, 0, 1, 0);
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
            }
            return ribbonGroupBox;

        };
        Services.RibbonQueryDb = (object parent) =>
        {
            Fluent.RibbonGroupBox ribbonGroupBox = new Fluent.RibbonGroupBox();
            ribbonGroupBox.Header = "その他";

            {
                Fluent.Button button = new Fluent.Button();
                button.LargeIcon = DevelopWorkspace.Base.Utils.Files.GetIconFile("word");
                button.Header = "メモ";
                button.Margin = new Thickness(1, 0, 1, 0);
                button.Click += (object sender, RoutedEventArgs e) =>
                   {
                       DevelopWorkspace.Base.Services.BusyWorkService(new Action(() =>
                       {
                           try
                           {
                               OpenMemoFile("dbsupport.docx");
                           }
                           catch (Exception ex)
                           {
                               DevelopWorkspace.Base.Logger.WriteLine(ex.Message, DevelopWorkspace.Base.Level.ERROR);
                           }
                       }));

                   };
                ribbonGroupBox.Items.Add(button);
            }
            {
                Fluent.Button button = new Fluent.Button();
                button.LargeIcon = DevelopWorkspace.Base.Utils.Files.GetIconFile("confluence");
                button.Header = "Confluence";
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
            }
            return ribbonGroupBox;

        };
        Services.RibbonQueryScript = (object parent) =>
        {
            Fluent.RibbonGroupBox ribbonGroupBox = new Fluent.RibbonGroupBox();
            ribbonGroupBox.Header = "その他";

            {
                Fluent.Button button = new Fluent.Button();
                button.LargeIcon = DevelopWorkspace.Base.Utils.Files.GetIconFile("word");
                button.Header = "メモ";
                button.Margin = new Thickness(5, 0, 5, 0);
                button.Click += (object sender, RoutedEventArgs e) =>
                   {
                       DevelopWorkspace.Base.Services.BusyWorkService(new Action(() =>
                       {
                           try
                           {
                               OpenMemoFile("script.docx");
                           }
                           catch (Exception ex)
                           {
                               DevelopWorkspace.Base.Logger.WriteLine(ex.Message, DevelopWorkspace.Base.Level.ERROR);
                           }
                       }));

                   };
                ribbonGroupBox.Items.Add(button);
            }
            {
                Fluent.Button button = new Fluent.Button();
                button.LargeIcon = DevelopWorkspace.Base.Utils.Files.GetIconFile("confluence");
                button.Header = "Confluence";
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
            }
            return ribbonGroupBox;

        };


    }
}

