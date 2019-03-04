using System;
using System.Collections.Generic;
using Manatee5.Model;
using Xamarin.Forms;

namespace Manatee5 {
  public partial class SubmissionList : CardList {
    public SubmissionList() {
      InitializeComponent();
    }

    public SubmissionToLabelStackConverter SubmissionConverter =
        new SubmissionToLabelStackConverter();
    public StackLayout SubmissionToLabel(IEnumerable<Card> cards) {
      var stack = new StackLayout();
      foreach (var c in cards) {
        stack.Children.Add(new Label { Text = c.Text });
      }
      return stack;
    }
  }

  public class SubmissionToLabelStackConverter : IValueConverter {
    public object Convert(object value, Type targetType, object parameter,
                          System.Globalization.CultureInfo culture) {

      var stack = new StackLayout {Spacing = 0, VerticalOptions = LayoutOptions.StartAndExpand};
      if (!(value is List<Card> list)) return stack;
      
      var endString = new string('\u00a0', 2);
      var queue = new Queue<Card>(list);
      stack.Children.Add(GenLabel(queue.Dequeue().Text));
      foreach (var c in queue) {
        stack.Children.Add(new BoxView() {
          Style=Application.Current.Resources["DividerStyle"] as Style});
        stack.Children.Add(GenLabel(c.Text));
      }
      return stack;
    }
    
    private Style CardStyle => Application.Current.Resources["CardLabel"] as Style;
    
    private Label GenLabel(string text) {
      var label = new Label();
      label.FormattedText = new FormattedString();
      label.FormattedText.Spans.Add(new Span {Text=text});
      label.Style = CardStyle; 
      return label;
    }

    public object ConvertBack(object value, Type targetType, object parameter,
                              System.Globalization.CultureInfo culture) {
      throw new NotImplementedException();
    }
  }


}