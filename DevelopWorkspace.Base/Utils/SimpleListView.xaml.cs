using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace DevelopWorkspace.Base.Utils
{
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
    /*
     * 使用方法说明
     * 
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


        Rule rule1 = new Rule { ContentType = "application/json", EndPoint = "rba-backend-item-api1", MatchString = "{}", ResponseFile = "getOrder.json", Likeness = 0 };
        Rule rule2 = new Rule { ContentType = "application/xml", EndPoint = "rba-bo-api3", MatchString = "{}", ResponseFile = "getOrder.json", Likeness = 0, Selected=true };
        Rule rule3 = new Rule { ContentType = "application/ocetstream", EndPoint = "rba-backend-item-api6", MatchString = "{}", ResponseFile = "getOrder.json", Likeness = 0 };
        Rule rule4 = new Rule { ContentType = "application/text", EndPoint = "rba-backend-item-api4", MatchString = "{}", ResponseFile = "getOrder.json", Likeness = 0 };
        List<Rule> rules = new List<Rule> { rule1, rule2 };
        SimpleListView simpleListView = new SimpleListView();
        simpleListView.setStyle(120, 120, 120, 120, 20);
        simpleListView.inflateView(rules);

        Window dialog = new Window();
        Grid grid = new Grid();
        dialog.Content = grid;

        StackPanel parent = new StackPanel();
        grid.Children.Add(simpleListView);
        dialog.ShowDialog();


    */
    public partial class SimpleListView : System.Windows.Controls.UserControl
    {
        public delegate void customizeColumnDataDefFunc(SimpleListViewColumnMeta propertyAttribute, PropertyInfo property,GridViewColumn viewColumn, StackPanel stackPanel);
        public delegate void customizeBlockUIFunc(StackPanel stackPanel);
        //数据列的创建代理，如果默认的不满足需求，那么可以定制这个代理
        public customizeColumnDataDefFunc CustomizeColumnDataDefFunc { get; set; }
        public customizeBlockUIFunc CustomizeHeaderDefFunc { get; set; }
        public customizeBlockUIFunc CustomizeFooterDefFunc { get; set; }

        SolidColorBrush backgroundColor = new SolidColorBrush(Color.FromArgb((byte)50, (byte)0, (byte)255, (byte)0));
        double fontSize = 12.0;
        class SortedColumn
        {
            public string ColumnName { get; set; }
            public GridViewColumn Column { get; set; }
            public ListSortDirection SortDirection { get; set; }
            public TextBox filterTextBox { get; set; }
        }
        CollectionViewSource view = new CollectionViewSource();
        bool bMultiSelect = false;
        Type tModelType;
        List<Action> actionList = new List<Action>();
        List<SortedColumn> columnList = new List<SortedColumn>();
        public eConfirmResult ConfirmResult { get; set; }

        bool filteringOn= true;
        public bool FilteringOn {
            get { return filteringOn; }
            set
            {
                filteringOn = value;
            }
        }
        public int SelectedIndex
        {
            get { return this.trvFamilies.SelectedIndex; }
            set
            {
                this.trvFamilies.SelectedIndex = value;
            }
        }
        public new object DataContext
        {
            get { return this.trvFamilies.DataContext; }
            set
            {
                view.Filter -= new FilterEventHandler(view_Filter);
                view.Source = value;
                view.Filter += new FilterEventHandler(view_Filter);
                this.trvFamilies.DataContext = view;
            }
        }

        public SimpleListView()
        {
            InitializeComponent();
            // 默认的数据列的创建代理
            CustomizeColumnDataDefFunc = (propertyAttribute, property, viewColumn, stackPanel) =>
            {
                if (property.PropertyType == typeof(Boolean))
                {
                    var checkBox = new CheckBox();
                    checkBox.FontSize = fontSize;
                    Binding textPropertyBinding = new Binding();
                    textPropertyBinding.Mode = BindingMode.TwoWay;
                    textPropertyBinding.Path = new PropertyPath(property.Name);
                    checkBox.SetBinding(CheckBox.IsCheckedProperty, textPropertyBinding);
                    checkBox.Checked += (object sender, RoutedEventArgs e) =>
                    {
                        if (bMultiSelect) return;

                    };
                    stackPanel.Children.Add(checkBox);
                }
                else
                {
                    if (propertyAttribute == null || !propertyAttribute.Editablity)
                    {
                        var textBlock = new TextBlock();
                        textBlock.FontSize = fontSize;
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
                        textBox.FontSize = fontSize;
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
            CustomizeHeaderDefFunc = stackPanel => 
            {
            };
            CustomizeFooterDefFunc = stackPanel =>
            {
            };

            //if (bDialog)
            //{
            //    var okButton = new Button();
            //    okButton.Height = 30;
            //    okButton.Margin = new Thickness(5, 5, 5, 5);
            //    okButton.Content = "OK";
            //    okButton.Click += (object sender, RoutedEventArgs e) =>
            //    {
            //        ConfirmResult = eConfirmResult.OK;
            //        this.Close();

            //    };
            //    this.Footer.Children.Add(okButton);
            //    var cancelButton = new Button();
            //    cancelButton.Margin = new Thickness(5,5,5,5);
            //    cancelButton.Content = "Cancel";
            //    cancelButton.Click += (object sender, RoutedEventArgs e) =>
            //    {
            //        ConfirmResult = eConfirmResult.CANCEL;
            //        this.Close();

            //    };
            //    this.Footer.Children.Add(cancelButton);
            //}
            //else
            //{
            //    this.Header.Visibility = Visibility.Hidden;
            //    this.Footer.Visibility = Visibility.Visible;

            //}

        }

        public void setStyle(byte red, byte green, byte blue, byte alpha, double fontSize = 12.0, SelectionMode selectionMode= SelectionMode.Single) {
            backgroundColor = new SolidColorBrush(Color.FromArgb(red, green, blue, alpha));
            this.fontSize = fontSize;
            this.trvFamilies.SelectionMode = selectionMode;
        }

        /**
         * 
         * 
         */
        public void inflateView<TModel>(List<TModel> tmodelList) where TModel : class
        {

            CustomizeHeaderDefFunc(this.Header);
            CustomizeFooterDefFunc(this.Footer);

            // 背景等风格定制
            Style style = new Style
            {
                TargetType = typeof(GridViewColumnHeader)
            };
            style.Setters.Add(new Setter(System.Windows.Controls.Control.BackgroundProperty, backgroundColor));
            style.Setters.Add(new Setter(System.Windows.Controls.Control.ForegroundProperty, Brushes.Black));
            style.Setters.Add(new Setter(System.Windows.Controls.Control.FontWeightProperty, FontWeights.Bold));
            style.Setters.Add(new Setter(System.Windows.Controls.Control.FontSizeProperty, fontSize));
            this.gridView.ColumnHeaderContainerStyle = style;


            tModelType = tmodelList.GetType().GetGenericArguments()[0];
            //We will be defining a PropertyInfo Object which contains details about the class property 
            System.Reflection.PropertyInfo[] arrayPropertyInfos = tModelType.GetProperties();
            //Now we will loop in all properties one by one to get value
            foreach (System.Reflection.PropertyInfo property in arrayPropertyInfos)
            {
                var propertyAttribute = (SimpleListViewColumnMeta)Attribute.GetCustomAttribute(property, typeof(SimpleListViewColumnMeta));
                GridViewColumn viewColumn = new GridViewColumn();
                SortedColumn sortedColumn = new SortedColumn { ColumnName = property.Name, Column = viewColumn, SortDirection = ListSortDirection.Ascending };

                // 默认显示或者明确指定显示的时候定制列的定义
                if (propertyAttribute == null || propertyAttribute.Visiblity)
                {
                    // 明确指定列宽度的时候
                    if (propertyAttribute != null && !Double.IsNaN(propertyAttribute.ColumnDisplayWidth))
                    {
                        viewColumn.Width = propertyAttribute.ColumnDisplayWidth;
                    }
                    else
                    {
                        viewColumn.Width = 100;
                    }
                    // 列头定制
                    viewColumn.HeaderTemplate = TemplateGenerator.CreateDataTemplate
                    (
                      () =>
                      {
                          StackPanel stackPanel = new StackPanel();
                          stackPanel.VerticalAlignment = VerticalAlignment.Top;
                          stackPanel.HorizontalAlignment = HorizontalAlignment.Left;
                          if (property.PropertyType == typeof(Boolean))
                          {
                              var titleBlock = new TextBlock();
                              if (propertyAttribute != null && !string.IsNullOrWhiteSpace(propertyAttribute.ColumnDisplayName))
                              {
                                  titleBlock.Text = propertyAttribute.ColumnDisplayName;
                              }
                              else
                              {
                                  titleBlock.Text = property.Name;
                              }
                              stackPanel.Children.Add(titleBlock);

                              var titleCheckBox = new CheckBox();
                              stackPanel.Children.Add(titleCheckBox);
                              titleCheckBox.Checked += (object sender, RoutedEventArgs e) =>
                              {
                                  var enumerable = view.Source as System.Collections.IEnumerable;
                                  foreach (object row in enumerable)
                                  {
                                      row.GetType().GetProperties().Single(pi => pi.Name == property.Name).SetValue(row, true);
                                  }
                              };
                              titleCheckBox.Unchecked += (object sender, RoutedEventArgs e) =>
                              {
                                  var enumerable = view.Source as System.Collections.IEnumerable;
                                  foreach (object row in enumerable)
                                  {
                                      row.GetType().GetProperties().Single(pi => pi.Name == property.Name).SetValue(row, false);
                                  }
                              };
                          }
                          else
                          {
                              var titleBlock = new TextBlock();
                              if (propertyAttribute != null && !string.IsNullOrWhiteSpace(propertyAttribute.ColumnDisplayName))
                              {
                                  titleBlock.Text = propertyAttribute.ColumnDisplayName;
                              }
                              else
                              {
                                  titleBlock.Text = property.Name;
                              }
                              stackPanel.Children.Add(titleBlock);
                          }
                          //如果对象类型为字符串时自动赋予过滤机能
                          TextBox filterTextBox = null;
                          if (FilteringOn && property.PropertyType == typeof(string))
                          {
                              filterTextBox = new TextBox();
                              filterTextBox.HorizontalAlignment = HorizontalAlignment.Left;
                              sortedColumn.filterTextBox = filterTextBox;
                          }
                          actionList.Add(() =>
                          {
                              stackPanel.Width = viewColumn.ActualWidth;
                              if (filterTextBox != null) filterTextBox.Width = viewColumn.ActualWidth;
                          });
                          stackPanel.Width = viewColumn.ActualWidth;
                          if (filterTextBox != null)
                          {
                              filterTextBox.Width = viewColumn.ActualWidth;
                              filterTextBox.TextChanged += (sender, e) =>
                              {
                                  if (view.View != null) view.View.Refresh();
                              };
                              stackPanel.Children.Add(filterTextBox);
                          }
                          // dummy
                          if (stackPanel.Children.Count == 1) {
                              stackPanel.Children.Add(new TextBlock());
                          }
                          //使用GridViewColumn时因为不是父子关系只是sibling关系导致无法正确绑定，只好退而求其次选择ListView
                          //Binding widthBinding = new Binding();
                          //widthBinding.RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(GridViewColumn), 1);
                          //widthBinding.Path = new PropertyPath("ActualWidth");
                          //filterTextBox.SetBinding(TextBox.WidthProperty, widthBinding);
                          //System.Diagnostics.PresentationTraceSources.SetTraceLevel(widthBinding, System.Diagnostics.PresentationTraceLevel.High);

                          return stackPanel;
                      }
                     );

                    //https://www.thinbug.com/q/2205151
                    //我会处理PropertyChanged事件。在Visual Studio intellisense中看不到PropertyChanged事件，但你可以欺骗它：）
                    ((System.ComponentModel.INotifyPropertyChanged)viewColumn).PropertyChanged += (sender, e) =>
                    {
                        if (e.PropertyName == "ActualWidth")
                        {
                            foreach (Action action in actionList)
                            {
                                action();
                            }
                        }
                    };
                    // 列数据部分的定义
                    viewColumn.CellTemplate = TemplateGenerator.CreateDataTemplate
                    (
                      () =>
                      {
                          return customizeCellTemplateHelper(propertyAttribute, property, tmodelList, viewColumn);
                      }
                    );
                    this.gridView.Columns.Add(viewColumn);
                    columnList.Add(sortedColumn);
                }
            }
            view.Filter -= new FilterEventHandler(view_Filter);
            view.Source = tmodelList;
            view.Filter += new FilterEventHandler(view_Filter);
            this.trvFamilies.DataContext = view;
            this.trvFamilies.Dispatcher.BeginInvoke((Action)delegate ()
            {
                foreach (GridViewColumn c in gridView.Columns)
                {
                    if (double.IsNaN(c.Width))
                        c.Width = c.ActualWidth;
                    c.Width = double.NaN;
                }
            });

        }

        protected StackPanel customizeCellTemplateHelper<TModel>(SimpleListViewColumnMeta propertyAttribute, PropertyInfo property,List<TModel> tmodelList, GridViewColumn viewColumn) {
            StackPanel stackPanel = new StackPanel();
            stackPanel.Orientation = Orientation.Horizontal;

            if (propertyAttribute != null && !string.IsNullOrWhiteSpace(propertyAttribute.TaskName))
            {
                var method = tmodelList[0].GetType().GetMethods().Where(m => m.Name.Equals(propertyAttribute.TaskName)).FirstOrDefault();
                if (method != null)
                {
                    var detailButton = new Button();
                    detailButton.FontSize = fontSize;
                    detailButton.Margin = new Thickness(5, 1, 5, 1);
                    detailButton.Content = "...";
                    detailButton.Click += (object sender, RoutedEventArgs e) =>
                    {
                        ListViewItem listViewItem = GetVisualAncestor<ListViewItem>((DependencyObject)sender);
                        listViewItem.IsSelected = true;

                        listViewItem.DataContext.Dump();

                        method.Invoke(tmodelList[0], new object[] { listViewItem.DataContext,this.trvFamilies });
                    };
                    stackPanel.Children.Add(detailButton);
                }
            }
            CustomizeColumnDataDefFunc(propertyAttribute, property, viewColumn, stackPanel);
            return stackPanel;
        }
        // 获取选中行的数据
        public object SelectedItem {
            get
            {
                return trvFamilies.SelectedItem;
                        
            }
        }
        //过滤
        void view_Filter(object sender, FilterEventArgs e)
        {
            e.Accepted = true;
            columnList.ForEach(column => {
                if (e.Accepted)
                { 
                    if (column.filterTextBox == null || string.IsNullOrEmpty(column.filterTextBox.Text))
                    {
                        e.Accepted = true;
                    }
                    else
                    {
                        string rowString = e.Item.GetType().GetProperties().Single(pi => pi.Name == column.ColumnName).GetValue(e.Item, null) as string;
                        if (rowString == null || rowString.ToLower().Contains(column.filterTextBox.Text.ToLower()))
                        {
                            e.Accepted = true;
                        }
                        else
                        {
                            e.Accepted = false;
                        }
                    }
                }
            });
        }
        private static T GetVisualAncestor<T>(DependencyObject o) where T : DependencyObject
        {
            do
            {
                o = VisualTreeHelper.GetParent(o);
            } while (o != null && !typeof(T).IsAssignableFrom(o.GetType()));

            return (T)o;
        }
        // 排序
        void GridViewColumnHeaderClickedHandler(object sender,
                                         RoutedEventArgs e)
        {
            var headerClicked = e.OriginalSource as GridViewColumnHeader;
            if (headerClicked != null)
            {
                if (headerClicked.Role != GridViewColumnHeaderRole.Padding)
                {
                    foreach(var sortedColumn in this.columnList) {
                        if (sortedColumn.Column == headerClicked.Column) {
                            ICollectionView dataView =
                              CollectionViewSource.GetDefaultView(trvFamilies.ItemsSource);
                            dataView.SortDescriptions.Clear();
                            SortDescription sd = new SortDescription(sortedColumn.ColumnName, sortedColumn.SortDirection);
                            sortedColumn.SortDirection = sortedColumn.SortDirection == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending;
                            dataView.SortDescriptions.Add(sd);
                            dataView.Refresh();
                        }
                    }
                }
            }
        }
    }
}