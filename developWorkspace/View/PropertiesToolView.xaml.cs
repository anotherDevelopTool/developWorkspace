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
using System.Windows.Navigation;
using System.Windows.Shapes;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Rendering;
using ICSharpCode.AvalonEdit.Search;
using System.Windows.Threading;
using System.Threading;
using DevelopWorkspace.Base;

namespace DevelopWorkspace.Main.View
{
    /// <summary>
    /// OutputToolView.xaml 的交互逻辑
    /// </summary>
    public partial class PropertiesToolView : UserControl
    {

        public PropertiesToolView()
        {
            InitializeComponent();
            //SearchPanel.Install(this.LogViewTextEditor);

            //TODO 2019/02/23 不再使用model绑定的方式输出日志
            Base.Services.BusyWorkService(new Action(() =>
            {

            }));
        }
    }
}
