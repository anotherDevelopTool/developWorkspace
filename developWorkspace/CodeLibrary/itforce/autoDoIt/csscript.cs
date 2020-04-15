using System;
using Microsoft.CSharp;
using System.Drawing;
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
using AutoIt;
//css_reference AutoItX3.Assembly.dll
using System.Text;
using System.Text.RegularExpressions;

public class AutoItInfo:ViewModelBase{
	private string _service_type = "rba-bo-api";
    public string service_type
    {
      get { return _service_type; }
      set
      {
        if (_service_type != value)
        {
          _service_type = value;
          RaisePropertyChanged("service_type");
        }
      }
    }	
	private string _environment_name = "STG";
    public string environment_name
    {
      get { return _environment_name; }
      set
      {
        if (_environment_name != value)
        {
          _environment_name = value;
          RaisePropertyChanged("environment_name");
        }
      }
    }	
    private string _username = "os-xujiangji01";
    public string username
    {
      get { return _username; }
      set
      {
        if (_username != value)
        {
          _username = value;
          RaisePropertyChanged("username");
        }
      }
    }	
	private string _password = "bos-c202001";
    public string password
    {
      get { return _password; }
      set
      {
        if (_password != value)
        {
          _password = value;
          RaisePropertyChanged("password");
        }
      }
    }	
	private string _local_pc_user = "os-jiangjiang.xu";
    public string local_pc_user
    {
      get { return _local_pc_user; }
      set
      {
        if (_local_pc_user != value)
        {
          _local_pc_user = value;
          RaisePropertyChanged("local_pc_user");
        }
      }
    }		
	private string _bastion_host = "stg-loginjpe1101z.stg.jp.local";
    public string bastion_host
    {
      get { return _bastion_host; }
      set
      {
        if (_bastion_host != value)
        {
          _bastion_host = value;
          RaisePropertyChanged("bastion_host");
        }
      }
    }		
	private string _ttermpro = @"C:\xujingjiang\tools\teraterm-4.105\ttermpro.exe";
    public string ttermpro
    {
      get { return _ttermpro; }
      set
      {
        if (_ttermpro != value)
        {
          _ttermpro = value;
          RaisePropertyChanged("ttermpro");
        }
      }
    }		
	private string _filezilla = @"C:\xujingjiang\tools\FileZilla-3.47.2.1\filezilla.exe";
    public string filezilla
    {
      get { return _filezilla; }
      set
      {
        if (_filezilla != value)
        {
          _filezilla = value;
          RaisePropertyChanged("filezilla");
        }
      }
    }	
	private string _dbstring = "CORESTYL_USER_QA/stg_corestyl_user@SV_CORESTYL_QA";
    public string dbstring
    {
      get { return _dbstring; }
      set
      {
        if (_dbstring != value)
        {
          _dbstring = value;
          RaisePropertyChanged("dbstring");
        }
      }
    }	
	private string _target_host = "stg-bstylifebo101z.stg.jp.local";
    public string target_host
    {
      get { return _target_host; }
      set
      {
        if (_target_host != value)
        {
          _target_host = value;
          RaisePropertyChanged("target_host");
        }
      }
    }	
	private int _hostkbn = 0;
    public int hostkbn
    {
      get { return _hostkbn; }
      set
      {
        if (_hostkbn != value)
        {
          _hostkbn = value;
          RaisePropertyChanged("hostkbn");
        }
      }
    }	
	private int _processkbn = 1;
    public int processkbn
    {
      get { return _processkbn; }
      set
      {
        if (_processkbn != value)
        {
          _processkbn = value;
          RaisePropertyChanged("processkbn");
        }
      }
    }	
}
public class AutoItSetting{
	public List<AutoItInfo> setting;
}
public class Script
{
    //https://stackoverflow.com/questions/248362/how-do-i-build-a-datatemplate-in-c-sharp-code
    //TODO 面向Addin基类化
    [AddinMeta(Name = "autoDoIt", Date = "2009-07-20", Description = "autoDoIt utility",LargeIcon = "teraterm",Red =128,Green=145,Blue=213)]
    public class ViewModel : DevelopWorkspace.Base.Model.ScriptBaseViewModel
    {
        System.Windows.Controls.ListView listView;
        ICSharpCode.AvalonEdit.Edi.EdiTextEditor sqlSource;
        [MethodMeta(Name = "DB取得", Date = "2009-07-20", Description = "指定したSQL文の出力結果をExportして、ローカルに落とす", LargeIcon = "Export")]
        public void EventHandler1(object sender, RoutedEventArgs e)
        {
            try{
                dynamic autoItInfo = listView.SelectedItem;
                DevelopWorkspace.Base.Logger.WriteLine(autoItInfo.service_type);
                autoDoIt(autoItInfo.username,autoItInfo.password,autoItInfo.local_pc_user,autoItInfo.ttermpro,autoItInfo.filezilla,autoItInfo.hostkbn,autoItInfo.target_host,autoItInfo.dbstring,autoItInfo.processkbn,sqlSource.Text);

            }
            catch(Exception ex){
                DevelopWorkspace.Base.Logger.WriteLine(ex.ToString());
            }
            
        }
        [MethodMeta(Name = "Teraterm登録", Date = "2009-07-20", Description = "Teraterm登録", LargeIcon = "teraterm")]
        public void EventHandler2(object sender, RoutedEventArgs e)
        {
            try{
                dynamic autoItInfo = listView.SelectedItem;
                autoDoIt(autoItInfo.username,autoItInfo.password,autoItInfo.local_pc_user,autoItInfo.ttermpro,autoItInfo.filezilla,autoItInfo.hostkbn,autoItInfo.target_host,autoItInfo.dbstring,autoItInfo.processkbn,"");

            }
            catch(Exception ex){
                DevelopWorkspace.Base.Logger.WriteLine(ex.ToString());
            }
            
        }

		// hostkbn = 0:core 1:front
		// processkbn = 0:table data download 1:teraterm login only

		public void autoDoIt(string username,string password,string local_pc_user,string ttermpro,string filezilla,int hostkbn,string target_host,string dbstring,int processkbn,string sqltext)
		{
			
			AutoItX.Run(ttermpro,"");
			
			//踏み台サーバーログイン
			AutoItX.WinWaitActive("Tera Term: New connection");
			AutoItX.ControlSend("Tera Term: New connection", "", "Edit1", "stg-loginjpe1101z.stg.jp.local");
			AutoItX.ControlClick("Tera Term: New connection", "", "Button5");
			
			AutoItX.WinWaitActive("SSH Authentication");
			AutoItX.ControlSend("SSH Authentication", "", "Edit1", username);
			AutoItX.ControlSend("SSH Authentication", "", "Edit2", password);
			AutoItX.ControlClick("SSH Authentication", "", "Button13");
			
			AutoItX.WinWait("stg-loginjpe1101z.stg.jp.local");
			
			AutoItX.Sleep(1000);
			AutoItX.Send("cd /tmp");
			AutoItX.Send("{ENTER}");
			AutoItX.Send("rm -rf " + username);
			AutoItX.Send("{ENTER}");
			AutoItX.Send("mkdir " + username);
			AutoItX.Send("{ENTER}");
			AutoItX.Send("cd " + username);
			AutoItX.Send("{ENTER}");
			
			
			//STGなどのホストにログイン
			AutoItX.Send("ssh " + target_host);
			AutoItX.Send("{ENTER}");
			AutoItX.Sleep(1000);
			AutoItX.Send(password);
			AutoItX.Send("{ENTER}");
			AutoItX.Sleep(100);
			
			// teratermログイン後、終了、制御をユーザーに移譲する
			if(processkbn == 1 ){
				return;
			}
			
			// CORE
			if(hostkbn == 0 ){
				//ユーザー切り替え
				AutoItX.Send("sudo su syfuser");;
				AutoItX.Send("{ENTER}");
				AutoItX.Sleep(1000);
				AutoItX.Send(password);
				AutoItX.Send("{ENTER}");
				AutoItX.Sleep(100);
				
				//Oracleデータベース環境変数を有効化
				AutoItX.Send("source /usr/local/oracle/env/login.rc");
				AutoItX.Send("{ENTER}");
			}
			else{
				AutoItX.Send("csh");;
				AutoItX.Send("{ENTER}");
				AutoItX.Send("source /usr/local/oracle/env/tora111.rc");;
				AutoItX.Send("{ENTER}");
			}
			//tmpに切り替え
			AutoItX.Send("cd /tmp");
			AutoItX.Send("{ENTER}");
			AutoItX.Send("rm -rf " + username);
			AutoItX.Send("{ENTER}");
			AutoItX.Send("mkdir " + username);
			AutoItX.Send("{ENTER}");
			AutoItX.Send("cd " + username);
			AutoItX.Send("{ENTER}");
			
			using (System.IO.StringReader reader = new System.IO.StringReader(sqltext)) {
			    string sql = reader.ReadLine();
			}		

			using (System.IO.StringReader reader = new System.IO.StringReader(sqltext)) {
                string sql; 
                while ((sql = reader.ReadLine()) != null) 
                { 
                	Regex regex = new Regex(@"\bfrom\s+[a-zA-Z0-9_\.\-]{1,}\b", RegexOptions.IgnoreCase);
                    MatchCollection matchs = regex.Matches(sql);
                    if(matchs.Count > 0){
                    	string csvfilename = matchs[0].Value.Substring(5) + ".csv";
                    	//DevelopWorkspace.Base.Logger.WriteLine(csvfilename);
                    	
						//export data from database
						AutoItX.Send("sqlplus -s " + dbstring + " << EOF >/dev/null{ENTER}");
						AutoItX.Send("set term off   {ENTER}");
						AutoItX.Send("set echo off   {ENTER}");
						AutoItX.Send("set underline off   {ENTER}");
						AutoItX.Send("set colsep '\",\"' {ENTER}");
						AutoItX.Send("set linesize 32767   {ENTER}");
						AutoItX.Send("set pages 10000   {ENTER}");
						AutoItX.Send("set trimspool on   {ENTER}");
						AutoItX.Send("set trimout on   {ENTER}");
						AutoItX.Send("set feedback off   {ENTER}");
						AutoItX.Send("set heading on   {ENTER}");
						AutoItX.Send("set newpage 0   {ENTER}");
						AutoItX.Send("set headsep off   {ENTER}");
						AutoItX.Send("set termout off   {ENTER}");
						AutoItX.Send("set long 20000   {ENTER}");
						AutoItX.Send("spool " + csvfilename + "{ENTER}");
						AutoItX.Send(sql + "{ENTER}");
						AutoItX.Send("spool off;   {ENTER}");
						AutoItX.Send("exit;   {ENTER}");
						AutoItX.Send("EOF{ENTER}");                    	
                    	
                    }
                    //DevelopWorkspace.Base.Logger.WriteLine(DevelopWorkspace.Base.Dump.ToDump(matchs[0]));
    				
                } 
			}	
			
			AutoItX.Send("scp *.csv " + username + "@stg-loginjpe1101z.stg.jp.local:/tmp/" + username + "{ENTER}");
			AutoItX.Sleep(1000);
			AutoItX.Send(password);
			AutoItX.Send("{ENTER}");
			AutoItX.Sleep(100);
			
			
			AutoItX.Run(filezilla,"");
			AutoItX.WinWaitActive("FileZilla");
			AutoItX.ControlSend("FileZilla", "", "Edit1", "stg-loginjpe1101z.stg.jp.local");
			AutoItX.ControlSend("FileZilla", "", "Edit2", username);
			AutoItX.ControlSend("FileZilla", "", "Edit3", password);
			AutoItX.ControlSend("FileZilla", "", "Edit4", "22");
			AutoItX.ControlClick("FileZilla", "", "Button1");
			
			
			var dir = new DirectoryInfo(@"D:\Users\" + local_pc_user + @"\Downloads\");
			
			foreach (var file in dir.EnumerateFiles("*.csv")) {
				file.Delete();
			}		
			
			
			AutoItX.WinWaitActive("sftp://" + username + "@stg-loginjpe1101z.stg.jp.local");
			AutoItX.ControlClick("sftp://" + username + "@stg-loginjpe1101z.stg.jp.local", "", "Edit5");
			AutoItX.Sleep(1000);
			AutoItX.ControlSend("sftp://" + username + "@stg-loginjpe1101z.stg.jp.local", "", "Edit5", @"D:\Users\" + local_pc_user + @"\Downloads\");
			AutoItX.ControlSend("sftp://" + username + "@stg-loginjpe1101z.stg.jp.local", "", "Edit5", "{ENTER}");
			
			AutoItX.ControlClick("sftp://" + username + "@stg-loginjpe1101z.stg.jp.local", "", "Edit7");
			AutoItX.ControlSend("sftp://" + username + "@stg-loginjpe1101z.stg.jp.local", "", "Edit7", @"/tmp/" + username);
			AutoItX.ControlSend("sftp://" + username + "@stg-loginjpe1101z.stg.jp.local", "", "Edit7", "{ENTER}");
			AutoItX.Sleep(1000);
			AutoItX.ControlListView("sftp://" + username + "@stg-loginjpe1101z.stg.jp.local", "", "SysListView325", "SelectAll","1","1");
			AutoItX.ControlListView("sftp://" + username + "@stg-loginjpe1101z.stg.jp.local", "", "SysListView325", "DeSelect","0","0");

			// todo filezilla.exeをスクリーンの左上の端っこに事前に移動して置く必要があります
			Rectangle  rectParent = AutoItX.WinGetPos("sftp://" + username + "@stg-loginjpe1101z.stg.jp.local", "");
			Rectangle  rect = AutoItX.ControlGetPos("sftp://" + username + "@stg-loginjpe1101z.stg.jp.local", "", "SysListView325");
			AutoItX.MouseClick ("RIGHT",rectParent.X + rect.X+20,rectParent.Y + rect.Y+120);
			AutoItX.Sleep(1000);
			AutoItX.Send("{DOWN}");
			AutoItX.Send("{ENTER}");
					
			

		}        
        public override UserControl getView(string strXaml)
        {
            StringReader strreader = new StringReader(strXaml);
            XmlTextReader xmlreader = new XmlTextReader(strreader);
            UserControl view = XamlReader.Load(xmlreader) as UserControl;
            listView = DevelopWorkspace.Base.Utils.WPF.FindLogicaChild<System.Windows.Controls.ListView>(view, "trvFamilies");
            sqlSource = DevelopWorkspace.Base.Utils.WPF.FindLogicaChild<ICSharpCode.AvalonEdit.Edi.EdiTextEditor>(view, "sqlSource");
            sqlSource.Text = @"select * from F_WEBJUCHUHDR where  rownum < 1000;
select * from  F_JUCHUHDR where rownum < 1000;
select * from  F_JUCHUDTL where rownum < 1000;
select * from  F_KOKANJUCHU where rownum < 1000;
select * from  F_SHKSEIDTL where rownum < 1000;
";
			string json = File.ReadAllText(getResPathByExt("setting"), Encoding.UTF8);
			AutoItSetting autoItSetting = (AutoItSetting)JsonConvert.DeserializeObject(json, typeof(AutoItSetting));
			
            listView.DataContext = autoItSetting.setting;
            listView.SelectedIndex = 0;
            
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