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
using System.Windows.Shapes;
using System.Xml;
using System.Windows.Markup;
using WPFMediaKit;
using System.Windows.Forms;
using  System.Diagnostics;
using System.IO;
using Newtonsoft.Json;
using DevelopWorkspace.Base;
public class Script
{
    public static void Main(string[] args)
    {
        DevelopWorkspace.Base.Logger.WriteLine("Process called");
        string faceimage_filename ="";
        string face_collectionid ="";
	 
        string strXaml = args[0].ToString();
        StringReader strreader = new StringReader(strXaml);
        XmlTextReader xmlreader = new XmlTextReader(strreader);
        System.Windows.Window win = XamlReader.Load(xmlreader) as System.Windows.Window;

        System.Windows.Controls.ComboBox cb = DevelopWorkspace.Base.Utils.WPF.FindLogicaChild<System.Windows.Controls.ComboBox>(win, "cb");
                System.Windows.Controls.Image img = DevelopWorkspace.Base.Utils.WPF.FindLogicaChild<System.Windows.Controls.Image>(win, "img");
        WPFMediaKit.DirectShow.Controls.VideoCaptureElement vce = DevelopWorkspace.Base.Utils.WPF.FindLogicaChild<WPFMediaKit.DirectShow.Controls.VideoCaptureElement>(win, "vce");
        cb.ItemsSource = WPFMediaKit.DirectShow.Controls.MultimediaUtil.VideoInputNames;
        cb.SelectedIndex = 0;
        vce.VideoCaptureSource = (string)cb.SelectedItem;
        System.Windows.Controls.Button btnCapurure = DevelopWorkspace.Base.Utils.WPF.FindLogicaChild<System.Windows.Controls.Button>(win, "btnCapture");
        System.Windows.Controls.Button btnSelect = DevelopWorkspace.Base.Utils.WPF.FindLogicaChild<System.Windows.Controls.Button>(win, "btnSelect");
        System.Windows.Controls.Button btnRekognition = DevelopWorkspace.Base.Utils.WPF.FindLogicaChild<System.Windows.Controls.Button>(win, "btnRekognition");
        
        
        
        System.Windows.Controls.ListView lsvFaces = DevelopWorkspace.Base.Utils.WPF.FindLogicaChild<System.Windows.Controls.ListView>(win, "lsvFaces");
        System.Windows.Controls.ListView lstCollection = DevelopWorkspace.Base.Utils.WPF.FindLogicaChild<System.Windows.Controls.ListView>(win, "lstCollection");
/*
        lsvFaces.ItemsSource = new[] {
		new {faceid = "9877-12344-8900", externalid = 31,affnity=99.12 },
		new {faceid = "9877-12344-8900", externalid = 31,affnity=99.12 },
		new {faceid = "9877-12344-8900", externalid = 31,affnity=99.12 },
		new {faceid = "9877-12344-8900", externalid = 31,affnity=99.12 },
		new {faceid = "9877-12344-8900", externalid = 31,affnity=99.12 }

        };
        */
        lstCollection.ItemsSource = new[] {
		new {CollectionId = "AiProjectRekognitionFaceCollectionXujingjiang", FacesNum = 31 },
		new {CollectionId = "exampleCollection", FacesNum = 31 },
		new {CollectionId = "exampleCollection2", FacesNum = 31 },
		new {CollectionId = "rekognition-pattern-1", FacesNum = 31 }
        };
        lstCollection.SelectedIndex = 0;

         btnSelect.Click += (obj, subargs) =>
        {
             OpenFileDialog openFileDialog = new OpenFileDialog();
             openFileDialog.Title = "select your image for Rekognition";
             openFileDialog.Filter = "jpg|*.jpg|jpeg|*.jpeg";
             openFileDialog.FileName = string.Empty;
             openFileDialog.FilterIndex = 1;
             openFileDialog.RestoreDirectory = true;
             openFileDialog.DefaultExt = "jpg";
             DialogResult result = openFileDialog.ShowDialog();
             if (result == System.Windows.Forms.DialogResult.Cancel)
             {
                 return;
             }
             string filename = openFileDialog.FileName;       
             string fullpath = @"C:\Users\xujingjiang\Source\Repos\developworkspace\developWorkspace\developWorkspace\bin\Debug\";
            try{
            	
           //BitmapImage image = new BitmapImage(new Uri(filename, UriKind.Relative));
            //img.Source = bi;
            //img.Source =new BitmapImage(new Uri(@"C:\Users\xujingjiang\Source\Repos\developworkspace\developWorkspace\developWorkspace\bin\Debug\2017311211026pic.jpg"));
            img.Source =new BitmapImage(new Uri(filename));
                faceimage_filename = filename;
            }
            catch(Exception ex){
    			DevelopWorkspace.Base.Logger.WriteLine(ex.ToString());
            }
            
             
             
         	
        };
         
        btnRekognition.Click += (obj, subargs) =>
        {
           try{
        		lsvFaces.ItemsSource = null;
                	dynamic selected = lstCollection.SelectedItem;
        		//string s3copyCommand="aws s3 cp {0} s3://AiProjectRekognitionBucket/ --region us-east-1 >result.txt";
        		//string searchCommand=@"aws rekognition search-faces-by-image --image ""S3Object={Bucket=""AiProjectRekognitionBucket"",Name=""{key}""}"" --max-faces 10 --collection-id ""{collection}"" --region us-east-1 >>result.txt  2>&1";
        		string s3copyCommand="aws s3 cp {0} s3://AiProjectRekognitionBucket/ --region us-east-1 ";
        		string searchCommand=@"aws rekognition search-faces-by-image --image ""S3Object={Bucket=""AiProjectRekognitionBucket"",Name=""{key}""}"" --max-faces 20 --collection-id ""{collection}"" --region us-east-1 ";
        		string realS3copyCommand = string.Format(s3copyCommand,faceimage_filename);
        		DevelopWorkspace.Base.Logger.WriteLine(realS3copyCommand);
        		
        		string[] paths = faceimage_filename.Split(new char[1] {'\\'}); 
        		string realSearchCommand = searchCommand.Replace("{key}",paths[paths.GetLength(0)-1]);
        		realSearchCommand = realSearchCommand.Replace("{collection}",(string)selected.CollectionId);
        		DevelopWorkspace.Base.Logger.WriteLine(realSearchCommand);
        		string json ="";
        		AwsCommand(realS3copyCommand);
        		json =AwsCommand(realSearchCommand);
        		//DevelopWorkspace.Base.Logger.WriteLine(json);
        		//获取该员工同事所有姓名（读取json数组） 
        		//dynamic jo = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);;  
        		//DevelopWorkspace.Base.Logger.WriteLine(DevelopWorkspace.Base.Dump.ToDump(jo));  
        		//var faces=from face in jo["FaceMatches"] select face;  
        		//foreach (var face in faces)  
                     //   DevelopWorkspace.Base.Logger.WriteLine(face.ToString());  
        		//System.Diagnostics.Process.Start("cmd.exe /C " + realSearchCommand);
        		lsvFaces.ItemsSource = null;
        		Dictionary<string, object> htmlAttributes = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
        		var faces = htmlAttributes["FaceMatches"] as Newtonsoft.Json.Linq.JArray;
        		List<object> lstFaceInfo = new List<object>();
        		foreach(var face in faces){
        			lstFaceInfo.Add( new {faceid = face["Face"]["FaceId"].ToString(), externalid = face["Face"]["ExternalImageId"],affnity=face["Face"]["Confidence"].ToString() });
        		}
        		lsvFaces.ItemsSource = lstFaceInfo;
            }
            catch(Exception ex){
                DevelopWorkspace.Base.Logger.WriteLine(ex.ToString());
            }
        };

        btnCapurure.Click += (obj, subargs) =>
        {
            RenderTargetBitmap bmp = new RenderTargetBitmap((int)vce.ActualWidth,(int)vce.ActualHeight,96,96,PixelFormats.Default);
            vce.Measure(vce.RenderSize);
            vce.Arrange(new Rect(vce.RenderSize));
            bmp.Render(vce);
            BitmapEncoder encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bmp));

    		string fullpath = @"C:\Users\coxujingjiang.NNETCO\Desktop\workshop\aws\評価データ\";
            string now = DateTime.Now.Year + "" + DateTime.Now.Month + "" + DateTime.Now.Day + "" +
            DateTime.Now.Hour + "" + DateTime.Now.Minute + "" + DateTime.Now.Second;
            string filename = now + "pic.jpg";
            FileStream fsstream = new FileStream(fullpath + filename, FileMode.Create);
            encoder.Save(fsstream);
            fsstream.Close();
           
            faceimage_filename = fullpath + filename;
            
           try{
                	
               //BitmapImage image = new BitmapImage(new Uri(filename, UriKind.Relative));
                //img.Source = bi;
                //img.Source =new BitmapImage(new Uri(@"C:\Users\xujingjiang\Source\Repos\developworkspace\developWorkspace\developWorkspace\bin\Debug\2017311211026pic.jpg"));
                img.Source =new BitmapImage(new Uri(fullpath + filename));
            }
            catch(Exception ex){
			     DevelopWorkspace.Base.Logger.WriteLine(ex.ToString());
            }
        };
        win.Show();
    }
    public static string AwsCommand(string cliCommand)
    {
	    ProcessStartInfo start = new ProcessStartInfo ("cmd.exe" );
        start.FileName = "cmd.exe" ;   // 设定程序名
        start.Arguments = " /c " + cliCommand;
        start.CreateNoWindow = true ;  // 不显示dos 窗口
        start.UseShellExecute = false ;    // 是否指定操作系统外壳进程启动程序，没有这行，调试时编译器会通知你加上的...orz
        start.RedirectStandardInput = true ;
        start.RedirectStandardOutput = true ;  // 重新定向标准输入、输出流

        start.RedirectStandardError = true ;  // 重新定向标准输入、输出流

        Process p = Process .Start(start);
        StreamReader reader = p.StandardOutput;  // 截取输出流
        StreamReader readerError = p.StandardError;  // 截取输出流
		string json ="";
        string line = reader.ReadLine();     // 每次读一行
        while (!reader.EndOfStream)  // 不为空则读取
        {
            DevelopWorkspace.Base.Logger.WriteLine(line);
            if(json != "") json = json + line;
            if(json == "" && line[0] == '{') json = json + line;
            line = reader.ReadLine();
        }
        if(json != "") json = json + line;

        line = readerError.ReadLine();     // 每次读一行
        while (!readerError.EndOfStream)  // 不为空则读取
        {
            DevelopWorkspace.Base.Logger.WriteLine(line);
            line = readerError.ReadLine();
        }
        p.WaitForExit();    // 等待程序执行完退出进程
        p.Close();      // 关闭进程
        reader.Close(); // 关闭流

        return json;
    }

}
