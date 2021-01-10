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
using System.Reflection;
using System.Windows.Data;

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
    class ElementWidhtConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double totalWidth = double.Parse(value.ToString());
            return totalWidth - 5;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }
    [TestClass]
    public class SimpleListViewTest
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
        [TestMethod]
        public void TestMethod2()
        {
            Rule rule1 = new Rule { ContentType = "application/json", EndPoint = "rba-backend-item-api1", MatchString = "{}", ResponseFile = "getOrder.json", Likeness = 0 };
            Rule rule2 = new Rule { ContentType = "application/xml", EndPoint = "rba-bo-api3", MatchString = "{}", ResponseFile = "getOrder.json", Likeness = 0, Selected = true };
            Rule rule3 = new Rule { ContentType = "application/ocetstream", EndPoint = "rba-backend-item-api6", MatchString = "{}", ResponseFile = "getOrder.json", Likeness = 0 };
            Rule rule4 = new Rule { ContentType = "application/text", EndPoint = "rba-backend-item-api4", MatchString = "{}", ResponseFile = "getOrder.json", Likeness = 0 };
            List<Rule> rules = new List<Rule> { rule1, rule2 };

            SimpleListView simpleListView = new SimpleListView();
            simpleListView.FilteringOn = false;
            simpleListView.setStyle(120, 120, 255, 120, 12);
            simpleListView.CustomizeColumnDataDefFunc = (propertyAttribute, property, viewColumn, stackPanel) =>
            {
                if (property.PropertyType == typeof(Boolean))
                {
                    var checkBox = new CheckBox();
                    checkBox.FontSize = 12;
                    Binding textPropertyBinding = new Binding();
                    textPropertyBinding.Mode = BindingMode.TwoWay;
                    textPropertyBinding.Path = new PropertyPath(property.Name);
                    checkBox.SetBinding(CheckBox.IsCheckedProperty, textPropertyBinding);
                    checkBox.Checked += (object sender, RoutedEventArgs e) =>
                    {
                        //if (bMultiSelect) return;

                    };
                    stackPanel.Children.Add(checkBox);
                }
                else
                {
                    if (propertyAttribute == null || !propertyAttribute.Editablity)
                    {
                        var textBlock = new TextBlock();
                        textBlock.FontSize = 12;
                        textBlock.MinWidth = 120;
                        textBlock.SetBinding(TextBlock.TextProperty, new Binding()
                        {
                            Path = new PropertyPath(property.Name),
                        });
                        stackPanel.Children.Add(textBlock);
                    }
                    else
                    {
                        var textBox = new TextBox();
                        textBox.FontSize = 12;
                        textBox.MinWidth = 120;
                        textBox.SetBinding(TextBox.TextProperty, new Binding()
                        {
                            Path = new PropertyPath(property.Name),
                            Mode = BindingMode.TwoWay,
                            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                        });

                        textBox.SetBinding(TextBox.WidthProperty, new Binding()
                        {
                            Source = viewColumn,
                            Path = new PropertyPath("ActualWidth"),
                            Converter = new ElementWidhtConverter(),
                            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged

                        });
                        stackPanel.Children.Add(textBox);
                    }
                }
            };
            simpleListView.CustomizeFooterDefFunc = stackPanel =>
            {
                var okButton = new Button();
                okButton.Height = 30;
                okButton.Margin = new Thickness(5, 5, 5, 5);
                okButton.Content = "OK";
                okButton.Click += (object sender, RoutedEventArgs e) =>
                {

                }; 
                stackPanel.Children.Add(okButton);
            };

            simpleListView.inflateView(rules);


            Window dialog = new Window();
            Grid grid = new Grid();
            dialog.Content = grid;

            StackPanel parent = new StackPanel();
            grid.Children.Add(simpleListView);
            dialog.ShowDialog();
            var selectedItem = simpleListView.SelectedItem;
        }
    }
}
