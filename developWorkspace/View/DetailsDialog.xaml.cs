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
using static DevelopWorkspace.Main.MainWindow;

namespace DevelopWorkspace.Main.View
{
     /// <summary>
    /// ConfirmForm.xaml 的交互逻辑
    /// </summary>
    public partial class DetailsDialog : Fluent.RibbonWindow
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="confirmMessage">确认信息</param>
        /// <param name="rowInfoList">选项列表</param>
        /// <param name="MultiSelect">控制单选还是允许复选</param>
        public DetailsDialog(TableInfo tableinfo)
        {
            InitializeComponent();
            tableinfo.Columns[0].ThemeColorBrush = tableinfo.ThemeColorBrush;
            this.trvFamilies.DataContext = tableinfo.Columns;
            this.tableTitle.Text = $"Column Detail:{tableinfo.TableName}";
        }
         private void button1_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();

        }

        private void RibbonWindow_Deactivated(object sender, EventArgs e)
        {
            try
            {
                this.Close();
            }
            catch (Exception ex) { }
        }

        private void RibbonWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = false;
        }

        private void RibbonWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) {
                try
                {
                    this.Close();
                }
                catch (Exception ex) { }
            }
        }
    }
}