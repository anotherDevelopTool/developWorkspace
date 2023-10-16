using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows;

namespace DevelopWorkspace.Main
{
    class InvertVisibleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is Visibility)
            {
                Visibility visibility = (Visibility)value;
                if (visibility == Visibility.Visible)
                {
                    return Visibility.Collapsed;
                }
                else if (visibility == Visibility.Collapsed)
                {
                    return Visibility.Visible;
                }
                else if (visibility == Visibility.Hidden)
                {
                    return Visibility.Visible;
                }
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {


            return value;
        }
    }
}
