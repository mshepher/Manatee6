using System;
using System.Data;
using System.Collections.Generic;
using Serilog;
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

    public class Min10 : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
                              System.Globalization.CultureInfo culture)
        {
            if (value is int i && i < 10)
            {
                return 10;
            }
            else
            {
                return value;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter,
                                  System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ArithmeticConverter : IValueConverter {
    
    private string Expression;
    private DataTable dt = new DataTable();
    
    public ArithmeticConverter(string expression) {
        var temp = expression.Replace("x", "1");
        //if this fails, we want an immediate exception
        var dispose = dt.Compute(temp, "");
        Expression = expression;
    }
    
    // https://stackoverflow.com/questions/333737/evaluating-string-342-yield-int-18
    public object Convert(object value, Type targetType, object parameter,
                          System.Globalization.CultureInfo culture)
    {
 

        
        try {
                var temp = Expression.Replace("x", value.ToString());
                var result = dt.Compute(temp, "");
                return result;
        }
        catch (Exception e) {
          Log.Error("Failed to parse string {s} into a mathematical expression {Newline}{@e}",value,e);
        }
      return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter,
                              System.Globalization.CultureInfo culture)
    {
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