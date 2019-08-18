namespace DevelopWorkspace.Main.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.IO;
    using System.Windows.Media.Imaging;
    using System.Windows.Media;
    using DevelopWorkspace.Base;
    using DevelopWorkspace.Base.Model;

    class PropertiesToolViewModel : ToolViewModel
    {
        public PropertiesToolViewModel()
          : base("Properties")
        {
            Workspace.This.ActiveDocumentChanged += new EventHandler(OnActiveDocumentChanged);
            ContentId = ToolContentId;
        }
        public const string ToolContentId = "Properties";

        void OnActiveDocumentChanged(object sender, EventArgs e)
        {
            FileSize = 0;
            LastModified = DateTime.MinValue;

            if (Workspace.This.ActiveDocument != null)
            {
                FileViewModel f = Workspace.This.ActiveDocument as FileViewModel;

                if (f != null)
                {
                    if (f.FilePath != null && File.Exists(f.FilePath))
                    {
                        var fi = new FileInfo(f.FilePath);
                        FileSize = fi.Length;
                        LastModified = fi.LastWriteTime;
                    }

                }
            }
        }

        #region FileSize

        private long _fileSize;
        public long FileSize
        {
            get { return _fileSize; }
            set
            {
                if (_fileSize != value)
                {
                    _fileSize = value;
                    RaisePropertyChanged("FileSize");
                }
            }
        }

        #endregion

        #region LastModified

        private DateTime _lastModified;
        public DateTime LastModified
        {
            get { return _lastModified; }
            set
            {
                if (_lastModified != value)
                {
                    _lastModified = value;
                    RaisePropertyChanged("LastModified");
                }
            }
        }

        #endregion

        public override Uri IconSource
        {
            get
            {
                return new Uri("pack://application:,,,/DevelopWorkspace;component/Images/property-blue.png", UriKind.RelativeOrAbsolute);
            }
        }
    }
}
