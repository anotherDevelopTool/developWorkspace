namespace DevelopWorkspace.Main
{
    using Newtonsoft.Json;
    using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;
  using System.Windows.Data;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    class CancelButtonPosConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (Double.Parse(value.ToString()) + 480)/2 -220;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }
    class CancelButtonTopPosConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (Double.Parse(value.ToString()) + 160) / 2;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }
    class ActiveDocumentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }
    class DbProviderImageConverter : IMultiValueConverter
    {
        /// 需传入一组对象，（基础值 比对值）
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int provideId = int.Parse(values[0].ToString());
            int connectionHistoryId = int.Parse(values[1].ToString());
            string strUri = "";
            if (connectionHistoryId == 1 ) strUri= "/DevelopWorkspace;component/Images/script_setting.png";
            else if (provideId == 1) strUri = "/DevelopWorkspace;component/Images/sqlite.png";
            else if (provideId == 2) strUri = "/DevelopWorkspace;component/Images/postgressql.png";
            else if (provideId == 3) strUri = "/DevelopWorkspace;component/Images/mysql.png";
            else  strUri = "/DevelopWorkspace;component/Images/oracle.png";
            return new BitmapImage(new Uri(strUri, UriKind.Relative));

        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            return new object[] { 100, 10 };
        }
    }

    //class DbProviderImageConverter : IValueConverter
    //{
    //  public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    //  {
    //      if (value.ToString() == "1") return "/DevelopWorkspace;component/Images/sqlite.png";
    //      if (value.ToString() == "2") return "/DevelopWorkspace;component/Images/postgressql.png";
    //      if (value.ToString() == "3") return "/DevelopWorkspace;component/Images/mysql.png";
    //      if (value.ToString() == "4") return "/DevelopWorkspace;component/Images/oracle.png";
    //      return value;
    //  }

    //  public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    //  {
    //      return value;
    //  }
    //}
    class ElementWidhtConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        double totalWidth = double.Parse(value.ToString());
        return totalWidth-45;
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        return value;
    }
  }

  class QDataColorConvert : IMultiValueConverter
   {
       /// 需传入一组对象，（基础值 比对值）
       public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
       {
           int totalWidth = int.Parse(values[0].ToString());
           int othersWidth = int.Parse(values[1].ToString());
           return totalWidth - othersWidth;
           // return values[0];
       }

       public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
       {
           return new object[]{100,10};
       }
   }
    class SqlCommnadContextMenuConvert : IMultiValueConverter
    {
        /// 需传入一组对象，（基础值 比对值）
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return values.Clone();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
    class BackgroundBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            AppConfig.JsonArgb schemaColorArgbRaw = (AppConfig.JsonArgb)JsonConvert.DeserializeObject(value.ToString(), typeof(AppConfig.JsonArgb));
            SolidColorBrush brush = new SolidColorBrush(Color.FromArgb((byte)255, (byte)schemaColorArgbRaw.red, (byte)schemaColorArgbRaw.green, (byte)schemaColorArgbRaw.blue));
            return brush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }

}
