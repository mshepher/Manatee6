using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace Manatee7 {
  public class IsZeroConverter : IValueConverter {
    public object Convert(object value, Type targetType, object parameter,
                          System.Globalization.CultureInfo culture) {
      if (value is int i) {
        return (i == 0);
      }

      return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter,
                              System.Globalization.CultureInfo culture) {
      throw new NotImplementedException();
    }
  }

    public class DivideBy2 : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
                              System.Globalization.CultureInfo culture)
        {
            try
            {
                return (double)value * 2;
            }
            catch
            {
                return value;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter,
                                  System.Globalization.CultureInfo culture)
        {
            try
            {
                return (double)value / 2;
            }
            catch
            {
                return value;
            }
        }
    }

    public class BoolToSelectionModeConverter : IValueConverter {
    public object Convert(object value, Type targetType, object parameter,
                          System.Globalization.CultureInfo culture) {
      if (value is bool) {
        return ((bool) value
            ? Syncfusion.ListView.XForms.SelectionMode.Single
            : Syncfusion.ListView.XForms.SelectionMode.None);
      }

      return Syncfusion.ListView.XForms.SelectionMode.None;
    }

    public object ConvertBack(object value, Type targetType, object parameter,
                              System.Globalization.CultureInfo culture) {
      throw new NotImplementedException();
    }
  }

  public class ToColor : IValueConverter {
    public object Convert(object value, Type targetType, object parameter,
                          System.Globalization.CultureInfo culture) {
      var dictionary = new Dictionary<int, Color>() {
          {0, Color.Red}, {1, Color.Green}, {2, Color.SkyBlue}, {3, Color.Gold}
      };

      return dictionary[(Math.Abs(value.GetHashCode())) % 4];
    }

    public object ConvertBack(object value, Type targetType, object parameter,
                              System.Globalization.CultureInfo culture) {
      throw new NotImplementedException();
    }
  }


  public class IsNotZeroConverter : IValueConverter {
    public object Convert(object value, Type targetType, object parameter,
                          System.Globalization.CultureInfo culture) {
      if (value is int i) {
        return (i != 0);
      }

      return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter,
                              System.Globalization.CultureInfo culture) {
      throw new NotImplementedException();
    }
  }

  public class FourColors : IValueConverter {
    public object Convert(object value, Type targetType, object parameter,
                          System.Globalization.CultureInfo culture) {
      if (value is int i) {
        return (i != 0);
      }

      return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter,
                              System.Globalization.CultureInfo culture) {
      throw new NotImplementedException();
    }
  }


  public class InvertBool : IValueConverter {
    public object Convert(object value, Type targetType, object parameter,
                          System.Globalization.CultureInfo culture) {
      return !(bool)value;
    }

    public object ConvertBack(object value, Type targetType, object parameter,
                              System.Globalization.CultureInfo culture) {
      return !(bool)value;
    }
  }
}