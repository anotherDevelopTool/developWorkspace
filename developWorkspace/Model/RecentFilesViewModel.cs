namespace DevelopWorkspace.Main.Model
{
    using System;
    using System.IO;
    using DevelopWorkspace.Base;
    using DevelopWorkspace.Base.Model;
    using System.Windows.Media;

    internal class RecentFilesViewModel : ToolViewModel
  {
    public const string ToolContentId = "RecentFilesTool";

    public RecentFilesViewModel()
      : base("Recent Files")
    {
      ////Workspace.This.ActiveDocumentChanged += new EventHandler(OnActiveDocumentChanged);
      ContentId = ToolContentId;
    }

    public override Uri IconSource
    {
      get
      {
                return null;
        //return new Uri("pack://application:,,,/SimpleControls;component/MRU/Images/NoPin16.png", UriKind.RelativeOrAbsolute);
      }
    }

  }
}
