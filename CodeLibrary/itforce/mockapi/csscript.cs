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
using Heidesoft.Components.Controls;
using System.Windows.Threading;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Diagnostics;
using System.Threading;
using Xceed.Wpf.AvalonDock.Layout;
using Excel = Microsoft.Office.Interop.Excel;
using System.Reflection;
using DevelopWorkspace.Base.Model;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System.Net;

public class ElasticInfo : ViewModelBase
{

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
    public List<FieldInfo> FieldInfoList { get; set; }
}
public class FieldInfo
{
    public string FieldName { get; set; }
    public string FieldType { get; set; }
}

public class Script
{
    [AddinMeta(Name = "mockapi", Date = "2009-07-20", Description = "mockapi", LargeIcon = "apimock", Red = 128, Green = 145, Blue = 213)]
    public class ViewModel : DevelopWorkspace.Base.Model.ScriptBaseViewModel
    {
        System.Windows.Controls.ListView listView;
        List<ElasticInfo> elasticInfoList = new List<ElasticInfo>();
        HttpListener _listener = new HttpListener();

        [MethodMeta(Name = "ON", Date = "2009-07-20", Description = "ON", LargeIcon = "apiconect")]
        public void EventHandler1(object sender, RoutedEventArgs e)
        {
            try
            {
                _listener.Prefixes.Add("http://localhost:8081/");
                _listener.Prefixes.Add("http://127.0.0.1:8081/");
                _listener.Start();
                _listener.BeginGetContext(new AsyncCallback(GetContextCallback), null);

            }
            catch (Exception ex)
            {
                DevelopWorkspace.Base.Logger.WriteLine(ex.ToString());
            }
        }
        [MethodMeta(Name = "OFF", Date = "2009-07-20", Description = "OFF", LargeIcon = "disconect")]
        public void EventHandler2(object sender, RoutedEventArgs e)
        {
            try
            {
                _listener.Stop();
            }
            catch (Exception ex)
            {
                DevelopWorkspace.Base.Logger.WriteLine(ex.ToString());
            }

        }

        public void GetContextCallback(IAsyncResult result)
        {
            try
            {
                HttpListenerContext context = _listener.EndGetContext(result);
                HttpListenerRequest request = context.Request;
                HttpListenerResponse response = context.Response;

                System.Text.StringBuilder sb = new System.Text.StringBuilder();

                sb.Append("");
                sb.Append(string.Format("HttpMethod: {0}", request.HttpMethod));

                sb.Append(string.Format("Uri:        {0}", request.Url.AbsoluteUri));

                sb.Append(string.Format("LocalPath:  {0}", request.Url.LocalPath));
                foreach (string key in request.QueryString.Keys)
                {
                    sb.Append(string.Format("Query:      {0} = {1}", key, request.QueryString[key]));
                }
                sb.Append("");

                string responseString = sb.ToString();
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                response.ContentLength64 = buffer.Length;

                string documentContents;
                using (Stream receiveStream = request.InputStream)
                {
                    using (StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8))
                    {
                        documentContents = readStream.ReadToEnd();
                    }
                }

                DevelopWorkspace.Base.Logger.WriteLine(documentContents);

                request.Dump();

                using (System.IO.Stream outputStream = response.OutputStream)
                {
                    outputStream.Write(buffer, 0, buffer.Length);
                }
                _listener.BeginGetContext(new AsyncCallback(GetContextCallback), null);
            }
            catch (Exception ex)
            {
                DevelopWorkspace.Base.Logger.WriteLine(ex.ToString());
            }
        }

        public override UserControl getView(string strXaml)
        {
            StringReader strreader = new StringReader(strXaml);
            XmlTextReader xmlreader = new XmlTextReader(strreader);
            UserControl view = XamlReader.Load(xmlreader) as UserControl;
            listView = DevelopWorkspace.Base.Utils.WPF.FindLogicaChild<System.Windows.Controls.ListView>(view, "trvFamilies");

            listView.DataContext = elasticInfoList;
            //          listView.SelectedIndex = 0;

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