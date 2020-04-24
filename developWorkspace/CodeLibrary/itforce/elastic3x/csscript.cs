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
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
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
	public List<FieldInfo> FieldInfoList  { get; set; }
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
    [AddinMeta(Name = "elasticSearch", Date = "2009-07-20", Description = "elastic utility",Red =128,Green=145,Blue=213)]
    public class ViewModel : DevelopWorkspace.Base.Model.ScriptBaseViewModel
    {
        System.Windows.Controls.ListView listView;
        ElasticClient client;
        List<ElasticInfo> elasticInfoList = new List<ElasticInfo>();
        
        [MethodMeta(Name = "import", Date = "2009-07-20", Description = "导入指定index的所有document到EXCEL", LargeIcon = "import")]
        public async void EventHandler2(object sender, RoutedEventArgs e)
        {
            try{
                dynamic content = listView.SelectedItem;
                string slectetedIndex = content.index;
                await getMapping(slectetedIndex);
				var elasticInfo = elasticInfoList.FirstOrDefault(current => current.index == slectetedIndex);    
				
                dynamic xlApp = DevelopWorkspace.Base.Excel.GetLatestActiveExcelRef();
                xlApp.Visible = true;
    
    
                var targetSheet = xlApp.ActiveWorkbook.ActiveSheet;
                object[,] value2_copy = targetSheet.Range(targetSheet.Cells(1, 1), targetSheet.Cells(targetSheet.UsedRange.Rows.Count, targetSheet.UsedRange.Columns.Count)).Value2;
                List<string> headerList = new List<string>();
                //header
                for (int iidx = 1; iidx < value2_copy.GetLength(1) + 1; iidx++) {
                    headerList.Add(value2_copy[2, iidx].ToString());
                }

				string payload_json = "";
                string bulk_header_leftpart = @"{ ""index"" : { ""_index"" : """ + slectetedIndex + @""",""_id"": """;
                string bulk_header_rightpart = @"""} }";
                for (int iidx = 4; iidx < value2_copy.GetLength(0) + 1; iidx++)
                {
                	string rowIdString = "";
					string rowString = "{";
                    for (int jjdx = 1; jjdx < value2_copy.GetLength(1) + 1; jjdx++) {
						string property_name = headerList[jjdx-1];
						if(property_name == "_id"){
							string idstring = value2_copy[iidx, jjdx] == null ? "" : value2_copy[iidx, jjdx].ToString();
							DevelopWorkspace.Base.Logger.WriteLine(idstring,DevelopWorkspace.Base.Level.DEBUG);
							rowIdString = bulk_header_leftpart + idstring + bulk_header_rightpart;
						}
						else{
							
							var fieldInfo = elasticInfo.FieldInfoList.FirstOrDefault(current => current.FieldName == property_name); 
							if( fieldInfo != null ){
								rowString += @"""" + fieldInfo.FieldName + @""":";
								//fieldInfo.FieldType
								if( value2_copy[iidx, jjdx] == null ){
									rowString += @"""" + "null" + @"""";
								}
								else{
									rowString += @"""" + value2_copy[iidx, jjdx].ToString() + @"""";
								}
								if(jjdx == value2_copy.GetLength(1)  ){
									
								}
								else{
									rowString += ",";
								}
								
							}
						}
                    }
					rowString += "}";
					DevelopWorkspace.Base.Logger.WriteLine(rowIdString,DevelopWorkspace.Base.Level.DEBUG);
					DevelopWorkspace.Base.Logger.WriteLine(rowString,DevelopWorkspace.Base.Level.DEBUG);
                    payload_json += rowIdString + "\n";
                    payload_json += rowString + "\n";
                }
                
				bulk(payload_json);                
                
            }
            catch(Exception ex){
                DevelopWorkspace.Base.Logger.WriteLine(ex.ToString());
            }
            
        }
        [MethodMeta(Name = "export", Date = "2009-07-20", Description = "指定EXCEL内的数据反映到Elastic", LargeIcon = "export")]
        public void EventHandler3(object sender, RoutedEventArgs e)
        {
            dynamic content = listView.SelectedItem;
            string slectetedIndex = content.index;

			search(slectetedIndex);
            
        }
        [MethodMeta(Name = "delete", Date = "2009-07-20", Description = "删除指定index的所有document，index本身不删除", LargeIcon = "delete")]
        public void EventHandler4(object sender, RoutedEventArgs e)
        {
            dynamic content = listView.SelectedItem;
            string slectetedIndex = content.index;
            delete(slectetedIndex);
        }

        public override UserControl getView(string strXaml)
        {
            StringReader strreader = new StringReader(strXaml);
            XmlTextReader xmlreader = new XmlTextReader(strreader);
            UserControl view = XamlReader.Load(xmlreader) as UserControl;
            listView = DevelopWorkspace.Base.Utils.WPF.FindLogicaChild<System.Windows.Controls.ListView>(view, "trvFamilies");
            
            getIndices();
            
            listView.DataContext = elasticInfoList;
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
				}
			}        
	    }    
	    public async Task getMapping(string indexname)
	    {
	        var elasticInfo = elasticInfoList.FirstOrDefault(current => current.index == indexname);
	        if( elasticInfo.FieldInfoList != null ) return;
			elasticInfo.FieldInfoList = new List<FieldInfo>();
	        var client = new HttpClient();
	        var result = await client.GetStringAsync("http://lab-arbarepelk101z.dev.jp.local:9200/" + indexname + "/_mapping");
	        
	        var results = JToken.Parse(result.ToString()).SelectTokens(indexname + ".mappings.properties").ToList();
            elasticInfo.FieldInfoList.Add(new FieldInfo { FieldName = "_id",FieldType = "String"});
	        foreach (JProperty prop in ((JObject)results[0]).Properties())
	        {
	            foreach (JProperty types in ((JObject)prop.FirstOrDefault()).Properties())
	            {
	                if(types.Name == "type"){
		                if (types.Value.Type == JTokenType.String)
		                {
		                    elasticInfo.FieldInfoList.Add(new FieldInfo { FieldName = prop.Name,FieldType = types.Value.ToString()});
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
	    public async Task search(string indexname)
	    {
			var response = string.Empty;
			using (var client = new HttpClient())
			{
				  
				var payload = @"
	                           {  	
	                             ""size"":1000,
	                			 ""query"":
	                    			{    
	                    			  ""match_all"": {}  
	                      			}  
	                    		}";
				HttpContent c = new StringContent(payload, System.Text.Encoding.UTF8, "application/json");
				HttpResponseMessage result = await client.PostAsync("http://lab-arbarepelk101z.dev.jp.local:9200/" + indexname + "/_search", c);
				if (result.IsSuccessStatusCode)
				{
					response = result.StatusCode.ToString();
					string resstring = await result.Content.ReadAsStringAsync();
			        int docs_num = Int32.Parse(JToken.Parse(resstring).SelectToken("hits.total.value").ToString());
			        if( docs_num > 1000 ) docs_num = 1000;
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
					var selected  = targetSheet.Range(targetSheet.Cells(1, 1), targetSheet.Cells( docs_num + 3, elasticInfo.FieldInfoList.Count()));
					object[,] value2_copy = selected.Value2;
			
					value2_copy[1,1] = indexname;
					foreach (var fieldinfo in elasticInfo.FieldInfoList)
					{
						jdx++;
						value2_copy[2,jdx] = fieldinfo.FieldName;
						value2_copy[3,jdx] = fieldinfo.FieldType;
						
					}
					idx =3;
					foreach (var doc in documents)
					{
						idx++;
						jdx =1;
						if ( jdx > docs_num ) break;

						value2_copy[idx,1] = doc.SelectToken("_id").ToString();
						foreach (JProperty jproperty in ((JObject)doc.SelectToken("_source")).Properties())
						{
							int offset = elasticInfo.FieldInfoList.FindIndex(r => r.FieldName == jproperty.Name);
							
							if (jproperty.Value.Type == JTokenType.Integer ||
								jproperty.Value.Type == JTokenType.Float ||
								jproperty.Value.Type == JTokenType.String ||
								jproperty.Value.Type == JTokenType.Boolean ||
								jproperty.Value.Type == JTokenType.Date)
							{
								if( offset >= 0 ){
									value2_copy[idx,offset +1] = jproperty.Value.ToString();
								}

							}
							else if (jproperty.Value.Type == JTokenType.Null) {
								if( offset >= 0 ){
									value2_copy[idx,offset + 1] = "";
								}
							}
						}
					}
					
					var header = targetSheet.Range(targetSheet.Cells(1, 1), targetSheet.Cells( 3, elasticInfo.FieldInfoList.Count()));
					header.Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.FromArgb(255, 0, 255, 0));        
					selected.NumberFormat = "@";
					selected.Value2 = value2_copy;
					targetSheet.Columns("A:AZ").EntireColumn.AutoFit();
				
				}
				else{
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
				else{
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
				else{
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