using System;
using System.Drawing;
using System.IO;
using System.Data.SQLite;
using System.Data;
using Microsoft.CSharp;
using System.Collections.Generic;
using DevelopWorkspace.Base;
using Excel = Microsoft.Office.Interop.Excel;
using System.Threading;
using System.Threading.Tasks;
using AutoIt;
//css_reference AutoItX3.Assembly.dll
public class Script
{
	public static void Main(string[] args){
		autoDoItOnCoreDB();
	}
	public static void autoDoItOnCoreDB(){
		//各自の環境に合わせるもの...
        string username = "os-xujiangji01";
        string password = "bos-c202001";
        string local_pc_user = "os-jiangjiang.xu";
		string ttermpro = @"C:\xujingjiang\tools\teraterm-4.105\ttermpro.exe";
		string filezilla = @"C:\xujingjiang\tools\FileZilla-3.47.2.1\filezilla.exe";
		string dbstring = "CORESTYL_USER_QA/stg_corestyl_user@SV_CORESTYL_QA";
		string target_host = "stg-bstylifebo101z.stg.jp.local";
		int hostkbn = 0;
		int processkbn = 0;
		
		autoDoIt(username,password,local_pc_user,ttermpro,filezilla,hostkbn,target_host,dbstring)
	}	
	public static void autoDoItOnFrontDB(){
		//各自の環境に合わせるもの...
        string username = "os-xujiangji01";
        string password = "bos-c202001";
        string local_pc_user = "os-jiangjiang.xu";
		string ttermpro = @"C:\xujingjiang\tools\teraterm-4.105\ttermpro.exe";
		string filezilla = @"C:\xujingjiang\tools\FileZilla-3.47.2.1\filezilla.exe";
		string dbstring = "fstyluser_qa/stg_fstyluser@100.67.71.39:2050/sv_fstyl_qa";
		string target_host = "stg-bstylife101z.stg.jp.local";
		int hostkbn = 1;
		int processkbn = 0;
		autoDoIt(username,password,local_pc_user,ttermpro,filezilla,hostkbn,target_host,dbstring)
	}	
	
	// hostkbn = 0:core 1:front
	// processkbn = 0:table data download 1:teraterm login only

    public static void autoDoIt(string username,string password,string local_pc_user,string ttermpro,string filezilla,int hostkbn,string target_host,string dbstring,int processkbn = 0)
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
		//export data from database
		AutoItX.Send("sqlplus -s " + qa_dbstring + " << EOF >/dev/null{ENTER}");
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
		AutoItX.Send("spool Export-F_SHKSEIDTL.csv   {ENTER}");
		AutoItX.Send("select * from F_SHKSEIDTL where KYAKUNO=70949351;   {ENTER}");
		AutoItX.Send("spool off;   {ENTER}");
		AutoItX.Send("exit;   {ENTER}");
		AutoItX.Send("EOF{ENTER}");
		
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
		AutoItX.ControlSend("sftp://" + username + "@stg-loginjpe1101z.stg.jp.local", "", "Edit5", @"D:\Users\" + local_pc_user + "\Downloads\");
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
}