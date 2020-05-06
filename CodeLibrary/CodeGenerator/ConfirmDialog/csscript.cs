using System;
using System.IO;
using NVelocity;
using NVelocity.App;
using NVelocity.Runtime;
using Microsoft.CSharp;
using System.Collections.Generic;
using DevelopWorkspace.Base.Utils;
using System.Linq;
using DevelopWorkspace.Base;
public class Script
{

    public static void Main(string[] args)
 {
     DevelopWorkspace.Base.Logger.WriteLine("Process called");


            string message = "選択してください";
            List<RowInfo> rowInfoList = new List<RowInfo>();
            string[] titles = new string[] { "user", "user-name1", "user-name2", "user-name3", "user-name4" };
            rowInfoList.Add(new RowInfo() { TitleList = titles, ColumnList = new string[] { "user1", "user11", "XXX","YYYY","SSS" }, Selected=true });
            rowInfoList.Add(new RowInfo() { TitleList = titles, ColumnList = new string[] { "user2", "user12", "XXX", "YYYY", "SSS" }, Selected = false });
            rowInfoList.Add(new RowInfo() { TitleList = titles, ColumnList = new string[] { "user3", "user13", "XXX", "YYYY", "SSS" }, Selected = false });
            rowInfoList.Add(new RowInfo() { TitleList = titles, ColumnList = new string[] { "user3", "user13", "XXX", "YYYY", "SSS" }, Selected = false });
            rowInfoList.Add(new RowInfo() { TitleList = titles, ColumnList = new string[] { "user3", "user13", "XXX", "YYYY", "SSS" }, Selected = false });
            rowInfoList.Add(new RowInfo() { TitleList = titles, ColumnList = new string[] { "user3", "user13", "XXX", "YYYY", "SSS" }, Selected = false });
            rowInfoList.Add(new RowInfo() { TitleList = titles, ColumnList = new string[] { "user3", "user13", "XXX", "YYYY", "SSS" }, Selected = false });
            rowInfoList.Add(new RowInfo() { TitleList = titles, ColumnList = new string[] { "user3", "user13", "XXX", "YYYY", "SSS" }, Selected = false });
            rowInfoList.Add(new RowInfo() { TitleList = titles, ColumnList = new string[] { "user3", "user13", "XXX", "YYYY", "SSS" }, Selected = false });
            rowInfoList.Add(new RowInfo() { TitleList = titles, ColumnList = new string[] { "user3", "user13", "XXX", "YYYY", "SSS" }, Selected = false });
            rowInfoList.Add(new RowInfo() { TitleList = titles, ColumnList = new string[] { "user3", "user13", "XXX", "YYYY", "SSS" }, Selected = false });
            rowInfoList.Add(new RowInfo() { TitleList = titles, ColumnList = new string[] { "user3", "user13", "XXX", "YYYY", "SSS" }, Selected = false });
            rowInfoList.Add(new RowInfo() { TitleList = titles, ColumnList = new string[] { "user3", "user13", "XXX", "YYYY", "SSS" }, Selected = false });
            rowInfoList.Add(new RowInfo() { TitleList = titles, ColumnList = new string[] { "user3", "user13", "XXX", "YYYY", "SSS" }, Selected = false });
            
            ConfirmDialog confirmDialog = new ConfirmDialog(message,rowInfoList,false);
            confirmDialog.ShowDialog();
		if(confirmDialog.ConfirmResult == eConfirmResult.OK){
			DevelopWorkspace.Base.Logger.WriteLine("OK");
			RowInfo selectedRowInfo = (from rowinfo in rowInfoList where rowinfo.Selected == true select rowinfo).FirstOrDefault();
			DevelopWorkspace.Base.Logger.WriteLine(selectedRowInfo.ColumnList[0]);
            }

 }
}