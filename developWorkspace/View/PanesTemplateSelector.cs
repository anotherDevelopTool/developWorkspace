namespace DevelopWorkspace.Main
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;
  using System.Windows.Controls;
  using System.Windows;
  using Xceed.Wpf.AvalonDock.Layout;
  using DevelopWorkspace.Base;
    using DevelopWorkspace.Base.Model;

    class PanesTemplateSelector : DataTemplateSelector
    {
        public PanesTemplateSelector()
        {
        
        }

        
        public DataTemplate ThirdPartyToolViewTemplate
        {
            get;
            set;
        }
        public DataTemplate FileViewTemplate
        {
            get;
            set;
        }

        public DataTemplate StartPageViewTemplate
        {
          get;
          set;
        }

        public DataTemplate RecentFilesViewTemplate
        {
          get;
          set;
        }

        public DataTemplate OutputToolViewTemplate
        {
            get;
            set;
        }
        public DataTemplate PropertiesToolViewTemplate
        {
            get;
            set;
        }
        public DataTemplate CSScriptRunViewTemplate
        {
            get;
            set;
        }
        public DataTemplate ScriptAddinViewTemplate
        {
            get;
            set;
        }
        public DataTemplate DataExcelUtilViewTemplate
        {
            get;
            set;
        }

        public override System.Windows.DataTemplate SelectTemplate(object item, System.Windows.DependencyObject container)
        {
            var itemAsLayoutContent = item as LayoutContent;

            if (item is ScriptBaseViewModel)
                return ScriptAddinViewTemplate;

            if (item is Model.FileViewModel)
                return FileViewTemplate;

            if (item is Model.StartPageViewModel)
              return StartPageViewTemplate;

            if (item is Model.CSScriptRunModel)
                return CSScriptRunViewTemplate;

            if (item is Model.DataExcelUtilModel)
                return DataExcelUtilViewTemplate;

            if (item is Model.OutputToolViewModel)
                return OutputToolViewTemplate;
            if (item is Model.PropertiesToolViewModel)
                return PropertiesToolViewTemplate;


            if (item is Model.RecentFilesViewModel)
              return RecentFilesViewTemplate;
            if (item is Model.ThirdPartyToolModel)
                return ThirdPartyToolViewTemplate;

            return base.SelectTemplate(item, container);
        }
    }
}
