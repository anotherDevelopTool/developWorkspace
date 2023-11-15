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
 
    /// <summary>
    /// ConfirmForm.xaml 的交互逻辑
    /// </summary>
    public partial class CriticalMessageBox : Window
    {
        public eConfirmResult ConfirmResult { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="confirmMessage">确认信息</param>
        /// <param name="rowInfoList">选项列表</param>
        /// <param name="MultiSelect">控制单选还是允许复选</param>
        public CriticalMessageBox(string confirmMessage)
        {
            InitializeComponent();

            this.message.Text = confirmMessage;
            this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            ConfirmResult = eConfirmResult.CANCEL;
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

    }
}