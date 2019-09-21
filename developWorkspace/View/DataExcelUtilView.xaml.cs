using Heidesoft.Components.Controls;
using ICSharpCode.AvalonEdit.Highlighting;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml.Linq;
using System.Data;
using System.Threading;
using DevelopWorkspace.Base.Utils;
using System.Text.RegularExpressions;
using SqlParser = DevelopWorkspace.Base.Utils.SqlParserWrapper;
using System.Diagnostics;
using Xceed.Wpf.Toolkit.PropertyGrid;
using static DevelopWorkspace.Main.AppConfig;
using DevelopWorkspace.Base;
using DevelopWorkspace.Base.Model;
using Newtonsoft.Json;
using DevelopWorkspace.Main.Model;
using System.Runtime.InteropServices;
using System.Globalization;

namespace DevelopWorkspace.Main.View
{
    //https://stackoverflow.com/questions/53890185/how-to-connect-with-odp-net-core-to-oracle-9i-database-managed-driver
    //How to connect with ODP.NET Core to Oracle 9i database - Managed Driver
    //如果使用高于Access to Oracle Database 10g Release 2 or later版本的ORACLE，可以免安装oracle客户端
    //public enum DbProviderKind { SQLite=1,Postgres=2,MySQL=3,Oracle=4 };
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class DataExcelUtilView : UserControl
    {
        public delegate void LongTimeBusyDelegate();

        List<TableInfo> tableList = new List<TableInfo>();

        Fluent.Ribbon ribbon;

        ComboBox cmbSavedDatabases;
        Fluent.Button btnLoad;
        Fluent.Button btnDrawDataToExcel;
        Fluent.Button btnApplyExcelToDb;
        Fluent.Button btnMakeDiff;

        Fluent.Button btnExecuteQuery;
        Fluent.Button btnFormatQuery;
        Fluent.Button btnPreviousQuery;
        Fluent.Button btnNextQuery;
        Fluent.Button btnExportDataToExcel;
        PropertyGrid propertygrid1;
        DatabaseConfig databaseConfig;
        CheckBox chkDummyData;
        PaneViewModel model;

        //最终RestfulService公开的话，需要考虑线程安全性等等问题
        public static List<TableInfo> ALL_TABLES = null;

        List<KeyValuePair<string, string>> logicalName4Columns = new List<KeyValuePair<string, string>>();
        List<KeyValuePair<string, string>> logicalName4Tables = new List<KeyValuePair<string, string>>();
        CollectionViewSource view = new CollectionViewSource();
        int iAllCheck = 0;
        XlApp _xls = new XlApp();
        public XlApp xlApp
        {
            get
            {
                return _xls;
            }
        }
        public bool IsDbReady
        {
            get
            {
                return isDbReady;
            }

            set
            {
                isDbReady = value;
                if (isDbReady)
                {
                    this.btnLoad.IsEnabled = false;
                    this.btnDrawDataToExcel.IsEnabled = true;
                    //this.btnApplyExcelToDb.IsEnabled = true;
                    //this.btnMakeDiff.IsEnabled = true;

                    btnExecuteQuery.IsEnabled = true;
                    if (this.SqlParser.IsParsedSqlQuery) btnExportDataToExcel.IsEnabled = true;
                }
                else {
                    this.btnLoad.IsEnabled = true;
                    this.btnDrawDataToExcel.IsEnabled = false;
                    this.btnApplyExcelToDb.IsEnabled = false;
                    this.btnMakeDiff.IsEnabled = false;

                    btnExecuteQuery.IsEnabled = false;

                }
            }
        }

        /*
                        load    export  apply   diff    setting
        initial		    true	false	false	false	true
        load success	false	true	true	true	true
        load failure	true	false	false	false	true
        doing start		false	false	false	false	false
        doing end       恢复现场
        */
        public enum ViewActionState { initial, load_sucess, load_failure, do_start, do_end };
        bool org_load, org_export, org_apply, org_diff, org_setting;
        public void SetViewActionState(ViewActionState actionState)
        {
            switch (actionState) {
                case ViewActionState.initial:
                case ViewActionState.load_failure:
                    this.btnLoad.IsEnabled = true;
                    this.btnDrawDataToExcel.IsEnabled = false;
                    this.btnApplyExcelToDb.IsEnabled = false;
                    this.btnMakeDiff.IsEnabled = false;
                    break;
                case ViewActionState.load_sucess:
                    this.btnLoad.IsEnabled = false;
                    this.btnDrawDataToExcel.IsEnabled = true;
                    this.btnApplyExcelToDb.IsEnabled = true;
                    this.btnMakeDiff.IsEnabled = true;
                    //激活现在EXCEL当前sheet预测内容事件
                    (Application.Current.MainWindow as DevelopWorkspace.Main.MainWindow).TriggerExcelWatchEvent();

                    break;
                case ViewActionState.do_start:
                    org_load = this.btnLoad.IsEnabled;
                    org_export = this.btnDrawDataToExcel.IsEnabled;
                    org_apply = this.btnApplyExcelToDb.IsEnabled;
                    org_diff = this.btnMakeDiff.IsEnabled;
                    this.btnLoad.IsEnabled = false;
                    this.btnDrawDataToExcel.IsEnabled = false;
                    //this.btnApplyExcelToDb.IsEnabled = false;
                    //this.btnMakeDiff.IsEnabled = false;
                    break;
                case ViewActionState.do_end:
                    this.btnLoad.IsEnabled = org_load;
                    this.btnDrawDataToExcel.IsEnabled = org_export;
                    //this.btnApplyExcelToDb.IsEnabled = org_apply;
                    //this.btnMakeDiff.IsEnabled = org_diff;
                    break;
            }
        }
        SqlParser SqlParser = new SqlParser();
        int currentQueryIdx = 0;
        bool isDbReady = false;

        public DataExcelUtilView()
        {
            Base.Services.BusyWorkService(new Action(() =>
            {
                InitializeComponent();

                //ribbon工具条注意resource定义在usercontrol内这样click等事件直接可以和view代码绑定
                ribbon = Base.Utils.WPF.FindChild<Fluent.Ribbon>(Application.Current.MainWindow, "ribbon");
                //之前的active内容关联的tab需要隐藏
                if (Base.Services.ActiveModel != null)
                {
                    foreach (object tab in Base.Services.GetRibbon(Base.Services.ActiveModel))
                    {
                        ribbon.Tabs.Remove(tab as Fluent.RibbonTabItem);
                    }
                }
                //从control.resources里面取出ribbontabitem的xaml定义同时实例化
                var ribbonTabTool = FindResource("RibbonTabTool") as Fluent.RibbonTabItem;
                var sqlTabTool = FindResource("SqlTabTool") as Fluent.RibbonTabItem;
                ribbon.Tabs.Add(ribbonTabTool);
                ribbon.Tabs.Add(sqlTabTool);
                ribbon.SelectedTabIndex = ribbon.Tabs.Count - 2;
                Base.Services.ActiveModel.RibbonTabIndex = ribbon.SelectedTabIndex;
                Base.Services.RegRibbon(this.DataContext as Base.Model.PaneViewModel, new List<object> { ribbonTabTool, sqlTabTool });
                model = this.DataContext as Base.Model.PaneViewModel;
                Base.Services.ActiveModel = this.DataContext as Base.Model.PaneViewModel;

                //(ribbonTabTool.FindName("cmbSavedDatabases1") as ComboBox).ItemsSource = (from history in DbSettingEngine.GetEngine().ConnectionHistories
                //                                                                          select history).ToArray<ConnectionHistory>(); ;
                cmbSavedDatabases = Base.Utils.WPF.FindLogicaChild<ComboBox>(ribbonTabTool, "cmbSavedDatabases");
                btnLoad = Base.Utils.WPF.FindLogicaChild<Fluent.Button>(ribbonTabTool, "btnLoad");
                btnApplyExcelToDb = Base.Utils.WPF.FindLogicaChild<Fluent.Button>(ribbonTabTool, "btnApplyExcelToDb");
                btnMakeDiff = Base.Utils.WPF.FindLogicaChild<Fluent.Button>(ribbonTabTool, "btnMakeDiff");
                btnDrawDataToExcel = Base.Utils.WPF.FindLogicaChild<Fluent.Button>(ribbonTabTool, "btnDrawDataToExcel");

                btnExecuteQuery = Base.Utils.WPF.FindLogicaChild<Fluent.Button>(sqlTabTool, "btnExecuteQuery");

                btnFormatQuery = Base.Utils.WPF.FindLogicaChild<Fluent.Button>(sqlTabTool, "btnFormatQuery");
                btnPreviousQuery = Base.Utils.WPF.FindLogicaChild<Fluent.Button>(sqlTabTool, "btnPreviousQuery");
                btnNextQuery = Base.Utils.WPF.FindLogicaChild<Fluent.Button>(sqlTabTool, "btnNextQuery");
                btnExportDataToExcel = Base.Utils.WPF.FindLogicaChild<Fluent.Button>(sqlTabTool, "btnExportDataToExcel");
                chkDummyData = Base.Utils.WPF.FindLogicaChild<CheckBox>(sqlTabTool, "chkDummyData");

                propertygrid1 = Base.Utils.WPF.FindLogicaChild<PropertyGrid>(ribbonTabTool, "propertygrid1");


                //针对setting...的sqlite的路径需要根据startup.homedir改写.
                var connectionHistory = (from history in DbSettingEngine.GetEngine().ConnectionHistories
                                         select history).ToArray<ConnectionHistory>();
                connectionHistory[0].ConnectionString = $"Data Source ={ System.IO.Path.Combine(StartupSetting.instance.homeDir, "workspaceEngine.db")}; Pooling = true; FailIfMissing = false";
                cmbSavedDatabases.ItemsSource = connectionHistory;
                cmbSavedDatabases.SelectedIndex = 0;

                this.btnApplyExcelToDb.IsEnabled = false;
                this.btnDrawDataToExcel.IsEnabled = false;
                this.btnMakeDiff.IsEnabled = false;

                databaseConfig = JsonConfig<DatabaseConfig>.load();
                propertygrid1.SelectedObject = databaseConfig;

                //这里面的代码MainWindow和插件之间互相参照，代码需要整理.尤其ribbon的状态管理等目前依然有为解决BUG
                (Application.Current.MainWindow as DevelopWorkspace.Main.MainWindow).ribbonSelectionChangeEvent += new RibbonSelectionChangeEventHandler(RibbonSelectionChangeEventFunc);
                //减少绑定时错误提前准备属性
                SolidColorBrush brush = new SolidColorBrush(Color.FromArgb((byte)120, (byte)255, (byte)255, (byte)255));
                (this.DataContext as DataExcelUtilModel).ThemeColorBrush = brush;

                (Application.Current.MainWindow as DevelopWorkspace.Main.MainWindow).WorksheetActiveChangeEvent += DataExcelUtilView_WorksheetActiveChangeEvent;

                (this.DataContext as PaneViewModel).ThemeColorBrush = new SolidColorBrush(Color.FromArgb((byte)50, (byte)0, (byte)255, (byte)0));


            }));
        }

        private void DataExcelUtilView_WorksheetActiveChangeEvent(object sender, WorksheetActiveChangeEventArgs e)
        {
            DevelopWorkspace.Base.Logger.WriteLine(e.TableName, Base.Level.TRACE);

            this.tabControl1.Dispatcher.BeginInvoke((Action)delegate ()
            {
                if (tableList.Where(table => table.TableName.Equals(e.TableName)).Count() > 0)
                {
                    this.btnApplyExcelToDb.IsEnabled = true;
                    this.btnMakeDiff.IsEnabled = true;
                }
                else
                {
                    this.btnApplyExcelToDb.IsEnabled = false;
                    this.btnMakeDiff.IsEnabled = false;
                }
                //org_apply = this.btnApplyExcelToDb.IsEnabled;
                //org_diff = this.btnMakeDiff.IsEnabled;

            });

        }

        //////////////////实验代码 如何释放参照？/////////////////////////////

        //逻辑树
        void WalkDownLogicalTree(object current)
        {
            DependencyObject depObj = current as DependencyObject;
            if (depObj != null)
            {
                foreach (object logicalChild in LogicalTreeHelper.GetChildren(depObj))
                    WalkDownLogicalTree(logicalChild);
            }
        }
        private void doBusyWork(LongTimeBusyDelegate delegator)
        {
            try
            {
                //busy.FadeTime = TimeSpan.Zero;
                //busy.IsBusyIndicatorShowing = true;
                // in order for setting the opacity to take effect, you have to delay the task slightly to ensure WPF has time to process the updated visual
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    try
                    {
                        delegator();
                    }
                    catch (Exception ex)
                    {
                        DevelopWorkspace.Base.Logger.WriteLine(ex.Message, Base.Level.ERROR);
                    }
                    //busy.IsBusyIndicatorShowing = false;
                    //busy.ClearValue(BusyDecorator.FadeTimeProperty);
                }), DispatcherPriority.Background);
            }
            catch (Exception ex)
            {
                DevelopWorkspace.Base.Services.ErrorMessage(ex.Message);
                DevelopWorkspace.Base.Logger.WriteLine(ex.Message, Base.Level.ERROR);
            }
        }

        private void LoadTables()
        {
            List<string> getRowCountSqlList = new List<string>();

            int iProviderId = (cmbSavedDatabases.SelectedItem as ConnectionHistory).ProviderID;
            Provider iProvider = (from provider in DbSettingEngine.GetEngine().Providers
                                  where provider.ProviderID == iProviderId
                                  select provider).FirstOrDefault<Provider>();

            //2019/02/26 常用的select文的机制在WhereClauseHistories中有实现，不需要json外置文件，注释掉
            //var selectedClause = databaseConfig.favoriteSelectClause.Find(selectClause => selectClause.provider.Equals(iProvider.ProviderName));

            //System.Reflection.Assembly addinAssembly = System.Reflection.Assembly.UnsafeLoadFrom(string.Format("{0}{1}",
            //    AppDomain.CurrentDomain.BaseDirectory,
            //    iProvider.LoadAssembly));

            //2019/02/22
            System.Reflection.Assembly addinAssembly = null;

            string dllname = iProvider.LoadAssembly.ToLower();

            addinAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.FullName.ToLower() == dllname);
            if (addinAssembly == null)
            {
                string asmFile = App.FindFileInPath(dllname, StartupSetting.instance.homeDir);
                if (!string.IsNullOrEmpty(asmFile))
                {
                    try
                    {
                        addinAssembly = System.Reflection.Assembly.LoadFrom(asmFile);
                    }
                    catch
                    {
                        addinAssembly = null;
                        DevelopWorkspace.Base.Logger.WriteLine($"can't load assembly: { dllname },please copy it to {StartupSetting.instance.homeDir} or it's subdirectory");
                        return;
                    }
                }
                else
                {
                    DevelopWorkspace.Base.Logger.WriteLine($"can't load assembly: { dllname },please copy it to {StartupSetting.instance.homeDir} or it's subdirectory");
                    return;
                }
            }
            Type typDbConnection = addinAssembly.GetType(iProvider.TypeDes);

            System.Reflection.ConstructorInfo ctorViewModel = typDbConnection.GetConstructor(Type.EmptyTypes);
            System.Data.Common.DbConnection con = ctorViewModel.Invoke(new Object[] { }) as System.Data.Common.DbConnection;

            //这几个作为后续机能时数据库接续的信息，需要保证它的可用性.每次重新load数据库信息时需要对之前的信息进行清除，画面做初始化
            IsDbReady = false;
            tableList.Clear();
            logicalName4Columns.Clear();
            logicalName4Tables.Clear();

            //view.Source = tableList;

            tableChecked.IsChecked = false;
            tableNameFilter.Text = "";
            tableRemarkFilter.Text = "";
            view.Filter += new FilterEventHandler(view_Filter);
            this.trvFamilies.DataContext = view;

            xlApp.Provider = iProvider;
            xlApp.ConnectionHistory = cmbSavedDatabases.SelectedItem as ConnectionHistory;
            xlApp.DbConnection = ctorViewModel.Invoke(new Object[] { }) as System.Data.Common.DbConnection;
            xlApp.DbConnection.ConnectionString = (cmbSavedDatabases.SelectedItem as ConnectionHistory).ConnectionString;
            xlApp.ConnectionString = (cmbSavedDatabases.SelectedItem as ConnectionHistory).ConnectionString;
            DevelopWorkspace.Base.Logger.WriteLine($"ConnectionString:{xlApp.ConnectionString}", Base.Level.DEBUG);


            AppConfig.JsonArgb headerColorArgbRaw = (AppConfig.JsonArgb)JsonConvert.DeserializeObject(xlApp.ConnectionHistory.ExcelHeaderThemeColor, typeof(AppConfig.JsonArgb));
            AppConfig.JsonArgb schemaColorArgbRaw = (AppConfig.JsonArgb)JsonConvert.DeserializeObject(xlApp.ConnectionHistory.ExcelSchemaThemeColor, typeof(AppConfig.JsonArgb));
            AppConfig.JsonArgb supportDataColorArgbRaw = (AppConfig.JsonArgb)JsonConvert.DeserializeObject(xlApp.ConnectionHistory.ExcelSupportDataThemeColor, typeof(AppConfig.JsonArgb));
            System.Drawing.Color themeColor = System.Drawing.Color.FromArgb(255, headerColorArgbRaw.red, headerColorArgbRaw.green, headerColorArgbRaw.blue);
            System.Drawing.Color schemaThemeColor = System.Drawing.Color.FromArgb(255, schemaColorArgbRaw.red, schemaColorArgbRaw.green, schemaColorArgbRaw.blue);
            SolidColorBrush brush = new SolidColorBrush(Color.FromArgb((byte)255, (byte)schemaColorArgbRaw.red, (byte)schemaColorArgbRaw.green, (byte)schemaColorArgbRaw.blue));

            //2019/8/11
            //Bug:目前碰到了所有数据都准备妥当但是在加载ListView显示时明显慢了半拍，原因？
            //发现原因了，是因为处理LOG太多占用了UI线程时间把LEVEL调整到INFO就恢复了正常
            (this.DataContext as PaneViewModel).ThemeColorBrush = brush;

            //main tabitem ?
            //(Application.Current.MainWindow as DevelopWorkspace.Main.MainWindow).ThemeColorBrush = brush;

            //applyWhereClause.Background = brush;
            //applyDeleteClause.Background = brush;
            //BindingData bind = new BindingData { themeColor = new SolidColorBrush(Color.FromArgb((byte)argbRaw.alpha, (byte)argbRaw.red, (byte)argbRaw.green, (byte)argbRaw.blue)), tableList = tableList };
            //dataview.DataContext = bind;
            //2019/03/06
            xlApp.SchemaName = (cmbSavedDatabases.SelectedItem as ConnectionHistory).Schema;

            string limitCondition = $"{iProvider.LimitCondition.FormatWith(new { MaxRecord = AppConfig.DatabaseConfig.This.maxRecordCount })}";

            //DB连接
            try
            {
                //2019/03/08 如果DataTypeConditiones表的数据类型重复登陆的时候会引起字典主键冲突例外
                DevelopWorkspace.Base.Logger.WriteLine("dataTypeConditiones aquire....begin", Base.Level.TRACE);
                DevelopWorkspace.Base.Logger.WriteLine("dataTypeConditiones aquire config hint....if same item error occured,you can check dataTypeConditiones table where there are same datatype field", Base.Level.TRACE);
                xlApp.dataTypeConditiones = (from datatypeCondition in DbSettingEngine.GetEngine().DataTypeConditiones
                                             where datatypeCondition.ProviderID == iProviderId
                                             select datatypeCondition).ToDictionary(k => k.DataTypeName.ToLower(), v => { return v; });
                DevelopWorkspace.Base.Logger.WriteLine("dataTypeConditiones aquire....ok", Base.Level.TRACE);

                DevelopWorkspace.Base.Logger.WriteLine($"Shema:{xlApp.SchemaName}", Base.Level.DEBUG);
                model.Title = $"DB[{(cmbSavedDatabases.SelectedItem as ConnectionHistory).ConnectionHistoryName}]";

                xlApp.DbConnection.Open();
                DbCommand dbCommand = xlApp.DbConnection.CreateCommand();

                //TODO
                //数据检索条件以及删除条件的hint选项做成
                this.whereClause.Items.Clear();
                this.deleteClause.Items.Clear();

                var wherehistories = from wherehistory in (cmbSavedDatabases.SelectedItem as ConnectionHistory).WhereClauseHistories where string.IsNullOrEmpty(wherehistory.TableName) select wherehistory;
                foreach (WhereClauseHistory whereClauseHistory in wherehistories)
                {
                    this.whereClause.Items.Add(whereClauseHistory.WhereClauseString);
                    this.deleteClause.Items.Add(whereClauseHistory.WhereClauseString);

                }
                #region 2019/3/13 下面这段逻辑不再需要，直接可以由SelectColumnListSql这个复合SQL文抽出
                //字段或者表对应的注释信息(remark)尝试直接从数据库获取，如果不能的话，再诉诸于workspaceEngine数据库的登录信息
                //下面这段代码目前MySQL版本上测试过

                if (!string.IsNullOrEmpty(iProvider.SelectColumnLogicalNameSql))
                {
                    //字段对应的注释信息
                    if (string.IsNullOrEmpty((cmbSavedDatabases.SelectedItem as ConnectionHistory).Schema))
                    {
                        DevelopWorkspace.Base.Logger.WriteLine(iProvider.SelectColumnLogicalNameSql, Base.Level.DEBUG);
                        dbCommand.CommandText = iProvider.SelectColumnLogicalNameSql;
                    }
                    else
                    {
                        dbCommand.CommandText = string.Format(iProvider.SelectColumnLogicalNameSql, (cmbSavedDatabases.SelectedItem as ConnectionHistory).Schema);
                        DevelopWorkspace.Base.Logger.WriteLine(dbCommand.CommandText, Base.Level.DEBUG);

                    }
                    using (DbDataReader rdr = dbCommand.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            logicalName4Columns.Add(new KeyValuePair<string, string>(rdr["physicalName"].ToString(), rdr["remark"].ToString()));
                        }
                    }
                    //表对应的注释信息
                    if (string.IsNullOrEmpty((cmbSavedDatabases.SelectedItem as ConnectionHistory).Schema))
                    {
                        dbCommand.CommandText = iProvider.SelectTableLogicalNameSql;
                        DevelopWorkspace.Base.Logger.WriteLine(dbCommand.CommandText, Base.Level.DEBUG);

                    }
                    else
                    {
                        dbCommand.CommandText = string.Format(iProvider.SelectTableLogicalNameSql, (cmbSavedDatabases.SelectedItem as ConnectionHistory).Schema);
                        DevelopWorkspace.Base.Logger.WriteLine(dbCommand.CommandText, Base.Level.DEBUG);
                    }
                    using (DbDataReader rdr = dbCommand.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            logicalName4Tables.Add(new KeyValuePair<string, string>(rdr["physicalName"].ToString(), rdr["remark"].ToString()));
                        }
                    }
                }

                #endregion

                Base.Services.BusyWorkIndicatorService($"aquiring table list");

                string selectedSchema = (cmbSavedDatabases.SelectedItem as ConnectionHistory).Schema;

                //DevelopWorkspace.Base.Logger.WriteLine("table list....start", Base.Level.DEBUG);

                //表一览取得
                if (string.IsNullOrEmpty(selectedSchema))
                {
                    dbCommand.CommandText = iProvider.SelectTableListSql;
                    DevelopWorkspace.Base.Logger.WriteLine(dbCommand.CommandText, Base.Level.DEBUG);
                }
                else
                {
                    dbCommand.CommandText = string.Format(iProvider.SelectTableListSql, selectedSchema);
                    DevelopWorkspace.Base.Logger.WriteLine(dbCommand.CommandText, Base.Level.DEBUG);
                }

                using (DbDataReader rdr = dbCommand.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        var whereClause = (from wherehistory in (cmbSavedDatabases.SelectedItem as ConnectionHistory).WhereClauseHistories where wherehistory.TableName == rdr["name"].ToString() select wherehistory.WhereClauseString).FirstOrDefault();
                        //tableList.Add(new TableInfo() { TableName = rdr["name"].ToString(), SchemaName = xlApp.SchemaName, Remark = rdr["name"].ToString().GetLogicalName(), WhereClause = whereClause == null ? "" : whereClause, DateTimeFormatter = iProvider.DateTimeFormatter, DeleteClause = "1=0" });
                        tableList.Add(new TableInfo() {
                            TableName = rdr["name"].ToString(),
                            RowCount = string.IsNullOrEmpty(rdr["rowcount"].ToString()) ? 0 : int.Parse(rdr["rowcount"].ToString()),
                            SchemaName = xlApp.SchemaName,
                            Remark = string.IsNullOrWhiteSpace(rdr["remark"].ToString()) ? rdr["name"].ToString() : rdr["remark"].ToString(),
                            WhereClause = whereClause == null ? "" : whereClause,
                            LimitCondition = limitCondition,
                            DateTimeFormatter = iProvider.DateTimeFormatter,
                            DeleteClause = "1=0",
                            ExcelTableHeaderThemeColor = themeColor,
                            ExcelSchemaHeaderThemeColor = schemaThemeColor,
                            ThemeColorBrush = brush });

                        //2019/08/11 当数据库为SQLite时，表的当前行数自动手动统计
                        if (iProvider.ProviderID == 1 || AppConfig.DatabaseConfig.This.doRealCount)
                        {
                            if (string.IsNullOrEmpty(selectedSchema))
                                getRowCountSqlList.Add($"select '{rdr["name"].ToString()}' as name,'' as remark,count(*) as rowcount FROM {rdr["name"].ToString()}");
                            else
                                getRowCountSqlList.Add($"select '{rdr["name"].ToString()}' as name,'' as remark,count(*) as rowcount FROM {selectedSchema}.{rdr["name"].ToString()}");
                        }
                    }
                }
                //DevelopWorkspace.Base.Logger.WriteLine("table rowcount....start", Base.Level.DEBUG);

                //2019/08/11 当数据库为SQLite时，表的当前行数自动手动统计
                if (iProvider.ProviderID == 1 || AppConfig.DatabaseConfig.This.doRealCount)
                {
                    string unionSelectSql = getRowCountSqlList.Aggregate((total, next) => total + " union " + next);
                    //表一览取得
                    dbCommand.CommandText = unionSelectSql;
                    DevelopWorkspace.Base.Logger.WriteLine(dbCommand.CommandText, Base.Level.DEBUG);
                    int tableIdx = 0;
                    using (DbDataReader rdr = dbCommand.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            (from tableinfo in tableList where tableinfo.TableName == rdr["name"].ToString() select tableinfo).FirstOrDefault().RowCount = int.Parse(rdr["rowcount"].ToString()); ;
                            tableIdx++;
                        }
                    }
                }

                //2019/3/13
                //利用左结合的方式优化检索速度-作为技巧留下痕迹实际没有用到
                if (DatabaseConfig.This.withRemark && tableList.Count != 0) {
                    List<ProjectKeyword> projectKeywordsList = DbRemarkHelper.projectKeywordsList;
                    //var remarks = from tableinfo in tableList join projectKeyword in projectKeywordsList on tableinfo.TableName.ToLower() equals projectKeyword.ProjectKeywordName.ToLower() into tm from defualt in tm.DefaultIfEmpty(new ProjectKeyword()) where string.IsNullOrEmpty(tableinfo.Remark) select new { tableinfo.TableName, defualt.ProjectKeywordRemark };
                    var remarkList = from tableinfo in tableList join projectKeyword in projectKeywordsList on tableinfo.TableName.ToLower() equals projectKeyword.ProjectKeywordName.ToLower() select new { tableinfo, projectKeyword.ProjectKeywordRemark };
                    foreach (var remark in remarkList) {
                        remark.tableinfo.Remark = remark.ProjectKeywordRemark;
                    }
                }
                //DevelopWorkspace.Base.Logger.WriteLine("table columninfo....start", Base.Level.DEBUG);
                //2019/03/09
                //针对mysql不能通过getschematable取得正确的类型信息，这里通过数据库字典的方式取得列属性信息取得
                if (tableList.Count != 0 && !string.IsNullOrEmpty(iProvider.SelectColumnListSql))
                {
                    Base.Services.BusyWorkIndicatorService($"aquiring column infomation");
                    string tableNameList = "";
                    for (int idx = 0; idx < tableList.Count; idx++)
                    {
                        tableNameList += "'" + tableList[idx].TableName + "'";
                        if (idx != tableList.Count - 1)
                        {
                            tableNameList += ",";
                        }
                    }
                    dbCommand.CommandText = iProvider.SelectColumnListSql.FormatWith(new { Schema = selectedSchema, TableNameList = tableNameList });
                    DevelopWorkspace.Base.Logger.WriteLine(dbCommand.CommandText, Base.Level.DEBUG);

                    List<ColumnInfo> columns = new List<ColumnInfo>();
                    string previousTableName = "";
                    string tableName = "";
                    TableInfo currenttable = new TableInfo();
                    string tableNameKey = "TableName";
                    string columnNameKey = "ColumnName";
                    string dataTypeNameKey = "DataTypeName";
                    string iskeyKey = "Iskey";
                    string remarkKey = "Remark";
                    string dataLengthKey = "DataLength";

                    using (DbDataReader rdr = dbCommand.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            tableName = rdr[tableNameKey].ToString();

                            if(!tableName.Equals(currenttable.TableName)) currenttable = tableList.First(t => t.TableName == tableName);

                            if (previousTableName == "") previousTableName = tableName;
                            if (tableName != previousTableName)
                            {
                                TableInfo tableInfo = tableList.First(t => t.TableName == previousTableName);
                                //2019/03/12 如果provider的selectTableList和selectColumnList的SQL缺少整合性会导致一览的表名在列里面不存在导致BUG，这个时候会提醒这个问题及时去修改
                                if (tableInfo != null)
                                {
                                    tableInfo.Loaded = true;
                                    tableInfo.Columns = columns;
                                    columns = new List<ColumnInfo>();
                                }
                                else {
                                    DevelopWorkspace.Base.Logger.WriteLine($"{tableName} don't exist in tablelist,please confirm your provider setting", Base.Level.WARNING);
                                }

                                previousTableName = tableName;
                            }
                            ColumnInfo columnInfo = new ColumnInfo() { ColumnName = rdr[columnNameKey].ToString(),
                                ColumnType = rdr[dataTypeNameKey].ToString().ToLower(),
                                //2019/08/31
                                parent = currenttable,
                                Schemas = new List<string>() {
                                                                            string.IsNullOrEmpty(rdr[iskeyKey].ToString())?"":"*",
                                                                            rdr[columnNameKey].ToString(),
                                                                            rdr[remarkKey].ToString(),
                                                                            rdr[dataTypeNameKey].ToString().ToLower(),
                                                                            rdr[dataLengthKey].ToString()
                                                                       } };
                            columnInfo.dataTypeCondtion = xlApp.GetDataTypeCondition(rdr[dataTypeNameKey].ToString().ToLower());
                            columns.Add(columnInfo);

                        }
                        //last one
                        TableInfo ti = tableList.First(t => t.TableName == tableName);
                        ti.Loaded = true;
                        ti.Columns = columns;

                    }
                    //如果有没有load的表信息，则说明provider里的设定不整合
                    if (tableList.Find((TableInfo tableinfo) => tableinfo.Loaded == false) != null) {
                        DevelopWorkspace.Base.Logger.WriteLine($"{tableName} don't exist in tablelist,please confirm your provider setting", Base.Level.WARNING);
                    }
                }
                //目前下面这个处理之前针对所有DB，之后不能正确取得datatypeName的原因，现在这个处理只有SQLite在使用
                xlApp.schemaList = (from tableSchema in iProvider.TableSchemas select tableSchema.SchemaName).ToArray();
                foreach (TableInfo ti in tableList)
                {
                    //只有在需要时才去载入Schema信息
                    ti.LazyLoadSchemaAction = () =>
                    {
                        return LazyLoadSchema(xlApp.DbConnection, xlApp.
                                                ConnectionString,
                                                ti.SchemaName,
                                                ti.TableName,
                                                xlApp.schemaList,ti);
                    };
                }

                //2019/3/13
                //利用左结合的方式优化检索速度-作为技巧留下痕迹实际没有用到
                if (DatabaseConfig.This.withRemark)
                {
                    List<ProjectKeyword> projectKeywordsList = DbRemarkHelper.projectKeywordsList;
                    List<ColumnInfo> allColumns = new List<ColumnInfo>();
                    foreach (var tableinfo in tableList) allColumns.AddRange(tableinfo.Columns);
                    //var remarks = from tableinfo in tableList join projectKeyword in projectKeywordsList on tableinfo.TableName.ToLower() equals projectKeyword.ProjectKeywordName.ToLower() into tm from defualt in tm.DefaultIfEmpty(new ProjectKeyword()) where string.IsNullOrEmpty(tableinfo.Remark) select new { tableinfo.TableName, defualt.ProjectKeywordRemark };
                    var remarkList = from columnInfo in allColumns join projectKeyword in projectKeywordsList on columnInfo.ColumnName.ToLower() equals projectKeyword.ProjectKeywordName.ToLower() select new { columnInfo, projectKeyword.ProjectKeywordRemark };
                    foreach (var remark in remarkList)
                    {
                        remark.columnInfo.Schemas[2] = remark.ProjectKeywordRemark;
                    }
                }
                //DevelopWorkspace.Base.Logger.WriteLine("table reorder....start", Base.Level.DEBUG);

                //针对ViewOrderNum字段进行降序排序确保常用表考前显示
                var tableList4ViewOrderNum = tableList.Select(table =>
                {
                    var whereClause = xlApp.ConnectionHistory.WhereClauseHistories.FirstOrDefault(y => y.TableName == table.TableName);
                    table.ViewOrderNum = (whereClause != null) ? whereClause.ViewOrderNum : 0;
                    return table;
                });

                tableList = (from table in tableList4ViewOrderNum orderby table.ViewOrderNum descending select table).ToList();

                //DevelopWorkspace.Base.Logger.WriteLine("table bind....start", Base.Level.DEBUG);

                view.Source = tableList;
                view.Filter += new FilterEventHandler(view_Filter);
                this.trvFamilies.DataContext = view;

                //tabControl1.DataContext = new { ThemeColorBrush = brush };
                //testcode
                //最终RestfulService公开的话，需要考虑线程安全性等等问题
                DataExcelUtilView.ALL_TABLES = tableList;
                IsDbReady = true;
                //DevelopWorkspace.Base.Logger.WriteLine("table bind....end", Base.Level.DEBUG);

            }
            catch (Exception ex) {
                DevelopWorkspace.Base.Logger.WriteLine(ex.Message, Base.Level.ERROR);
                throw ex;
            }
            finally
            {
                xlApp.DbConnection.Close();
            }
        }
        /// <summary>
        /// 这块的代码是Shema情报只有在使用的时候再去调用，这样性能体验会好一些，不过这块的代码不够优美需要日后优化
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="connectionString"></param>
        /// <param name="tableName"></param>
        /// <param name="schemaList"></param>
        /// <param name="logicalName4Columns"></param>
        /// <param name="logicalName4Tables"></param>
        /// <returns></returns>
        public List<ColumnInfo> LazyLoadSchema(System.Data.Common.DbConnection conn, string connectionString, string schemaName, string tableName, string[] schemaList,TableInfo parent)
        {
            DevelopWorkspace.Base.Logger.WriteLine("get columninfo....start", Base.Level.DEBUG);
            //2019/03/11 postgressql/mysql等都切换到数据字典的方式，这个方法留作数据字典无法对应的数据库
            List<ColumnInfo> columns = new List<ColumnInfo>();

            System.Reflection.ConstructorInfo ctorViewModel = conn.GetType().GetConstructor(Type.EmptyTypes);
            System.Data.Common.DbConnection nestedConn = ctorViewModel.Invoke(new Object[] { }) as System.Data.Common.DbConnection;
            nestedConn.ConnectionString = connectionString;
            try
            {
                nestedConn.Open();
                DbCommand dbCommand = nestedConn.CreateCommand();
                dbCommand.CommandText = string.Format("SELECT * FROM {0} where 1=0", string.IsNullOrEmpty(schemaName) ? tableName : $"{schemaName}.{tableName}");
                DevelopWorkspace.Base.Logger.WriteLine(dbCommand.CommandText, Base.Level.DEBUG);
                using (DbDataReader rdr = dbCommand.ExecuteReader(CommandBehavior.KeyInfo))
                {
                    System.Data.DataTable schemaTable = rdr.GetSchemaTable();
                    foreach (System.Data.DataRow row in schemaTable.Rows)
                    {
                        ColumnInfo columnInfo = new ColumnInfo() { ColumnName = row[XlApp.SCHEMA_COLUMN_NAME].ToString() };

                        columnInfo.parent = parent;

                        //
                        Array.ForEach(schemaList, keyword =>
                        {
                            if (keyword == XlApp.SCHEMA_REMARK)
                            {
                                String columnName = row[XlApp.SCHEMA_COLUMN_NAME].ToString().ToUpper();
                                columnInfo.Schemas.Add(columnName.GetLogicalName());
                            }
                            else if (keyword == XlApp.SCHEMA_IS_KEY)
                            {
                                if ("True".Equals(row[keyword].ToString()))
                                {
                                    columnInfo.Schemas.Add("*");
                                }
                                else
                                {
                                    columnInfo.Schemas.Add("");

                                }
                            }
                            //2019/3/8
                            else if (keyword == XlApp.SCHEMA_DATATYPE_NAME)
                            {
                                string dataTypeName;
                                if (schemaTable.Columns.Contains("DataTypeName"))
                                {
                                    if (string.IsNullOrEmpty(row[keyword].ToString()))
                                    {
                                        dataTypeName = row["DataType"].ToString();
                                    }
                                    else
                                    {
                                        dataTypeName = row[keyword].ToString();
                                    }
                                }
                                else {
                                    dataTypeName = row["DataType"].ToString();
                                }
                                columnInfo.ColumnType = dataTypeName.ToLower();
                                columnInfo.dataTypeCondtion = xlApp.GetDataTypeCondition(dataTypeName);
                                columnInfo.Schemas.Add(dataTypeName.ToLower());
                            }
                            else
                            {
                                columnInfo.Schemas.Add(row[keyword].ToString());
                            }
                        }
                        );
                        columns.Add(columnInfo);
                    }

                }

            }
            catch (Exception ex)
            {
                DevelopWorkspace.Base.Logger.WriteLine(ex.Message, Base.Level.ERROR);
                System.Diagnostics.Debug.WriteLine($"can't aquire schema infomation from {tableName}", Base.Level.ERROR);
                throw new Exception($"can't aquire schema infomation from {tableName}");
            }
            finally {
                nestedConn.Close();
            }
            return columns;
        }

        void view_Filter(object sender, FilterEventArgs e)
        {
            //只显示选中行,如果没有选中的就不进行过滤
            if (iAllCheck == 3 && tableList.Where( ti => ti.Selected).Count() > 0 )
            {
                if ((e.Item as TableInfo).Selected)
                    e.Accepted = true;
                else
                    e.Accepted = false;
            }
            //解除显示选中行
            else if (iAllCheck == 4)
            {
                e.Accepted = true;
            }
            else if ((e.Item as TableInfo).TableName.ToLower().IndexOf(this.tableNameFilter.Text.ToLower()) >= 0 &&
                (e.Item as TableInfo).Remark.ToLower().IndexOf(this.tableRemarkFilter.Text.ToLower()) >= 0)
            {
                if (iAllCheck == 1) (e.Item as TableInfo).Selected = true;
                if (iAllCheck == 2) (e.Item as TableInfo).Selected = false;
                e.Accepted = true;
            }
            else {
                e.Accepted = false;
            }
        }
        private void btnLoad_Click(object sender, RoutedEventArgs e)
        {
            SetViewActionState(ViewActionState.do_start);
            Base.Services.BusyWorkService(new Action(() =>
            {
                try
                {
                    LoadTables();
                    SetViewActionState(ViewActionState.load_sucess);
                }
                catch (Exception ex) {
                    SetViewActionState(ViewActionState.load_failure);
                }
                finally
                {
                    //SetViewActionState(ViewActionState.do_end);
                }
            }));
        }
        private string getCameralString(string _functionName) { 
            string functionName = _functionName;
            TextInfo txtInfo = new CultureInfo("en-us", false).TextInfo;
            functionName = txtInfo.ToTitleCase(functionName).Replace("_", string.Empty).Replace(" ", string.Empty);
            functionName = $"{functionName.First().ToString().ToLowerInvariant()}{functionName.Substring(1)}";
            return functionName;


        }

        private void Schema2Xml(object sender, RoutedEventArgs e)
        {

            TableInfo ti = (this.trvFamilies.SelectedItem as TableInfo);
            if (ti == null) return;
            XElement root = new XElement("TableInfo");
            root.Add(new XElement("TableName", ti.TableName));
            root.Add(new XElement("Remark", ti.Remark));
            XElement columns = new XElement("Columns");
            root.Add(columns);
            ti.Columns.ToList<ColumnInfo>().ForEach(delegate (ColumnInfo ci)
            {
                if (ci.IsIncluded)
                {
                    XElement line = new XElement("Column");
                    for (int i = 0; i < xlApp.schemaList.Count(); i++)
                    {
                        line.Add(new XElement(xlApp.schemaList[i], ci.Schemas[i]));
                    }
                    line.Add(new XElement("CameralColumnName", getCameralString(ci.Schemas[1])));
                    columns.Add(line);
                }
            });
            this.txtOutput.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("XML");
            this.txtOutput.Text = root.ToString();
            Clipboard.SetText(this.txtOutput.Text);
        }
        private void MakeSelSql(object sender, RoutedEventArgs e)
        {
            TableInfo ti = (this.trvFamilies.SelectedItem as TableInfo);
            if (ti == null) return;
            this.txtOutput.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("SQL");
            this.txtOutput.Text = ti.SelectDataSQL;
            Clipboard.SetText(this.txtOutput.Text);
        }
        private void Schema2Json(object sender, RoutedEventArgs e)
        {
            TableInfo ti = (this.trvFamilies.SelectedItem as TableInfo);
            if (ti == null) return;
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(ti, Newtonsoft.Json.Formatting.Indented);
            this.txtOutput.Text = json;
            Clipboard.SetText(this.txtOutput.Text);
        }
        private void XmlSerializer(object sender, RoutedEventArgs e)
        {
            TableInfo ti = (this.trvFamilies.SelectedItem as TableInfo);
            if (ti == null) return;
            string serializerString = "";
            System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(TableInfo));
            using (System.IO.MemoryStream stream = new System.IO.MemoryStream())
            {
                System.IO.TextWriter writer = new System.IO.StreamWriter(stream, Encoding.UTF8);
                System.Xml.Serialization.XmlSerializerNamespaces ns = new System.Xml.Serialization.XmlSerializerNamespaces();
                //ns.Add("", "");//不输出xmlns
                serializer.Serialize(writer, ti);
                stream.Position = 0;
                byte[] buf = new byte[stream.Length];
                stream.Read(buf, 0, buf.Length);
                serializerString = System.Text.Encoding.UTF8.GetString(buf);
            }
            this.txtOutput.Text = serializerString;
            Clipboard.SetText(this.txtOutput.Text);
        }
        private void SqlFormatter(object sender, RoutedEventArgs e)
        {

        }
        private void drawDataToExcel()
        {
            var selected = from ti in tableList where ti.Selected == true select ti;
            if (selected.Count() == 0) return;
            TableInfo[] selectedList = selected.ToArray();
            try
            {
                Base.Services.CancelLongTimeTaskOn();

                xlApp.DbConnection.Open();
                xlApp.LoadDataIntoExcel(selected.ToArray(), xlApp.DbConnection.CreateCommand());


                //为了能保持使用Ctrl+KeyUp/KeyDown调整过的顺序，排在前面的进行加权处理
                //对表访问次数进行保存已被之后的常用表排序靠前的体验提升
                //TODO 加权方式是否可以更加贴近用户使用场景...需要进一步收集现场反馈
                int exportCount = selected.Count();
                var selectedWithIdx = selected.Select((ti, idx) => new { ti, idx });
                var updateWhereClauses = (from whereClause in xlApp.ConnectionHistory.WhereClauseHistories
                                      join tblWithIdx in selectedWithIdx on whereClause.TableName equals tblWithIdx.ti.TableName select new { target = whereClause,reftable = tblWithIdx.ti,listOrder = tblWithIdx.idx });

                foreach (var whereClause in updateWhereClauses) {
                    whereClause.target.ViewOrderNum = whereClause.target.ViewOrderNum + exportCount - whereClause.listOrder;
                    whereClause.target.WhereClauseString = whereClause.reftable.WhereClause;
                }
                foreach (var tblWithIdx in selectedWithIdx)
                {
                    if((from whereClause in updateWhereClauses where whereClause.target.TableName.Equals(tblWithIdx.ti.TableName) select whereClause).Count() == 0)
                    xlApp.ConnectionHistory.WhereClauseHistories.Add(new WhereClauseHistory() {
                        ConnectionHistoryID = xlApp.ConnectionHistory.ConnectionHistoryID,
                        WhereClauseString = tblWithIdx.ti.WhereClause,
                        TableName = tblWithIdx.ti.TableName,
                        ViewOrderNum = 1 + exportCount - tblWithIdx.idx  });
                }

                DbSettingEngine.GetEngine().SaveChanges();

                (Application.Current.MainWindow as DevelopWorkspace.Main.MainWindow).TriggerExcelWatchEvent(selectedList[0].TableName);

            }
            finally
            {
                Base.Services.CancelLongTimeTaskOff();

                //2019/2/27如果没有关闭的话sqlite的链接在下一次连接时会失败
                xlApp.DbConnection.Close();
            }
        }
        private void btnDrawDataToExcel_Click(object sender, RoutedEventArgs e)
        {
            SetViewActionState(ViewActionState.do_start);
            Base.Services.BusyWorkService(new Action(() =>
            {
                try
                {
                    drawDataToExcel();
                }
                finally
                {
                    SetViewActionState(ViewActionState.do_end);

                }
            }));
        }

        private void btnApplyExcelToDb_Click(object sender, RoutedEventArgs e)
        {
            SetViewActionState(ViewActionState.do_start);
            Base.Services.BusyWorkService(new Action(() =>
            {
                try
                {
                    xlApp.DbConnection.Open();
                    xlApp.DoAccordingActivedSheet(xlApp.Provider, xlApp.SchemaName,tableList, xlApp.DbConnection.CreateCommand(), eProcessType.DB_APPLY);
                }
                finally
                {
                    xlApp.DbConnection.Close();
                    SetViewActionState(ViewActionState.do_end);

                }
            }));
        }
        //指定导出格式sheet的内容和最新DB比较
        private void btnMakeDiff_Click(object sender, RoutedEventArgs e)
        {
            SetViewActionState(ViewActionState.do_start);
            Base.Services.BusyWorkService(new Action(() =>
            {
                try
                {
                    Base.Services.CancelLongTimeTaskOn();

                    xlApp.DbConnection.Open();
                    //根据sheet的内容做出基础dataset
                    DataSet diffDataSet = xlApp.DoAccordingActivedSheet(xlApp.Provider, xlApp.SchemaName,tableList, xlApp.DbConnection.CreateCommand(), eProcessType.DIFF_USE);
                    if (diffDataSet == null) return;

                    //2019/03/15
                    if (Base.Services.longTimeTaskState == LongTimeTaskState.Cancel) return;

                    //根据基础dataset的表名确定DB抽出时的表信息一览
                    List<string> tableNameList = new List<string>();
                    foreach (DataTable table in diffDataSet.Tables)
                    {
                        tableNameList.Add(table.TableName);
                    }
                    var selected = from ti in tableList join tablename in tableNameList on ti.TableName equals tablename select ti;
                    TableInfo[] selectedList = selected.ToArray();
                    if (selectedList.Count() == diffDataSet.Tables.Count)
                    {
                        //取出最新DB内容并对基础dataset进行比对加工
                        xlApp.GetDiffDataSet(selectedList, xlApp.DbConnection.CreateCommand(), diffDataSet);

                        //2019/03/15
                        if (Base.Services.longTimeTaskState == LongTimeTaskState.Cancel) return;

                        //差分结果出力
                        //为了提供处理速度，针对没有差分数据的表进行除外处理
                        int iTablesNum = diffDataSet.Tables.Count;
                        for (int idx = 0; idx < iTablesNum; idx++)
                        {
                            DataTable table = diffDataSet.Tables[iTablesNum - idx - 1];
                            //空表，没有变化的表，以及没有变化的列
                            if (databaseConfig.exceptZeroRowDataTable && table.Rows.Count == 0) diffDataSet.Tables.Remove(table);
                            else if (databaseConfig.exceptZeroDiffRowTable && (table.GetChanges(DataRowState.Unchanged) != null && table.Rows.Count == table.GetChanges(DataRowState.Unchanged).Rows.Count)) diffDataSet.Tables.Remove(table);
                            else if (databaseConfig.exceptUnchangedRow)
                            {
                                int iRowNum = table.Rows.Count;
                                for (int row = 0; row < iRowNum; row++)
                                {
                                    if (table.Rows[iRowNum - row - 1].RowState == DataRowState.Unchanged)
                                    {
                                        table.Rows.Remove(table.Rows[iRowNum - row - 1]);
                                    }
                                }
                            }
                        }
                        if (diffDataSet.Tables.Count == 0)
                        {
                            DevelopWorkspace.Base.Logger.WriteLine("Diff Result:no any difference between active sheet and DB", Base.Level.WARNING);
                        }
                        else
                        {
                            xlApp.DrawDifferenceToExcel(diffDataSet);
                        }
                    }
                    else {
                        DevelopWorkspace.Base.Logger.WriteLine("Please compare active data with same schema of database", Base.Level.WARNING);
                    }
                }
                finally
                {
                    Base.Services.CancelLongTimeTaskOff();

                    xlApp.DbConnection.Close();
                    SetViewActionState(ViewActionState.do_end);
                }
            }));
        }
        private void cmbSavedDatabases_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string tip = Application.Current.Resources["dbsupport.lang.tools.dbsupport.hint.select"].ToString();
            //string tip = "データベース接続情報を選択してから、Schema情報ボタンを押下し、DBからスキーマ情報を読み込んでください。現在の接続情報：";
            cmbSavedDatabases.ToolTip =tip + "\n--- --- --- --- ---\n" + ((e.Source as ComboBox).SelectedItem as ConnectionHistory).ConnectionString;
            //2019/03/07 界面按钮根据操作状态调整
            if((cmbSavedDatabases.SelectedItem as ConnectionHistory).ConnectionString == xlApp.ConnectionString)
                IsDbReady = true;
            else
                IsDbReady = false;

        }

        private void filter_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (view.View != null) view.View.Refresh();
        }

        private void toggleSelect_Checked(object sender, RoutedEventArgs e)
        {
            iAllCheck = 1;
            if (view.View != null) view.View.Refresh();
            iAllCheck = 0;
        }

        private void toggleSelect_Unchecked(object sender, RoutedEventArgs e)
        {
            iAllCheck = 2;
            if (view.View != null) view.View.Refresh();
            iAllCheck = 0;
        }
        //将输入的WHERE条件反映到选中的表信息中，防止抽出过多不必要的数据
        private void applyWhereClause_Click(object sender, RoutedEventArgs e)
        {
            foreach (TableInfo ti in tableList)
            {
                if (ti.Selected)
                {
                    ti.WhereClause = this.whereClause.Text;
                }
            }
            if (view.View != null) view.View.Refresh();
        }
        //将输入的DELETE条件反映到选中的表信息中，通常这个选项在插入数据库之前预先清除原有数据时使用
        private void applyDeleteClause_Click(object sender, RoutedEventArgs e)
        {
            foreach (TableInfo ti in tableList)
            {
                if (ti.Selected)
                {
                    ti.DeleteClause = this.deleteClause.Text;
                }
            }
            if (view.View != null) view.View.Refresh();
        }

        //如果是SQL文入力的话，则对各个按钮进行有效化判断
        private void txtOutput_TextChanged(object sender, EventArgs e)
        {
            if (SqlParser.Parse(txtOutput.Text,false))
            {

                //if(IsDbReady) btnExecuteQuery.IsEnabled = true;
                btnFormatQuery.IsEnabled = true;
                btnPreviousQuery.IsEnabled = true;
                btnNextQuery.IsEnabled = true;
                if (IsDbReady) btnExportDataToExcel.IsEnabled = true;

            }
            else {
                //btnExecuteQuery.IsEnabled = false;
                btnFormatQuery.IsEnabled = false;
                btnPreviousQuery.IsEnabled = false;
                btnNextQuery.IsEnabled = false;
                btnExportDataToExcel.IsEnabled = false;
            }
        }

        private void btnExecuteQuery_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(this.txtOutput.SelectedText) && string.IsNullOrWhiteSpace(this.txtOutput.Text)) return;
            DevelopWorkspace.Base.Logger.WriteLine(this.txtOutput.SelectedText);

            SetViewActionState(ViewActionState.do_start);
            Base.Services.BusyWorkService(new Action(() =>
            {
                    try
                    {
                        xlApp.DbConnection.Open();
                        DbCommand cmd = xlApp.DbConnection.CreateCommand();

                        string limitCondition = $"{xlApp.Provider.LimitCondition.FormatWith(new { MaxRecord = AppConfig.DatabaseConfig.This.maxRecordCount })}";

                        Regex regex = new Regex(@"^\s{0,}select\b", RegexOptions.IgnoreCase);

                        string commandText = "";

                        if (string.IsNullOrWhiteSpace(this.txtOutput.SelectedText))
                        {
                            if(regex.Match(this.txtOutput.Text).Success)
                                commandText = $"select * from ({this.txtOutput.Text}) subquery where {limitCondition}";
                            else
                                commandText = this.txtOutput.Text;

                        }
                        else
                        {
                            if (regex.Match(this.txtOutput.SelectedText).Success)
                                commandText = $"select * from ({this.txtOutput.SelectedText}) subquery where {limitCondition}";
                            else
                                commandText = this.txtOutput.SelectedText;
                        }
                        //有时从项目文件里取出的代码带有/*parameter*/的注解，为了能够无需删除它后方可执行的麻烦这里略作替换 
                        //似乎用不上，属于SQL本身的语法
                        //commandText = Regex.Replace(commandText, @"/\*.+?\*/", m => "", RegexOptions.IgnoreCase); // Append the rest of the match

                        cmd.CommandText = commandText;
                        DevelopWorkspace.Base.Logger.WriteLine(cmd.CommandText, Base.Level.DEBUG);

                        List<string> titleList = new List<string>();
                        List<int> columnPadSizeList = new List<int>();
                        List<List<string>> dataListList = new List<List<string>>();
                        bool titleInitial = false;
                        using (DbDataReader rdr = cmd.ExecuteReader())
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
                            Base.Logger.WriteLine(titleOutput);
                            int outputLimit = dataListList.Count;
                            if (outputLimit > AppConfig.DatabaseConfig.This.maxRecordCount) outputLimit = AppConfig.DatabaseConfig.This.maxRecordCount;
                            for (int idx = 0; idx < dataListList.Count; idx++)
                            {
                                string dataOutput = "";
                                for (int jdx = 0; jdx < dataListList[idx].Count; jdx++)
                                {
                                    dataOutput += string.Format("{0," + (0 - columnPadSizeList[jdx] - 4) + "}", dataListList[idx][jdx]);
                                }
                                Base.Logger.WriteLine(dataOutput);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Base.Logger.WriteLine(ex.Message, Base.Level.ERROR);
                    }
                    finally
                    {
                        xlApp.DbConnection.Close();
                    SetViewActionState(ViewActionState.do_end);
                }
            }));
        }

        private void trvFamilies_KeyDown(object sender, KeyEventArgs e)
        {
            if (sender as ListViewItem == null) return;

            if (e.Key == Key.Enter) {
                var tableinfo = ((ListViewItem)sender).Content as TableInfo;
                tableinfo.Selected = !tableinfo.Selected;
                tableinfo.RaisePropertyChanged("Selected");
            }
            else if (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl))
            {

                switch (e.Key)
                {

                    case Key.Up:
                        int currentIdx = 0;
                        List<TableInfo> reOrderList = null;
                        int totalnum = 0;

                        currentIdx = this.trvFamilies.SelectedIndex;
                        if (currentIdx <= 0) return;
                        totalnum = tableList.Count;
                        reOrderList = new List<TableInfo>();
                        for (int i = 0; i < tableList.Count; i++) {
                            if (i == currentIdx - 1) {
                                reOrderList.Add(tableList.ElementAt(i + 1));
                                reOrderList.Add(tableList.ElementAt(i));
                                i++;
                            }
                            else
                                reOrderList.Add(tableList.ElementAt(i));
                        }
                        tableList = reOrderList;
                        view.Source = tableList;
                        view.Filter += new FilterEventHandler(view_Filter);
                        this.trvFamilies.DataContext = view;
                        this.trvFamilies.SelectedIndex = currentIdx - 1;
                        //handle D key
                        break;
                    case Key.Down:

                        currentIdx = this.trvFamilies.SelectedIndex;
                        if (currentIdx >= tableList.Count-1) return;
                        totalnum = tableList.Count;
                        reOrderList = new List<TableInfo>();
                        for (int i = 0; i < tableList.Count; i++)
                        {
                            if (i == currentIdx )
                            {
                                reOrderList.Add(tableList.ElementAt(i + 1));
                                reOrderList.Add(tableList.ElementAt(i));
                                i++;
                            }
                            else
                                reOrderList.Add(tableList.ElementAt(i));
                        }
                        tableList = reOrderList;
                        view.Source = tableList;
                        view.Filter += new FilterEventHandler(view_Filter);
                        this.trvFamilies.DataContext = view;
                        this.trvFamilies.SelectedIndex = currentIdx + 1;

                        //handle X key
                        break;
                }
            }
            System.Diagnostics.Debug.WriteLine("SelectedIndex:" + this.trvFamilies.SelectedIndex);
        }

        private void checked_Checked(object sender, RoutedEventArgs e)
        {
            //点击checkbox时要选中当前行配合Ctrl+Up/Down的动作
            //System.Diagnostics.Debug.WriteLine((sender as CheckBox).IsChecked);
            ListViewItem listViewItem = GetVisualAncestor<ListViewItem>((DependencyObject)sender);
            listViewItem.IsSelected = true;
            //System.Diagnostics.Debug.WriteLine(this.trvFamilies.SelectedIndex);

        }
        private static T GetVisualAncestor<T>(DependencyObject o) where T : DependencyObject
        {
            do
            {
                o = VisualTreeHelper.GetParent(o);
            } while (o != null && !typeof(T).IsAssignableFrom(o.GetType()));

            return (T)o;
        }

        private void trvFamilies_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(delegate

            {

                ListView view = sender as ListView;

                view.ScrollIntoView(view.SelectedItem);



            }));
        }
        private void RibbonSelectionChangeEventFunc(object sender, RibbonSelectionChangeEventArgs e)
        {
            var model = sender as PaneViewModel;
            if(this.DataContext as Base.Model.PaneViewModel == model) { 
                System.Diagnostics.Debug.WriteLine($"RibbonSelectionChangeEventFunc: {e.SelectedIndex}");
                if (e.SelectedIndex == 1) tabControl1.SelectedIndex = 0;
                if (e.SelectedIndex == 2) tabControl1.SelectedIndex = 1;
            }
        }
        private void tabControl1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var tc = sender as TabControl; //The sender is a type of TabControl...

            if (tc != null)
            {
                System.Diagnostics.Debug.WriteLine($"tabControl1_SelectionChanged: {tc.SelectedIndex}");
                if(tc.SelectedIndex == 0) 
                    ribbon.SelectedTabIndex = ribbon.Tabs.Count - 2;
                else
                    ribbon.SelectedTabIndex = ribbon.Tabs.Count - 1;

                //Do Stuff ...
            }
        }

        private void details_Click(object sender, RoutedEventArgs e)
        {
            ListViewItem listViewItem = GetVisualAncestor<ListViewItem>((DependencyObject)sender);
            listViewItem.IsSelected = true;

            DetailsDialog detailsDialog = new DetailsDialog(listViewItem.DataContext as TableInfo);
            Point position = ((Button)sender).PointToScreen(new Point(0d, 0d));

            detailsDialog.Top = position.Y - detailsDialog.Height / 2 + 50;
            //detailsDialog.Left = position.X - detailsDialog.Width - ((Button)sender).ActualWidth - 10;
            detailsDialog.Left = position.X + ((Button)sender).ActualWidth + 10;
            detailsDialog.Show();

        }
        //双击时可以选择/解除
        private void trvFamilies_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            var tableinfo = ((ListViewItem)sender).Content as TableInfo;
            tableinfo.Selected = !tableinfo.Selected;
            tableinfo.RaisePropertyChanged("Selected");
        }

        private void trvFamilies_LeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl)) {
                var tableinfo = ((ListViewItem)sender).Content as TableInfo;
                tableinfo.Selected = !tableinfo.Selected;
                tableinfo.RaisePropertyChanged("Selected");
            }
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            ListViewItem listViewItem = GetVisualAncestor<ListViewItem>((DependencyObject)sender);
            listViewItem.IsSelected = true;

        }

        private void toggleSelectFilter_Checked(object sender, RoutedEventArgs e)
        {
            iAllCheck = 3;
            if (view.View != null) view.View.Refresh();
            iAllCheck = 0;

        }

        private void toggleSelectFilter_Unchecked(object sender, RoutedEventArgs e)
        {
            iAllCheck = 4;
            if (view.View != null) view.View.Refresh();
            iAllCheck = 0;

        }

        private void btnFormatQuery_Click(object sender, RoutedEventArgs e)
        {

            txtOutput.Text = SqlParser.format(txtOutput.Text);
        }
        
        private void btnPreviousQuery_Click(object sender, RoutedEventArgs e)
        {
            if (SqlParser.queries == null) return;
            currentQueryIdx--;
            if (currentQueryIdx < 0) currentQueryIdx = 0;
            this.txtOutput.Select(SqlParser.queries[currentQueryIdx].Index, SqlParser.queries[currentQueryIdx].Length);
        }

        private void btnNextQuery_Click(object sender, RoutedEventArgs e)
        {
            if (SqlParser.queries == null) return;
            currentQueryIdx++;
            if (currentQueryIdx > SqlParser.queries.Count - 1) currentQueryIdx = SqlParser.queries.Count - 1;
            this.txtOutput.Select(SqlParser.queries[currentQueryIdx].Index, SqlParser.queries[currentQueryIdx].Length);

        }
        /// <summary>
        /// 根据输入的SQL文来解析出表一览以及抽出条件后把数据导入到EXCEL里
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnExportDataToExcel_Click(object sender, RoutedEventArgs e)
        {
            //从SQLparser获取抽取对象TABLE，关联条件，以及抽取条件（为了简化实装的难度，目前只支持=的条件式，而且对AND，OR这样的分歧条件一律视为默认（AND？后者优先）处理
            //在数据做成时需要考虑一下：如果入力的SQL本身可以抽取数据的情况下，那么直接从DB抽取可能会更好
            //这个时候需要对所有的对象表进行下面的处理
            //from 单表 where 抽取条件 来获取目标表的数据
            //如果有数据则直接抽取出来作为后部数据，如果没有数据，则from 单表 LIMIT 件数来抽取作为数据
            if (!SqlParser.Parse(txtOutput.Text)) return;

            string message = "";
            List<RowInfo> rowInfoList = new List<RowInfo>();
            string[] titles = new string[] { "TableName"};

            (from pair in SqlParser.SelectedTables() select pair.Value).Distinct().ToList().ForEach((table) => { rowInfoList.Add(new RowInfo() { TitleList = titles, ColumnList = new string[] { table }, Selected = true }); });
            ConfirmDialog confirmDialog = new ConfirmDialog(message, rowInfoList,true);
            confirmDialog.Owner = DevelopWorkspace.Base.Utils.WPF.GetTopWindow(this);

            confirmDialog.ShowDialog();
            if (confirmDialog.ConfirmResult == eConfirmResult.OK)
            {
                DevelopWorkspace.Base.Logger.WriteLine("begin to export data into excel",Base.Level.DEBUG);
                List<TableInfo> selectedTables = new List<TableInfo>();
                foreach (RowInfo selectedRow in (from rowinfo in rowInfoList where rowinfo.Selected == true select rowinfo)) {
                    TableInfo selectedTableInfo = (from ti in tableList where ti.TableName.ToLower() == selectedRow.ColumnList[0].ToLower() select ti).FirstOrDefault();
                    if (selectedTableInfo != null) {
                        //辅助数据做成信息
                        if(this.chkDummyData.IsChecked == true) selectedTableInfo.WhereCondition = SqlParser.WhereConditiones();
                        selectedTables.Add(selectedTableInfo);
                    }
                }
                try
                {
                    xlApp.DbConnection.Open();
                    xlApp.LoadDataIntoExcel(selectedTables.ToArray(), xlApp.DbConnection.CreateCommand());
                }
                finally
                {
                    xlApp.DbConnection.Close();
                }
                //数据做完后清除条件信息
                foreach(TableInfo ti in selectedTables)
                {
                    ti.WhereCondition = null;
                }

                DevelopWorkspace.Base.Logger.WriteLine("finish to export data into excel", Base.Level.DEBUG);
            }
        }
    }

    public static class DbRemarkHelper
    {
        public static List<ProjectKeyword> projectKeywordsList = DbSettingEngine.GetEngine().ProjectKeywords.ToList();
        //TODO 2019/3/12
        //这块暂时不要用，对性能影响比较大
        public static string GetLogicalName(this string physicalName)
        {
            //DbSettingEngine settingEngine = DbSettingEngine.GetEngine();
            String columnName = physicalName.ToUpper();
            var projectKeywords = from projectKeyword in projectKeywordsList
                                  where projectKeyword.ProjectKeywordName.ToUpper() == physicalName.ToUpper()
                                  select projectKeyword;
            return projectKeywords.FirstOrDefault() == null ? physicalName : projectKeywords.FirstOrDefault().ProjectKeywordRemark;
        }
    }
}
