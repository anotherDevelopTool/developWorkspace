using System;
using Microsoft.CSharp;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using System.Text;
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
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Heidesoft.Components.Controls;
using System.Windows.Threading;
using System.Linq;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Diagnostics;
using System.Threading;
using Xceed.Wpf.AvalonDock.Layout;
using System.Windows.Media;
using DevelopWorkspace.Base;
using Excel = Microsoft.Office.Interop.Excel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection;
using System.Collections.Generic;
using DevelopWorkspace.Base.Model;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using DevelopWorkspace.Main;
using DevelopWorkspace.UX.enhance;
public class BindableAvalonEditor : ICSharpCode.AvalonEdit.TextEditor, INotifyPropertyChanged
{
    /// <summary>
    /// A bindable Text property
    /// </summary>
    public new string Text
    {
        get
        {
            return (string)GetValue(TextProperty);
        }
        set
        {
            SetValue(TextProperty, value);
            RaisePropertyChanged("Text");
        }
    }

    /// <summary>
    /// The bindable text property dependency property
    /// </summary>
    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(
            "Text",
            typeof(string),
            typeof(BindableAvalonEditor),
            new FrameworkPropertyMetadata
            {
                DefaultValue = default(string),
                BindsTwoWayByDefault = true,
                PropertyChangedCallback = OnDependencyPropertyChanged
            }
        );

    protected static void OnDependencyPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
        var target = (BindableAvalonEditor)obj;

        if (target.Document != null)
        {
            var caretOffset = target.CaretOffset;
            var newValue = args.NewValue;

            if (newValue == null)
            {
                newValue = "";
            }

            target.Document.Text = (string)newValue;
            target.CaretOffset = Math.Min(caretOffset, newValue.ToString().Length);
        }
    }

    protected override void OnTextChanged(EventArgs e)
    {
        if (this.Document != null)
        {
            Text = this.Document.Text;
        }

        base.OnTextChanged(e);
    }

    /// <summary>
    /// Raises a property changed event
    /// </summary>
    /// <param name="property">The name of the property that updates</param>
    public void RaisePropertyChanged(string property)
    {
        if (PropertyChanged != null)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(property));
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
}

public class ElasticInfo : INotifyPropertyChanged
{
    public virtual void RaisePropertyChanged(string propertyName)
    {
        if (PropertyChanged != null)
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
    }

    public event PropertyChangedEventHandler PropertyChanged;

    private Boolean _isNotKey = false;
    [SimpleListViewColumnMeta(Visiblity = false)]
    public Boolean IsNotKey
    {
        get { return _isNotKey; }
        set
        {
            if (_isNotKey != value)
            {
                _isNotKey = value;
                RaisePropertyChanged("IsNotKey");
            }
        }
    }

    private string _index = null;
    [SimpleListViewColumnMeta(ColumnDisplayWidth = 240.0)]
    public string index
    {
        get { return _index; }
        set
        {
            if (_index != value)
            {
                _index = value;
                RaisePropertyChanged("index");
            }
        }
    }
    private long _size = 0;
    public long size
    {
        get { return _size; }
        set
        {
            if (_size != value)
            {
                _size = value;
                RaisePropertyChanged("size");
            }
        }
    }
    [SimpleListViewColumnMeta(Visiblity = false)]
    public List<FieldInfo> FieldInfoList { get; set; }

    private string _query = null;
    [SimpleListViewColumnMeta(ColumnDisplayWidth = 480.0, Editablity = true)]
    public string Query
    {
        get { return _query; }
        set
        {
            if (_query != value)
            {
                _query = value;
                RaisePropertyChanged("query");
            }
        }
    }
}
class ElementWidhtConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        double totalWidth = double.Parse(value.ToString());
        return totalWidth - 15;
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        return value;
    }
}

public class FieldInfo
{
    public string FieldName { get; set; }
    public string FieldType { get; set; }
}

public class Script
{

    //https://stackoverflow.com/questions/248362/how-do-i-build-a-datatemplate-in-c-sharp-code
    //TODO 面向Addin基类化
    [AddinMeta(Name = "elasticSearch", Date = "2009-07-20", Description = "elastic utility", LargeIcon = "elasticsearch", Red = 128, Green = 145, Blue = 213)]
    public class ViewModel : DevelopWorkspace.Base.Model.ScriptBaseViewModel
    {
        DevelopWorkspace.Base.Utils.SimpleListView listView;
        List<ElasticInfo> elasticInfoList = new List<ElasticInfo>();

        [MethodMeta(Name = "Excelにエクスポート", Date = "2009-07-20", Description = "指定EXCEL内的数据反映到Elastic", LargeIcon = "export")]
        public void EventHandler3(object sender, RoutedEventArgs e)
        {
            dynamic content = listView.SelectedItem;
            string slectetedIndex = content.index;
            string query = content.Query;

            search(slectetedIndex, query);

        }
        [MethodMeta(Name = "Elasticへ更新", Date = "2009-07-20", Description = "导入指定index的所有document到EXCEL,@timestampe must input like 2017-02-03T19:27:20.606Z", LargeIcon = "import")]
        public async void EventHandler2(object sender, RoutedEventArgs e)
        {
            try
            {

                dynamic xlApp = DevelopWorkspace.Base.Excel.GetLatestActiveExcelRef();
                xlApp.Visible = true;


                var targetSheet = xlApp.ActiveWorkbook.ActiveSheet;
                object[,] value2_copy = targetSheet.Range(targetSheet.Cells(1, 1), targetSheet.Cells(targetSheet.UsedRange.Rows.Count, targetSheet.UsedRange.Columns.Count)).Value2;

                string slectetedIndex = value2_copy[1, 1].ToString();
                await getMapping(slectetedIndex);
                var elasticInfo = elasticInfoList.FirstOrDefault(current => current.index == slectetedIndex);
                if (elasticInfo == null)
                {
                    DevelopWorkspace.Base.Logger.WriteLine("Please Conform whether data of activesheet are well formatted", DevelopWorkspace.Base.Level.ERROR);
                    return;
                }
                List<string> headerList = new List<string>();
                //header
                for (int iidx = 1; iidx < value2_copy.GetLength(1) + 1; iidx++)
                {
                    headerList.Add(value2_copy[2, iidx].ToString());
                }

                string payload_json = "";
                string bulk_header_leftpart = @"{ ""index"" : { ""_index"" : """ + slectetedIndex + @""",""_id"": """;
                string bulk_header_rightpart = @"""} }";
                for (int iidx = 4; iidx < value2_copy.GetLength(0) + 1; iidx++)
                {
                    string rowIdString = "";
                    string rowString = "{";
                    for (int jjdx = 1; jjdx < value2_copy.GetLength(1) + 1; jjdx++)
                    {
                        string property_name = headerList[jjdx - 1];
                        if (property_name == "_id")
                        {
                            string idstring = value2_copy[iidx, jjdx] == null ? "" : value2_copy[iidx, jjdx].ToString();
                            DevelopWorkspace.Base.Logger.WriteLine(idstring, DevelopWorkspace.Base.Level.DEBUG);
                            rowIdString = bulk_header_leftpart + idstring + bulk_header_rightpart;
                        }
                        else
                        {

                            var fieldInfo = elasticInfo.FieldInfoList.FirstOrDefault(current => current.FieldName == property_name);
                            if (fieldInfo != null)
                            {
                                rowString += @"""" + fieldInfo.FieldName + @""":";
                                //fieldInfo.FieldType
                                if (value2_copy[iidx, jjdx] == null)
                                {
                                    rowString += @"""" + "null" + @"""";
                                }
                                else
                                {
                                    rowString += @"""" + value2_copy[iidx, jjdx].ToString() + @"""";
                                }
                                if (jjdx == value2_copy.GetLength(1))
                                {

                                }
                                else
                                {
                                    rowString += ",";
                                }

                            }
                        }
                    }
                    rowString += "}";
                    DevelopWorkspace.Base.Logger.WriteLine(rowIdString, DevelopWorkspace.Base.Level.DEBUG);
                    DevelopWorkspace.Base.Logger.WriteLine(rowString, DevelopWorkspace.Base.Level.DEBUG);
                    payload_json += rowIdString + "\n";
                    payload_json += rowString + "\n";
                }

                bulk(payload_json);

            }
            catch (Exception ex)
            {
                DevelopWorkspace.Base.Logger.WriteLine(ex.ToString());
            }

        }

        [MethodMeta(Name = "インデックスクリア", Date = "2009-07-20", Description = "删除指定index的所有document，index本身不删除", LargeIcon = "delete")]
        public void EventHandler4(object sender, RoutedEventArgs e)
        {
            dynamic content = listView.SelectedItem;
            string slectetedIndex = content.index;
            delete(slectetedIndex);
        }

        public Fluent.RibbonGroupBox  getHelpRibbonGroupBox()
        {
            return UX.getHelpRibbonGroupBox("elasticSearch","https://confluence.rakuten-it.com/confluence/pages/viewpage.action?pageId=2427411586");
        }
        public override UserControl getView(string strXaml)
        {
            StringReader strreader = new StringReader(strXaml);
            XmlTextReader xmlreader = new XmlTextReader(strreader);
            UserControl view = XamlReader.Load(xmlreader) as UserControl;
            listView = DevelopWorkspace.Base.Utils.WPF.FindLogicaChild<DevelopWorkspace.Base.Utils.SimpleListView>(view, "trvFamilies");

            getIndices();

            listView.setStyle(120, 120, 255, 120, 12);
            listView.CustomizeColumnDataDefFunc = (propertyAttribute, property, viewColumn, stackPanel) =>
            {
                if (property.PropertyType == typeof(Boolean))
                {
                    var checkBox = new CheckBox();
                    checkBox.FontSize = 12;
                    Binding textPropertyBinding = new Binding();
                    textPropertyBinding.Mode = BindingMode.TwoWay;
                    textPropertyBinding.Path = new PropertyPath(property.Name);
                    checkBox.SetBinding(CheckBox.IsCheckedProperty, textPropertyBinding);
                    checkBox.Checked += (object sender, RoutedEventArgs e) =>
                    {
                        //if (bMultiSelect) return;

                    };
                    stackPanel.Children.Add(checkBox);
                }
                else
                {
                    if (propertyAttribute == null || !propertyAttribute.Editablity)
                    {
                        var textBlock = new TextBlock();
                        textBlock.FontSize = 12;
                        textBlock.MinWidth = 120;
                        textBlock.SetBinding(TextBlock.TextProperty, new Binding()
                        {
                            Path = new PropertyPath(property.Name),
                        });
                        stackPanel.Children.Add(textBlock);
                    }
                    else
                    {
                        var textBox = new BindableAvalonEditor();
                        textBox.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                        textBox.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;


                        textBox.FontSize = 12;
                        textBox.MinWidth = 120;
                        textBox.SetBinding(BindableAvalonEditor.TextProperty, new Binding()
                        {
                            Path = new PropertyPath(property.Name),
                            Mode = BindingMode.TwoWay,
                            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                        });

                        textBox.SetBinding(TextBox.WidthProperty, new Binding()
                        {
                            Source = viewColumn,
                            Path = new PropertyPath("ActualWidth"),
                            Converter = new ElementWidhtConverter(),
                            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged

                        });
                        stackPanel.Children.Add(textBox);
                    }
                }
            };




            listView.inflateView(elasticInfoList);

            listView.SelectedIndex = 0;

            return view;
        }
        public async Task getIndices()
        {
            var response = string.Empty;
            using (var client = new HttpClient())
            {
                HttpResponseMessage result = await client.GetAsync("http://lab-arbarepelk101z.dev.jp.local:9200/_aliases");
                if (result.IsSuccessStatusCode)
                {
                    response = await result.Content.ReadAsStringAsync();
                    JObject properties = JObject.Parse(response);
                    foreach (JProperty prop in properties.Properties())
                    {
                        elasticInfoList.Add(new ElasticInfo { IsNotKey = false, index = prop.Name, size = 0 });
                        getCount(prop.Name);
                    }
                    elasticInfoList = elasticInfoList.OrderBy(info => info.index).ToList();
                    listView.DataContext = elasticInfoList;


                }
            }

        }
        public async Task getMapping(string indexname)
        {
            var elasticInfo = elasticInfoList.FirstOrDefault(current => current.index == indexname);
            if (elasticInfo.FieldInfoList != null) return;
            elasticInfo.FieldInfoList = new List<FieldInfo>();
            var client = new HttpClient();
            var result = await client.GetStringAsync("http://lab-arbarepelk101z.dev.jp.local:9200/" + indexname + "/_mapping");

            var results = JToken.Parse(result.ToString()).SelectTokens(indexname + ".mappings.properties").ToList();
            elasticInfo.FieldInfoList.Add(new FieldInfo { FieldName = "_id", FieldType = "String" });
            foreach (JProperty prop in ((JObject)results[0]).Properties())
            {
                foreach (JProperty types in ((JObject)prop.FirstOrDefault()).Properties())
                {
                    if (types.Name == "type")
                    {
                        if (types.Value.Type == JTokenType.String)
                        {
                            elasticInfo.FieldInfoList.Add(new FieldInfo { FieldName = prop.Name, FieldType = types.Value.ToString() });
                        }
                    }
                }
            }
        }
        public async Task getCount(string indexname)
        {
            var elasticInfo = elasticInfoList.FirstOrDefault(current => current.index == indexname);
            var client = new HttpClient();
            var result = await client.GetStringAsync("http://lab-arbarepelk101z.dev.jp.local:9200/" + indexname + "/_count");

            int docs_num = Int32.Parse(JToken.Parse(result.ToString()).SelectToken("count").ToString());
            elasticInfo.size = docs_num;

        }
        public async Task search(string indexname, string query)
        {
            var response = string.Empty;
            using (var client = new HttpClient())
            {

                string payload = query;
                if (string.IsNullOrWhiteSpace(payload))
                {
                    payload = @"
                               {      
                                 ""size"":1000,
                                 ""query"":
                                    {    
                                      ""match_all"": {}  
                                      }  
                                }";
                }
                HttpContent c = new StringContent(payload, System.Text.Encoding.UTF8, "application/json");
                HttpResponseMessage result = await client.PostAsync("http://lab-arbarepelk101z.dev.jp.local:9200/" + indexname + "/_search", c);
                if (result.IsSuccessStatusCode)
                {
                    response = result.StatusCode.ToString();
                    string resstring = await result.Content.ReadAsStringAsync();
                    int docs_num = Int32.Parse(JToken.Parse(resstring).SelectToken("hits.total.value").ToString());
                    if (docs_num > 1000) docs_num = 1000;
                    var documents = JToken.Parse(resstring)
                                        .SelectTokens("hits.hits[*]")
                                        .ToList();

                    await getMapping(indexname);
                    var elasticInfo = elasticInfoList.FirstOrDefault(current => current.index == indexname);

                    int idx = 1;
                    int jdx = 0;

                    dynamic xlApp = DevelopWorkspace.Base.Excel.GetLatestActiveExcelRef();
                    xlApp.Visible = true;

                    var targetSheet = xlApp.ActiveWorkbook.ActiveSheet;
                    //var selected = targetSheet.Range(targetSheet.Cells(1, 1), targetSheet.Cells(docs_num + 3, elasticInfo.FieldInfoList.Count()));

                    string[,] value2_copy = new string[docs_num + 3, elasticInfo.FieldInfoList.Count()];
                    value2_copy[0, 0] = indexname;
                    foreach (var fieldinfo in elasticInfo.FieldInfoList)
                    {
                        value2_copy[1, jdx] = fieldinfo.FieldName;
                        value2_copy[2, jdx] = fieldinfo.FieldType;
                        jdx++;

                    }
                    idx = 2;
                    foreach (var doc in documents)
                    {
                        idx++;
                        jdx = 0;
                        if (jdx > docs_num) break;

                        value2_copy[idx, 0] = doc.SelectToken("_id").ToString();
                        foreach (JProperty jproperty in ((JObject)doc.SelectToken("_source")).Properties())
                        {
                            int offset = elasticInfo.FieldInfoList.FindIndex(r => r.FieldName == jproperty.Name);

                            if (jproperty.Value.Type == JTokenType.Integer ||
                                jproperty.Value.Type == JTokenType.Float ||
                                jproperty.Value.Type == JTokenType.String ||
                                jproperty.Value.Type == JTokenType.Boolean ||
                                jproperty.Value.Type == JTokenType.Date)
                            {
                                if (offset >= 0)
                                {
                                    value2_copy[idx, offset] = jproperty.Value.ToString();
                                }

                            }
                            else if (jproperty.Value.Type == JTokenType.Null)
                            {
                                if (offset >= 0)
                                {
                                    value2_copy[idx, offset] = "";
                                }
                            }
                        }
                    }
                    List<List<string>> table = new List<List<string>>();
                    for (int i = 0; i < value2_copy.GetLength(0); i++)
                    {
                        List<string> temp = new List<string>();
                        for (int j = 0; j < value2_copy.GetLength(1); j++)
                        {
                            temp.Add(value2_copy[i, j]);
                        }
                        table.Add(temp);
                    }
                    table.exportToActiveSheetOfExcel();

                }
                else
                {
                    DevelopWorkspace.Base.Logger.WriteLine(result.ToString());
                }
            }
        }
        public async Task bulk(string payload)
        {

            var response = string.Empty;
            using (var client = new HttpClient())
            {

                HttpContent c = new StringContent(payload, System.Text.Encoding.UTF8, "application/json");
                HttpResponseMessage result = await client.PostAsync("http://lab-arbarepelk101z.dev.jp.local:9200/_bulk", c);
                if (result.IsSuccessStatusCode)
                {
                    response = result.StatusCode.ToString();
                    DevelopWorkspace.Base.Logger.WriteLine(result.ToString());
                    string resstring = await result.Content.ReadAsStringAsync();
                    DevelopWorkspace.Base.Logger.WriteLine(resstring);
                }
                else
                {
                    DevelopWorkspace.Base.Logger.WriteLine(result.ToString());
                }
            }
        }
        public async Task delete(string indexname)
        {

            var response = string.Empty;
            using (var client = new HttpClient())
            {

                var payload = @"
	                           {  	
	                			 ""query"":
	                    			{    
	                    			  ""match_all"": {}  
	                      			}  
	                    		}";
                HttpContent c = new StringContent(payload, System.Text.Encoding.UTF8, "application/json");
                HttpResponseMessage result = await client.PostAsync("http://lab-arbarepelk101z.dev.jp.local:9200/" + indexname + "/_delete_by_query", c);
                if (result.IsSuccessStatusCode)
                {
                    response = result.StatusCode.ToString();
                    string resstring = await result.Content.ReadAsStringAsync();
                    DevelopWorkspace.Base.Logger.WriteLine(resstring);
                }
                else
                {
                    DevelopWorkspace.Base.Logger.WriteLine(result.ToString());
                }
            }
        }

    }

    public class MainWindow : Window
    {
        public MainWindow(string strXaml)
        {
            Width = 600;
            Height = 800;

            Grid grid = new Grid();
            Content = grid;

            StackPanel parent = new StackPanel();
            grid.Children.Add(parent);

            ViewModel model = new ViewModel();

            var methods = model.GetType().GetMethods().Where(m => m.GetCustomAttributes(typeof(MethodMetaAttribute), false).Length > 0).ToList();
            for (int i = 0; i < methods.Count; i++)
            {
                var method = methods[i];
                var methodAttribute = (MethodMetaAttribute)Attribute.GetCustomAttribute(methods[i], typeof(MethodMetaAttribute));
                Button btn = new Button();
                btn.Content = methodAttribute.Name; ;
                parent.Children.Add(btn);
                btn.Click += (obj, subargs) =>
                {
                    method.Invoke(model, new object[] { obj, subargs });
                };
            }

            var view = model.getView(strXaml);
            view.Height = 600;
            view.Width = 800;
            parent.Children.Add(view);

            model.install(strXaml);
        }
    }
    public static void Main(string[] args)
    {
        DevelopWorkspace.Base.Logger.WriteLine("Process called");
        string strXaml = args[0].ToString();
        MainWindow win = new MainWindow(strXaml);
        win.Show();
    }
}