using System;
using System.Windows;
using DevelopWorkspace.Base;
namespace DevelopWorkspace.DataCreateAddin
{
    //TODO 面向Addin基类化
    [AddinMeta(Name = "hyddd", Date = "2009-07-20", Description = "单体测试数据生成辅助工具插件")]
    public class ViewModel : DevelopWorkspace.Base.Model.AddinBaseViewModel
    {
        public ViewModel()
        {
            Title = "DevelopWorkspace.DataCreateAddin";
            ContentId = "DevelopWorkspace.DataCreateAddin";
        }
        public override DataTemplate GetDataTemplate()
        {
            ResourceDictionary dic = new ResourceDictionary();
            Uri uri = new Uri("pack://application:,,,/DevelopWorkspace.DataCreateAddin;component/Dictionary1.xaml", UriKind.Absolute);
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
