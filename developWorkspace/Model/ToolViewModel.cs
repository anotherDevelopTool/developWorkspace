﻿namespace DevelopWorkspace.Main.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using DevelopWorkspace.Base.Model;
    public class ToolViewModel : PaneViewModel
    {
        public ToolViewModel(string name)
        {
            Name = name;
            Title = name;
        }

        public string Name
        {
            get;
            private set;
        }


        #region IsVisible

        private bool _isVisible = true;
        public bool IsVisible
        {
            get { return _isVisible; }
            set
            {
                if (_isVisible != value)
                {
                    _isVisible = value;
                    RaisePropertyChanged("IsVisible");
                }
            }
        }

        #endregion


    }
}
