using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows;
using System.Globalization;
using ICSharpCodeX.AvalonEdit.Document;
using static System.Net.Mime.MediaTypeNames;

namespace DevelopWorkspace.Main
{
    public class StringToTextDocumentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string text)
            {
                return new TextDocument { Text = text };
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TextDocument document)
            {
                return document.Text;
            }

            return null;
        }
    }
}
