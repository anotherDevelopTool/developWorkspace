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
using System.Collections.ObjectModel;
using System.IO;
using System.Xml;
using DevelopWorkspace.Base;
namespace DevelopWorkspace.CodeGeneratorAddin
{
    //TODO 面向Addin基类化
    [AddinMeta(Name = "hyddd", Date = "2009-07-20", Description = "代码自动生成工具插件")]
    public class ViewModel : DevelopWorkspace.Base.Model.AddinBaseViewModel
    {
        public ViewModel() {
            Title = "DevelopWorkspace.CodeGeneratorAddin";
            ContentId = "DevelopWorkspace.CodeGeneratorAddin";
        }
        public override DataTemplate GetDataTemplate() { 
                ResourceDictionary dic = new ResourceDictionary();
                Uri uri = new Uri("pack://application:,,,/DevelopWorkspace.CodeGeneratorAddin;component/Dictionary1.xaml", UriKind.Absolute);
                dic.Source = uri;
                object rawResource = dic["addin"];
                DataTemplate t = rawResource as DataTemplate;
                return t;
        }
        public object ribbonTemplate()
        {
                return null;
        }
    }
}
