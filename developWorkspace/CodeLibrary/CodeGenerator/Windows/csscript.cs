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
using DevelopWorkspace.Base;
public class Script
{
    public static void Main(string[] args)
    {
        DevelopWorkspace.Base.Logger.WriteLine("Process called");

        DevelopWorkspace.Base.Logger.WriteLine(args[0].ToString());
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
        System.Windows.Controls.ListView lsvFaces = DevelopWorkspace.Base.Utils.WPF.FindLogicaChild<System.Windows.Controls.ListView>(win, "lsvFaces");
        System.Windows.Controls.ListView lstCollection = DevelopWorkspace.Base.Utils.WPF.FindLogicaChild<System.Windows.Controls.ListView>(win, "lstCollection");

        lsvFaces.ItemsSource = new[] {
		new {faceid = "9877-12344-8900", externalid = 31,affnity=99.12 },
		new {faceid = "9877-12344-8900", externalid = 31,affnity=99.12 },
		new {faceid = "9877-12344-8900", externalid = 31,affnity=99.12 },
		new {faceid = "9877-12344-8900", externalid = 31,affnity=99.12 },
		new {faceid = "9877-12344-8900", externalid = 31,affnity=99.12 }

        };
        lstCollection.ItemsSource = new[] {
		new {CollectionId = "RekognitionAsserrt", FacesNum = 31 },
		new {CollectionId = "RekognitionAsserrt", FacesNum = 31 },
		new {CollectionId = "RekognitionAsserrt", FacesNum = 31 },
		new {CollectionId = "RekognitionAsserrt", FacesNum = 31 },
		new {CollectionId = "RekognitionAsserrt", FacesNum = 31 },
		new {CollectionId = "RekognitionAsserrt", FacesNum = 31 }
        };

         btnSelect.Click += (obj, subargs) =>
        {
             OpenFileDialog openFileDialog = new OpenFileDialog();
             openFileDialog.Title = "选择文件";
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
            }
            catch(Exception ex){
			DevelopWorkspace.Base.Logger.WriteLine(ex.ToString());
            }
            
             
             
         	
        };
        btnCapurure.Click += (obj, subargs) =>
        {
            //DevelopWorkspace.Base.Logger.WriteLine("hhhhhhhh");
            //dynamic selected = lsvFaces.SelectedItem;

            //if (selected != null){
		//	DevelopWorkspace.Base.Logger.WriteLine(selected.faceid);
           // }
                       RenderTargetBitmap bmp = new RenderTargetBitmap((int)vce.ActualWidth,(int)vce.ActualHeight,96,96,PixelFormats.Default);
            vce.Measure(vce.RenderSize);
            vce.Arrange(new Rect(vce.RenderSize));
            bmp.Render(vce);


            BitmapEncoder encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bmp));

		string fullpath = @"C:\Users\xujingjiang\Source\Repos\developworkspace\developWorkspace\developWorkspace\bin\Debug\";
            string now = DateTime.Now.Year + "" + DateTime.Now.Month + "" + DateTime.Now.Day + "" +
                DateTime.Now.Hour + "" + DateTime.Now.Minute + "" + DateTime.Now.Second;
            string filename = now + "pic.jpg";
            FileStream fsstream = new FileStream(fullpath + filename, FileMode.Create);
            encoder.Save(fsstream);
            fsstream.Close();
           
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


}
