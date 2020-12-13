using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.UI;
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
        SimpleListView simpleListView = new SimpleListView();

        Window dialog = new Window();
        Grid grid = new Grid();
        dialog.Content = grid;

        StackPanel parent = new StackPanel();
        grid.Children.Add(simpleListView);
        dialog.ShowDialog();


    */
    public partial class SimpleListView : System.Windows.Controls.UserControl
    {
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

        public SimpleListView()
        {
            InitializeComponent();
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

        public void CreateView<TModel>(List<TModel> tmodelList) where TModel : class
        {
            tModelType = tmodelList.GetType().GetGenericArguments()[0];
            //We will be defining a PropertyInfo Object which contains details about the class property 
            System.Reflection.PropertyInfo[] arrayPropertyInfos = tModelType.GetProperties();
            //Now we will loop in all properties one by one to get value
            foreach (System.Reflection.PropertyInfo property in arrayPropertyInfos)
            {
                var propertyAttribute = (SimpleListViewColumnMeta)Attribute.GetCustomAttribute(property, typeof(SimpleListViewColumnMeta));
                GridViewColumn viewColumn = new GridViewColumn();
                SortedColumn sortedColumn = new SortedColumn { ColumnName = property.Name, Column = viewColumn, SortDirection = ListSortDirection.Ascending };

                if (propertyAttribute != null && !Double.IsNaN(propertyAttribute.ColumnDisplayWidth))
                {
                    viewColumn.Width = propertyAttribute.ColumnDisplayWidth;
                }
                else
                {
                    viewColumn.Width = 100;
                }

                viewColumn.HeaderTemplate = TemplateGenerator.CreateDataTemplate
                (
                  () =>
                  {
                      StackPanel stackPanel = new StackPanel();
                      stackPanel.HorizontalAlignment = HorizontalAlignment.Left;
                      if (property.PropertyType == typeof(Boolean))
                      {
                          var titleBlock = new TextBlock();
                          titleBlock.Text = property.Name;
                          stackPanel.Children.Add(titleBlock);

                          var titleCheckBox = new CheckBox();
                          stackPanel.Children.Add(titleCheckBox);
                          titleCheckBox.Checked += (object sender, RoutedEventArgs e) => {
                              var enumerable = view.Source as System.Collections.IEnumerable;
                              foreach (object row in enumerable)
                              {
                                  row.GetType().GetProperties().Single(pi => pi.Name == property.Name).SetValue(row, true);
                              }
                          };
                          titleCheckBox.Unchecked += (object sender, RoutedEventArgs e) => {
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
                      TextBox filterTextBox=null;
                      if (property.PropertyType == typeof(string)) {
                          filterTextBox = new TextBox();
                          filterTextBox.HorizontalAlignment = HorizontalAlignment.Left;
                          sortedColumn.filterTextBox = filterTextBox;
                      }
                      actionList.Add(() => {
                        stackPanel.Width = viewColumn.ActualWidth;
                        if(filterTextBox != null) filterTextBox.Width = viewColumn.ActualWidth;
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
                viewColumn.CellTemplate = TemplateGenerator.CreateDataTemplate
                (
                  () =>
                  {
                      StackPanel stackPanel = new StackPanel();
                      stackPanel.Orientation = Orientation.Horizontal;

                      if (propertyAttribute != null && !string.IsNullOrWhiteSpace(propertyAttribute.TaskName))
                      {
                          var method = tmodelList[0].GetType().GetMethods().Where(m => m.Name.Equals(propertyAttribute.TaskName)).FirstOrDefault();
                          if (method != null)
                          {
                              var detailButton = new Button();
                              detailButton.Content = "...";
                              detailButton.Click += (object sender, RoutedEventArgs e) => {
                                  ListViewItem listViewItem = GetVisualAncestor<ListViewItem>((DependencyObject)sender);
                                  listViewItem.IsSelected = true;
                                  method.Invoke(tmodelList[0], new object[] { listViewItem.DataContext });
                              };
                              stackPanel.Children.Add(detailButton);
                          }
                      }
                      if (property.PropertyType == typeof(Boolean))
                      {
                          var checkBox = new CheckBox();
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
                          var textBlock = new TextBlock();
                          Binding textPropertyBinding = new Binding();
                          textPropertyBinding.Path = new PropertyPath(property.Name);
                          textBlock.SetBinding(TextBlock.TextProperty, textPropertyBinding);
                          stackPanel.Children.Add(textBlock);
                      }
                      return stackPanel;
                  }
                );
                this.gridView.Columns.Add(viewColumn);
                columnList.Add(sortedColumn);
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

            //this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        }

        public object SelectedItem {
            get
            {
                return trvFamilies.SelectedItem != null ? (trvFamilies.SelectedItem as ListViewItem).DataContext : null;
                        
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