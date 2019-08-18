using DevelopWorkspace.Main.View;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Description;
using System.ServiceModel.Web;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;
using System.Xml.Linq;
/// <summary>
/// TODO 将来把工具的一些服务对外公开提供通过IE等浏览器以及脚本访问代码做成等服务
/// </summary>
namespace DevelopWorkspace.Main.RestfulService
{
    public class Employee
    {
        public string Dept { get; set; }
        public string Name { get; set; }
        public int Salary { get; set; }

    }
    [DataContract(Namespace = "YourNamespaceHere")]
    public class DataRequest
    {
        [DataMember]
        public string ID { get; set; }
        [DataMember]
        public string Data { get; set; }
    }
    public static class Routing
    {
        public const string GetClientRoute = "/Client/{id}";
    }
    [ServiceContract(Name = "DevelopWorkspaceServices")]
    public interface IDevelopWorkspaceServices
    {
        //这个函数演示如何处理结果是xml格式的场合
        [OperationContract]
        [WebGet(UriTemplate = Routing.GetClientRoute, BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Xml)]
        [Description("获取所有员工列表")]
        XmlElement GetClientNameById(string Id);

        [OperationContract()]
        [Description("这个函数演示如何下载文件")]
        [WebGet(UriTemplate = "/Download/{id}", BodyStyle = WebMessageBodyStyle.Bare)]
        Stream GetCsvFile(string id);

        [OperationContract()]
        [WebGet(UriTemplate = "/index", BodyStyle = WebMessageBodyStyle.Bare)]
        Stream index();

        [OperationContract()]
        [WebGet(UriTemplate = "/resource/{resourceid}", BodyStyle = WebMessageBodyStyle.Bare)]
        Stream resource(string resourceid);


        //这个函数演示如何处理结果是xml格式的场合
        [Description("这个函数演示如何处理结果是xml格式的场合")]
        [WebGet(UriTemplate = "GetAllTablesSchema", ResponseFormat = WebMessageFormat.Xml)]
        IEnumerable<TableInfo> GetAllTablesSchema();
        //其他成员

        //这个函数演示如何处理请求入力是xml格式的场合
        [OperationContract]
        [Description("这个函数演示如何处理请求入力是xml格式的场合")]
        [WebInvoke(Method = "POST", UriTemplate = "GetData", RequestFormat = WebMessageFormat.Xml, BodyStyle = WebMessageBodyStyle.Bare)]
        string GetData(XElement parameter);

        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "GetJsonData", BodyStyle = WebMessageBodyStyle.Bare)]
        string GetJsonData(string jsonString);

        [OperationContract]
        [Description("这个函数演示如何上传文件")]
        [WebInvoke(Method = "POST", UriTemplate = "FileUpload/{fileName}")]
        void FileUpload(string fileName, Stream fileStream);
    }
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Single, IncludeExceptionDetailInFaults = true)]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class DevelopWorkspaceServices : IDevelopWorkspaceServices
    {
        public XmlElement GetClientNameById(string Id)
        {
            //Random r = new Random();
            string ReturnString = "<tablename>user</tablename><remark>user</remark>";
            //for (int i = 0; i < Convert.ToUInt32(Id); i++)
            //    ReturnString += char.ConvertFromUtf32(r.Next(65, 85));
            string xmlFilePath = @"C:\Users\Public\TableInfo.xml";
            XmlDocument doc = new XmlDocument();
            doc.Load(xmlFilePath);
            return doc.DocumentElement;

        }
        //这个函数演示如何处理结果文件流的场合(下载)
        public Stream GetCsvFile(string id)
        {
            string s = "123,456";
            WebOperationContext.Current.OutgoingResponse.ContentType = "text/csv";
            WebOperationContext.Current.OutgoingResponse.Headers["Content-Disposition"] = "attachment; filename=\"file1.csv\"";
            return GenerateStreamFromString(s);
        }

        public Stream index() {
            Base.Logger.WriteLine("index");
            //Base.Logger.WriteLine(string.Format("resource:{0}", resource));
            //string result = "<a href='someLingk' >Some link</a>";
            string result = System.IO.File.ReadAllText(@"SimpleRestServer\pages\index.html");
            byte[] resultBytes = Encoding.UTF8.GetBytes(result);
            WebOperationContext.Current.OutgoingResponse.ContentType = "text/html";
            return new MemoryStream(resultBytes);
        }
        public Stream resource(string resourceid)
        {
            Base.Logger.WriteLine(string.Format("resourceid:{0}", resourceid));
            string result = System.IO.File.ReadAllText(@"SimpleRestServer\resources\js\jquery.min.js");
            byte[] resultBytes = Encoding.UTF8.GetBytes(result);
            WebOperationContext.Current.OutgoingResponse.ContentType = "text/javascript";
            return new MemoryStream(resultBytes);

        }

        //这个函数演示如何处理结果是xml格式的场合
        public IEnumerable<TableInfo> GetAllTablesSchema()
        {
            return DataExcelUtilView.ALL_TABLES;
        }

        public string GetData(XElement parameter)
        {
            //Do stuff
            return "your data here";
        }

        public string GetJsonData(string jsonString)
        {
            //DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T));
            //MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(jsonString));
            //T obj = (T)ser.ReadObject(ms);
            //return obj;            //Do stuff
            return "your data here";
        }
        public void FileUpload(string fileName, Stream fileStream)
        {
            FileStream fileToupload = new FileStream("D:\\FileUpload\\" + fileName, FileMode.Create);

            byte[] bytearray = new byte[10000];
            int bytesRead, totalBytesRead = 0;
            do
            {
                bytesRead = fileStream.Read(bytearray, 0, bytearray.Length);
                totalBytesRead += bytesRead;
            } while (bytesRead > 0);

            fileToupload.Write(bytearray, 0, bytearray.Length);
            fileToupload.Close();
            fileToupload.Dispose();

        }
        public Stream GenerateStreamFromString(string s)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }

    public class RestfulService
    {
        static AutoResetEvent autoEvent = new AutoResetEvent(false);
        public static void Start(string[] args)
        {
            /**
             * 当前用户是管理员的时候，直接启动应用程序
             * 如果不是管理员，则使用启动对象启动程序，以确保使用管理员身份运行
             */
            //获得当前登录的Windows用户标示
            System.Security.Principal.WindowsIdentity identity = System.Security.Principal.WindowsIdentity.GetCurrent();
            System.Security.Principal.WindowsPrincipal principal = new System.Security.Principal.WindowsPrincipal(identity);
            //判断当前登录用户是否为管理员
            if (principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator))
            {
                Thread thread = new Thread(new ThreadStart(ThreadMethod));
                thread.Start();
            }
            else {
                MessageBoxResult result = MessageBox.Show(Application.Current.MainWindow, "这个功能需要一管理者身份启动才可以使用");
            }
        }
        static void ThreadMethod()
        {
            WebServiceHost _serviceHost = null;
            try
            {
                DevelopWorkspaceServices DemoServices = new DevelopWorkspaceServices();
                WebHttpBinding binding = new WebHttpBinding();
                WebHttpBehavior behavior = new WebHttpBehavior();
                behavior.HelpEnabled = true;
                _serviceHost = new WebServiceHost(DemoServices, new Uri("http://localhost:54321/DevelopWorkspaceServices"));
                ServiceEndpoint endpoint = _serviceHost.AddServiceEndpoint(typeof(IDevelopWorkspaceServices), binding, "");
                endpoint.Behaviors.Add(behavior);
                _serviceHost.Open();
                // Wait for work method to signal.
                autoEvent.WaitOne();
                //TODO 对启动的service关闭的话，需要把同期对象复原，这个需要放到MainWindow？
                //autoEvent.Set();

            }
            catch (Exception ex)
            {
                if (_serviceHost != null)
                {
                    _serviceHost.Close();
                }
            }
        }
        /// <summary>
        /// 测试POST
        /// </summary>
        static void test()
        {
            ASCIIEncoding encoding = new ASCIIEncoding();
            string SampleXml = "<DataRequest xmlns=\"YourNamespaceHere\">" +
                                            "<ID>" +
                                            "10" +
                                            "</ID>" +
                                            "<Data>" +
                                            "20" +
                                            "</Data>" +
                                        "</DataRequest>";

            string postData = SampleXml.ToString();
            byte[] data = encoding.GetBytes(postData);

            string url = "http://localhost:8090/DEMOService/GetData";

            string strResult = string.Empty;

            // declare httpwebrequet wrt url defined above
            HttpWebRequest webrequest = (HttpWebRequest)WebRequest.Create(url);
            // set method as post
            webrequest.Method = "POST";
            // set content type
            webrequest.ContentType = "application/xml";
            // set content length
            webrequest.ContentLength = data.Length;
            // get stream data out of webrequest object
            Stream newStream = webrequest.GetRequestStream();
            newStream.Write(data, 0, data.Length);
            newStream.Close();

            //Gets the response
            WebResponse response = webrequest.GetResponse();
            //Writes the Response
            Stream responseStream = response.GetResponseStream();

            StreamReader sr = new StreamReader(responseStream);
            string s = sr.ReadToEnd();


        }

    }
}
