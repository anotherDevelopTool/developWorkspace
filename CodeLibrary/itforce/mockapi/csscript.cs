using System;
using Microsoft.CSharp;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using System.Data.SQLite;
using System.Data;
using System.Data.Common;
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
using System.Text.RegularExpressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Threading;
using System.Diagnostics;
class SimpleHTTPServer
{
    private readonly string[] _indexFiles = { 
        "index.html", 
        "index.htm", 
        "default.html", 
        "default.htm" 
    };
    
    private static IDictionary<string, string> _mimeTypeMappings = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase) {
        #region extension to MIME type list
        {".asf", "video/x-ms-asf"},
        {".asx", "video/x-ms-asf"},
        {".avi", "video/x-msvideo"},
        {".bin", "application/octet-stream"},
        {".cco", "application/x-cocoa"},
        {".crt", "application/x-x509-ca-cert"},
        {".css", "text/css"},
        {".deb", "application/octet-stream"},
        {".der", "application/x-x509-ca-cert"},
        {".dll", "application/octet-stream"},
        {".dmg", "application/octet-stream"},
        {".ear", "application/java-archive"},
        {".eot", "application/octet-stream"},
        {".exe", "application/octet-stream"},
        {".flv", "video/x-flv"},
        {".gif", "image/gif"},
        {".hqx", "application/mac-binhex40"},
        {".htc", "text/x-component"},
        {".htm", "text/html"},
        {".html", "text/html"},
        {".ico", "image/x-icon"},
        {".img", "application/octet-stream"},
        {".iso", "application/octet-stream"},
        {".jar", "application/java-archive"},
        {".jardiff", "application/x-java-archive-diff"},
        {".jng", "image/x-jng"},
        {".jnlp", "application/x-java-jnlp-file"},
        {".jpeg", "image/jpeg"},
        {".jpg", "image/jpeg"},
        {".js", "application/x-javascript"},
        {".mml", "text/mathml"},
        {".mng", "video/x-mng"},
        {".mov", "video/quicktime"},
        {".mp3", "audio/mpeg"},
        {".mpeg", "video/mpeg"},
        {".mpg", "video/mpeg"},
        {".msi", "application/octet-stream"},
        {".msm", "application/octet-stream"},
        {".msp", "application/octet-stream"},
        {".pdb", "application/x-pilot"},
        {".pdf", "application/pdf"},
        {".pem", "application/x-x509-ca-cert"},
        {".pl", "application/x-perl"},
        {".pm", "application/x-perl"},
        {".png", "image/png"},
        {".prc", "application/x-pilot"},
        {".ra", "audio/x-realaudio"},
        {".rar", "application/x-rar-compressed"},
        {".rpm", "application/x-redhat-package-manager"},
        {".rss", "text/xml"},
        {".run", "application/x-makeself"},
        {".sea", "application/x-sea"},
        {".shtml", "text/html"},
        {".sit", "application/x-stuffit"},
        {".swf", "application/x-shockwave-flash"},
        {".tcl", "application/x-tcl"},
        {".tk", "application/x-tcl"},
        {".txt", "text/plain"},
        {".war", "application/java-archive"},
        {".wbmp", "image/vnd.wap.wbmp"},
        {".wmv", "video/x-ms-wmv"},
        {".xml", "text/xml"},
        {".xpi", "application/x-xpinstall"},
        {".zip", "application/zip"},
        #endregion
    };
    private Thread _serverThread;
    private string _rootDirectory;
    private HttpListener _listener;
    private int _port;
 
    public int Port
    {
        get { return _port; }
        private set { }
    }
 
    /// <summary>
    /// Construct server with given port.
    /// </summary>
    /// <param name="path">Directory path to serve.</param>
    /// <param name="port">Port of the server.</param>
    public SimpleHTTPServer(string path, int port)
    {
        this.Initialize(path, port);
    }
 
    /// <summary>
    /// Construct server with suitable port.
    /// </summary>
    /// <param name="path">Directory path to serve.</param>
    public SimpleHTTPServer(string path)
    {
        //get an empty port
        TcpListener l = new TcpListener(IPAddress.Loopback, 0);
        l.Start();
        int port = ((IPEndPoint)l.LocalEndpoint).Port;
        l.Stop();
        this.Initialize(path, port);
    }
 
    /// <summary>
    /// Stop server and dispose all functions.
    /// </summary>
    public void Stop()
    {
        _serverThread.Abort();
        _listener.Stop();
    }
 
    private void Listen()
    {
        try{
            _listener = new HttpListener();
            _listener.Prefixes.Add("http://localhost:" + _port.ToString() + "/");
            _listener.Start();
            while (true)
            {
                try
                {
                    HttpListenerContext context = _listener.GetContext();
                    Process(context);
                }
                catch (Exception ex)
                {
     
                }
            }
        }
        catch (Exception ex)
        {

        }
        

    }
 
    private void Process(HttpListenerContext context)
    {
        string filename = context.Request.Url.AbsolutePath;
        Console.WriteLine(filename);
        filename = filename.Substring(1);
 
        if (string.IsNullOrEmpty(filename))
        {
            foreach (string indexFile in _indexFiles)
            {
                if (File.Exists(Path.Combine(_rootDirectory, indexFile)))
                {
                    filename = indexFile;
                    break;
                }
            }
        }
 
        filename = Path.Combine(_rootDirectory, filename);
 
        if (File.Exists(filename))
        {
            try
            {
                Stream input = new FileStream(filename, FileMode.Open);
                
                //Adding permanent http response headers
                string mime;
                context.Response.ContentType = _mimeTypeMappings.TryGetValue(Path.GetExtension(filename), out mime) ? mime : "application/octet-stream";
                context.Response.ContentLength64 = input.Length;
                context.Response.AddHeader("Date", DateTime.Now.ToString("r"));
                context.Response.AddHeader("Last-Modified", System.IO.File.GetLastWriteTime(filename).ToString("r"));
 
                byte[] buffer = new byte[1024 * 16];
                int nbytes;
                while ((nbytes = input.Read(buffer, 0, buffer.Length)) > 0)
                    context.Response.OutputStream.Write(buffer, 0, nbytes);
                input.Close();
                
                context.Response.StatusCode = (int)HttpStatusCode.OK;
                context.Response.OutputStream.Flush();
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }
 
        }
        else
        {
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
        }
        
        context.Response.OutputStream.Close();
    }
 
    private void Initialize(string path, int port)
    {
        this._rootDirectory = path;
        this._port = port;
        _serverThread = new Thread(this.Listen);
        _serverThread.Start();
    }
 
 
}

public class EndPointInfo : ViewModelBase
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

    private string _endPoint = null;
    public string EndPoint
    {
        get { return _endPoint; }
        set
        {
            if (_endPoint != value)
            {
                _endPoint = value;
                RaisePropertyChanged("endPoint");
            }
        }
    }
    private string _rulePath = "";
    public string RulePath
    {
        get { return _rulePath; }
        set
        {
            if (_rulePath != value)
            {
                _rulePath = value;
                RaisePropertyChanged("rulePath");
            }
        }
    }
    public List<Rule> RuleList { get; set; }
}
public class Rule
{
    public string ContentType { get; set; }
    public string EndPoint { get; set; }
    public string MatchString { get; set; }
    public string ResponseFile { get; set; }
    public int Likeness { get; set; }
}
public class Levenshtein
{


    ///*****************************
    /// Compute Levenshtein distance 
    /// Memory efficient version
    ///*****************************
    public int iLD(String sRow, String sCol)
    {
        int RowLen = sRow.Length;  // length of sRow
        int ColLen = sCol.Length;  // length of sCol
        int RowIdx;                // iterates through sRow
        int ColIdx;                // iterates through sCol
        char Row_i;                // ith character of sRow
        char Col_j;                // jth character of sCol
        int cost;                   // cost

        /// Test string length
        if (Math.Max(sRow.Length, sCol.Length) > Math.Pow(2, 31))
            throw (new Exception("\nMaximum string length in Levenshtein.iLD is " + Math.Pow(2, 31) + ".\nYours is " + Math.Max(sRow.Length, sCol.Length) + "."));

        // Step 1

        if (RowLen == 0)
        {
            return ColLen;
        }

        if (ColLen == 0)
        {
            return RowLen;
        }

        /// Create the two vectors
        int[] v0 = new int[RowLen + 1];
        int[] v1 = new int[RowLen + 1];
        int[] vTmp;



        /// Step 2
        /// Initialize the first vector
        for (RowIdx = 1; RowIdx <= RowLen; RowIdx++)
        {
            v0[RowIdx] = RowIdx;
        }

        // Step 3

        /// Fore each column
        for (ColIdx = 1; ColIdx <= ColLen; ColIdx++)
        {
            /// Set the 0'th element to the column number
            v1[0] = ColIdx;

            Col_j = sCol[ColIdx - 1];


            // Step 4

            /// Fore each row
            for (RowIdx = 1; RowIdx <= RowLen; RowIdx++)
            {
                Row_i = sRow[RowIdx - 1];


                // Step 5

                if (Row_i == Col_j)
                {
                    cost = 0;
                }
                else
                {
                    cost = 1;
                }

                // Step 6

                /// Find minimum
                int m_min = v0[RowIdx] + 1;
                int b = v1[RowIdx - 1] + 1;
                int c = v0[RowIdx - 1] + cost;

                if (b < m_min)
                {
                    m_min = b;
                }
                if (c < m_min)
                {
                    m_min = c;
                }

                v1[RowIdx] = m_min;
            }

            /// Swap the vectors
            vTmp = v0;
            v0 = v1;
            v1 = vTmp;

        }


        // Step 7

        /// Value between 0 - 100
        /// 0==perfect match 100==totaly different
        /// 
        /// The vectors where swaped one last time at the end of the last loop,
        /// that is why the result is now in v0 rather than in v1
        System.Console.WriteLine("iDist=" + v0[RowLen]);
        int max = System.Math.Max(RowLen, ColLen);
        return ((100 * v0[RowLen]) / max);
    }





    ///*****************************
    /// Compute the min
    ///*****************************

    private int Minimum(int a, int b, int c)
    {
        int mi = a;

        if (b < mi)
        {
            mi = b;
        }
        if (c < mi)
        {
            mi = c;
        }

        return mi;
    }

    ///*****************************
    /// Compute Levenshtein distance         
    ///*****************************

    public int LD(String sNew, String sOld)
    {
        int[,] matrix;              // matrix
        int sNewLen = sNew.Length;  // length of sNew
        int sOldLen = sOld.Length;  // length of sOld
        int sNewIdx; // iterates through sNew
        int sOldIdx; // iterates through sOld
        char sNew_i; // ith character of sNew
        char sOld_j; // jth character of sOld
        int cost; // cost

        /// Test string length
        if (Math.Max(sNew.Length, sOld.Length) > Math.Pow(2, 31))
            throw (new Exception("\nMaximum string length in Levenshtein.LD is " + Math.Pow(2, 31) + ".\nYours is " + Math.Max(sNew.Length, sOld.Length) + "."));

        // Step 1

        if (sNewLen == 0)
        {
            return sOldLen;
        }

        if (sOldLen == 0)
        {
            return sNewLen;
        }

        matrix = new int[sNewLen + 1, sOldLen + 1];

        // Step 2

        for (sNewIdx = 0; sNewIdx <= sNewLen; sNewIdx++)
        {
            matrix[sNewIdx, 0] = sNewIdx;
        }

        for (sOldIdx = 0; sOldIdx <= sOldLen; sOldIdx++)
        {
            matrix[0, sOldIdx] = sOldIdx;
        }

        // Step 3

        for (sNewIdx = 1; sNewIdx <= sNewLen; sNewIdx++)
        {
            sNew_i = sNew[sNewIdx - 1];

            // Step 4

            for (sOldIdx = 1; sOldIdx <= sOldLen; sOldIdx++)
            {
                sOld_j = sOld[sOldIdx - 1];

                // Step 5

                if (sNew_i == sOld_j)
                {
                    cost = 0;
                }
                else
                {
                    cost = 1;
                }

                // Step 6

                matrix[sNewIdx, sOldIdx] = Minimum(matrix[sNewIdx - 1, sOldIdx] + 1, matrix[sNewIdx, sOldIdx - 1] + 1, matrix[sNewIdx - 1, sOldIdx - 1] + cost);

            }
        }

        // Step 7

        /// Value between 0 - 100
        /// 0==perfect match 100==totaly different
        DevelopWorkspace.Base.Logger.WriteLine("Dist=" + matrix[sNewLen, sOldLen]);
        int max = System.Math.Max(sNewLen, sOldLen);
        return (100 * matrix[sNewLen, sOldLen]) / max;
    }
}
public class Script
{
    [AddinMeta(Name = "mockapi", Date = "2009-07-20", Description = "mockapi", LargeIcon = "apimock", Red = 128, Green = 145, Blue = 213)]
    public class ViewModel : DevelopWorkspace.Base.Model.ScriptBaseViewModel
    {
        System.Windows.Controls.ListView listView;
        DevelopWorkspace.Base.Utils.SimpleListView simpleListView = new DevelopWorkspace.Base.Utils.SimpleListView();
        List<EndPointInfo> endPointInfoList = new List<EndPointInfo>();
        HttpListener _listener = new HttpListener();
        List<Rule> rules = new List<Rule>();
        Levenshtein l = new Levenshtein();
        SimpleHTTPServer myServer;
        [MethodMeta(Name = "ON", Date = "2009-07-20", Description = "turn Mock Api Service ON", LargeIcon = "apiconect")]
        public void EventHandler1(object sender, RoutedEventArgs e)
        {
            try
            {
            
                myServer = new SimpleHTTPServer(@"C:\Users\xujingjiang\Source\Repos\developSupportToolls\developWorkspace\bin\Debug",8084);

                endPointInfoList.Where(endpoint => { return endpoint.IsNotKey; }).ToList().ForEach(endpoint =>
                {
                    _listener.Prefixes.Add("http://localhost:" + endpoint.EndPoint + "/");
                });
                _listener.Start();
                _listener.BeginGetContext(new AsyncCallback(GetContextCallback), null);

            }
            catch (Exception ex)
            {
                DevelopWorkspace.Base.Logger.WriteLine(ex.ToString(), Level.ERROR);
            }
        }
        [MethodMeta(Name = "OFF", Date = "2009-07-20", Description = "turn Mock Api Service OFF", LargeIcon = "disconect")]
        public void EventHandler2(object sender, RoutedEventArgs e)
        {
            try
            {
                myServer.Stop();
                _listener.Stop();
            }
            catch (Exception ex)
            {
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
                sb.Append(string.Format("HttpMethod: {0}\n", request.HttpMethod));

                sb.Append(string.Format("Uri:        {0}\n", request.Url.AbsoluteUri));

                sb.Append(string.Format("LocalPath:  {0}\n", request.Url.LocalPath));
                foreach (string key in request.QueryString.Keys)
                {
                    sb.Append(string.Format("Query:      {0} = {1}\n", key, request.QueryString[key]));
                }
                sb.Append("");
                string responseString = sb.ToString();
                DevelopWorkspace.Base.Logger.WriteLine(responseString);


                string requestString;
                using (Stream receiveStream = request.InputStream)
                {
                    using (StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8))
                    {
                        requestString = readStream.ReadToEnd();
                    }
                }

                DevelopWorkspace.Base.Logger.WriteLine(requestString);

                List<Rule> findedRules = rules.Where(rule => (rule.EndPoint.Equals(request.Url.LocalPath))).ToList();
                findedRules.ForEach(rule => { rule.Likeness = l.LD(requestString, rule.MatchString); });
                Rule findedRule = findedRules.Where(rule => rule.Likeness == findedRules.Min(r => r.Likeness)).FirstOrDefault();
                using (System.IO.Stream outputStream = response.OutputStream)
                {

                    if (findedRule == null)
                    {
                        DevelopWorkspace.Base.Logger.WriteLine("please set suitable response file", Level.WARNING);
                    }
                    else
                    {
                        findedRule.Dump();
                        response.StatusCode = (int)HttpStatusCode.OK;
                        response.ContentType = findedRule.ContentType;
                        response.ContentEncoding = Encoding.UTF8;
                        
                        string responseFilePath = findedRule.ResponseFile;
                        if (FileExist(ref responseFilePath))
                        {
                            responseString = System.IO.File.ReadAllText(responseFilePath, Encoding.UTF8);

                            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);


                            response.ContentLength64 = buffer.Length;
                            outputStream.Write(buffer, 0, buffer.Length);
                        }
                        else
                        {
                            DevelopWorkspace.Base.Logger.WriteLine(responseFilePath + " does not exists,please confirm setting", Level.WARNING);
                        }
                    }
                    outputStream.Close();
                }

                _listener.BeginGetContext(new AsyncCallback(GetContextCallback), null);
            }
            catch (Exception ex)
            {
                DevelopWorkspace.Base.Logger.WriteLine(ex.ToString());
            }
        }

        public bool FileExist(ref string filename)
        {
            if (Regex.IsMatch(filename, "^[a-z]:", RegexOptions.IgnoreCase))
            {
            }
            else
            {
                filename = System.IO.Path.Combine(DevelopWorkspace.Main.StartupSetting.instance.homeDir, filename);
            }
            if (File.Exists(filename))
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        public override UserControl getView(string strXaml)
        {
            StringReader strreader = new StringReader(strXaml);
            XmlTextReader xmlreader = new XmlTextReader(strreader);
            UserControl view = XamlReader.Load(xmlreader) as UserControl;
            listView = DevelopWorkspace.Base.Utils.WPF.FindLogicaChild<System.Windows.Controls.ListView>(view, "trvFamilies");
            endPointInfoList = new List<EndPointInfo>
            {
                new EndPointInfo{ EndPoint = "18002",  RulePath="rba-backend-item-api" },
                new EndPointInfo{ EndPoint = "18005", RulePath="rba-backend-report-api" },
                new EndPointInfo{ EndPoint = "18006", RulePath="rba-backend-image-api" }
            };
            simpleListView.CreateView(endPointInfoList);
            //listView.DataContext = endPointInfoList;
            //listView.SelectedIndex = 0;

            try
            {
                using (SQLiteConnection sqLiteConnection = new SQLiteConnection(@"Data Source = addins.db; Pooling = true; FailIfMissing = false"))
                {
                    sqLiteConnection.Open();
                    SQLiteCommand sqLiteCommand = new SQLiteCommand(sqLiteConnection)
                    {
                        CommandText = "SELECT * FROM EndPointRules"
                    };

                    using (DbDataReader rdr = sqLiteCommand.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            rules.Add(new Rule { ContentType = rdr["ContentType"].ToString(), EndPoint = rdr["EndPoint"].ToString(), MatchString = rdr["MatchString"].ToString(), ResponseFile = rdr["ResponseFile"].ToString(), Likeness = 0 });
                        }
                    }
                    sqLiteConnection.Close();
                }
            }
            catch (Exception ex)
            {
                DevelopWorkspace.Base.Logger.WriteLine(ex.ToString(), Level.ERROR);

            }

            return simpleListView;
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