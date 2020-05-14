namespace DevelopWorkspace.Base
{
  using System;
  using System.Collections.Generic;
    using System.IO;
    using System.Linq;
  using System.Text;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media.Imaging;

    public class RelayCommand : ICommand
  {
    #region Fields

    readonly Action<object> _execute;
    readonly Predicate<object> _canExecute;

    #endregion // Fields

    #region Constructors

    public RelayCommand(Action<object> execute)
      : this(execute, null)
    {
    }

    public RelayCommand(Action<object> execute, Predicate<object> canExecute)
    {
      if (execute == null)
        throw new ArgumentNullException("execute");

      _execute = execute;
      _canExecute = canExecute;
    }
    #endregion // Constructors

    #region ICommand Members

    public bool CanExecute(object parameter)
    {
      return _canExecute == null ? true : _canExecute(parameter);
    }

    public event EventHandler CanExecuteChanged
    {
      add { CommandManager.RequerySuggested += value; }
      remove { CommandManager.RequerySuggested -= value; }
    }

    public void Execute(object parameter)
    {
      _execute(parameter);
    }

    #endregion // ICommand Members
  }

    public class ContextMenuCommand : RelayCommand
    {
        public string header { get; set; }
        public string tooltip { get; set; }
        public int refcount { get; set; }
        public Uri image { get; set; }
        public ContextMenuCommand(string _header,string _tooltip,string _imagePath,Action<object> execute, Predicate<object> canExecute) : base(execute, canExecute)
        {
            header = _header;
            tooltip = _tooltip;
            string iconfile = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "addins", string.IsNullOrEmpty(_imagePath) ? "plugin" : _imagePath + ".png");
            if (File.Exists(iconfile))
            {
                var uri = new Uri(iconfile);
                //image = new Image { Source = new BitmapImage(uri) };
                image = uri;
            }
            else
            {
                try
                {
                    var resourceString = "/DevelopWorkspace;component/Images/" + (string.IsNullOrEmpty(_imagePath) ? "plugin" : _imagePath) + ".png";
                    //image = new Image { Source = new BitmapImage(new Uri(resourceString, UriKind.Relative)) };
                    image = new Uri(resourceString, UriKind.Relative);
                }
                catch (Exception ex)
                {
                }
            }

        }
    }
}
