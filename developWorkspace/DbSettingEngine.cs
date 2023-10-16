namespace DevelopWorkspace.Main
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Configuration;
    using System.Data;
    using System.Data.Common;
    using System.Data.Entity;
    using System.Data.Entity.Core.Common;
    using System.Data.SQLite;
    using System.Data.SQLite.EF6;
    using System.Linq;

    class SqliteDbConfiguration : DbConfiguration
    {
        //如何解决EF框架脱离app.config配置的约束.
        //https://qiita.com/minoru-nagasawa/items/961f6eae809a379c1b52
        public SqliteDbConfiguration()
        {
            string assemblyName = typeof(SQLiteProviderFactory).Assembly.GetName().Name;

            RegisterDbProviderFactories(assemblyName);
            SetProviderFactory("System.Data.SQLite", SQLiteFactory.Instance);
            SetProviderFactory("System.Data.SQLite.EF6", SQLiteProviderFactory.Instance);
            SetProviderServices("System.Data.SQLite", (System.Data.Entity.Core.Common.DbProviderServices)SQLiteProviderFactory.Instance.GetService(typeof(System.Data.Entity.Core.Common.DbProviderServices)));
        }

        static void RegisterDbProviderFactories(string assemblyName)
        {
            var dataSet = ConfigurationManager.GetSection("system.data") as DataSet;
            if (dataSet != null)
            {
                var dbProviderFactoriesDataTable = dataSet.Tables.OfType<DataTable>()
                    .First(x => x.TableName == typeof(DbProviderFactories).Name);

                var dataRow = dbProviderFactoriesDataTable.Rows.OfType<DataRow>()
                    .FirstOrDefault(x => x.ItemArray[2].ToString() == assemblyName);

                if (dataRow != null)
                    dbProviderFactoriesDataTable.Rows.Remove(dataRow);

                dbProviderFactoriesDataTable.Rows.Add(
                    "SQLite Data Provider (Entity Framework 6)",
                    ".NET Framework Data Provider for SQLite (Entity Framework 6)",
                    assemblyName,
                    typeof(SQLiteProviderFactory).AssemblyQualifiedName
                    );
            }
        }
    }
    public class DbSettingEngine : DbContext
    {
        static DbSettingEngine _engine = null;
        public static DbSettingEngine GetEngine(Boolean background=false) {
            //2017/03/30 取消下面的判断确保每次使用时都从DB中抽取最新的数据
            //或许对性能会产生一定的影响
            if(_engine == null)
            {
                try
                {
                    if (_engine == null) DbConfiguration.SetConfiguration(new SqliteDbConfiguration());
                }
                catch (System.Data.ConstraintException) { }

                StartupSetting startup = AppDomain.CurrentDomain.GetData("StartupSetting") as StartupSetting;
                string engineDbPath = System.IO.Path.Combine(startup.homeDir, "workspaceEngine.db");

                var connection = new SQLiteConnection($"Data Source={ engineDbPath }");
                DbSettingEngine engine = new DbSettingEngine(connection);
                if (background) return engine;
                _engine = engine;

            }
            return _engine;
        }
        private DbSettingEngine(DbConnection connection)
            : base(connection,true)
        {
        }

        public virtual DbSet<Provider> Providers { get; set; }
        public virtual DbSet<TableSchema> TableSchemas { get; set; }
        public virtual DbSet<ProjectKeyword> ProjectKeywords { get; set; }
        public virtual DbSet<DataTypeCondition> DataTypeConditiones { get; set; }
        public virtual DbSet<ConnectionHistory> ConnectionHistories { get; set; }
        public virtual DbSet<Snapshot> Snapshots { get; set; }
        public virtual DbSet<CustSelectSql> CustSelectSqls { get; set; }
        public virtual DbSet<ThirdPartyTool> ThirdPartyTools { get; set; }
        public virtual DbSet<Message> Messages { get; set; }

    }

    [Table("ConnectionHistories")]
    public class ConnectionHistory
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ConnectionHistory()
        {
            this.WhereClauseHistories = new HashSet<WhereClauseHistory>();

        }
        [Key, Column(Order = 0)]
        public int ConnectionHistoryID { get; set; }
        public int ProviderID { get; set; }
        public string ConnectionHistoryName { get; set; }
        public string ConnectionString { get; set; }
        public string Schema { get; set; }

        public string ExcelHeaderThemeColor { get; set; }
        public string ExcelSchemaThemeColor { get; set; }
        public string ExcelSupportDataThemeColor { get; set; }

        public virtual ICollection<WhereClauseHistory> WhereClauseHistories { get; set; }
        public virtual ICollection<CustSelectSql> CustSelectSqls { get; set; }

    }
    [Table("WhereClauseHistories")]
    public class WhereClauseHistory
    {
        [Key, Column(Order = 0)]
        public int WhereClauseID { get; set; }
        public int ConnectionHistoryID { get; set; }
        public string TableName { get; set; }
        public string WhereClauseString { get; set; }
        public string DeleteClauseString { get; set; }
        public int ViewOrderNum { get; set; }

    }

    [Table("Snapshots")]
    public class Snapshot
    {
        [Key, Column(Order = 0)]
        public int SnapshotID { get; set; }
        public int ConnectionHistoryID { get; set; }
        public string SnapshotName { get; set; }

        [Column("GroupName")] 
        public string Group { get; set; }
        public string TableName { get; set; }
        public string WhereClause { get; set; }
        public string DeleteClause { get; set; }

    }
    [Table("CustSelectSqls")]
    public class CustSelectSql
    {
        [Key, Column(Order = 0)]
        public int CustSelectSqlID { get; set; }
        public int ConnectionHistoryID { get; set; }
        public string CustSelectSqlName { get; set; }
        public string SqlStatementText { get; set; }

    }
    [Table("ProjectKeywords")]
    public class ProjectKeyword
    {
        [Key, Column(Order = 0)]
        public int ProjectKeywordID { get; set; }
        public string ProjectKeywordName { get; set; }
        public string ProjectKeywordRemark { get; set; }

    }

    [Table("TableSchemas")]
    public class TableSchema
    {
        [Key, Column(Order = 0)]
        public int TableSchemaID { get; set; }
        [Key, Column(Order = 1)]
        public int ProviderID { get; set; }
        public string SchemaName { get; set; }
    }
    [Table("DataTypeConditiones")]
    public class DataTypeCondition
    {
        [Key, Column(Order = 0)]
        public int DataTypeConditionID { get; set; }
        [Key, Column(Order = 1)]
        public int ProviderID { get; set; }
        public string DataTypeName { get; set; }
        public int ProcessKbn { get; set; }
        public string ExcelFormatString { get; set; }
        public string DatabaseFormatString { get; set; }
        public string UpdateFormatString { get; set; }


    }

    [Table("Providers")]
    public class Provider
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Provider()
        {
            this.TableSchemas = new HashSet<TableSchema>();
            this.DataTypeConditiones = new HashSet<DataTypeCondition>();

        }

        [Key, Column(Order = 0)]
        public int ProviderID { get; set; }
        public string ProviderName { get; set; }
        public string LoadPath { get; set; }
        public string LoadAssembly { get; set; }
        public string TypeDes { get; set; }
        public string SelectTableListSql { get; set; }
        public string SelectColumnListSql { get; set; }
        public string SelectColumnLogicalNameSql { get; set; }
        public string SelectTableLogicalNameSql { get; set; }
        public string DateTimeFormatter { get; set; }

        public string AppendColumnType { get; set; }
        public string AppendDual { get; set; }
        public string LimitCondition { get; set; }



        public virtual ICollection<TableSchema> TableSchemas { get; set; }
        public virtual ICollection<DataTypeCondition> DataTypeConditiones { get; set; }

    }


    [Table("ThirdPartyTools")]
    public class ThirdPartyTool
    {
        [Key, Column(Order = 0)]
        public int ThirdPartyID { get; set; }
        public string ExeFilePath { get; set; }
        public string StartInfo { get; set; }
        public string MenuTitle { get; set; }

    }

    [Table("Messages")]
    public class Message
    {
        [Key, Column(Order = 0)]
        public int Id { get; set; }
        public string MessageId { get; set; }
        public string Language { get; set; }
        public string Content { get; set; }
    }


}
