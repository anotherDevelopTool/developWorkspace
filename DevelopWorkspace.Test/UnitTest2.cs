using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DevelopWorkspace.Base.Utils;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows;
using System.ComponentModel;
using DevelopWorkspace.Base;

namespace DevelopWorkspace.Test
{
    public class Rule : INotifyPropertyChanged
    {
        private bool _selected = false;
        public bool Selected
        {
            get { return _selected; }
            set
            {
                _selected = value;
                RaisePropertyChanged("Selected");
            }
        }
        public string ContentType { get; set; }
        public string EndPoint { get; set; }
        [SimpleListViewColumnMeta(Editablity =true)]
        public string MatchString { get; set; }
        //可以自定义显示列标题以及点击后的动作
        [SimpleListViewColumnMeta(ColumnDisplayName = "レスポンスファイル", TaskName = "OpenFile")]
        public string ResponseFile { get; set; }
        public int Likeness { get; set; }
        public virtual void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OpenFile(object selectedRow)
        {

        }
    }

    [TestClass]
    public class UnitTest2
    {




        [TestMethod]
        public void TestMethod1()
        {
            Rule rule1 = new Rule { ContentType = "application/json", EndPoint = "rba-backend-item-api1", MatchString = "{}", ResponseFile = "getOrder.json", Likeness = 0 };
            Rule rule2 = new Rule { ContentType = "application/xml", EndPoint = "rba-bo-api3", MatchString = "{}", ResponseFile = "getOrder.json", Likeness = 0, Selected = true };
            Rule rule3 = new Rule { ContentType = "application/ocetstream", EndPoint = "rba-backend-item-api6", MatchString = "{}", ResponseFile = "getOrder.json", Likeness = 0 };
            Rule rule4 = new Rule { ContentType = "application/text", EndPoint = "rba-backend-item-api4", MatchString = "{}", ResponseFile = "getOrder.json", Likeness = 0 };
            List<Rule> rules = new List<Rule> { rule1, rule2 };
            SimpleListView simpleListView = new SimpleListView();
            simpleListView.setStyle(120, 120, 255, 120, 12);
            simpleListView.inflateView(rules);

            Window dialog = new Window();
            Grid grid = new Grid();
            dialog.Content = grid;

            StackPanel parent = new StackPanel();
            grid.Children.Add(simpleListView);
            dialog.ShowDialog();




        }

    }
}
