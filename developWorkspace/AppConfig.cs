using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace DevelopWorkspace.Main
{
    public enum Language { csharp, java,javascript,xml }
    public enum EngineDomain { required, single, shared }

    public class AppConfig
    {
        public class JsonArgb{
            public int red { get; set; }
            public int green { get; set; }
            public int blue { get; set; }
        }
        public class ConfigBase: INotifyPropertyChanged
        {
            [ReadOnly(true)]
            public string typeName { get; set; }
            [ReadOnly(true)]
            public string jsonfile { get; set; }

            public event PropertyChangedEventHandler PropertyChanged;
            protected virtual void RaisePropertyChanged(string propertyName)
            {
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        public class JsonConfig<T> where T : ConfigBase, new()
        {
            public static T load()
            {
                string jsonfile = System.IO.Path.Combine(StartupSetting.instance.homeDir, $"{ typeof(T).Name}.json");
                T t = internalLoad(jsonfile);
                t.jsonfile = jsonfile;
                return t;
            }
            public static T load(string path)
            {
                string jsonfile = System.IO.Path.Combine(path, $"{ typeof(T).Name}.json");
                T t = internalLoad(jsonfile);
                t.jsonfile = jsonfile;
                return t;
            }
            static T internalLoad(string filepath)
            {
                T t;
                if (System.IO.File.Exists(filepath))
                {
                    string json = File.ReadAllText(filepath, Encoding.UTF8);
                    t = (T)JsonConvert.DeserializeObject(json, typeof(T));
                    t.jsonfile = filepath;
                    if (t.typeName == null) t.typeName = typeof(T).Name;
                    if (!typeof(T).Name.Equals(t.typeName))
                    {
                        DevelopWorkspace.Base.Logger.WriteLine($"can't load json:{filepath} correctly with typename incorresponding");
                        throw new Exception($"can't load json:{filepath} correctly with typename incorresponding");
                    }
                }
                else
                {
                    if (filepath.Equals(System.IO.Path.Combine(StartupSetting.instance.homeDir, $"{ typeof(T).Name}.json")))
                    {
                        //t = new T();
                        //JsonConvert.DeserializeObject的动作式样是先实例化T，之后对它的属性进行覆盖，如果属性是集合，则进行追加
                        //这样默认的构造体里就不能进行赋值行为，另建一个带参构造体通过它进行实例化
                        t = (T)Activator.CreateInstance(typeof(T), 0);
                    }
                    else
                    {
                        t = internalLoad((System.IO.Path.Combine(StartupSetting.instance.homeDir, $"{ typeof(T).Name}.json")));
                    }

                }
                return t;
            }
            public static void flush(T userSetting)
            {
                //在反序列化时会激活属性set方法，这个时候jsonfile可能还么有来得及设定
                if (!string.IsNullOrEmpty(userSetting.jsonfile))
                {
                    try
                    {
                        string json = JsonConvert.SerializeObject(userSetting, Newtonsoft.Json.Formatting.Indented);
                        File.WriteAllText(userSetting.jsonfile, json, Encoding.UTF8);
                    }
                    catch (Exception ex) {
                        DevelopWorkspace.Base.Logger.WriteLine(ex.Message);
                    }
                }
            }
        }

        public class ScriptConfig : ConfigBase
        {
            [Category(@"z...PreloadAssembly")]
            [ExpandableObject()]
            public SettingsConfig Settings { get; set; }
            [Category(@"a..Script")]
            [Editor(typeof(Xceed.Wpf.Toolkit.PropertyGrid.Editors.EnumComboBoxEditor), typeof(Xceed.Wpf.Toolkit.PropertyGrid.Editors.EnumComboBoxEditor))]
            public EngineDomain AppDomain { get; set; }
            [Category(@"a..Script")]
            [Editor(typeof(Xceed.Wpf.Toolkit.PropertyGrid.Editors.EnumComboBoxEditor), typeof(Xceed.Wpf.Toolkit.PropertyGrid.Editors.EnumComboBoxEditor))]
            public Language ScriptLanguage { get; set; }
            [Category(@"a..Script")]
            [Editor(typeof(Xceed.Wpf.Toolkit.PropertyGrid.Editors.EnumComboBoxEditor), typeof(Xceed.Wpf.Toolkit.PropertyGrid.Editors.EnumComboBoxEditor))]
            public Language FirstInputType { get; set; }
            [Category(@"a..Script")]
            [Editor(typeof(Xceed.Wpf.Toolkit.PropertyGrid.Editors.EnumComboBoxEditor), typeof(Xceed.Wpf.Toolkit.PropertyGrid.Editors.EnumComboBoxEditor))]
            public Language SecondInputType { get; set; }
            [Category(@"a..Script")]
            public string Title { get; set; }
            [Category(@"a..Script")]
            public string Description { get; set; }
            [Category(@"a..Script")]
            public string Path { get; set; }

            public ScriptConfig()
            {
            }
            public ScriptConfig(int create)
            {
                Title = "noname";
                Description = "noname";
                AppDomain = EngineDomain.shared;
                ScriptLanguage = Language.csharp;
                FirstInputType = Language.xml;
                SecondInputType = Language.xml;
                Settings = new SettingsConfig
                {
                    Url = "",
                    ApiKey = "",
                    RefAssemblies = new List<string>() { "WindowsBase",
                                                                                "PresentationCore",
                                                                                "PresentationFramework",
                                                                                "System.Xaml",
                                                                                "PresentationFramework" }
                };

            }
            public class ConnectionStringsConfig
            {
                public string MyDb { get; set; }
            }

            public class SettingsConfig
            {
                public string Url { get; set; }
                public string ApiKey { get; set; }
                public bool UseCache { get; set; }
                public List<string> RefAssemblies { get; set; }
            }
        }

        public class DatabaseConfig : ConfigBase
        {
            public static DatabaseConfig This;
            public class selectClause
            {
                public string provider { get; set; }
                public List<string> clauses { get; set; }
            }
            [Category(@"database aquire...")]
            [ReadOnly(true)]
            public bool backgroundWorkerMode { get; set; }
            [Category(@"database aquire...")]
            public bool withRemark { get; set; }
            [Category(@"database aquire...")]
            public bool plainFormatMode { get; set; }
            [Category(@"database aquire...")]
            public int plainFormatRoundupSize { get; set; }
            [Category(@"database aquire...")]
            public int maxRecordCount { get; set; }
            [Category(@"database aquire...")]
            public bool doRealCount { get; set; }
            [Category(@"database apply...")]
            public bool sqlOnly { get; set; }
            [Category(@"database apply...")]
            public int sqlRoundupSize { get; set; }
            [Category(@"database apply...")]
            public bool batchUpdate { get; set; }

            [Category(@"database apply...")]
            public List<string> singleConditionDatatypeList { get; set; }
            [Category(@"database diff...")]
            public bool exceptZeroRowDataTable { get; set; }
            [Category(@"database diff...")]
            public bool exceptZeroDiffRowTable { get; set; }
            [Category(@"database diff...")]
            public bool exceptUnchangedRow { get; set; }
            [Category(@"database diff...")]
            public bool applyFormatConditionForDiffResult { get; set; }
            [ExpandableObject()]
            public List<selectClause> favoriteSelectClause { get; set; }
            public DatabaseConfig()
            {
                This = this;
            }
            public DatabaseConfig(int create)
            {
                plainFormatMode = false;
                plainFormatRoundupSize = 10000;
                maxRecordCount = 10000;
                doRealCount = false;
                backgroundWorkerMode = false;
                sqlOnly = false;
                sqlRoundupSize = 100;
                batchUpdate = false;
                withRemark = true;
                singleConditionDatatypeList = new List<string>() {
                                    "System.String",//postgres
                                    "System.DateTime",//postgres
                                    "System.Byte[]",
                                    "nchar",
                                    "varchar",
                                    "nvarchar",
                                    "ntext",
                                    "datetime",
                                    "System.TimeSpan",
                                    "System.Boolean",
                                    "System.Object"
                };

                exceptZeroRowDataTable = false;
                exceptZeroDiffRowTable = false;
                exceptUnchangedRow = false;
                applyFormatConditionForDiffResult = false;
                favoriteSelectClause = new List<selectClause>
                {

                    new selectClause(){
                        provider = "SQLite",
                        clauses = new List<string>() { "1=1 LIMIT 100" }
                    },
                    new selectClause(){
                        provider = "Postgres",
                        clauses = new List<string>() { "1=1 LIMIT 100" }
                    },
                    new selectClause(){
                        provider = "MySQL",
                        clauses = new List<string>() { "1=1 LIMIT 100" }
                    },
                    new selectClause(){
                        provider = "Oracle",
                        clauses = new List<string>() { "rownum<=10" }
                    }
                };
                This = this;
            }
            public static bool isStringLikeColumn(string type) {
                return (from elem in This.singleConditionDatatypeList where elem == type select elem).FirstOrDefault<string>() == null?false:true;
            }
        }


        public class SysConfig : ConfigBase
        {
            public static SysConfig This;

            [Category(@"position...")]
            public double Top { get; set; }
            [Category(@"position...")]
            public double Left { get; set; }
            [Category(@"position...")]
            public double Height { get; set; }
            [Category(@"position...")]
            public double Width { get; set; }
            [Category(@"position...")]
            public Boolean Maximized { get; set; }

            private Base.Level _level = Base.Level.DEBUG;
            [Category(@"common...")]
            public Base.Level logLevel
            {
                get { return _level; }
                set
                {
                    if (_level != value)
                    {
                        _level = value;
                        RaisePropertyChanged("logLevel");
                    }
                }
            }
            [Category(@"common...")]
            public Boolean WatchExcelActivity { get; set; }
            [Category(@"common...")]
            public string Language { get; set; }

            public SysConfig()
            {
                This = this;
                PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(PropertyChangedHandler);

            }
            public SysConfig(int create)
            {
                logLevel = Base.Level.INFO;

                Top = 20;
                Left = 20;
                Height = 800;
                Width = 800;
                Maximized = false;
                WatchExcelActivity = false;

                PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(PropertyChangedHandler);
            }
            public void PropertyChangedHandler(object sender, System.ComponentModel.PropertyChangedEventArgs e)
            {
                if (e.PropertyName == "logLevel")
                {
                    Base.Logger.setLevel(this.logLevel);
                }
                JsonConfig<SysConfig>.flush(this);
            }
        }


    }
}
