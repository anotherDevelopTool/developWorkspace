namespace DevelopWorkspace.Main.Model
{
    using System;
    using System.IO;
    using System.Windows.Input;
    using DevelopWorkspace.Base;
    using DevelopWorkspace.Base.Model;
    using System.Windows.Media;

    class StartPageViewModel : FileBaseViewModel
  {
    public StartPageViewModel()
    {
      this.Title = "Start Page";
      this.StartPageTip = "Welcome to Edi. Review the content of the start page to get started.";
      this.ContentId = "{StartPage_ContentId}";
    }

    #region CloseCommand
    RelayCommand _closeCommand = null;
    override public ICommand CloseCommand
    {
      get
      {
        if (_closeCommand == null)
        {
          _closeCommand = new RelayCommand((p) => OnClose(), (p) => CanClose());
        }

        return _closeCommand;
      }
    }

    private bool CanClose()
    {
      return true;
    }

    private void OnClose()
    {
      //Workspace.This.Close(this);
    }
    #endregion

    override public ICommand SaveCommand
    {
      get
      {
        return null;
      }
    }

    public override Uri IconSource
    {
      get
      {
                return null;
        // This icon is visible in AvalonDock's Document Navigator window
        //return new Uri("pack://application:,,,/Edi;component/Images/document.png", UriKind.RelativeOrAbsolute);
      }
    }

    public string StartPageTip { get; set; }

    override public bool IsDirty
    {
      get
      {
        return false;
      }

      set
      {
        throw new NotSupportedException("Start page cannot be saved therfore setting dirty cannot be useful.");
      }
    }

    override public string FilePath
    {
      get
      {
        return this.ContentId;
      }

      protected set
      {
        throw new NotSupportedException();
      }
    }
  }
}
