namespace DevelopWorkspace.Main.Model
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Windows;
    using System.Windows.Input;
    using DevelopWorkspace.Base;
    using DevelopWorkspace.Base.Model;
    using DevelopWorkspace.Main.Command;
    using Microsoft.Win32;
    using System.ComponentModel;
    using System.Windows.Data;
    using System.Windows.Media;

    public enum ToggleEditorOption
    {
        WordWrap = 0,
        ShowLineNumber = 1,
        ShowEndOfLine = 2,
        ShowSpaces = 3,
        ShowTabs = 4
    }

    class Workspace : ViewModelBase
    {

        protected Workspace()
        {

            _files = new ObservableCollection<ViewModelBase>();
            //_addinMenuItems = new ObservableCollection<AddinsCache.AddinsTableRow>();
            //foreach (AddinsCache.AddinsTableRow row in AddinBaseViewModel.GetCacheAddinsData())
            //{
            //    _addinMenuItems.Add(row);
            //}
            
            //_toolMenuItems = new ObservableCollection<ThirdPartyTool>();
            //foreach (ThirdPartyTool tool in (from ThirdPartyTool in DbSettingEngine.GetEngine().ThirdPartyTools
            //                                 select ThirdPartyTool))
            //{
            //    _toolMenuItems.Add(tool);
            //}

        }

        static Workspace _this = new Workspace();

        public static Workspace This
        {
            get { return _this; }
        }

        ObservableCollection<ViewModelBase> _files = null;
        ReadOnlyObservableCollection<ViewModelBase> _readonyFiles = null;
        public ReadOnlyObservableCollection<ViewModelBase> Files
        {
            get
            {
                if (_readonyFiles == null)
                {
                    _readonyFiles = new ReadOnlyObservableCollection<ViewModelBase>(_files);
                    //_files.Add(new CSScriptRunModel() { Title = "C# Script Console" });
                   //_files.Add(new DataExcelUtilModel() { Title = "DataBase Support Utility" });
                }

                return _readonyFiles;
            }
        }
        /// <summary>
        /// 从外部文件取出插件信息标识到菜单项上
        /// </summary>
        ObservableCollection<AddinsCache.AddinsTableRow> _addinMenuItems = null;
        ReadOnlyObservableCollection<AddinsCache.AddinsTableRow> _readonyAddinMenuItems = null;
        public ReadOnlyObservableCollection<AddinsCache.AddinsTableRow> AddinMenuItems
        {
            get
            {
                //if (_readonyAddinMenuItems == null)
                    //_readonyAddinMenuItems = new ReadOnlyObservableCollection<AddinsCache.AddinsTableRow>(_addinMenuItems);

                return _readonyAddinMenuItems;
            }
        }

        /// <summary>
        /// 从外部文件取出插件信息标识到菜单项上
        /// </summary>
        ObservableCollection<ThirdPartyTool> _toolMenuItems = null;
        ReadOnlyObservableCollection<ThirdPartyTool> _readonyToolMenuItems = null;
        public ReadOnlyObservableCollection<ThirdPartyTool> ToolMenuItems
        {
            get
            {
                //if (_readonyToolMenuItems == null)
                //    _readonyToolMenuItems = new ReadOnlyObservableCollection<ThirdPartyTool>(_toolMenuItems);

                return _readonyToolMenuItems;
            }
        }
        ToolViewModel[] _tools = null;

        public IEnumerable<ToolViewModel> Tools
        {
            get
            {
                if (_tools == null)
                    _tools = new ToolViewModel[] { this.FileStats };
                //_tools = new ToolViewModel[] { this.FileStats, new PropertiesToolViewModel() };

                return _tools;
            }
        }

        OutputToolViewModel _fileStats = null;
        public OutputToolViewModel FileStats
        {
            get
            {
                if (_fileStats == null)
                    _fileStats = new OutputToolViewModel();

                return _fileStats;
            }
        }

        #region LoadAddinCommand
        RelayCommand _loadAddinCommandCommand = null;
        public ICommand LoadAddinCommand
        {
            get
            {
                if (_loadAddinCommandCommand == null)
                {
                    _loadAddinCommandCommand = new RelayCommand((p) => OnLoad(p), (p) => CanLoad(p));
                }

                return _loadAddinCommandCommand;
            }
        }

        private bool CanLoad(object parameter)
        {
            return true;
        }

        private void OnLoad(object parameter)
        {
            // 主程序初始化时做遍历并在反映到ribbon上
            //List< ScriptBaseViewModel> scripts = ScriptBaseViewModel.ScanAddins();
            //foreach (ScriptBaseViewModel script in scripts) {
            //    var classAttribute = (AddinMetaAttribute)Attribute.GetCustomAttribute(script.GetType(), typeof(AddinMetaAttribute));
            //    if (classAttribute != null)
            //    {
            //        script.Title = classAttribute.Name;
            //    }
            //    SolidColorBrush brush = new SolidColorBrush(Color.FromArgb((byte)255, classAttribute.Red, classAttribute.Green, classAttribute.Blue));
            //    script.ThemeColorBrush = brush;
            //    script.IsActive = true;
            //    _files.Add(script);
            //    ActiveDocument = script;
            //}
            AddinMetaAttribute attribute = parameter as AddinMetaAttribute;
            ScriptBaseViewModel instance = ScriptBaseViewModel.LoadViewModel(attribute);
            SolidColorBrush brush = new SolidColorBrush(Color.FromArgb(180, attribute.Red, attribute.Green, attribute.Blue));
            instance.ThemeColorBrush = brush;
            instance.IsActive = true;
            instance.Title = attribute.Name;
            _files.Add(instance);
            ActiveDocument = instance;
        }

        #endregion

        //#region OpenCommand
        //RelayCommand _openCommand = null;
        //public ICommand OpenCommand
        //{
        //    get
        //    {
        //        if (_openCommand == null)
        //        {
        //            _openCommand = new RelayCommand((p) => OnOpen(p), (p) => CanOpen(p));
        //        }

        //        return _openCommand;
        //    }
        //}

        //private bool CanOpen(object parameter)
        //{
        //    return true;
        //}

        //private void OnOpen(object parameter)
        //{
        //    //dummycode for demenstrate logger usage
        //    Logger.WriteLine("OnOpen1");

        //    var dlg = new OpenFileDialog();
        //    if (dlg.ShowDialog().GetValueOrDefault())
        //    {
        //        var fileViewModel = Open(dlg.FileName);
        //        ActiveDocument = fileViewModel;
        //    }
        //}

        //public FileViewModel Open(string filepath)
        //{
        //    List<FileViewModel> filesFileViewModel = this._files.OfType<FileViewModel>().ToList();

        //    // Verify whether file is already open in editor, and if so, show it
        //    var fileViewModel = filesFileViewModel.FirstOrDefault(fm => fm.FilePath == filepath);

        //    if (fileViewModel != null)
        //    {
        //        this.ActiveDocument = fileViewModel; // File is already open so show it

        //        return fileViewModel;
        //    }

        //    fileViewModel = new FileViewModel(filepath);
        //    _files.Add(fileViewModel);
        //    return fileViewModel;
        //}

        //#endregion

        //#region NewCommand
        //RelayCommand _newCommand = null;
        //public ICommand NewCommand
        //{
        //    get
        //    {
        //        if (_newCommand == null)
        //        {
        //            _newCommand = new RelayCommand((p) => OnNew(p), (p) => CanNew(p));
        //        }

        //        return _newCommand;
        //    }
        //}

        //private bool CanNew(object parameter)
        //{
        //    return true;
        //}

        //private void OnNew(object parameter)
        //{
        //    _files.Add(new FileViewModel());
        //    ActiveDocument = _files.Last();
        //}

        //#endregion

        #region ActiveDocument

        private ViewModelBase _activeDocument = null;
        public ViewModelBase ActiveDocument
        {
            get { return _activeDocument; }
            set
            {
                if (_activeDocument != value)
                {
                    _activeDocument = value;
                    RaisePropertyChanged("ActiveDocument");
                    if (ActiveDocumentChanged != null)
                        ActiveDocumentChanged(this, EventArgs.Empty);
                }
            }
        }

        public event EventHandler ActiveDocumentChanged;

        #endregion

        internal void Close(PaneViewModel fileToClose)
        {
            if (fileToClose is FileViewModel) { 
                if ((fileToClose as FileViewModel).IsDirty)
                {
                    var res = MessageBox.Show(string.Format("Save changes for file '{0}'?", (fileToClose as FileViewModel).FileName), "AvalonDock Test App", MessageBoxButton.YesNoCancel);
                    if (res == MessageBoxResult.Cancel)
                        return;
                    if (res == MessageBoxResult.Yes)
                    {
                        Save((fileToClose as FileViewModel));
                    }
                }
            }
            //TODO avalondock的机制当点击tab页面的x按钮时会激活Close方法在这里可以加载清除处理等...
            if (fileToClose.clearance!= null && !fileToClose.clearance(null)) return;

            _files.Remove(fileToClose);
        }

        internal void Save(FileViewModel fileToSave, bool saveAsFlag = false)
        {
            if (fileToSave.FilePath == null || saveAsFlag)
            {
                var dlg = new SaveFileDialog();
                if (dlg.ShowDialog().GetValueOrDefault())
                    fileToSave.FilePath = dlg.SafeFileName;
            }

            File.WriteAllText(fileToSave.FilePath, fileToSave.Document.Text);
            (ActiveDocument as FileViewModel).IsDirty = false;
        }

        #region ToggleEditorOptionCommand
        RelayCommand _toggleEditorOptionCommand = null;
        public ICommand ToggleEditorOptionCommand
        {
            get
            {
                if (this._toggleEditorOptionCommand == null)
                {
                    this._toggleEditorOptionCommand = new RelayCommand((p) => OnToggleEditorOption(p),
                                                                       (p) => CanToggleEditorOption(p));
                }

                return this._toggleEditorOptionCommand;
            }
        }

        private bool CanToggleEditorOption(object parameter)
        {
            if (this.ActiveDocument != null)
                return true;

            return false;
        }

        private void OnToggleEditorOption(object parameter)
        {
            FileViewModel f = this.ActiveDocument as FileViewModel;

            if (parameter == null)
                return;

            if ((parameter is ToggleEditorOption) == false)
                return;

            ToggleEditorOption t = (ToggleEditorOption)parameter;

            if (f != null)
            {
                switch (t)
                {
                    case ToggleEditorOption.WordWrap:
                        f.WordWrap = !f.WordWrap;
                        break;

                    case ToggleEditorOption.ShowLineNumber:
                        f.ShowLineNumbers = !f.ShowLineNumbers;
                        break;

                    case ToggleEditorOption.ShowSpaces:
                        f.TextOptions.ShowSpaces = !f.TextOptions.ShowSpaces;
                        break;

                    case ToggleEditorOption.ShowTabs:
                        f.TextOptions.ShowTabs = !f.TextOptions.ShowTabs;
                        break;

                    case ToggleEditorOption.ShowEndOfLine:
                        f.TextOptions.ShowEndOfLine = !f.TextOptions.ShowEndOfLine;
                        break;

                    default:
                        break;
                }
            }
        }
        #endregion ToggleEditorOptionCommand

        /// <summary>
        /// This property manages the data visible in the Recent Files ViewModel.
        /// </summary>
        private RecentFilesViewModel _recentFiles = null;
        public RecentFilesViewModel RecentFiles
        {
            get
            {
                if (_recentFiles == null)
                    _recentFiles = new RecentFilesViewModel();

                return _recentFiles;
            }
        }


        /// <summary>
        /// Bind a window to some commands to be executed by the viewmodel.
        /// </summary>
        /// <param name="win"></param>
        public void InitCommandBinding(Window win)
        {
          //  win.CommandBindings.Add(new CommandBinding(ApplicationCommands.New,
          //  (s, e) =>
          //  {
          //      this.OnNew(null);
          //  }));

          //  win.CommandBindings.Add(new CommandBinding(ApplicationCommands.Open,
          //  (s, e) =>
          //  {
          //      this.OnOpen(null);
          //  }));

          //  win.CommandBindings.Add(new CommandBinding(AppCommand.LoadFile,
          //  (s, e) =>
          //  {
          //      if (e == null)
          //          return;

          //      string filename = e.Parameter as string;

          //      if (filename == null)
          //          return;

          //      this.Open(filename);
          //  }));

          //  win.CommandBindings.Add(new CommandBinding(AppCommand.BrowseURL,
          //  (s, e) =>
          //  {
          //      Process.Start(new ProcessStartInfo("http://Edi.codeplex.com"));
          //  }));

          //  win.CommandBindings.Add(new CommandBinding(AppCommand.ShowStartPage,
          //  (s, e) =>
          //  {
          //      StartPageViewModel spage = this.GetStartPage(true);

          //      if (spage != null)
          //      {
          //    //this.ActiveDocument = spage;
          //}
            //}
        //));
        }

        #region NewDbUtilCommand
        RelayCommand _newDbUtilCommand = null;
        public ICommand NewDbUtilCommand
        {
            get
            {
                if (_newDbUtilCommand == null)
                {
                    _newDbUtilCommand = new RelayCommand((p) => OnNewDbUtil(p), (p) => CanNewDbUtil(p));
                }

                return _newDbUtilCommand;
            }
        }

        private bool CanNewDbUtil(object parameter)
        {
            return true;
        }

        private void OnNewDbUtil(object parameter)
        {
            _files.Add(new DataExcelUtilModel() { Title = "DataBase Support Utility" });
            ActiveDocument = _files.Last();

            ICollectionView collectionView = CollectionViewSource.GetDefaultView(this._files);

        }

        #endregion
        #region NewScriptCommand
        RelayCommand _newScriptCommand = null;
        public ICommand NewScriptCommand
        {
            get
            {
                if (_newScriptCommand == null)
                {
                    _newScriptCommand = new RelayCommand((p) => OnNewScript(p), (p) => CanNewScript(p));
                }

                return _newScriptCommand;
            }
        }

        private bool CanNewScript(object parameter)
        {
            return true;
        }

        private void OnNewScript(object parameter)
        {
            _files.Add(new CSScriptRunModel() { Title = "Script Console" });
            ActiveDocument = _files.Last();
        }

        #endregion
        #region ThirdPartyToolCommand
        RelayCommand _ThirdPartyToolCommand = null;
        public ICommand ThirdPartyToolCommand
        {
            get
            {
                if (_ThirdPartyToolCommand == null)
                {
                    _ThirdPartyToolCommand = new RelayCommand((p) => OnThirdPartyTool(p), (p) => CanThirdPartyTool(p));
                }

                return _ThirdPartyToolCommand;
            }
        }

        private bool CanThirdPartyTool(object parameter)
        {
            return true;
        }

        private void OnThirdPartyTool(object parameter)
        {
            int thirdPartyID = Convert.ToInt32(parameter);
            ThirdPartyTool thirdPartyTool = (from ThirdPartyTool in DbSettingEngine.GetEngine().ThirdPartyTools
             where ThirdPartyTool.ThirdPartyID == thirdPartyID
                                             select ThirdPartyTool).FirstOrDefault();

            _files.Add(new ThirdPartyToolModel { Title = thirdPartyTool.MenuTitle, ThirdPartyTool = thirdPartyTool });
            ActiveDocument = _files.Last();
        }

        #endregion
        /// <summary>
        /// Construct and add a new <seealso cref="StartPageViewModel"/> to intenral
        /// list of documents, if none is already present, otherwise return already
        /// present <seealso cref="StartPageViewModel"/> from internal document collection.
        /// </summary>
        /// <param name="CreateNewViewModelIfNecessary"></param>
        /// <returns></returns>
        internal StartPageViewModel GetStartPage(bool CreateNewViewModelIfNecessary)
        {
            List<StartPageViewModel> l = this._files.OfType<StartPageViewModel>().ToList();

            if (l.Count == 0)
            {
                if (CreateNewViewModelIfNecessary == false)
                    return null;
                else
                {
                    StartPageViewModel s = new StartPageViewModel();
                    //this._files.Add(s);

                    return s;
                }
            }

            return l[0];
        }
    }

}
