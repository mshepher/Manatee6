using System;
using System.Collections.Generic;
using Manatee7.Model;
using Xamarin.Forms;

namespace Manatee7 {
    public partial class SubmissionList : CardList {
        public SubmissionList() {
            InitializeComponent();
        }
    }

    public class SubmissionToLabelStackConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter,
                              System.Globalization.CultureInfo culture) {

            var stack = new StackLayout {Spacing = 0, VerticalOptions = LayoutOptions.StartAndExpand};
            if (!(value is List<Card> list)) return stack;
      
            var queue = new Queue<Card>(list);
            stack.Children.Add(GenLabel(queue.Dequeue().Text));
            foreach (var c in queue) {
                stack.Children.Add(new BoxView {Style=_dividerStyle});
                stack.Children.Add(GenLabel(c.Text));
            }
            return stack;
        }

        private readonly Style _dividerStyle = Application.Current.Resources["DividerStyle"] as Style;
    
        private readonly Style _cardStyle = Application.Current.Resources["CardLabel"] as Style;
    
        private Label GenLabel(string text) {
            var label = new Label { FormattedText = new FormattedString(), Style = _cardStyle};
            label.FormattedText.Spans.Add(new Span {Text=text});
            return label;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
                                  System.Globalization.CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}