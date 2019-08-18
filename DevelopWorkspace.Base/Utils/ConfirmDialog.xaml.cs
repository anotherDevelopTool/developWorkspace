using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DevelopWorkspace.Base.Utils
{
    public enum eConfirmResult
    {
        OK,
        CANCEL
    }
    public class RowInfo
    {
        public RowInfo()
        {
        }
        public bool Selected { get; set; }
        public string[] TitleList { get; set; }
        public string[] ColumnList { get; set; }
    }

    /// <summary>
    /// ConfirmForm.xaml 的交互逻辑
    /// </summary>
    public partial class ConfirmDialog : Window
    {
        CollectionViewSource view = new CollectionViewSource();
        bool bMultiSelect = false;
        public eConfirmResult ConfirmResult { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="confirmMessage">确认信息</param>
        /// <param name="rowInfoList">选项列表</param>
        /// <param name="MultiSelect">控制单选还是允许复选</param>
        public ConfirmDialog(string confirmMessage, List<RowInfo> rowInfoList, bool MultiSelect = false)
        {
            InitializeComponent();

            for (int i = 0; i < rowInfoList[0].TitleList.Count(); i++)
            {
                GridViewColumn gvc = new GridViewColumn();
                gvc.Header = rowInfoList[0].TitleList[i];
                gvc.CellTemplate = GeneratePropertyBoundTemplate("ColumnList[" + i + "]", "ItemDisplayTemplate");
                this.gridView.Columns.Add(gvc);
            }
            this.bMultiSelect = MultiSelect;
            view.Source = rowInfoList;
            this.message.Text = confirmMessage;
            this.trvFamilies.DataContext = view;
            this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            ConfirmResult = eConfirmResult.CANCEL;
        }
        private DataTemplate GeneratePropertyBoundTemplate(string property, string templateKey)
        {
            var template = FindResource(templateKey);
            FrameworkElementFactory factory = new FrameworkElementFactory(typeof(ContentPresenter));
            factory.SetValue(ContentPresenter.ContentTemplateProperty, template);
            factory.SetBinding(ContentPresenter.ContentProperty, new Binding(property));
            return new DataTemplate { VisualTree = factory };
        }
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            ConfirmResult = eConfirmResult.CANCEL;
            this.Close();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            ConfirmResult = eConfirmResult.OK;
            this.Close();

        }

        private void toggleSelect_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void toggleSelect_Unchecked(object sender, RoutedEventArgs e)
        {

        }

        private void checked_Checked(object sender, RoutedEventArgs e)
        {
            if (bMultiSelect) return;
            foreach (RowInfo rowInfo in view.Source as List<RowInfo>)
            {
                rowInfo.Selected = false;
            }
            (sender as CheckBox).IsChecked = true;
            if (view.View != null) view.View.Refresh();
        }
    }
}