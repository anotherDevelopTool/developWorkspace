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
using WPFMediaKit;
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
using Nest;
using Elasticsearch.Net;
using System.Reflection;
using System.Collections.Generic;
using DevelopWorkspace.Base.Model;
public class ElasticInfo:ViewModelBase{

    private Boolean _isNotKey = false;
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
}

public class Sale1
{
			
			
    [Text(Name = "BAIKA")]
    public long? BAIKA { get; set; }
			
    [Text(Name = "CANCELGAK")]
    public long? CANCELGAK { get; set; }
			
    [Text(Name = "CANCELSU")]
    public long? CANCELSU { get; set; }
			
    [Text(Name = "CATEGORY")]
    public string CATEGORY { get; set; }
			
    [Text(Name = "COLORCD")]
    public string COLORCD { get; set; }
			
    [Text(Name = "COLORSIZEMEI")]
    public string COLORSIZEMEI { get; set; }
			
    [Text(Name = "CREATE_TIMESTAMP")]
    public string CREATE_TIMESTAMP { get; set; }
			
    [Text(Name = "FAVORITE_COUNT")]
    public long? FAVORITE_COUNT { get; set; }
			
    [Text(Name = "FROM_TIMESTAMP")]
    public string FROM_TIMESTAMP { get; set; }
			
    [Text(Name = "FROM_TIMESTAMP_DATE")]
    public string FROM_TIMESTAMP_DATE { get; set; }
			
    [Text(Name = "HENPINGAK")]
    public long? HENPINGAK { get; set; }
			
    [Text(Name = "HENPINSU")]
    public long? HENPINSU { get; set; }
			
    [Text(Name = "HINBAN")]
    public string HINBAN { get; set; }
			
    [Text(Name = "HINMEI")]
    public string HINMEI { get; set; }
			
    [Text(Name = "INVENTORY_COUNT")]
    public long? INVENTORY_COUNT { get; set; }
			
    [Text(Name = "JANCD")]
    public string JANCD { get; set; }
			
    [Text(Name = "JUCHUGAK")]
    public long? JUCHUGAK { get; set; }
			
    [Text(Name = "JUCHUSU")]
    public long? JUCHUSU { get; set; }
			
    [Text(Name = "KAKAK")]
    public long? KAKAK { get; set; }
			
    [Text(Name = "MAKERCOLORCD")]
    public string MAKERCOLORCD { get; set; }
			
    [Text(Name = "MAKERHINBAN")]
    public string MAKERHINBAN { get; set; }
			
    [Text(Name = "MAKERSIZECD")]
    public string MAKERSIZECD { get; set; }
			
    [Text(Name = "MAKER_SHOHINCD")]
    public string MAKER_SHOHINCD { get; set; }
			
    [Text(Name = "POROMOCD")]
    public string POROMOCD { get; set; }
			
    [Text(Name = "PRIMARY_KEY")]
    public string PRIMARY_KEY { get; set; }
			
    [Text(Name = "PROHINBAN")]
    public string PROHINBAN { get; set; }
			
    [Text(Name = "PROMOCD")]
    public string PROMOCD { get; set; }
			
    [Text(Name = "PROSHOKBN")]
    public string PROSHOKBN { get; set; }
			
    [Text(Name = "RBA_SHOHINCD")]
    public string RBA_SHOHINCD { get; set; }
			
    [Text(Name = "REQUEST_COUNT")]
    public long? REQUEST_COUNT { get; set; }
			
    [Text(Name = "SALESFORM")]
    public string SALESFORM { get; set; }
			
    [Text(Name = "SEX")]
    public string SEX { get; set; }
			
    [Text(Name = "SHICHUGAK")]
    public long? SHICHUGAK { get; set; }
			
    [Text(Name = "SHICHUSU")]
    public long? SHICHUSU { get; set; }
			
    [Text(Name = "SHUKKAGAK")]
    public long? SHUKKAGAK { get; set; }
			
    [Text(Name = "SHUKKASU")]
    public long? SHUKKASU { get; set; }
			
    [Text(Name = "SIZECD")]
    public string SIZECD { get; set; }
			
    [Text(Name = "TORIHIKISAKI")]
    public string TORIHIKISAKI { get; set; }
			
    [Text(Name = "URIAGEGAK")]
    public long? URIAGEGAK { get; set; }
			
    [Text(Name = "URIAGESU")]
    public long? URIAGESU { get; set; }
			
    [Text(Name = "YEAR")]
    public string YEAR { get; set; }
			
    [Text(Name = "YEARMONTH")]
    public string YEARMONTH { get; set; }
			
    [Text(Name = "YEARMONTHDAY")]
    public string YEARMONTHDAY { get; set; }
			
}

public class Sale
{
			
			
    [Text(Name = "BAIKA")]
    public long BAIKA { get; set; }
			
    [Text(Name = "CANCELGAK")]
    public long CANCELGAK { get; set; }
			
    [Text(Name = "CANCELSU")]
    public long CANCELSU { get; set; }
			
    [Text(Name = "CATEGORY")]
    public string CATEGORY { get; set; }
			
    [Text(Name = "COLORCD")]
    public string COLORCD { get; set; }
			
    [Text(Name = "COLORSIZEMEI")]
    public string COLORSIZEMEI { get; set; }
			
    [Text(Name = "CREATE_TIMESTAMP")]
    public string CREATE_TIMESTAMP { get; set; }
			
    [Text(Name = "FAVORITE_COUNT")]
    public long FAVORITE_COUNT { get; set; }
			
    [Text(Name = "FROM_TIMESTAMP")]
    public string FROM_TIMESTAMP { get; set; }
			
    [Text(Name = "FROM_TIMESTAMP_DATE")]
    public string FROM_TIMESTAMP_DATE { get; set; }
			
    [Text(Name = "HENPINGAK")]
    public long HENPINGAK { get; set; }
			
    [Text(Name = "HENPINSU")]
    public long HENPINSU { get; set; }
			
    [Text(Name = "HINBAN")]
    public string HINBAN { get; set; }
			
    [Text(Name = "HINMEI")]
    public string HINMEI { get; set; }
			
    [Text(Name = "INVENTORY_COUNT")]
    public long INVENTORY_COUNT { get; set; }
			
    [Text(Name = "JANCD")]
    public string JANCD { get; set; }
			
    [Text(Name = "JUCHUGAK")]
    public long JUCHUGAK { get; set; }
			
    [Text(Name = "JUCHUSU")]
    public long JUCHUSU { get; set; }
			
    [Text(Name = "KAKAK")]
    public long KAKAK { get; set; }
			
    [Text(Name = "MAKERCOLORCD")]
    public string MAKERCOLORCD { get; set; }
			
    [Text(Name = "MAKERHINBAN")]
    public string MAKERHINBAN { get; set; }
			
    [Text(Name = "MAKERSIZECD")]
    public string MAKERSIZECD { get; set; }
			
    [Text(Name = "MAKER_SHOHINCD")]
    public string MAKER_SHOHINCD { get; set; }
			
    [Text(Name = "POROMOCD")]
    public string POROMOCD { get; set; }
			
    [Text(Name = "PRIMARY_KEY")]
    public string PRIMARY_KEY { get; set; }
			
    [Text(Name = "PROHINBAN")]
    public string PROHINBAN { get; set; }
			
    [Text(Name = "PROMOCD")]
    public string PROMOCD { get; set; }
			
    [Text(Name = "PROSHOKBN")]
    public string PROSHOKBN { get; set; }
			
    [Text(Name = "RBA_SHOHINCD")]
    public string RBA_SHOHINCD { get; set; }
			
    [Text(Name = "REQUEST_COUNT")]
    public long REQUEST_COUNT { get; set; }
			
    [Text(Name = "SALESFORM")]
    public string SALESFORM { get; set; }
			
    [Text(Name = "SEX")]
    public string SEX { get; set; }
			
    [Text(Name = "SHICHUGAK")]
    public long SHICHUGAK { get; set; }
			
    [Text(Name = "SHICHUSU")]
    public long SHICHUSU { get; set; }
			
    [Text(Name = "SHUKKAGAK")]
    public long SHUKKAGAK { get; set; }
			
    [Text(Name = "SHUKKASU")]
    public long SHUKKASU { get; set; }
			
    [Text(Name = "SIZECD")]
    public string SIZECD { get; set; }
			
    [Text(Name = "TORIHIKISAKI")]
    public string TORIHIKISAKI { get; set; }
			
    [Text(Name = "URIAGEGAK")]
    public long URIAGEGAK { get; set; }
			
    [Text(Name = "URIAGESU")]
    public long URIAGESU { get; set; }
			
    [Text(Name = "YEAR")]
    public string YEAR { get; set; }
			
    [Text(Name = "YEARMONTH")]
    public string YEARMONTH { get; set; }
			
    [Text(Name = "YEARMONTHDAY")]
    public string YEARMONTHDAY { get; set; }
			

}

public class Script
{

    public static string invokeGetByName(Sale sale, string methodName) {

        PropertyInfo prop = sale.GetType().GetProperty(methodName);
        object returnValue = prop.GetValue(sale);
        return returnValue == null ? "" :  String.Join("", returnValue.ToString().Where(c => c != '\n' && c != '\r' && c != '\t'));
    }   
    public static void invokeSetByName(Sale sale, string methodName,string methodValue)
    {

        PropertyInfo prop = sale.GetType().GetProperty(methodName);
        if (prop.PropertyType.Name.Equals("Int64") || prop.PropertyType.Name.Equals("Int32") )
        {
            prop.SetValue(sale, long.Parse(methodValue));
        }
        else
        {
            prop.SetValue(sale, methodValue);
        }
    }
    
    //https://stackoverflow.com/questions/248362/how-do-i-build-a-datatemplate-in-c-sharp-code
    //TODO 面向Addin基类化
    [AddinMeta(Name = "elasticSearch", Date = "2009-07-20", Description = "elastic utility",Red =128,Green=145,Blue=213)]
    public class ViewModel : DevelopWorkspace.Base.Model.ScriptBaseViewModel
    {
        System.Windows.Controls.ListView listView;
        ElasticClient client;
        
        [MethodMeta(Name = "load", Date = "2009-07-20", Description = "所有index载入", LargeIcon = "elasticsearch")]
        public void EventHandler1(object sender, RoutedEventArgs e)
        {
            try{
                foreach( var item in listView.DataContext as ElasticInfo[]){
                    string slectetedIndex = item.index;
                    var searchResults = client.Search<Sale>(s => s
                                            .Index(slectetedIndex)
                                            .Size(0)
                                            .Query(q => q
                                                .MatchAll()
                                            )
                                        );
                    //DevelopWorkspace.Base.Logger.WriteLine(DevelopWorkspace.Base.Dump.ToDump(searchResults),DevelopWorkspace.Base.Level.TRACE);
                    item.size = searchResults.HitsMetadata.Total.Value;
                }
            }
            catch(Exception ex){
                DevelopWorkspace.Base.Logger.WriteLine(ex.ToString());
            }
        }
        [MethodMeta(Name = "import", Date = "2009-07-20", Description = "导入指定index的所有document到EXCEL", LargeIcon = "import")]
        public void EventHandler2(object sender, RoutedEventArgs e)
        {
            try{
                dynamic content = listView.SelectedItem;
                string slectetedIndex = content.index;
    
                PropertyInfo[] properties = typeof(Sale).GetProperties();
    
                dynamic xlApp = DevelopWorkspace.Base.Excel.GetLatestActiveExcelRef();
                xlApp.Visible = true;
    
    
                var targetSheet = xlApp.ActiveWorkbook.ActiveSheet;
                object[,] value2_copy = targetSheet.Range(targetSheet.Cells(1, 1), targetSheet.Cells(targetSheet.UsedRange.Rows.Count, targetSheet.UsedRange.Columns.Count)).Value2;
                List<string> headerList = new List<string>();
                List<Sale> sales = new List<Sale>();
                //header
                for (int iidx = 1; iidx < value2_copy.GetLength(1); iidx++) {
                    headerList.Add(value2_copy[1, iidx].ToString());
                    System.Diagnostics.Debug.WriteLine(value2_copy[1,iidx].ToString());
                }
                for (int iidx = 2; iidx < value2_copy.GetLength(0) + 1; iidx++)
                {
                    Sale sale = new Sale();
                    for (int jjdx = 1; jjdx < value2_copy.GetLength(1); jjdx++) {
                        invokeSetByName(sale, headerList[jjdx-1], value2_copy[iidx, jjdx].ToString());
                    }
                    //var response = client.Index(sale, idx => idx.Index( slectetedIndex));
                    sales.Add(sale);
                    DevelopWorkspace.Base.Logger.WriteLine(iidx.ToString());
                    
                }
                if(sales.Count() == 0) return;
                //注意id没有设定
                //如果有问题的话 参考这个链接 https://qiita.com/mino_s2000/items/191817f9d1d16320c478
                var response = client.Bulk(idx => idx.Index( slectetedIndex).IndexMany(sales));

                DevelopWorkspace.Base.Logger.WriteLine("Process called");
                
                var searchResults = client.Search<Sale>(s => s
                                        .Index(slectetedIndex)
                                        .Size(0)
                                        .Query(q => q
                                            .MatchAll()
                                        )
                                    );
                content.size = searchResults.HitsMetadata.Total.Value;
            }
            catch(Exception ex){
                DevelopWorkspace.Base.Logger.WriteLine(ex.ToString());
            }
            
        }
        [MethodMeta(Name = "export", Date = "2009-07-20", Description = "指定EXCEL内的数据反映到Elastic", LargeIcon = "export")]
        public void EventHandler3(object sender, RoutedEventArgs e)
        {
            try{
                dynamic content = listView.SelectedItem;
                string slectetedIndex = content.index;
    
                int idx = 1;
                int jdx = 1;
        
                var searchResults = client.Search<Sale>(s => s
                                        .Index(slectetedIndex)
                                        .Size(1000)
                                        .Query(q => q
                                            .MatchAll()
                                        )
                                    );
                PropertyInfo[] properties = typeof(Sale).GetProperties();
        
                //DevelopWorkspace.Base.Logger.WriteLine(properties.Select(p => p.Name).Aggregate((c1, c2) => c1 + "\t" + c2));
                
                var rowWithIdx = searchResults.Documents.Select((sale, iidx) => new { row = sale, idx = iidx });
        
                dynamic xlApp = DevelopWorkspace.Base.Excel.GetLatestActiveExcelRef();
                xlApp.Visible = true;
        
                var targetSheet = xlApp.ActiveWorkbook.ActiveSheet;
                var selected  = targetSheet.Range(targetSheet.Cells(1, 1), targetSheet.Cells(rowWithIdx.Count() + 1, properties.Count() + 1));
                object[,] value2_copy = selected.Value2;
        
                //header
                foreach(var property in properties) {
                    value2_copy[1,idx] = property.Name;
                    idx++;
                }
                
                idx =2;
                //rowdata 
                foreach(var rowdata in rowWithIdx) {
                    List<string> columnList = new List<string>();
                    jdx=1;
                    foreach (PropertyInfo property in properties)
                    {
                        columnList.Add(invokeGetByName(rowdata.row, property.Name));
                        value2_copy[idx,jdx] = invokeGetByName(rowdata.row, property.Name);
                        jdx++;
                    }
                    idx++;
                    
                    //DevelopWorkspace.Base.Logger.WriteLine(columnList.Aggregate((c1,c2) => c1 + "\t" + c2));
                }
                var header = targetSheet.Range(targetSheet.Cells(1, 1), targetSheet.Cells(1, properties.Count()));
                header.Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.FromArgb(255, 0, 255, 0));        
                
                selected.NumberFormat = "@";
                selected.Value2 = value2_copy;
                targetSheet.Columns("A:AZ").EntireColumn.AutoFit();
                DevelopWorkspace.Base.Logger.WriteLine("Process called");
            }
            catch(Exception ex){
                DevelopWorkspace.Base.Logger.WriteLine(ex.ToString());
            }
            
        }
        [MethodMeta(Name = "delete", Date = "2009-07-20", Description = "删除指定index的所有document，index本身不删除", LargeIcon = "delete")]
        public void EventHandler4(object sender, RoutedEventArgs e)
        {
            try{
                dynamic content = listView.SelectedItem;
                string slectetedIndex = content.index;
    
                //delete all documents before insert 
                var searchResults = client.Search<Sale>(s => s
                                        .Index(slectetedIndex)
                                        .Size(1000)
                                        .Query(q => q
                                            .MatchAll()
                                        )
                                    );
                foreach( var hit in searchResults.Hits){
                    client.Delete<Sale>(hit.Id, idx => idx.Index( slectetedIndex));
                }            
                
                
                searchResults = client.Search<Sale>(s => s
                                        .Index(slectetedIndex)
                                        .Size(0)
                                        .Query(q => q
                                            .MatchAll()
                                        )
                                    );
                content.size = searchResults.HitsMetadata.Total.Value;
            }
            catch(Exception ex){
                DevelopWorkspace.Base.Logger.WriteLine(ex.ToString());
            }
        }
        public override UserControl getView(string strXaml)
        {
            StringReader strreader = new StringReader(strXaml);
            XmlTextReader xmlreader = new XmlTextReader(strreader);
            UserControl view = XamlReader.Load(xmlreader) as UserControl;
            listView = DevelopWorkspace.Base.Utils.WPF.FindLogicaChild<System.Windows.Controls.ListView>(view, "trvFamilies");
            listView.DataContext = new ElasticInfo[] { 
                new ElasticInfo { IsNotKey = false, index = "sales_reports_current", size = 200 }, 
                new ElasticInfo { IsNotKey = false, index = "sales_reports-2019-10", size = 200 },
                new ElasticInfo { IsNotKey = false, index = "sales_reports-2019-09", size = 200 },
                new ElasticInfo { IsNotKey = false, index = "sales_reports-2019-08", size = 200 },
                new ElasticInfo { IsNotKey = false, index = "testindex003", size = 200 },
            };
            listView.SelectedIndex = 0;
            
            var node = new Uri("http://lab-arbarepelk101z.dev.jp.local:9200");
            var settings = new ConnectionSettings(node).DisableDirectStreaming();
            settings.OnRequestCompleted(call =>
            {
                DevelopWorkspace.Base.Logger.WriteLine(Encoding.UTF8.GetString(call.RequestBodyInBytes),DevelopWorkspace.Base.Level.TRACE);
                DevelopWorkspace.Base.Logger.WriteLine(Encoding.UTF8.GetString(call.ResponseBodyInBytes),DevelopWorkspace.Base.Level.TRACE);
            });                    
            
            client = new ElasticClient(settings);
            
            EventHandler1(null,null);
            // (listView.SelectedItem as ListViewItem).Content
            //btnCapurure.Click += (obj, subargs) =>
            //{
            //    DevelopWorkspace.Base.Logger.WriteLine("Process called");
            //};
            return view;
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

            parent.Children.Add(model.getView(strXaml));

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