namespace DevelopWorkspace.Main
{
    using System;
    using Microsoft.Office.Interop.Excel;
    using System.IO;
    using System.Data;
    using System.Data.Common;
    using System.Linq;
    using Microsoft.CSharp;
    using System.Collections.Generic;
    using System.Diagnostics;
    using DevelopWorkspace.Base;
    using View;
    using DevelopWorkspace.Base.Utils;
    using System.Runtime.InteropServices;
    using static DevelopWorkspace.Main.AppConfig;
    using System.Drawing;
    using System.Threading;
    using System.Collections.Concurrent;
    using System.Threading.Tasks;
    using System.Windows.Threading;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Windows.Input;
    using System.ComponentModel;
    using DevelopWorkspace.Base.Model;

    public delegate List<ColumnInfo> LazyLoadSchemaDelegate();
    public enum eProcessType
    {
        SQL_ONLY,
        DB_APPLY,
        DIFF_USE
    }
    /// <summary>
    /// 使用Json.NET进行序列化.NET对象，以保证和js端的应用匹配
    /// http://www.newtonsoft.com/json/help/html/PropertyJsonIgnore.htm
    /// </summary>
    public class TableInfo : ViewModelBase
    {
        public TableInfo()
        {
            this.Loaded = false;
        }

        public string TableName { get; set; }
        [System.Xml.Serialization.XmlIgnore]
        public string SchemaName { get; set; }

        [System.Xml.Serialization.XmlIgnore]
        public bool Selected { get; set; }

        [System.Xml.Serialization.XmlIgnore]
        public List<SqlParserWrapper.WhereCondition> WhereCondition { get; set; }

        [System.Xml.Serialization.XmlIgnore]
        public bool Loaded { get; set; }



        [System.Xml.Serialization.XmlIgnore]
        public string DateTimeFormatter { get; set; }

        public System.Drawing.Color ExcelTableHeaderThemeColor { get; set; }
        public System.Drawing.Color ExcelSchemaHeaderThemeColor { get; set; }
        public System.Windows.Media.SolidColorBrush ThemeColorBrush { get; set; }
        public string Remark { get; set; }
        public int RowCount { get; set; }
        public string LimitCondition { get; set; }
        public int ViewOrderNum { get; set; }

        //利用正则对数据字段进行替换，但是要过滤关键词以及字段名，如果这些被'号包围则视为数据
        enum FilterType { SingleValue, MultiValue, MultiValueWithOR, CustomClause };
        List<string> sqlkeywordsString = DatabaseConfig.This.sqlKeywords;
        bool IsKeyword(string keyword, List<string> columnsString)
        {
            if (sqlkeywordsString.Find(a => a.Equals(keyword)) != null) return true;
            if (columnsString.Find(a => a.Equals(keyword)) != null) return true;
            return false;
        }
        string FILTER_PATTERN = @"(\'.+?\'|[^\s (),;\w]+|[^\s (),;\W]+)";
        FilterType checkFilterType(string filterString, List<string> columnsString)
        {
            var matches = Regex.Matches(filterString, FILTER_PATTERN, RegexOptions.IgnoreCase);
            int occurtimesAboutOR = 0;
            int occurtimesAboutValue = 0;

            foreach (Match match in matches)
            {
                if (!"or".Equals(match.Value.ToLower()) && IsKeyword(match.Value, columnsString))
                {
                    return FilterType.CustomClause;
                }
                else if ("or".Equals(match.Value.ToLower()))
                {
                    occurtimesAboutOR++;
                }
                else
                {
                    occurtimesAboutValue++;
                }
            }
            if (occurtimesAboutOR > 0)
                return FilterType.MultiValueWithOR;
            else if (occurtimesAboutValue > 1)
                return FilterType.MultiValue;
            else
                return FilterType.SingleValue;

        }
        string substitute(string match, List<string> columnsString, int processKbn,string columnName,FilterType filterType)
        {
            if (IsKeyword(match, columnsString))
                return match;
            else
            {
                if (filterType == FilterType.MultiValueWithOR || filterType == FilterType.SingleValue)
                {
                    if (match[0] == 39)
                        return $" {columnName} =  {match} ";
                    else
                    {
                        if (processKbn == 0)
                            return $" {columnName} =  '{match}' ";
                        else
                            return $" {columnName} =  {match} ";
                    }
                }
                else {
                    if (match[0] == 39)
                        return match;
                    else
                    {
                        if (processKbn == 0)
                            return "'" + match + "'";
                        else
                            return match;
                    }
                }
            }
        }
        string START_PATTERN = @"^(\s{0,}<=|\s{0,}>=|\s{0,}>|\s{0,}<|\s{0,}=|\s{0,}is\b|\s{0,}in\b|\s{0,}between\b)";
        [Newtonsoft.Json.JsonIgnore]
        [System.Xml.Serialization.XmlIgnore]
        private string _whereClause;
        public string WhereClause
        {
            set
            {
                _whereClause = value;
                if ("*".Equals(value)){
                    _whereClause = getClauseString();
                }

                RaisePropertyChanged("WhereClause");
            }
            get
            {
                return _whereClause;
            }
        }
        public string getClauseString(bool isSelectClause = true)
        {
            string retClauseString = "";
            if (Columns != null)
            {
                List<string> columnsString = (from column in Columns select column.ColumnName).ToList<string>();
                //对所有的column的where条件进行遍历，这么做比较粗暴会把一些信息丢失掉，比如 or 关系被强制成 and
                Columns.ForEach(delegate (ColumnInfo ci)
                {
                    string inputClauseString;
                    if (isSelectClause)
                        inputClauseString = ci.WhereClause;
                    else
                        inputClauseString = ci.DeleteClause;

                    if (ci.IsIncluded && !string.IsNullOrWhiteSpace(inputClauseString))
                    {
                        string clauseString = " ";

                        FilterType filterType = checkFilterType(inputClauseString, columnsString);

                        var substitutedString = Regex.Replace(inputClauseString, FILTER_PATTERN, m => substitute(m.Value, columnsString, ci.dataTypeCondtion.ProcessKbn, ci.ColumnName, filterType), RegexOptions.IgnoreCase); // Append the rest of the match
                        if (filterType == FilterType.MultiValue)
                        {
                            clauseString += $"{ci.ColumnName} in ( {substitutedString} )";
                        }
                        else
                        {
                            //如果开始文字为各种符号的话，则添加字段名
                            Regex regex = new Regex(START_PATTERN, RegexOptions.IgnoreCase);
                            var result = regex.Match(substitutedString);
                            if (result.Success)
                            {
                                clauseString += $"{ci.ColumnName} {substitutedString}";
                            }
                            else
                                if (filterType == FilterType.MultiValueWithOR || filterType == FilterType.SingleValue)
                                clauseString += substitutedString;
                            else
                                clauseString += $"{ci.ColumnName} = {substitutedString}";

                        }
                        if (string.IsNullOrWhiteSpace(retClauseString))
                        {
                            retClauseString += clauseString;
                        }
                        else
                        {
                            Regex regex = new Regex(@"^(\s{0,}or\b)", RegexOptions.IgnoreCase);
                            var result = regex.Match(substitutedString);
                            if (result.Success)
                            {
                                retClauseString += clauseString;
                            }
                            else
                                retClauseString += " and " + clauseString;
                        }

                    }
                });
            }
            return retClauseString;

        }
        //默认设为1=0，也就是说Excel的数据登录前不做删除处理，需要根据实际项目定制条件
        [Newtonsoft.Json.JsonIgnore]
        [System.Xml.Serialization.XmlIgnore]
        private string _deleteClause;
        public string DeleteClause
        {
            set
            {
                _deleteClause = value;
                if ("*".Equals(value))
                {
                    _deleteClause = getClauseString(false);
                }

                RaisePropertyChanged("DeleteClause");
            }
            get
            {
                return _deleteClause;
            }
        }
        [Newtonsoft.Json.JsonIgnore]
        [System.Xml.Serialization.XmlIgnore]
        string _selectDataSql = null;
        [Newtonsoft.Json.JsonIgnore]
        [System.Xml.Serialization.XmlIgnore]
        public string SelectDataSQL
        {
            get
            {
                //2019/8/24 可以定制字段可选，暂且每次发行SQL文时都重新组装，也许会带来些许性能劣化 
                //if (string.IsNullOrEmpty(_selectDataSql))
                {
                    //2019/03/08 performance tunning
                    DevelopWorkspace.Base.Logger.WriteLine("SelectDataSQL.get", Level.DEBUG);

                    StringBuilder selectDataSqlBuilder = new StringBuilder("select ", 250);
                    bool isFirtIncluded = true;
                    for (int idx = 0; idx < Columns.Count; idx++)
                    {
                        //todo exclude some column 
                        if (!Columns[idx].IsIncluded) continue;

                        ColumnInfo ci = Columns[idx];
                        //日期类型经常会导致例外发生，索性在取得数据是利用各个数据库自身的功能把日期型转换成字符串型
                        //除了日期型以后还会出现其他类型需要类似处理，或许
                        //另外在这里的FormatWith是扩展了系统类的一个实践
                        //http://james.newtonking.com/archive/2008/03/29/formatwith-2-0-string-formatting-with-named-variables
                        //可以通过这个方式缩短代码量
                        //目前这个版本针对blob，clob类型没有做对应，将来是否有需要？只有到那时候才知道
                        //http://stackoverflow.com/questions/5371222/getting-binary-data-using-sqldatareader
                        //if (ci.ColumnType.Equals("System.DateTime"))
                        if (isFirtIncluded)
                        {
                            isFirtIncluded = false;
                        }
                        else {
                            selectDataSqlBuilder.Append(",");
                        }

                        if (!string.IsNullOrEmpty(ci.dataTypeCondtion.ExcelFormatString))
                        {
                            selectDataSqlBuilder.Append(ci.dataTypeCondtion.ExcelFormatString.FormatWith(ci));

                        }
                        else
                        {
                            selectDataSqlBuilder.Append(ci.ColumnName);
                        }
                    }
                    string fullTableName = string.IsNullOrEmpty(SchemaName) ? TableName : SchemaName + "." + TableName;
                    selectDataSqlBuilder.Append(" from " + fullTableName);
                    _selectDataSql = selectDataSqlBuilder.ToString();
                }

                // where内容整理，除了where的过滤条件，对order by以及限制取得件数做简单调整
                // 对限制件数目前支持 oracle的rownum方式以及 limit 的方式
                string analyzedWhereString = "";
                string analyzedOrderString = "";
                string analyzedLimitString = "";

                if (!string.IsNullOrEmpty(WhereClause))
                {
                    Regex regex = new Regex(@"^\bwhere\s+|\blimit\b\s{0,}[0-9]+$|\border\s+by\s+", RegexOptions.IgnoreCase);
                    var result = regex.Matches(WhereClause);
                    int whereKeywordIndex = -1;
                    int orderKeywordIndex = -1;
                    int limitKeywordIndex = -1;

                    foreach (Match match in result)
                    {
                        if (match.Value.IndexOf("where", StringComparison.CurrentCultureIgnoreCase) >= 0)
                        {
                            whereKeywordIndex = match.Value.Length;
                        }
                        if (match.Value.IndexOf("order", StringComparison.CurrentCultureIgnoreCase) >= 0)
                        {
                            orderKeywordIndex = match.Index;
                        }
                        if (match.Value.IndexOf("limit", StringComparison.CurrentCultureIgnoreCase) >= 0)
                        {
                            limitKeywordIndex = match.Index;
                        }
                    }
                    int whereLastIndex = WhereClause.Length;
                    if ((limitKeywordIndex >= 0 && orderKeywordIndex >= 0))
                    {
                        whereLastIndex = new int[] { orderKeywordIndex, limitKeywordIndex }.Min();
                    }
                    else if (limitKeywordIndex >= 0)
                    {
                        whereLastIndex = limitKeywordIndex;
                    }
                    else if (orderKeywordIndex >= 0)
                    {
                        whereLastIndex = orderKeywordIndex;
                    }

                    if (whereKeywordIndex == -1) whereKeywordIndex = 0;
                    analyzedWhereString = WhereClause.Substring(whereKeywordIndex, whereLastIndex - whereKeywordIndex);

                    if (orderKeywordIndex >= 0)
                    {
                        if (limitKeywordIndex >= 0)
                            analyzedOrderString = WhereClause.Substring(orderKeywordIndex, limitKeywordIndex - orderKeywordIndex);
                        else
                            analyzedOrderString = WhereClause.Substring(orderKeywordIndex);
                    }
                    if (limitKeywordIndex >= 0) analyzedLimitString = WhereClause.Substring(limitKeywordIndex);
                }
                string modifiedSql = _selectDataSql;
                if (!string.IsNullOrWhiteSpace(analyzedWhereString))
                {
                    modifiedSql += " where " + analyzedWhereString;
                }
                if (!string.IsNullOrWhiteSpace(analyzedOrderString))
                {
                    modifiedSql += " " + analyzedOrderString;
                }

                if (!string.IsNullOrWhiteSpace(analyzedLimitString))
                {
                    modifiedSql = LimitCondition.FormatWith(new { RawSQL = modifiedSql,MaxRecord = analyzedLimitString.Substring("limit".Length) });
                }
                else {
                    modifiedSql = LimitCondition.FormatWith(new { RawSQL = modifiedSql, MaxRecord = AppConfig.DatabaseConfig.This.maxRecordCount });
                }

                return modifiedSql;
            }
        }
        [Newtonsoft.Json.JsonIgnore]
        [System.Xml.Serialization.XmlIgnore]
        public List<List<string>> ExportSchemaRegion
        {
            get
            {
                List<List<string>> exportSchemaRegion = new List<List<string>>();
                for (int iSchemaIdx = 0; iSchemaIdx < Columns[0].Schemas.Count; iSchemaIdx++)
                {
                    List<string> exportSchemaRow = new List<string>();
                    for (int iColumnIdx = 0; iColumnIdx < Columns.Count; iColumnIdx++)
                    {
                        //toddo
                        if (!Columns[iColumnIdx].IsIncluded) continue;
                        exportSchemaRow.Add(Columns[iColumnIdx].Schemas[iSchemaIdx]);
                    }
                    exportSchemaRegion.Add(exportSchemaRow);
                }
                return exportSchemaRegion;
            }
        }

        //只有在需要时才去载入Schema信息
        [Newtonsoft.Json.JsonIgnore]
        [System.Xml.Serialization.XmlIgnore]
        public LazyLoadSchemaDelegate LazyLoadSchemaAction { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        [System.Xml.Serialization.XmlIgnore]
        private List<ColumnInfo> _columns;

        public List<ColumnInfo> Columns
        {
            set {
                _columns = value;
            }
            get
            {
                //只有在需要时才去载入Schema信息
                if (!Loaded && LazyLoadSchemaAction != null)
                {
                    lock (typeof(TableInfo))
                    {
                        _columns = LazyLoadSchemaAction();
                    }
                    Loaded = true;
                }
                return _columns;
            }
        }

        //private static RelayCommand _ButtonClick;
        //public ICommand DetailsClick
        //{
        //    get
        //    {
        //        if (_ButtonClick == null)
        //            _ButtonClick = new RelayCommand(param => this.BtClick(param));
        //        return _ButtonClick;
        //    }
        //}

        //private void BtClick(object parameter)
        //{
        //    DevelopWorkspace.Main.View.DetailsDialog detailsDialog = new DevelopWorkspace.Main.View.DetailsDialog(parameter as TableInfo);
        //    //detailsDialog.Owner = DevelopWorkspace.Base.Utils.WPF.GetTopWindow(null);
        //    detailsDialog.Show();
        //}


    }

    public class ColumnInfo : ViewModelBase
    {
        List<string> _schemas = new List<string>();
        public string ColumnName { get; set; }
        public string ColumnType { get; set; }


        public TableInfo parent { get; set; }

        private string _whereClause = "";
        public string WhereClause {
            set
            {
                //当画面入力后失去焦点时会激活这个方法，如果通知给tableinfo
                _whereClause = value;

                if (string.IsNullOrWhiteSpace(value)) return;

                //给tableinfo进行所有字段条件遍历的机会
                parent.WhereClause = "*";

            }
            get
            {
                return _whereClause;
            }

        }
        private string _deleteClause = "";
        public string DeleteClause
        {
            set
            {
                //当画面入力后失去焦点时会激活这个方法，如果通知给tableinfo
                _deleteClause = value;

                if (string.IsNullOrWhiteSpace(value)) return;

                //给tableinfo进行所有字段条件遍历的机会
                parent.DeleteClause = "*";

            }
            get
            {
                return _deleteClause;
            }

        }


        public DataTypeCondition dataTypeCondtion { get; set; }
        public System.Windows.Media.SolidColorBrush ThemeColorBrush { get; set; }

        //2019/8/24 可以定制字段可选，主键必须能选即不可解除可选状态
        public bool IsNotKey
        {
            get
            {
                return !Schemas[0].Equals("*");
            }
        }
        bool defaultInclude = true;
        public bool IsIncluded
        {
            get
            {
                //todo ORACLE表中有comment字段的话会引发例外，这里暂且强制不可选
                if (ColumnName.ToLower().Equals("comment")) return false;
                return defaultInclude;
            }

            set
            {
                defaultInclude = value;
            }
        }
        public int Age { get; set; }

        public List<string> Schemas
        {
            get
            {
                return _schemas;
            }

            set
            {
                _schemas = value;
            }
        }
    }

    public class DbApplyWork
    {
        public string TableName { get; set; }
        public string DeleteSql { get; set; }
        public string DropTableSql { get; set; }
        public string CreateTableSql { get; set; }
        public List<DataTypeCondition> DataTypeConditionList { get; set; }
        public Dictionary<string, List<string>> Schemas { get; set; }
        /// <summary>
        /// int对应excel的所在行，为了定位DB更新时的错误行数
        /// </summary>
        public List<KeyValuePair<int, List<string>>> Rows = new List<KeyValuePair<int, List<string>>>();

    }

    public enum ColumnProcessFlg { STRING=0, NUMBER=1, DATETIME=2, TIMESTAMP=3, BINARY=4 }

    public class XlApp
    {
        const int START_ROW = 1;
        const int START_COL = 2;
        public const string SCHEMA_COLUMN_NAME = "ColumnName";
        public const string SCHEMA_IS_KEY = "IsKey";

        //2019/03/08 抛弃.net数据类型，严格按照数据库本身类型来处理数据
        //public const string SCHEMA_DATATYPE_NAME = "DataType";
        public const string SCHEMA_DATATYPE_NAME = "DataTypeName";
        public Dictionary<string,DataTypeCondition> dataTypeConditiones;

        public const string SCHEMA_REMARK = "Remark";
        const string SCHEMA_COLUMN_SIZE = "ColumnSize";
        // CPU密集处理时UI描画机会
        /// <summary>
        /// 表头行的输出信息定义
        /// </summary>
        string[] _schemaList = null;

        public string[] schemaList
        {
            get
            {
                return _schemaList;
            }
            set
            {
                _schemaList = value;
            }
        }
        //2019/03/08 抛弃.net数据类型，严格按照数据库本身类型来处理数据
        DataTypeCondition defaultDataTypeCondition = new DataTypeCondition()
        {
            //如果没有登录对应数据默认作为字符串来处理
            ProcessKbn = (int)ColumnProcessFlg.STRING,
            ExcelFormatString = null,
            DatabaseFormatString = null,
            UpdateFormatString = null
        };
        public DataTypeCondition GetDataTypeCondition(string dataTypeName)
        {
            //DevelopWorkspace.Base.Logger.WriteLine("start", Level.DEBUG);
            if (dataTypeConditiones.ContainsKey(dataTypeName.ToLower()))
            {
                return dataTypeConditiones[dataTypeName.ToLower()];
            }
            else
            {
                return defaultDataTypeCondition;
            }

        }
        System.Data.Common.DbConnection _dbConnection = null;
        string _connectionString = null;
        string _allTablesSql = "";
        public Provider Provider { get; set; }
        public ConnectionHistory ConnectionHistory { get; set; }

        public string SchemaName { get; set; }
        public DbConnection DbConnection
        {
            get
            {
                return _dbConnection;
            }

            set
            {
                _dbConnection = value;
            }
        }

        public string ConnectionString
        {

            get
            {
                return _connectionString;
            }

            set
            {
                _connectionString = value;
            }
        }

        public string AllTablesSql
        {
            get
            {
                return _allTablesSql;
            }

            set
            {
                _allTablesSql = value;
            }
        }

        //2019/02/27 移动到Base.Excel内并重写改善问题
        //static dynamic _xlApp = null;
        ///// <summary>
        ///// 尽量使用同一个Excel的实例，目前依然会出现不能很好的清除无效Excel进程的现象
        ///// </summary>
        ///// <returns></returns>
        //public static dynamic Excel
        //{

        //    get
        //    {
        //        if (XlApp._xlApp == null)
        //        {
        //            try
        //            {
        //                XlApp._xlApp = (Microsoft.Office.Interop.Excel.Application)
        //                    System.Runtime.InteropServices.Marshal.GetActiveObject("Excel.Application");
        //            }
        //            catch (Exception ex)
        //            {
        //                //下面这段代码是借鉴了ExcelDna这个库的hint,THX
        //                Process[] excels = Process.GetProcessesByName("Excel");
        //                foreach (Process excel in excels)
        //                {
        //                    ExcelDna.Integration.ExcelDnaUtil.Initialize(excel.Id, excel.Threads[0].Id);
        //                    XlApp._xlApp = ExcelDna.Integration.ExcelDnaUtil.Application;
        //                    if (XlApp._xlApp != null) break;
        //                }
        //                if (XlApp._xlApp == null)
        //                {
        //                    //如果没有娶到那么创建新的实例
        //                    XlApp._xlApp = new Microsoft.Office.Interop.Excel.Application();
        //                }
        //            }
        //            //如果没有娶到那么创建新的实例
        //            if (XlApp._xlApp == null)
        //            {
        //                //如果没有娶到那么创建新的实例
        //                XlApp._xlApp = new Microsoft.Office.Interop.Excel.Application();
        //                XlApp._xlApp.Workbooks.Add();
        //            }
        //            //如果没有打开的WORKBOOK那么创建一个
        //            if (XlApp._xlApp.Workbooks.Count == 0)
        //            {
        //                XlApp._xlApp.Workbooks.Add();
        //            }
        //        }
        //        else
        //        {
        //            //人为的关闭Excel时的一个对应，即COM指向无效目标时那么在重新打开一个Excel实例
        //            try
        //            {
        //                //XlApp._xlApp.Visible = true;
        //                var targetSheet = XlApp._xlApp.ActiveWorkbook.ActiveSheet;
        //            }
        //            catch (Exception ex)
        //            {
        //                XlApp._xlApp = null;
        //                XlApp._xlApp = new Microsoft.Office.Interop.Excel.Application();
        //                XlApp._xlApp.Workbooks.Add();
        //            }
        //        }

        //        return XlApp._xlApp;
        //    }
        //}


        /// <summary>
        /// 
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="bDbCreate">对目标DB进行表DROP及CREATE，INSERT数据的操作，否则只进行INSERT数据</param>
        public DataSet DoAccordingActivedSheet(Provider provider, string schemaName,List<TableInfo> tableList, DbCommand cmd, eProcessType processType, bool bDbCreate = false)
        {
            //目前这个版本针对blob，clob类型没有做对应，将来是否有需要？只有到那时候才知道
            //http://stackoverflow.com/questions/5371222/getting-binary-data-using-sqldatareader
            //ToDo如果存在blob,clob类型的字段，那么在发行更新SQL的时候需要把它们从项目中给剔除来会更好些，这些项目的更新可以通过第三方tool来辅助完成
            string append_column_type = provider.AppendColumnType;
            string append_dual = provider.AppendDual; //oracle的时候使用from dual
            DataSet orgDataSet = null;
            DbTransaction dbTran = null;
            string fullTableName = null;
            int iRow = 0, iCol = 0, iRewindRow = 0;
            dynamic excel = null;
            //防止无限等待导致死锁
            cmd.CommandTimeout = 30;
            try
            {
                //2019/02/27
                excel = Excel.GetLatestActiveExcelRef();
                if (excel == null)
                {
                    DevelopWorkspace.Base.Services.ErrorMessage("エクスポートされたシートと同様なフォーマットのシートを選択して、再度実行してください");
                    return orgDataSet;
                }
                var targetSheet = excel.ActiveWorkbook.ActiveSheet;
                if (targetSheet.UsedRange.Rows.Count < 7)
                {
                    DevelopWorkspace.Base.Services.ErrorMessage("エクスポートされたシートと同様なフォーマットのシートを選択して、再度実行してください");
                    return new DataSet();
                }

                Dictionary<string, DbApplyWork> workArea = new Dictionary<string, DbApplyWork>();

                if (processType == eProcessType.DB_APPLY || processType == eProcessType.SQL_ONLY)
                {
                    dbTran = cmd.Connection.BeginTransaction();
                    DevelopWorkspace.Base.Logger.WriteLine("database transaction begin...",Level.DEBUG);
                }
                #region 从EXCEL搜集更新数据并缓存它为后续更新数据使用
                object[,] value2_copy = targetSheet.Range(targetSheet.Cells(START_ROW, START_COL),
                                            targetSheet.Cells(targetSheet.UsedRange.Rows.Count + START_ROW,
                                            targetSheet.UsedRange.Columns.Count + START_COL)).Value2;
                bool bTableTokenHit = false;
                bool bDbCreateOnOff = false;
                string strTableName = "";

                Dictionary<string, List<string>> dicShema = null;
                List<DataTypeCondition> dataTypeConditionList = null;
                List<string> lstRowData = null;

                //收集表数据处理
                for (iRow = 1; iRow < value2_copy.GetLength(0); iRow++)
                {
                    //如果整行都是空白那么判定其为表数据的结束
                    if (bTableTokenHit)
                    {
                        for (iCol = 1; iCol < value2_copy.GetLength(1); iCol++)
                        {
                            if (value2_copy[iRow, iCol] != null)
                                break;
                            if (iCol == value2_copy.GetLength(1) - 1)
                                bTableTokenHit = false;
                        }
                    }
                    //find where table begin
                    if (bTableTokenHit == false)
                    {
                        bTableTokenHit = true;
                        //前两个CELL不为空以外有一个为空则认定不是表头的开始
                        for (iCol = 1 + 2; iCol < value2_copy.GetLength(1); iCol++)
                        {
                            if (value2_copy[iRow, iCol] != null)
                                bTableTokenHit = false;
                        }
                        if (value2_copy[iRow, 1] == null || value2_copy[iRow, 2] == null)
                            bTableTokenHit = false;
                        //如果是表头则开始处理表名以及其余属性行信息
                        if (bTableTokenHit)
                        {
                            bDbCreateOnOff = true;

                            dicShema = new Dictionary<string, List<string>>();
                            foreach (string keyword in schemaList)
                            {
                                dicShema.Add(keyword, new List<string>());
                            }

                            strTableName = value2_copy[iRow, 1].ToString();

                            Base.Services.BusyWorkIndicatorService($"Collecting:{ strTableName }");

                            //缓存数据库操作
                            workArea.Add(strTableName, new DbApplyWork() { TableName = strTableName });

                            if (processType == eProcessType.DB_APPLY)
                            {
                                fullTableName = string.IsNullOrEmpty(schemaName) ? strTableName : schemaName + "." + strTableName;
                                TableInfo ti = (from tableinfo in tableList where tableinfo.TableName.ToUpper() == strTableName.ToUpper() select tableinfo).FirstOrDefault();
                                string deleteTextSql = "";
                                if (ti != null && !string.IsNullOrWhiteSpace(ti.DeleteClause))
                                {
                                    deleteTextSql = string.Format("delete from {0}", fullTableName + " where " + ti.DeleteClause);
                                }
                                //else
                                //{
                                //    deleteTextSql = string.Format("delete from {0}", fullTableName + " where 1=0");
                                //}

                                //缓存数据库操作
                                workArea[strTableName].DeleteSql = deleteTextSql;
                            }
                            #region 发现表定义后通过表头行区域获取表Schema情报
                            //表属性行定义取得
                            foreach (string keyword in schemaList)
                            {
                                iRow++;
                                for (iCol = 1; iCol < value2_copy.GetLength(1); iCol++)
                                {
                                    if (keyword == SCHEMA_COLUMN_NAME || keyword == SCHEMA_DATATYPE_NAME)
                                    {
                                        if (value2_copy[iRow, iCol] != null)
                                            dicShema[keyword].Add(value2_copy[iRow, iCol].ToString());
                                    }
                                    else
                                    {
                                        dicShema[keyword].Add(value2_copy[iRow, iCol] == null ? "" : value2_copy[iRow, iCol].ToString());
                                    }
                                }
                            }
                            //2019/03/11
                            //如果SCHEMA_COLUMN_NAME和SCHEMA_DATATYPE_NAME的长度不一致说明表的属性存在问题，处理终了
                            if (dicShema[SCHEMA_COLUMN_NAME].Count == 0 || dicShema[SCHEMA_COLUMN_NAME].Count != dicShema[SCHEMA_DATATYPE_NAME].Count) {
                                throw new Exception($"there are inconsistency with columnName and dataTypeName of {strTableName}");
                            }
                            //缓存数据库操作
                            workArea[strTableName].Schemas = dicShema;

                            //2019/03/09 一次性取出避免循环内大量调用
                            dataTypeConditionList = new List<DataTypeCondition>() { };
                            for (iCol = 0; iCol < dicShema[SCHEMA_DATATYPE_NAME].Count; iCol++)
                            {
                                dataTypeConditionList.Add(GetDataTypeCondition(dicShema[SCHEMA_DATATYPE_NAME][iCol]));
                            }
                            workArea[strTableName].DataTypeConditionList = dataTypeConditionList;

                            #endregion
                            if (processType == eProcessType.DIFF_USE)
                            {
                                if (orgDataSet == null) orgDataSet = new DataSet();
                                System.Data.DataTable table = new System.Data.DataTable(strTableName);
                                orgDataSet.Tables.Add(table);

                                //20190302为了正确排序
                                //dicShema[SCHEMA_COLUMN_NAME].ForEach(delegate (string keyword) { table.Columns.Add(keyword); });
                                for (int idx = 0; idx < dicShema[SCHEMA_COLUMN_NAME].Count; idx++) {
                                    //2019/03/02
                                    //无法做到穷举提到配置文件中
                                    //2019/03/08
                                    //if (DatabaseConfig.isStringLikeColumn(dicShema[SCHEMA_DATATYPE_NAME][idx]))
                                    //如果时数字以外的情况一律看作字符串（因为从excel能获取的只有这两类）
                                    if (dicShema[SCHEMA_IS_KEY][idx] == "*")
                                    {
                                        if (dataTypeConditionList[idx].ProcessKbn != (int)ColumnProcessFlg.NUMBER)
                                        {
                                            table.Columns.Add(dicShema[SCHEMA_COLUMN_NAME][idx]);
                                        }
                                        else
                                        {
                                            //TODO 2019/08/27 tinyint值里放如true？会导致崩溃，对应方法待定
                                            switch (dicShema[SCHEMA_DATATYPE_NAME][idx])
                                            {
                                                case "System.Decimal":
                                                    table.Columns.Add(dicShema[SCHEMA_COLUMN_NAME][idx], typeof(System.Decimal));
                                                    break;
                                                case "System.Double":
                                                    table.Columns.Add(dicShema[SCHEMA_COLUMN_NAME][idx], typeof(System.Double));
                                                    break;
                                                case "System.Integer":
                                                    table.Columns.Add(dicShema[SCHEMA_COLUMN_NAME][idx], typeof(int));
                                                    break;
                                                default:
                                                    table.Columns.Add(dicShema[SCHEMA_COLUMN_NAME][idx], typeof(System.Decimal));
                                                    break;
                                            }
                                        }
                                    }
                                    else {
                                        table.Columns.Add(dicShema[SCHEMA_COLUMN_NAME][idx]);
                                    }
                                    //switch (dicShema[SCHEMA_DATATYPE_NAME][idx])
                                    //{
                                    //    //现在取得数据时都使用了SchemaTable里面的DataType列这个关键字，即C#对应的数据类型，而不是DB本身的了
                                    //    //也就是说varchar等等不会出现在这个列表里面了
                                    //    //如果需要看则需要DataReader.GetDbDataType这个方法取得了，他不是从SchemaTable里取出来的，而是数据取得同时才可以取到，比较麻烦
                                    //    //TODO 20190302 这块代码如果碰到未知类型会异常，需要外置等方式，否则会需要频繁对应
                                    //    case "System.String"://postgres
                                    //    case "System.DateTime"://postgres
                                    //    case "System.Byte[]":
                                    //    case "nchar":
                                    //    case "varchar":
                                    //    case "nvarchar":
                                    //    case "ntext":
                                    //    case "datetime":
                                    //    case "System.Boolean":
                                    //    case "System.Object":
                                    //        table.Columns.Add(dicShema[SCHEMA_COLUMN_NAME][idx]);
                                    //        break;
                                    //    case "System.Decimal":
                                    //        table.Columns.Add(dicShema[SCHEMA_COLUMN_NAME][idx], typeof(System.Decimal));
                                    //        break;
                                    //    case "System.Double":
                                    //        table.Columns.Add(dicShema[SCHEMA_COLUMN_NAME][idx], typeof(System.Double));
                                    //        break;
                                    //    default:
                                    //        table.Columns.Add(dicShema[SCHEMA_COLUMN_NAME][idx], typeof(int));
                                    //        break;
                                    //}
                                }

                                List<DataColumn> primaryKeyList = new List<DataColumn>();
                                foreach (var primaryKey in dicShema[SCHEMA_IS_KEY].Select((token, idx) => new { token, idx }))
                                {
                                    if (primaryKey.token == "*")
                                    {
                                        primaryKeyList.Add(table.Columns[primaryKey.idx]);
                                    }
                                }
                                //2019/03/02 如果没有主键设定那么默认第一个字段为主键
                                if (primaryKeyList.Count == 0)
                                {
                                    DevelopWorkspace.Base.Logger.WriteLine($"do nothing with table:{table.TableName} where primarykey does not exist", Level.WARNING);
                                    //primaryKeyList.Add(table.Columns[0]);
                                }
                                else
                                {
                                    table.PrimaryKey = primaryKeyList.ToArray<DataColumn>();
                                    //2019/03/02 为了在diff时可以按照主键顺序输出
                                    table.DefaultView.Sort = (from column in table.PrimaryKey select column.ColumnName).Aggregate((total, next) => total + "," + next);
                                }
                            }

                            iRow++;
                        }
                    }
                    if (bTableTokenHit)
                    {
                        #region 逐行取得数据区域数据
                        //process data region
                        lstRowData = new List<string>();

                        for (iCol = 1; iCol < dicShema[SCHEMA_COLUMN_NAME].Count + 1; iCol++)
                        {
                            if (value2_copy[iRow, iCol] == null)
                            {
                                if (processType == eProcessType.DIFF_USE)
                                {
                                    lstRowData.Add(null);
                                }
                                else
                                {
                                    lstRowData.Add("null");
                                }
                                continue;
                            }
                            else
                            {
                                //2019/03/02
                                //无法做到穷举提到配置文件中
                                //if (DatabaseConfig.isStringLikeColumn(dicShema[SCHEMA_DATATYPE_NAME][iCol - 1]))
                                if (dataTypeConditionList[iCol - 1].ProcessKbn == (int)ColumnProcessFlg.NUMBER)
                                {
                                    lstRowData.Add(value2_copy[iRow, iCol].ToString());
                                }
                                else {
                                    if (processType == eProcessType.DIFF_USE)
                                    {
                                        lstRowData.Add(value2_copy[iRow, iCol].ToString());
                                    }
                                    else
                                    {
                                        string cellString = value2_copy[iRow, iCol].ToString();
                                        //如果值里面有单引号，需要如下特殊处理
                                        if (cellString.IndexOf("'") >= 0) cellString = cellString.Replace("'", "''");
                                        //这里主要时针对日期型如orale的date，timestamp类型，支持用户直接使用SYSDATE, CURRENT_TIMESTAMP内置函数
                                        //2019/09/25 日期型为主键时目前的逻辑会导致timestamp的值经过变化后得到的和原值不同导致insert/update的判断有误
                                        //如果日期字段为主键的话例外处理
                                        //if (dicShema[SCHEMA_IS_KEY][iCol - 1] != "*" && !string.IsNullOrEmpty(dataTypeConditionList[iCol - 1].UpdateFormatString))
                                        if (!string.IsNullOrEmpty(dataTypeConditionList[iCol - 1].UpdateFormatString))
                                        {
                                            string updateFormatString = dataTypeConditionList[iCol - 1].UpdateFormatString;
                                            
                                            if (Regex.Match(cellString, "^[0-9]{4}").Success)
                                            {
                                                //目前只做简单的格式不一致的变换（2019/09/21 -> 2019-09-21）
                                                if (updateFormatString.IndexOf("/") > 0 && cellString.Length > 5 && cellString.IndexOf("-") > 0)
                                                {
                                                    cellString = cellString.Replace("-", "/");
                                                }
                                                else if (updateFormatString.IndexOf("-") > 0 && cellString.Length > 5 && cellString.IndexOf("/") > 0)
                                                {
                                                    cellString = cellString.Replace("/", "-");
                                                }
                                                lstRowData.Add(updateFormatString.FormatWith(new { ColumnValue = cellString }));
                                            }
                                            else
                                                lstRowData.Add(cellString);
                                        }
                                        else {
                                            lstRowData.Add("'" + cellString + "'");
                                        }


                                    }
                                }
                                //switch (dicShema[SCHEMA_DATATYPE_NAME][iCol - 1])
                                //{
                                //    //现在取得数据时都使用了SchemaTable里面的DataType列这个关键字，即C#对应的数据类型，而不是DB本身的了
                                //    //也就是说varchar等等不会出现在这个列表里面了
                                //    //如果需要看则需要DataReader.GetDbDataType这个方法取得了，他不是从SchemaTable里取出来的，而是数据取得同时才可以取到，比较麻烦
                                //    case "System.String"://postgres
                                //    case "System.DateTime"://postgres
                                //    case "System.Byte[]":
                                //    case "nchar":
                                //    case "varchar":
                                //    case "nvarchar":
                                //    case "ntext":
                                //    case "datetime":
                                //    case "System.Boolean":
                                //    case "System.Object":
                                //        if (processType == eProcessType.DIFF_USE)
                                //        {
                                //            lstRowData.Add(value2_copy[iRow, iCol].ToString());
                                //        }
                                //        else
                                //        {
                                //            string cellString = value2_copy[iRow, iCol].ToString();
                                //            //如果值里面有单引号，需要如下特殊处理
                                //            if (cellString.IndexOf("'") >= 0) cellString = cellString.Replace("'", "''");
                                //            lstRowData.Add("'" + cellString + "'");
                                //        }
                                //        break;
                                //    default:
                                //        lstRowData.Add(value2_copy[iRow, iCol].ToString());
                                //        break;
                                //}
                            }
                        }
                        #endregion
                        #region 表创建，这块目的是把测试表以及测试数据放到SQLite内后实施SQL的检索已验证是否数据做成的妥当性，目前还处于一个初始状态
                        if (bDbCreate && bDbCreateOnOff)
                        {
                            bDbCreateOnOff = false;

                            try
                            {
                                string dropTableSql = string.Format("DROP TABLE {0};", strTableName);
                                //缓存数据库操作
                                workArea[strTableName].DropTableSql = dropTableSql;
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.Print(ex.Message);
                            }

                            string innerSql = "";
                            for (int idx = 0; idx < dicShema[SCHEMA_COLUMN_NAME].Count; idx++)
                            {
                                innerSql += dicShema[SCHEMA_COLUMN_NAME][idx];
                                innerSql += " " + dicShema[SCHEMA_DATATYPE_NAME][idx];
                                switch (dicShema[SCHEMA_DATATYPE_NAME][idx])
                                {
                                    case "char":
                                    case "nchar":
                                    case "varchar":
                                    case "nvarchar":
                                    case "ntext":
                                        innerSql += "(" + dicShema[SCHEMA_COLUMN_SIZE][idx] + ")";
                                        break;
                                    default:
                                        break;
                                }
                                if (idx != dicShema[SCHEMA_COLUMN_NAME].Count - 1)
                                {
                                    innerSql += ",";
                                }
                            }
                            string createTableSql = string.Format("CREATE  TABLE {0} ({1});", strTableName, innerSql);
                            //缓存数据库操作
                            workArea[strTableName].CreateTableSql = createTableSql;

                        }
                        #endregion 表创建
                        if (processType == eProcessType.DIFF_USE)
                        {
                            if (lstRowData.Where(o => o != null).ToList().Count == 0) continue;
                            //没有主键的表不做处理
                            if(orgDataSet.Tables[strTableName].PrimaryKey.Count() > 0 )
                                orgDataSet.Tables[strTableName].Rows.Add(lstRowData.Take(dicShema[SCHEMA_COLUMN_NAME].Count).ToArray());
                        }
                        else
                        {
                            //空白行Skip
                            //Todo 根据主键来动态的发行UPDATE还是INSERT文，这样可以增加工具的实用性
                            //首先本地需要加测一下是否有主键冲突后再判断是否更新还是插入
                            //可以一个表的数据缓存之后通过一个结合检索一次性的得到这个结果，问题是数据大的时候需要分割SQL文否则容易过大不能执行
                            if (lstRowData.Where(o => o != "null").ToList().Count == 0) continue;
                            //缓存数据库操作
                            workArea[strTableName].Rows.Add(new KeyValuePair<int, List<string>>(iRow, lstRowData));
                        }
                    }
                }
                if (orgDataSet != null)
                {
                    orgDataSet.AcceptChanges();
                    return orgDataSet;
                }
                #endregion

                //缓存数据库操作
                //Postsql的时候需要 text类型提示 oracle的时候需要from dual对应
                //逻辑的组织尽量使用LINQ以缩短代码（使用描述性编程以求达到可维护性）
                int iProcessCnt = 0;
                foreach (string tableKEY in workArea.Keys)
                {
                    fullTableName = string.IsNullOrEmpty(schemaName) ? tableKEY : schemaName + "." + tableKEY;

                    //显示处理进度
                    iProcessCnt++;
                    Base.Services.BusyWorkIndicatorService(string.Format("{0}/{1}:{2}", iProcessCnt, workArea.Keys.Count, tableKEY));


                    //没有数据是那么进行下一个表的处理
                    if (workArea[tableKEY].Rows.Count == 0) continue;

                    var lstColumnNameWithIdx = workArea[tableKEY].Schemas[SCHEMA_COLUMN_NAME].Select((token, idx) => new { token, idx });
                    var lstKeyWithIdx = workArea[tableKEY].Schemas[SCHEMA_IS_KEY].Select((token, idx) => new { token, idx });
                    //2019/03/09 数据类型规则
                    var lstDataConditionWithIdx = workArea[tableKEY].DataTypeConditionList.Select((token, idx) => new { token, idx });

                    lstKeyWithIdx = (from primary_key in lstKeyWithIdx where primary_key.token == "*" select primary_key);

                    //目前这个版本针对blob，clob类型没有做对应，将来是否有需要？只有到那时候才知道
                    //http://stackoverflow.com/questions/5371222/getting-binary-data-using-sqldatareader
                    var lstColumnTypeWithIdx = workArea[tableKEY].Schemas[SCHEMA_DATATYPE_NAME].Select((token, idx) => new { token, idx });
                    //2019/03/08
                    //var lstExcepColumnIdx = (from columnTypeWithIdx in lstColumnTypeWithIdx where columnTypeWithIdx.token == "System.Byte[]" select columnTypeWithIdx.idx);
//                    var lstExcepColumnIdx = (from columnTypeWithIdx in lstColumnTypeWithIdx where GetDataTypeCondition(columnTypeWithIdx.token).ProcessKbn == (int)ColumnProcessFlg.BINARY select columnTypeWithIdx.idx);
                    var lstExcepColumnIdx = (from columnTypeWithIdx in lstColumnTypeWithIdx join dataCondition in workArea[tableKEY].DataTypeConditionList on columnTypeWithIdx.token equals dataCondition.DataTypeName where dataCondition.ProcessKbn == (int)ColumnProcessFlg.BINARY select columnTypeWithIdx.idx);

                    //对于没有主键的表，则不做任何处理
                    //2019/9/26
                    //if (lstKeyWithIdx.Count() == 0)
                    //{
                    //    DevelopWorkspace.Base.Logger.WriteLine(string.Format("do nothing with table:{0} where primarykey does not exist", tableKEY),Level.WARNING);
                    //    continue;
                    //}

                    if (!string.IsNullOrEmpty(workArea[tableKEY].DeleteSql))
                    {
                        cmd.CommandText = workArea[tableKEY].DeleteSql;
                        DevelopWorkspace.Base.Logger.WriteLine(workArea[tableKEY].DeleteSql, Level.DEBUG);
                        cmd.ExecuteNonQuery();
                    }
                    if (!string.IsNullOrEmpty(workArea[tableKEY].DropTableSql))
                    {
                        DevelopWorkspace.Base.Logger.WriteLine(workArea[tableKEY].DropTableSql, Level.DEBUG);
                        cmd.CommandText = workArea[tableKEY].DropTableSql;
                        cmd.ExecuteNonQuery();
                    }
                    if (!string.IsNullOrEmpty(workArea[tableKEY].CreateTableSql))
                    {
                        DevelopWorkspace.Base.Logger.WriteLine(workArea[tableKEY].CreateTableSql, Level.DEBUG);
                        cmd.CommandText = workArea[tableKEY].CreateTableSql;
                        cmd.ExecuteNonQuery();
                    }
                    //有主健时需要判断update/insert
                    //更新还是新规的判定结果一括取得备用
                    List<List<string>> BatchSelectResult = new List<List<string>>();
                    if (lstKeyWithIdx.Count() != 0)
                    {
                        //TODO 主键时datetime时sql文需要to_char，但是有时候postgres的实际类型和datetime不匹配时仍有错误发生 gession数据库tcd_tcdata表时再现
                        //selectDataSql += DateTimeFormatter.FormatWith(ci);
                        //TableInfo ti = (from tableinfo in tableList where tableinfo.TableName.ToUpper() == tableKEY.ToUpper() select tableinfo).FirstOrDefault();
                        //TODO 格式需要export和apply前后一致？
                        var lstJoinCondtion = (from keyWithIdx in lstKeyWithIdx
                                               join column_name in lstColumnNameWithIdx
                                               on keyWithIdx.idx equals column_name.idx
                                               join dataCondition in lstDataConditionWithIdx
                                               on keyWithIdx.idx equals dataCondition.idx
                                               select string.Format("SUB_DUAL.{0}={1}",
                                                column_name.token, $"{ fullTableName }.{column_name.token}"));
                        //column_name.token, /*tableKEY,*/
                        //                   //2019/03/08
                        //                   dataCondition.token.ProcessKbn == (int)ColumnProcessFlg.DATETIME ? dataCondition.token.DatabaseFormatString.FormatWith(new { ColumnName = $"{ fullTableName }.{column_name.token}" }) : $"{ fullTableName }.{column_name.token}"));

                        //var lstSelectColumn = (from keyWithIdx in lstKeyWithIdx
                        //                       join column_name in lstColumnNameWithIdx
                        //                       on keyWithIdx.idx equals column_name.idx
                        //                       select string.Format("SUB_DUAL.{0}",
                        //                       column_name.token));
                        //所有表数据主键拼接结合检索SQL
                        string batchSelectSql = "select ";
                        string joinOnConditionSql = "\nleft join " + fullTableName + " on ";
                        joinOnConditionSql += lstJoinCondtion.Aggregate((total, next) => total + " and " + next);

                        //2019/09/26 如果有日期型尤其时timestamp型需要再次变化
                        var lstSelectColumn = (from keyWithIdx in lstKeyWithIdx
                                               join column_name in lstColumnNameWithIdx
                                               on keyWithIdx.idx equals column_name.idx
                                               join dataCondition in lstDataConditionWithIdx
                                               on keyWithIdx.idx equals dataCondition.idx
                                               select
                                                    dataCondition.token.ProcessKbn == (int)ColumnProcessFlg.DATETIME || dataCondition.token.ProcessKbn == (int)ColumnProcessFlg.TIMESTAMP ?
                                                        dataCondition.token.DatabaseFormatString.FormatWith(new { ColumnName = $"SUB_DUAL.{column_name.token}", AliasColumnName = column_name.token }) : $"SUB_DUAL.{column_name.token}");

                        batchSelectSql += lstSelectColumn.Aggregate((total, next) => total + "," + next);
                        batchSelectSql += string.Format(",{0}.{1} UpdateFLG from ( ", fullTableName, workArea[tableKEY].Schemas[SCHEMA_COLUMN_NAME][lstKeyWithIdx.First().idx]);

                        List<KeyValuePair<string, int>> lstKeyCollision = new List<KeyValuePair<string, int>>();
                        #region 所有表数据主键拼接结合检索SQL

                        List<string> dividedSqlList = new List<string>();
                        string dividedSql = batchSelectSql;
                        for (int rowIdx = 0; rowIdx < workArea[tableKEY].Rows.Count; rowIdx++)
                        {
                            KeyValuePair<int, List<string>> row = workArea[tableKEY].Rows[rowIdx];
                            var lstRowDataWithIdx = row.Value.Select((token, idx) => new { token, idx });
                            //Postsql的时候需要 text类型提示 oracle的时候需要from dual对应
                            //TODO 2019/03/05 需要进一步测试对应
                            var lstUnionSelect = (from columnNameWithIdx in lstColumnNameWithIdx
                                                  join keyWithIdx in lstKeyWithIdx
                                                   on columnNameWithIdx.idx equals keyWithIdx.idx
                                                  join rowDataWithIdx in lstRowDataWithIdx
                                                   on columnNameWithIdx.idx equals rowDataWithIdx.idx
                                                  select string.Format("{0} {1} as {2}", rowDataWithIdx.token.StartsWith("'") ? append_column_type : "", rowDataWithIdx.token, columnNameWithIdx.token));

                            //使用这个数据结构判断是否有主键冲突
                            lstKeyCollision.Add(new KeyValuePair<string, int>(
                                (from keyWithIdx in lstKeyWithIdx
                                 join rowDataWithIdx in lstRowDataWithIdx
                                  on keyWithIdx.idx equals rowDataWithIdx.idx
                                 select rowDataWithIdx.token).Aggregate((total, next) => total + "," + next),
                                row.Key));


                            string unionSelectSql = "select ";
                            unionSelectSql += lstUnionSelect.Aggregate((total, next) => total + "," + next);
                            unionSelectSql += append_dual;
                            //2019/02/25 SQL字符串长度过大会导致数据库错误，这里进行分割处理
                            if ((rowIdx + 1) % DatabaseConfig.This.sqlRoundupSize == 0 || rowIdx == workArea[tableKEY].Rows.Count - 1)
                            {
                                dividedSql += string.Format("{0}) SUB_DUAL", unionSelectSql);
                                dividedSqlList.Add(dividedSql);
                                dividedSql = batchSelectSql;
                            }
                            else
                            {
                                dividedSql += string.Format("{0} union\n", unionSelectSql);
                            }

                            //if (rowIdx == workArea[tableKEY].Rows.Count - 1)
                            //{
                            //    batchSelectSql += string.Format("{0}) SUB_DUAL", unionSelectSql);
                            //}
                            //else
                            //{
                            //    batchSelectSql += string.Format("{0} union\n", unionSelectSql);
                            //}
                        }
                        #endregion

                        //首先判断目前数据是否有主键冲突
                        var groupCollisions = from keyCollision in lstKeyCollision group keyCollision by keyCollision.Key into g where g.Count() > 1 select g;
                        bool bCollisionOccur = false;
                        foreach (var groupCollision in groupCollisions)
                        {
                            bCollisionOccur = true;
                            foreach (var collision in groupCollision)
                            {
                                DevelopWorkspace.Base.Logger.WriteLine(string.Format("primary key collision occured at {0} row，keys:{1}", collision.Value, collision.Key), Level.ERROR);
                            }
                        }
                        if (bCollisionOccur) throw new Exception("");

                        foreach (var eachSql in dividedSqlList)
                        {
                            string selectUionSql = eachSql;
                            selectUionSql += joinOnConditionSql;
                            DevelopWorkspace.Base.Logger.WriteLine(selectUionSql, Level.DEBUG);
                            cmd.CommandText = selectUionSql;
                            using (DbDataReader rdr = cmd.ExecuteReader())
                            {
                                while (rdr.Read())
                                {
                                    List<string> rowResult = new List<string>();
                                    for (int idx = 0; idx < rdr.FieldCount; idx++)
                                    {
                                        rowResult.Add(rdr[idx].ToString());
                                    }
                                    BatchSelectResult.Add(rowResult);
                                }
                            }
                        }

                    }
                    for (int rowIdx = 0; rowIdx < workArea[tableKEY].Rows.Count; rowIdx++)
                    {
                        iRewindRow = workArea[tableKEY].Rows.Count - rowIdx;
                        KeyValuePair<int, List<string>> row = workArea[tableKEY].Rows[rowIdx];
                        var lstRowDataWithIdx = row.Value.Select((token, idx) => new { token, idx });
                        bool IsUpdateRowData = false;
                        //根据结果组装UPDATE文或者INSERT文
                        if (lstKeyWithIdx.Count() != 0)
                        {
                            foreach (List<string> rowResult in BatchSelectResult)
                            {
                                int resIdx = 0;
                                bool isHit = false;
                                for (resIdx = 0; resIdx < rowResult.Count - 1; resIdx++)
                                {
                                    // 2019/09/26 yhou由于日期型被格式化，取出的内容和实际的存在一个被格式化，一个没有被格式化导致不能使用==进行比较
                                    ////字符字段被单引号括起来的原因，和数据库取出来时不一致啦，这里做下补丁处理，如果不考虑这个因素，完全可以使用LINQ描述这段逻辑

                                    // 通常的字符类型
                                    if (row.Value[resIdx].StartsWith("'"))
                                    {
                                        if (row.Value[lstKeyWithIdx.ToList()[resIdx].idx] != "'" + rowResult[resIdx].Replace("'", "''") + "'") break;
                                    }
                                    // 数字类型等
                                    else if (row.Value[resIdx].IndexOf("'") == -1)
                                    {
                                        if (row.Value[lstKeyWithIdx.ToList()[resIdx].idx] != rowResult[resIdx]) break;
                                    }
                                    // 如果row.value是日付类型，那么在这个时点已经通过to_date...等转意，需要使用下面的方式判断
                                    else
                                    {
                                        if (row.Value[lstKeyWithIdx.ToList()[resIdx].idx].IndexOf(rowResult[resIdx]) == -1) break;
                                    }
                                    //2019/02/25 如果所有的键值都相等则认为是更新
                                    if (resIdx == rowResult.Count - 2) isHit = true;
                                }
                                if (isHit)
                                {
                                    //2019/02/25 如果找到一致的记录后，这之后就不会再有一致的数据了，为了提高性能下一次不作为比较对象,顾删除之
                                    BatchSelectResult.Remove(rowResult);
                                    //2019/02/25 isHit为真时UpdateFLG是不可能为空的
                                    if (!string.IsNullOrEmpty(rowResult[resIdx]))
                                    {
                                        IsUpdateRowData = true;
                                        break;
                                    }
                                    //2019/02/25 如果所有的键值都相等则认为是更新
                                    break;
                                }
                            }
                        }
                        //目前这个版本针对blob，clob类型没有做对应，将来是否有需要？只有到那时候才知道
                        //http://stackoverflow.com/questions/5371222/getting-binary-data-using-sqldatareader
                        //更新SQL构筑并执行
                        if (IsUpdateRowData)
                        {
                            //Postsql的时候需要 text类型提示 oracle的时候需要from dual对应
                            var lstUpdateCondition = (from columnNameWithIdx in lstColumnNameWithIdx
                                                      join rowDataWithIdx in lstRowDataWithIdx
                                                       on columnNameWithIdx.idx equals rowDataWithIdx.idx
                                                      //目前这个工具还没有计划支持blob，clob等字段的编辑，下面的这个处理剔除不适合更新的字段
                                                      where !(lstExcepColumnIdx.Contains(columnNameWithIdx.idx))
                                                      select string.Format("{0}={1}", columnNameWithIdx.token, rowDataWithIdx.token));
                            var lstWhereCondition = (from columnNameWithIdx in lstColumnNameWithIdx
                                                     join keyWithIdx in lstKeyWithIdx
                                                      on columnNameWithIdx.idx equals keyWithIdx.idx
                                                     join rowDataWithIdx in lstRowDataWithIdx
                                                      on columnNameWithIdx.idx equals rowDataWithIdx.idx
                                                     select string.Format("{0}={1}", columnNameWithIdx.token, rowDataWithIdx.token));

                            string updateTextSql = string.Format("UPDATE {0} SET {1} WHERE {2}",
                            fullTableName,
                            lstUpdateCondition.Aggregate((total, next) => total + "," + next),
                            lstWhereCondition.Aggregate((total, next) => total + " and " + next));
                            DevelopWorkspace.Base.Logger.WriteLine(updateTextSql,Level.DEBUG);
                            cmd.CommandText = updateTextSql;
                            cmd.ExecuteNonQuery();
                        }
                        //新规SQL构筑并执行
                        else
                        {
                            string insertTextSql = string.Format("INSERT INTO {0} ({1}) VALUES ({2})",
                            fullTableName,
                            workArea[tableKEY].Schemas[SCHEMA_COLUMN_NAME].Aggregate((total, next) => total + "," + next),
                            row.Value.Take(workArea[tableKEY].Schemas[SCHEMA_COLUMN_NAME].Count).Aggregate((total, next) => total + "," + next));
                            DevelopWorkspace.Base.Logger.WriteLine(insertTextSql,Level.DEBUG);
                            cmd.CommandText = insertTextSql;
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                dbTran.Commit();
                DevelopWorkspace.Base.Logger.WriteLine("database committed",Level.INFO);

            }
            catch (Exception ex)
            {
                //DevelopWorkspace.Base.Services.ErrorMessage(ex.Message);
                DevelopWorkspace.Base.Logger.WriteLine(ex.Message, Base.Level.ERROR);
                DevelopWorkspace.Base.Logger.WriteLine(string.Format("there are some problem at {0} row {1} column in Activesheet", iRow - iRewindRow, iCol), Base.Level.ERROR);
                if (dbTran != null)
                {
                    dbTran.Rollback();
                    DevelopWorkspace.Base.Logger.WriteLine("database rollbacked");
                }

                return null;
            }

            finally
            {
                if(excel!=null) Marshal.ReleaseComObject(excel);
            }
            return orgDataSet;

        }
        public void GetDiffDataSet(TableInfo[] selectTableNameList, DbCommand cmd, DataSet orgDataset)
        {
            try
            {
                int iDo = 1;
                foreach (TableInfo tableinfo in selectTableNameList)
                {
                    //2019/03/15
                    if (Base.Services.longTimeTaskState == LongTimeTaskState.Cancel) return;

                    Base.Services.BusyWorkIndicatorService($"Comparing({iDo++}/{selectTableNameList.Count()}):{ tableinfo.TableName }");
                    GetTableDataWithSchema(tableinfo, cmd, orgDataset);
                }
            }
            catch (Exception ex)
            {
                DevelopWorkspace.Base.Logger.WriteLine(ex.Message, Base.Level.ERROR);
            }
        }
        public void DrawDifferenceToExcel(DataSet diffDataSet)
        {
            dynamic excel = Excel.GetLatestActiveExcelRef();
            try
            {
                //2019/02/27
                excel.Visible = true;
                excel.ScreenUpdating = false;
                string comparedSheet = excel.ActiveWorkbook.ActiveSheet.Name;

                //todo
                (System.Windows.Application.Current.MainWindow as DevelopWorkspace.Main.MainWindow).UnInstallExcelWatch();


                excel.ActiveWorkbook.Worksheets.Add(System.Reflection.Missing.Value,
                            excel.ActiveWorkbook.Worksheets[excel.ActiveWorkbook.Worksheets.Count],
                            System.Reflection.Missing.Value,
                             System.Reflection.Missing.Value);


                var targetSheet = excel.ActiveWorkbook.ActiveSheet;
                string sheetname = $"{comparedSheet}_vs_DB_at{DateTime.Now.ToString("h.mm.ss")}";
                //2019/3/13
                if (sheetname.Length > 31)
                {
                    DevelopWorkspace.Base.Logger.WriteLine("sheetname's length is longer than 31", Base.Level.WARNING);
                }
                else
                {
                    targetSheet.Name = $"{comparedSheet}_vs_DB_at{DateTime.Now.ToString("h.mm.ss")}";
                }

                int sartRow = START_ROW;
                int startCol = START_COL;
                Range selected;
                int iDo = 1;
                foreach (System.Data.DataTable table in diffDataSet.Tables)
                {

                    //2019/03/15
                    if (Base.Services.longTimeTaskState == LongTimeTaskState.Cancel) break;

                    Base.Services.BusyWorkIndicatorService($"Drawing({iDo++}/{diffDataSet.Tables.Count}) { table.TableName}");
                    System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate { }));
                    //按主键排序
                    //rawtable.DefaultView.Sort = (from column in rawtable.PrimaryKey select column.ColumnName).Aggregate((total, next) => total + "," + next);
                    //System.Data.DataTable table = rawtable.DefaultView.ToTable();

                    string tableName = table.TableName;
                    //Table属性定义行区域颜色定制
                    selected = targetSheet.Range(targetSheet.Cells(sartRow, startCol),
                        targetSheet.Cells(sartRow, startCol + 1));
                    selected.Interior.Pattern = Microsoft.Office.Interop.Excel.Constants.xlSolid;
                    selected.Interior.ThemeColor = Microsoft.Office.Interop.Excel.XlThemeColor.xlThemeColorAccent4;
                    selected.Value2 = new string[] { tableName, tableName };
                    XlApp.DrawBorder(selected);

                    sartRow++;

                    //List内容转换成二维数组
                    int iTotalRow = 0;
                    //第一列作为unchanged/delete/add的标识列
                    //string[,] value2_copy = new string[table.Rows.Count + 5, table.Columns.Count + 1];
                    string[,] value2_copy = new string[table.Rows.Count * 2 + 5, table.Columns.Count + 1];

                    //表头处理
                    for (int idx = 0; idx < table.Columns.Count; idx++)
                    {
                        value2_copy[iTotalRow, idx + 1] = (from dataColumn in table.PrimaryKey where dataColumn.ColumnName == table.Columns[idx].ColumnName select dataColumn).FirstOrDefault()!=null?"*":"";
                        value2_copy[iTotalRow+1, idx + 1] = table.Columns[idx].ColumnName;
                    }
                    iTotalRow++;
                    iTotalRow++;
                    iTotalRow++;
                    iTotalRow++;
                    iTotalRow++;


                    //System.Data.DataTable deletedTable = table.GetChanges(DataRowState.Deleted);
                    //if (deletedTable != null)
                    //{
                    //    for (int row = 0; row < deletedTable.Rows.Count; row++)
                    //    {
                    //        deletedTable.Rows[row].RejectChanges();
                    //    }
                    //}
                    //为了通过DefaultView得到包含delete行的排序的结果需要把delete行复原
                    List<DataRow> deleteRows = new List<DataRow>();
                    for (int row = 0; row < table.Rows.Count; row++)
                    {
                        if (table.Rows[row].RowState == DataRowState.Deleted) {
                            table.Rows[row].RejectChanges();
                            deleteRows.Add(table.Rows[row]);
                        }
                    }


                    for (int row = 0; row < table.Rows.Count; row++)
                    {
                        for (int col = 0; col < table.Columns.Count; col++)
                        {
                            //if (table.DefaultView[row].Row.RowState == DataRowState.Deleted)
                            //{
                            //    //原始列
                            //    //更新列(空白)
                            //    value2_copy[iTotalRow, col + 1] = table.DefaultView[row].Row[col, DataRowVersion.Original].ToString();
                            //    value2_copy[iTotalRow + 1, col + 1] = "";
                            //}
                            //删除行
                            if (deleteRows.Contains(table.DefaultView[row].Row))
                            {
                                //原始列
                                value2_copy[iTotalRow, col + 1] = table.DefaultView[row].Row[col, DataRowVersion.Original].ToString();
                            }
                            else if (table.DefaultView[row].Row.RowState == DataRowState.Modified)
                            {
                                //原始列
                                //更新列
                                value2_copy[iTotalRow, col + 1] = table.DefaultView[row].Row[col, DataRowVersion.Original].ToString();
                                value2_copy[iTotalRow + 1, col + 1] = table.DefaultView[row].Row[col].ToString();
                            }
                            else if (table.DefaultView[row].Row.RowState == DataRowState.Added)
                            {
                                //更新列
                                value2_copy[iTotalRow, col + 1] = table.DefaultView[row].Row[col].ToString();
                            }
                            else if (table.DefaultView[row].Row.RowState == DataRowState.Unchanged)
                            {
                                //更新列=原始列
                                value2_copy[iTotalRow, col + 1] = table.DefaultView[row].Row[col].ToString();
                            }
                        }
                        //第一列作为unchanged/delete/add的标识列
                        if (deleteRows.Contains(table.DefaultView[row].Row))
                        {
                            //原始列
                            //更新列
                            value2_copy[iTotalRow, 0] = "Deleted";
                            iTotalRow++;
                        }
                        else if (table.DefaultView[row].Row.RowState == DataRowState.Unchanged)
                        {
                            //更新列=原始列
                            //value2_copy[iTotalRow, 0] = table.DefaultView[row].Row.RowState.ToString();
                            iTotalRow++;
                        }
                        else if (table.DefaultView[row].Row.RowState == DataRowState.Added)
                        {
                            //追加类
                            value2_copy[iTotalRow, 0] = table.DefaultView[row].Row.RowState.ToString();
                            iTotalRow++;
                        }
                        else
                        {
                            //原始列
                            //更新列
                            value2_copy[iTotalRow, 0] = table.DefaultView[row].Row.RowState.ToString(); ;
                            value2_copy[iTotalRow + 1, 0] = table.DefaultView[row].Row.RowState.ToString();
                            iTotalRow++;
                            iTotalRow++;

                        }


                    }
                    //Table属性定义行区域颜色定制
                    selected = targetSheet.Range(targetSheet.Cells(sartRow, startCol),
                        targetSheet.Cells(sartRow + schemaList.GetLength(0) - 1, value2_copy.GetLength(1) + startCol - 2));
                    selected.Interior.Pattern = Microsoft.Office.Interop.Excel.Constants.xlSolid;
                    selected.Interior.ThemeColor = Microsoft.Office.Interop.Excel.XlThemeColor.xlThemeColorAccent5;
                    //Data拷贝到指定区域
                    selected = targetSheet.Range(targetSheet.Cells(sartRow, startCol - 1),
                                                 targetSheet.Cells(iTotalRow + sartRow - 1, value2_copy.GetLength(1) + startCol - 2));
                    //targetSheet.Cells(value2_copy.GetLength(0) + sartRow - 1, value2_copy.GetLength(1) + startCol - 2));
                    selected.NumberFormat = "@";
                    selected.Value2 = value2_copy;

                    selected = targetSheet.Range(targetSheet.Cells(sartRow, startCol),
                                                 targetSheet.Cells(iTotalRow + sartRow - 1, value2_copy.GetLength(1) + startCol - 2));
                    XlApp.DrawBorder(selected);

                    ///
                    if(DatabaseConfig.This.applyFormatConditionForDiffResult) DrawDiffResultConditionFormat(selected, sartRow);


                    sartRow = iTotalRow + sartRow + 2;
                    //sartRow = value2_copy.GetLength(0) + sartRow + 2;
                }
                targetSheet.Columns("A:AZ").EntireColumn.AutoFit();

                excel.ScreenUpdating = true;
            }
            finally
            {
                if (excel != null)
                {
                    //todo
                    (System.Windows.Application.Current.MainWindow as DevelopWorkspace.Main.MainWindow).InstallExcelWatch();

                    excel.ScreenUpdating = true;
                    Marshal.ReleaseComObject(excel);
                }
            }
            //excel.Quit();
        }

        public void LoadDataIntoExcel(TableInfo[] selectTableNameList, DbCommand cmd)
        {
            dynamic excel = null;
            try
            {
                //2019/02/27
                excel = Excel.GetLatestActiveExcelRef(true);
                if (excel == null)
                {
                    DevelopWorkspace.Base.Services.ErrorMessage("Excelをただしく起動できないので、PC環境をご確認の上、再度実行してください");
                    //DevelopWorkspace.Base.Services.ErrorMessage("Can't start Excel application correctly,please comfirm your PC enviroment");
                    return;
                }
                excel.Visible = true;
                var targetSheet = excel.ActiveWorkbook.ActiveSheet;
                if (targetSheet.UsedRange.Rows.Count > 1)
                {
                    //DevelopWorkspace.Base.Services.ErrorMessage("現在のシートにデータがあるため、エクスポート先として指定できない、新しいシートを選択して、再度実行してください");
                    DevelopWorkspace.Base.Logger.WriteLine("Somewhat has already existed in active worksheet,create a new worksheet", Level.WARNING);


                    //todo
                    (System.Windows.Application.Current.MainWindow as DevelopWorkspace.Main.MainWindow).UnInstallExcelWatch();

                    excel.ActiveWorkbook.Worksheets.Add(System.Reflection.Missing.Value, excel.ActiveWorkbook.Worksheets[excel.ActiveWorkbook.Worksheets.Count], System.Reflection.Missing.Value, System.Reflection.Missing.Value);

                    targetSheet = excel.ActiveWorkbook.ActiveSheet;
                    //return;
                }
                //int j = targetSheet.UsedRange.Columns.Count;
                excel.ScreenUpdating = false;


                //TODO 2019/3/4 
                //目前这个阶段如果表数量过大，每个表的后台程序都会占用一个链接（取columns时使用一个，结束后紧接着取表数据时有使用一个
                //如果有300各表理论上最大需要300个链接，当然使用连接池时，如果没有得到会待机等待，直到取得或者超时失败
                //Host=127.0.0.1;Port=5432;Username=postgres;Password=admin;Database=gsession;Pooling=true;MinPoolSize=1;MaxPoolSize=100;CommandTimeout=5000000;
                if (DatabaseConfig.This.backgroundWorkerMode) GeTableDataWithSchemaForSelectedTables(selectTableNameList, cmd);

                int sartRow = START_ROW;
                int startCol = START_COL;
                Range selected;
                int iProcessCnt = 0;

                //2019/03/10 TODO
                bool goodFormatMode = !DatabaseConfig.This.plainFormatMode;
                List<string[,]> cacheResults = new List<string[,]>() { };
                int tableProcessCount = 0;

                foreach (TableInfo tableInfo in selectTableNameList)
                {
                    iProcessCnt++;
                    Base.Services.BusyWorkIndicatorService(string.Format("{0}/{1}:{2}", iProcessCnt, selectTableNameList.Count(), tableInfo.TableName));


                    //2019/03/15
                    System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate { }));
                    if (Base.Services.longTimeTaskState == LongTimeTaskState.Cancel) break;

                    //System.Threading.Thread.Sleep(1000);
                    Services.executeWithBackgroundAction(() =>
                    {
                        if (goodFormatMode)
                        {
                            //Table属性定义行区域颜色定制
                            selected = targetSheet.Range(targetSheet.Cells(sartRow, startCol),
                                targetSheet.Cells(sartRow, startCol + 1));
                            selected.Interior.Pattern = Microsoft.Office.Interop.Excel.Constants.xlSolid;
                            //selected.Interior.ThemeColor = Microsoft.Office.Interop.Excel.XlThemeColor.xlThemeColorAccent4;
                            selected.Interior.Color = System.Drawing.ColorTranslator.ToOle(tableInfo.ExcelTableHeaderThemeColor);

                            selected.Value2 = new string[] { tableInfo.TableName, tableInfo.Remark };
                            XlApp.DrawBorder(selected);
                            sartRow++;

                            //TODO 2019/3/4
                            string[,] value2_copy = null;
                            if (DatabaseConfig.This.backgroundWorkerMode)
                                value2_copy = GetTableDataWithSchemaFromCache(tableInfo, cmd);
                            else
                                value2_copy = GetTableDataWithSchema(tableInfo, cmd);

                            //Table属性定义行区域颜色定制
                            selected = targetSheet.Range(targetSheet.Cells(sartRow, startCol),
                                targetSheet.Cells(sartRow + schemaList.GetLength(0) - 1, value2_copy.GetLength(1) + startCol - 1));
                            selected.Interior.Pattern = Microsoft.Office.Interop.Excel.Constants.xlSolid;
                            //selected.Interior.ThemeColor = Microsoft.Office.Interop.Excel.XlThemeColor.xlThemeColorAccent5;
                            selected.Interior.Color = System.Drawing.ColorTranslator.ToOle(tableInfo.ExcelSchemaHeaderThemeColor);

                            //通过SQL文做成数据时第一行为dummy数据，用于提示用户
                            if (tableInfo.WhereCondition != null)
                            {
                                selected = targetSheet.Range(targetSheet.Cells(sartRow + schemaList.GetLength(0), startCol),
                                    targetSheet.Cells(sartRow + schemaList.GetLength(0), value2_copy.GetLength(1) + startCol - 1));
                                selected.Interior.Pattern = Microsoft.Office.Interop.Excel.Constants.xlSolid;
                                selected.Interior.ThemeColor = Microsoft.Office.Interop.Excel.XlThemeColor.xlThemeColorAccent2;
                            }
                            //Data拷贝到指定区域
                            selected = targetSheet.Range(targetSheet.Cells(sartRow, startCol),
                                targetSheet.Cells(value2_copy.GetLength(0) + sartRow - 1, value2_copy.GetLength(1) + startCol - 1));
                            selected.NumberFormat = "@";
                            selected.Value2 = value2_copy;
                            XlApp.DrawBorder(selected);
                            sartRow = value2_copy.GetLength(0) + sartRow + 2;

                            //TODO 2019/3/4 为了防止缓存参照过长阻碍垃圾回收
                            value2_copy = null;
                        }
                        else
                        {
                            tableProcessCount++;
                            string[,] block_header = new string[,] { { tableInfo.TableName, tableInfo.Remark } };
                            string[,] block_data = GetTableDataWithSchema(tableInfo, cmd);

                            int total = 0;
                            int maxColumnSize = 0;
                            cacheResults.ForEach(block => { total += block.GetLength(0); });
                            //last one or much than 10000
                            if (tableProcessCount == selectTableNameList.Count() || total != 0 && total + block_header.GetLength(0) + block_data.GetLength(0) > DatabaseConfig.This.plainFormatRoundupSize)
                            {
                                if (tableProcessCount == selectTableNameList.Count())
                                {
                                    cacheResults.Add(block_header);
                                    cacheResults.Add(block_data);
                                }
                                //draw data to Excel
                                total = 0;
                                maxColumnSize = 0;
                                cacheResults.ForEach(block => { maxColumnSize = (block.GetLength(1) > maxColumnSize) ? block.GetLength(1) : maxColumnSize; });
                                cacheResults.ForEach(block => { total += block.GetLength(0); });
                                total += cacheResults.Count();

                                string[,] value2_copy = new string[total, maxColumnSize];

                                //merge into a single string[,]
                                int iRowNum, iColNum, iRow, iCol, iHeaderRow, iTotalRowNum = 0;
                                for (int idx = 0; idx < cacheResults.Count(); idx++)
                                {

                                    //header
                                    iRowNum = cacheResults[idx].GetLength(0);
                                    iColNum = cacheResults[idx].GetLength(1);
                                    for (iRow = 0; iRow < iRowNum; iRow++)
                                    {
                                        for (iCol = 0; iCol < iColNum; iCol++)
                                        {
                                            value2_copy[iTotalRowNum + iRow, iCol] = cacheResults[idx][iRow, iCol];
                                        }
                                    }
                                    idx++;
                                    iHeaderRow = iRowNum;
                                    //data
                                    iRowNum = cacheResults[idx].GetLength(0);
                                    iColNum = cacheResults[idx].GetLength(1);
                                    for (iRow = 0; iRow < iRowNum; iRow++)
                                    {
                                        for (iCol = 0; iCol < iColNum; iCol++)
                                        {
                                            value2_copy[iTotalRowNum + iRow + iHeaderRow, iCol] = cacheResults[idx][iRow, iCol];
                                        }
                                    }
                                    //table split row
                                    value2_copy[iTotalRowNum + iRow + iHeaderRow, 0] = "";
                                    value2_copy[iTotalRowNum + iRow + iHeaderRow + 1, 0] = "";

                                    iTotalRowNum += iHeaderRow + iRowNum + 2;

                                }
                                //Data拷贝到指定区域
                                selected = targetSheet.Range(targetSheet.Cells(sartRow, startCol),
                                    targetSheet.Cells(value2_copy.GetLength(0) + sartRow - 1, value2_copy.GetLength(1) + startCol - 1));
                                selected.NumberFormat = "@";
                                selected.Value2 = value2_copy;
                                //XlApp.DrawBorder(selected);
                                sartRow = value2_copy.GetLength(0) + sartRow + 2;

                                //clear data for GC
                                cacheResults.Clear();

                                //
                                if (!(tableProcessCount == selectTableNameList.Count()))
                                {
                                    cacheResults.Add(block_header);
                                    cacheResults.Add(block_data);
                                }
                            }
                            else
                            {
                                cacheResults.Add(block_header);
                                cacheResults.Add(block_data);
                            }

                        }
                    });

                }
                targetSheet.Columns("B:AZ").EntireColumn.AutoFit();
                if (!goodFormatMode) {
                    DrawPlainFormatConditionFormat(targetSheet.Range("$B:$C"));
                }
                excel.ScreenUpdating = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Print(ex.Message);
                DevelopWorkspace.Base.Logger.WriteLine(ex.Message, Base.Level.ERROR);
            }

            finally
            {
                if (excel != null) {
                    //todo
                    (System.Windows.Application.Current.MainWindow as DevelopWorkspace.Main.MainWindow).InstallExcelWatch();
                    excel.ScreenUpdating = true;
                    Marshal.ReleaseComObject(excel);
                 }
            }
        }
        /// <summary>
        /// 这个的写法参照VBA的关联部分，可以在VBA开发环境中的寻找各种定义
        /// </summary>
        /// <param name="selected"></param>
        static void DrawBorder(Range selected)
        {
            XlBordersIndex[] borderIndexes = new XlBordersIndex[] {
            XlBordersIndex.xlEdgeLeft,
            XlBordersIndex.xlEdgeTop,
            XlBordersIndex.xlEdgeBottom,
            XlBordersIndex.xlEdgeRight,
            XlBordersIndex.xlInsideVertical,
            XlBordersIndex.xlInsideHorizontal
            };
            foreach (XlBordersIndex idx in borderIndexes)
            {
                selected.Borders[idx].LineStyle = XlLineStyle.xlContinuous;
                selected.Borders[idx].ColorIndex = 0;
                selected.Borders[idx].TintAndShade = 0;
                selected.Borders[idx].Weight = XlBorderWeight.xlThin;
            }
        }
        static void DrawPlainFormatConditionFormat(Range rangeFormat) {

            var fcs = rangeFormat.FormatConditions;
            string whitespace = '"' + "" + '"';
            string keytoken = '"' + "*" + '"';
            var fc = (Microsoft.Office.Interop.Excel.FormatCondition)fcs.Add(
                Type: Microsoft.Office.Interop.Excel.XlFormatConditionType.xlExpression,
                Formula1: $"=AND($A1={whitespace},$B1<>{whitespace},$C1<>{whitespace},$D1={whitespace},$E1={whitespace},OR($B2={keytoken},$B2={whitespace}))"
            );
            var interior = fc.Interior;
            interior.Color = System.Drawing.ColorTranslator.ToOle(Color.Cyan);
            fc.StopIfTrue = false;
        }
        static void DrawDiffResultConditionFormat(Range rangeFormat,int startRow)
        {
            var fcs = rangeFormat.FormatConditions;
            //	var fc = fcs.Add
            //	    (Excel.XlFormatConditionType.xlExpression, Type.Missing, "=IF($F$1) >= 10", Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);

            var fc = (Microsoft.Office.Interop.Excel.FormatCondition)fcs.Add(
                Type: Microsoft.Office.Interop.Excel.XlFormatConditionType.xlExpression,
                Formula1: $"=AND($A{startRow}=" + '"' + "Modified" + '"' + $",MOD(COUNTIF($A${startRow}:$A{startRow}," + '"' + "Modified" + '"' + $"),2)=1,B{startRow}<>B{startRow+1})"
            );
            var interior = fc.Interior;
            fc.Font.Color = System.Drawing.ColorTranslator.ToOle(Color.Red);
            fc.StopIfTrue = false;

            ////
            fc = (Microsoft.Office.Interop.Excel.FormatCondition)fcs.Add(
                Type: Microsoft.Office.Interop.Excel.XlFormatConditionType.xlExpression,
                Formula1: $"=$A{startRow}=" + '"' + "Deleted" + '"'
            );

            interior = fc.Interior;
            interior.Color = System.Drawing.ColorTranslator.ToOle(Color.Gray);
            fc.StopIfTrue = false;
            ///////
            fc = (Microsoft.Office.Interop.Excel.FormatCondition)fcs.Add(
                Type: Microsoft.Office.Interop.Excel.XlFormatConditionType.xlExpression,
                Formula1: $"=$A{startRow}=" + '"' + "Added" + '"'
            );

            interior = fc.Interior;
            interior.Color = System.Drawing.ColorTranslator.ToOle(Color.Red);
            fc.StopIfTrue = false;
            //////
            fc = (Microsoft.Office.Interop.Excel.FormatCondition)fcs.Add(
                Type: Microsoft.Office.Interop.Excel.XlFormatConditionType.xlExpression,
                Formula1: $"=$A{startRow}=" + '"' + "Modified" + '"'
            );

            interior = fc.Interior;
            interior.Color = System.Drawing.ColorTranslator.ToOle(Color.Azure);
            fc.StopIfTrue = false;
            /////


            interior = null;
            fc = null;
            fcs = null;
        }

        /// <summary>
        /// 为了提高性能,异步读取各个表的信息
        /// 这块还处于实验状态
        /// 和数据库的连接池有关如果大连发行后台接续程序会照成大连链接等待导致超时异常
        /// </summary>
        object obj = new object();

        ConcurrentDictionary<string, string[,]> cache = new ConcurrentDictionary<string, string[,]>();
        void GeTableDataWithSchemaForSelectedTables(TableInfo[] selectTableNameList, DbCommand cmd, DataSet orgDataset = null)
        {
            ///异步发行读取作业
            cache = new ConcurrentDictionary<string, string[,]>();
            for (int idx = 0; idx < selectTableNameList.Count(); idx++)
            {
                TableInfo tableInfo = selectTableNameList[idx];
                Task task = new Task(GetTableDataWithSchemaBackground, new List<object>() { tableInfo, cmd, cache, orgDataset });
                task.Start();
            }
        }
        void GetTableDataWithSchemaBackground(object param)
        {
            var objectList = param as List<object>;
            TableInfo ti = objectList[0] as TableInfo;
            DbCommand cmd = objectList[1] as DbCommand;
            ConcurrentDictionary<string, string[,]> _cache = objectList[2] as ConcurrentDictionary<string, string[,]>;
            DataSet dataset = objectList[3] as DataSet;

            System.Reflection.ConstructorInfo ctorViewModel = cmd.Connection.GetType().GetConstructor(Type.EmptyTypes);
            System.Data.Common.DbConnection nestedConn = ctorViewModel.Invoke(new Object[] { }) as System.Data.Common.DbConnection;
            nestedConn.ConnectionString = cmd.Connection.ConnectionString;
            try
            {
                nestedConn.Open();
                DbCommand dbCommand = nestedConn.CreateCommand();
                string[,] data = null;
                try
                {
                    data = GetTableDataWithSchema(ti, dbCommand, dataset);
                }
                catch (Exception ex)
                {
                    ///由于是后台线程的调用，需要对OutputToolView的处理方式修正，否则UI例外发生
                    DevelopWorkspace.Base.Logger.WriteLine($"Exception occurred in GetTableDataWithSchemaBackground:{ex.Message}", Base.Level.ERROR);
                }
                //如果data为null，说明这个表的数据取得时出现异常，需要在调用端做处理比如是跳过整体处理还是只跳过异常表
                _cache.TryAdd((objectList[0] as TableInfo).TableName, data);

                Monitor.Enter(obj);
                ///调用端可能等待,唤醒它
                Monitor.Pulse(obj);
                Monitor.Exit(obj);
            }
            finally
            {
                nestedConn.Close();
            }
        }
        /// <summary>
        /// 指定表单的Schema情报以及数据取得
        /// 调用端需要判断取得值是否为NULL
        /// 在使用结束后对结果进行NULL以尽快清除它的引用防止数据在cache里堆积过大
        /// </summary>
        /// <param name="tableInfo"></param>
        /// <param name="cmd"></param>
        /// <param name="orgDataset"></param>
        /// <returns></returns>
        string[,] GetTableDataWithSchemaFromCache(TableInfo tableInfo, DbCommand cmd, DataSet orgDataset = null)
        {
            string[,] cacheResult;
            try
            {
                Monitor.Enter(obj);
                while (true)
                {
                    bool success = cache.TryGetValue(tableInfo.TableName, out cacheResult);
                    if (success)
                    {
                        return cacheResult;
                    }
                    else
                    {
                        Monitor.Wait(obj);
                    }
                }

            }
            finally
            {
                Monitor.Exit(obj);
            }
        }

        /// <summary>
        /// 指定表单的Schema情报以及数据取得
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        string[,] GetTableDataWithSchema(TableInfo tableInfo, DbCommand cmd, DataSet orgDataset = null)
        {
            List<List<string>> linked = new List<List<string>>();
            string[,] ret = null;
            cmd.CommandText = tableInfo.SelectDataSQL;
            DevelopWorkspace.Base.Logger.WriteLine(cmd.CommandText, Base.Level.DEBUG);

            foreach (List<string> schemaPrint in tableInfo.ExportSchemaRegion)
            {
                linked.Add(schemaPrint);
            }

            //2016/06/03 辅助数据做成
            if (tableInfo.WhereCondition != null)
            {
                List<string> rowData = new List<string>();
                List<string> columnNameList = (from column in tableInfo.Columns select column.ColumnName).ToList();
                foreach (string columnName in columnNameList)
                {
                    string dummyValue = "";
                    foreach (SqlParserWrapper.WhereCondition condition in tableInfo.WhereCondition)
                    {
                        if (condition.leftTableName.ToLower().Equals(tableInfo.TableName.ToLower()) && condition.LeftFieldName.ToLower().Equals(columnName.ToLower()))
                        {
                            if (condition.rightTableName == null)
                            {
                                if (condition.op.Equals("="))
                                {
                                    dummyValue = condition.value;
                                }
                                else
                                {
                                    dummyValue = condition.op + " " + condition.value;
                                }
                            }
                            else
                            {
                                dummyValue = condition.rightTableName + "." + condition.rightFieldName;
                            }
                        }
                        if (condition.rightTableName != null)
                        {
                            if (condition.rightTableName.ToLower().Equals(tableInfo.TableName.ToLower()) && condition.rightFieldName.ToLower().Equals(columnName.ToLower()))
                            {
                                dummyValue = condition.leftTableName + "." + condition.LeftFieldName;
                            }
                        }
                    }
                    rowData.Add(dummyValue);
                }
                linked.Add(rowData);
            }
            using (DbDataReader rdr = cmd.ExecuteReader())
            {
                List<string> rowData = null;
                List<string> columnNameList = (from column in tableInfo.Columns where column.IsIncluded select column.ColumnName).ToList();
                //deleted with refactor:2016/02/06
                //List<string> dataTypeList = (from column in tableInfo.Columns select column.ColumnType).ToList();
                while (rdr.Read())
                {
                    rowData = new List<string>();
                    foreach (string columnName in columnNameList)
                    {
                        //deleted with refactor:2016/02/06
                        //由于要适应各种DB处理，这里不能有这样的硬编码，类似的处理搬到取得SQL上做文章
                        //也就是说尽量让所有类型都转换成String类型后处理到excel上
                        //if (dataTypeList[columnNameList.IndexOf(columnName)] == "System.DateTime")
                        //{
                        //    //Todo:Date型时要统一格式否则会出现例外,下面的只是针对SQLite的一个对策
                        //    rowData.Add(string.Format("{0:yyyy-MM-dd HH:mm:ss}", rdr[columnName]));
                        //}
                        //else {
                        //    rowData.Add(rdr[columnName].ToString());
                        //}
                        rowData.Add(rdr[columnName].ToString());
                    }
                    #region DIFF数据做成
                    if (orgDataset != null)
                    {
                        if (orgDataset.Tables[tableInfo.TableName].PrimaryKey.Count() == 0)
                        {
                            //从ActiveSheet取得的dataset时排除掉了没有主键的表的数据,那么在这里需要同样的主力
                        }
                        else
                        {
                            string whereClause = "";
                            foreach (DataColumn keyColumn in orgDataset.Tables[tableInfo.TableName].PrimaryKey)
                            {
                                whereClause += string.Format(whereClause == "" ? "{0} = '{1}'" : "AND {0} = '{1}'", keyColumn.ColumnName, rdr[keyColumn.ColumnName]);
                            }
                            //Added row
                            DataRow[] selectedRows = orgDataset.Tables[tableInfo.TableName].Select(whereClause);
                            if (selectedRows.GetLength(0) == 0)
                            {
                                //新规2019/03/05
                                List<object> newRow = new List<object>();
                                rowData.ForEach((itemValue) => {
                                    if (string.IsNullOrEmpty(itemValue))
                                    {
                                        newRow.Add(DBNull.Value);
                                    }
                                    else
                                    {
                                        newRow.Add(itemValue);
                                    }
                                });
                                orgDataset.Tables[tableInfo.TableName].Rows.Add(newRow.ToArray());

                            }
                            //Modified row
                            else
                            {
                                foreach (var valueIdx in rowData.Select((value, idx) => new { value, idx }))
                                {
                                    //if (selectedRows[0][valueIdx.idx].ToString() != valueIdx.value)
                                    if (string.IsNullOrEmpty(valueIdx.value))
                                    {
                                        //DatabaseConfig.isStringLikeColumn(orgDataset.Tables[tableInfo.TableName].Columns[valueIdx.idx].DataType.FullName)
                                        selectedRows[0][valueIdx.idx] = DBNull.Value;
                                    }
                                    else
                                    {
                                        selectedRows[0][valueIdx.idx] = valueIdx.value;
                                    }
                                }
                            }
                        }
                    }
                    #endregion
                    linked.Add(rowData);
                }
                #region DIFF数据做成
                if (orgDataset != null)
                {
                    foreach (DataRow row in orgDataset.Tables[tableInfo.TableName].Rows)
                    {
                        if (row.RowState == DataRowState.Unchanged)
                        {
                            row.Delete();
                        }
                        else if (row.RowState == DataRowState.Modified)
                        {
                            bool bModified = false;
                            foreach (string columnName in columnNameList)
                            {
                                if (row[columnName, DataRowVersion.Original].ToString() != row[columnName, DataRowVersion.Current].ToString())
                                {
                                    bModified = true;
                                }
                            }
                            if (bModified == false)
                            {
                                row.AcceptChanges();
                            }
                        }
                    }
                }
                #endregion


                //List内容转换成二维数组
                ret = new string[linked.Count, columnNameList.Count];
                for (int row = 0; row < linked.Count; row++)
                {
                    for (int col = 0; col < columnNameList.Count; col++)
                    {
                        ret[row, col] = linked[row][col];
                    }
                }
                   
            }

            return ret;
        }

        //把数据输出到excel的当前sheet中
        public static void loadDataIntoActiveSheet(string header, List<List<string>> schemaList, List<List<string>> rowdataList) {
            List<List<string>> headerList = new List<List<string>> { new List<string> { header } };
            loadDataIntoActiveSheet(headerList, schemaList,rowdataList);
        }
        public static void loadDataIntoActiveSheet(string header,List<List<string>> rowdataList)
        {
            List<List<string>> headerList = new List<List<string>> { new List<string> { header } };
            loadDataIntoActiveSheet(headerList, null, rowdataList);
        }
        public static void loadDataIntoActiveSheet(List<List<string>> rowdataList)
        {
            List<List<string>> headerList = new List<List<string>> { new List<string>() };
            loadDataIntoActiveSheet(headerList, null, rowdataList);
        }
        public static void loadDataIntoActiveSheet(int headerHeight, int schemaHeight, List<List<List<string>>> allTables) {
            List<List<string>> headerList = allTables[0].GetRange(0, headerHeight);
            List<List<string>> schemaList = allTables[0].GetRange(headerHeight, schemaHeight);
            List<List<string>> rowdataList = allTables[0].GetRange(headerHeight + schemaHeight, allTables[0].Count - headerHeight - schemaHeight);
            allTables.RemoveAt(0);
            loadDataIntoActiveSheet(headerList, schemaList, rowdataList, allTables.ToArray());
        }
        public static void loadDataIntoActiveSheet(List<List<string>> headerList, List<List<string>> schemaList,List<List<string>> rowdataList, params List<List<string>>[] otherTables)
        {
            dynamic excel = null;
            try
            {
                //2019/02/27
                excel = Excel.GetLatestActiveExcelRef(true);
                if (excel == null)
                {
                    DevelopWorkspace.Base.Services.ErrorMessage("Excelをただしく起動できないので、PC環境をご確認の上、再度実行してください");
                    return;
                }
                excel.Visible = true;
                var targetSheet = excel.ActiveWorkbook.ActiveSheet;
                if (targetSheet.UsedRange.Rows.Count > 1)
                {
                    //(System.Windows.Application.Current.MainWindow as DevelopWorkspace.Main.MainWindow).UnInstallExcelWatch();
                    excel.ActiveWorkbook.Worksheets.Add(System.Reflection.Missing.Value, excel.ActiveWorkbook.Worksheets[excel.ActiveWorkbook.Worksheets.Count], System.Reflection.Missing.Value, System.Reflection.Missing.Value);
                    targetSheet = excel.ActiveWorkbook.ActiveSheet;
                }
                excel.ScreenUpdating = false;

                int startRow = 1;
                int startCol = 1;
                Range selected;
                int headerHeight = 0;
                int schemaHeight = 0;
                int rowdataHeight = 0;

                for (int i = -1; i < otherTables.Length; i++)
                {
                    if (i == -1)
                    {
                        //Table属性定义行区域颜色定制
                        if (headerList != null && headerList.Count() > 0 && headerList[0].Count() > 0)
                        {
                            for (int idx = headerList[0].Count-1; idx > 0; idx--) {
                                if (string.IsNullOrWhiteSpace(headerList[0][idx]))
                                {
                                    foreach (var headerlist in headerList) headerlist.RemoveAt(idx);
                                }
                                else
                                {
                                    break;
                                }
                            }
                            headerHeight = headerList.Count();
                            selected = targetSheet.Range(targetSheet.Cells(startRow, startCol),
                                targetSheet.Cells(startRow + headerList.Count() - 1, startCol + headerList[0].Count() - 1));
                            selected.Interior.Pattern = Microsoft.Office.Interop.Excel.Constants.xlSolid;
                            selected.Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.FromArgb(180, 212, 180, 180));
                            selected.Value2 = DevelopWorkspace.Base.Utils.DataConvert.To2dArray<string>(headerList);
                            XlApp.DrawBorder(selected);
                            startRow += headerList.Count();
                        }
                        //TODO 2019/3/4
                        if (schemaList != null && schemaList.Count() > 0 && schemaList[0].Count() > 0)
                        {
                            schemaHeight = schemaList.Count();
                            //Table属性定义行区域颜色定制
                            selected = targetSheet.Range(targetSheet.Cells(startRow, startCol),
                            targetSheet.Cells(startRow + schemaList.Count() - 1, startCol + schemaList[0].Count() - 1));
                            selected.Interior.Pattern = Microsoft.Office.Interop.Excel.Constants.xlSolid;
                            selected.Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.FromArgb(180, 180, 180, 212));
                            selected.Value2 = DevelopWorkspace.Base.Utils.DataConvert.To2dArray<string>(schemaList);
                            XlApp.DrawBorder(selected);
                            startRow += schemaList.Count();
                        }
                        if (rowdataList != null && rowdataList.Count() > 0 && rowdataList[0].Count() > 0)
                        {
                            rowdataHeight = rowdataList.Count();
                            //Table属性定义行区域颜色定制
                            selected = targetSheet.Range(targetSheet.Cells(startRow, startCol),
                            targetSheet.Cells(startRow + rowdataList.Count() - 1, startCol + rowdataList[0].Count() - 1));
                            selected.NumberFormat = "@";
                            selected.Value2 = DevelopWorkspace.Base.Utils.DataConvert.To2dArray<string>(rowdataList);
                            XlApp.DrawBorder(selected);
                            selected.EntireColumn.AutoFit();
                            startRow += rowdataList.Count();

                        }
                    }
                    else
                    {
                        if (otherTables[i].Count() < headerHeight + schemaHeight) continue;
                        int offset;
                        List<List<string>> tempHeaderList = new List<List<string>>();
                        List<List<string>> tempSchemaList = new List<List<string>>();
                        List<List<string>> tempRowdataList = new List<List<string>>();
                        for (offset = 0; offset < headerHeight; offset++)
                        {
                            tempHeaderList.Add(otherTables[i][offset]);
                        }
                        for (offset = headerHeight; offset < headerHeight + schemaHeight; offset++)
                        {
                            tempSchemaList.Add(otherTables[i][offset]);
                        }
                        for (offset = headerHeight + schemaHeight; offset < otherTables[i].Count(); offset++)
                        {
                            tempRowdataList.Add(otherTables[i][offset]);
                        }
                        //Table属性定义行区域颜色定制
                        if (tempHeaderList != null && tempHeaderList.Count() > 0 && tempHeaderList[0].Count() > 0)
                        {
                            selected = targetSheet.Range(targetSheet.Cells(startRow, startCol),
                                targetSheet.Cells(startRow + tempHeaderList.Count() - 1, startCol + tempHeaderList[0].Count() - 1));
                            selected.Interior.Pattern = Microsoft.Office.Interop.Excel.Constants.xlSolid;
                            selected.Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.FromArgb(180, 212, 180, 180));
                            selected.Value2 = DevelopWorkspace.Base.Utils.DataConvert.To2dArray<string>(tempHeaderList);
                            XlApp.DrawBorder(selected);
                            startRow += tempHeaderList.Count();
                        }
                        //TODO 2019/3/4
                        if (tempSchemaList != null && tempSchemaList.Count() > 0 && tempSchemaList[0].Count() > 0)
                        {
                            //Table属性定义行区域颜色定制
                            selected = targetSheet.Range(targetSheet.Cells(startRow, startCol),
                            targetSheet.Cells(startRow + tempSchemaList.Count() - 1, startCol + tempSchemaList[0].Count() - 1));
                            selected.Interior.Pattern = Microsoft.Office.Interop.Excel.Constants.xlSolid;
                            selected.Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.FromArgb(180, 180, 180, 212));
                            selected.Value2 = DevelopWorkspace.Base.Utils.DataConvert.To2dArray<string>(tempSchemaList);
                            XlApp.DrawBorder(selected);
                            startRow += tempSchemaList.Count();
                        }
                        if (tempRowdataList != null && tempRowdataList.Count() > 0 && tempRowdataList[0].Count() > 0)
                        {
                            //Table属性定义行区域颜色定制
                            selected = targetSheet.Range(targetSheet.Cells(startRow, startCol),
                            targetSheet.Cells(startRow + tempRowdataList.Count() - 1, startCol + tempRowdataList[0].Count() - 1));
                            selected.NumberFormat = "@";
                            selected.Value2 = DevelopWorkspace.Base.Utils.DataConvert.To2dArray<string>(tempRowdataList);
                            XlApp.DrawBorder(selected);
                            selected.EntireColumn.AutoFit();
                            startRow += tempRowdataList.Count();
                        }
                    }
                    startRow += 2;
                }
               
                excel.ScreenUpdating = true;
            }
            catch (Exception ex)
            {
                DevelopWorkspace.Base.Logger.WriteLine(ex.Message, Base.Level.ERROR);
            }
            finally
            {
                if (excel != null)
                {
                    //todo
                    //(System.Windows.Application.Current.MainWindow as DevelopWorkspace.Main.MainWindow).InstallExcelWatch();
                    excel.ScreenUpdating = true;
                    Marshal.ReleaseComObject(excel);
                }
            }
        }
        public static List<List<string>> getDataFromActiveSheet()
        {
            dynamic excel = null;
            List<List<string>> rowDataList = new List<List<string>>();
            try
            {
                //2019/02/27
                excel = Excel.GetLatestActiveExcelRef();
                if (excel == null)
                {
                    DevelopWorkspace.Base.Services.ErrorMessage("対象のExcelのワークシートを選択して、再度実行してください");
                    return rowDataList; 
                }
                var targetSheet = excel.ActiveWorkbook.ActiveSheet;
                if (targetSheet.UsedRange.Rows.Count < 2)
                {
                    DevelopWorkspace.Base.Services.ErrorMessage("対象のExcelのワークシートを選択して、再度実行してください");
                    return rowDataList;
                }
                object[,] value2_copy = targetSheet.Range(targetSheet.Cells(1, 1),
                                            targetSheet.Cells(targetSheet.UsedRange.Rows.Count + 1,
                                            targetSheet.UsedRange.Columns.Count + 1)).Value2;
                for (int iRow = 1; iRow < value2_copy.GetLength(0); iRow++)
                {
                    List<string> rowData = new List<string>();
                    for (int iCol = 1; iCol < value2_copy.GetLength(1); iCol++)
                    {
                        if (value2_copy[iRow, iCol] == null)
                        {
                            rowData.Add("");
                        }
                        else {
                            rowData.Add(value2_copy[iRow, iCol].ToString());
                        }

                    }
                    rowDataList.Add(rowData);
                }
                return rowDataList;
            }
            catch (Exception ex)
            {
                DevelopWorkspace.Base.Logger.WriteLine(ex.Message, Base.Level.ERROR);
                return rowDataList;
            }
            finally
            {
                if (excel != null)
                {
                    excel.ScreenUpdating = true;
                    Marshal.ReleaseComObject(excel);
                }
            }
        }

        public static void bringSpecialSheetToTop(string excelFilePath,string sheetName)
        {
            Microsoft.Office.Interop.Excel.Application excel = null;
            try
            {
                excel = Excel.GetLatestActiveExcelRef(true);
                foreach (Microsoft.Office.Interop.Excel.Workbook activeWorkbook in excel.Workbooks)
                {
                    if (excelFilePath.EndsWith(activeWorkbook.Name))
                    {
                        foreach (dynamic sheet in activeWorkbook.Sheets)
                        {
                            if (sheetName.Equals(sheet.Name))
                            {
                                sheet.Activate();
                                return;
                            }
                        }
                    }
                }
                Microsoft.Office.Interop.Excel.Workbook targetWorkbook = excel.Workbooks.Open(excelFilePath, false);
                foreach (dynamic sheet in targetWorkbook.Sheets)
                {
                    if (sheetName.Equals(sheet.Name))
                    {
                        sheet.Activate();
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                DevelopWorkspace.Base.Logger.WriteLine(ex.Message, Base.Level.ERROR);
            }
            finally
            {
                if (excel != null)
                {
                    excel.ScreenUpdating = true;
                    Marshal.ReleaseComObject(excel);
                }
            }
        }

    }
}