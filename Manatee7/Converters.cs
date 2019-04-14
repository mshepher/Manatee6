using System;
using System.Data;
using System.Collections.Generic;
using Serilog;
using Xamarin.Forms;

namespace Manatee7 {
    
    public class ArithmeticConverter : IValueConverter {
    
        private readonly string _expression;
        private static readonly DataTable dt = new DataTable();
    
        public ArithmeticConverter(string expression) {
            _expression = expression;
        }
    
        // https://stackoverflow.com/questions/333737/evaluating-string-342-yield-int-18
        public object Convert(object value, Type targetType, object parameter,
                              System.Globalization.CultureInfo culture) {
            try {
                var temp = _expression.Replace("x", value.ToString());
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

    public class BoolToSelectionModeConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter,
                              System.Globalization.CultureInfo culture) {
            if (value is bool b) {
                return (b
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