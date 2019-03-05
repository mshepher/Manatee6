using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Manatee7.Model;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace Manatee7 {
  public partial class MultiSelectCardList : CardList {
    private readonly CardOrderConverter _converter;

    public IEnumerable<Card> SelectedCards => _converter.Cards.Where(x => x != null);

    public MultiSelectCardList() {
      var blanks = Game.Instance.CallCard.Blanks;
      InitializeComponent();
      _converter = (Resources["Converter"] as CardOrderConverter);
      
      SelectionChanging += (sender, e) => {
        Debug.Assert(_converter.Cards.Length == blanks);
        foreach (Card c in e.RemovedItems) {
          var index = _converter.Cards.IndexOf(c);
          if (index > -1)
            _converter.Cards[index] = null;
        }

        foreach (Card c in e.AddedItems) {
          Debug.Assert(c != null);
          var i = 0;
          for (;i < blanks;i++) {
            if (_converter.Cards[i] != null) continue;
            _converter.Cards[i] = c;
            break;
          }

          if (i < blanks) continue;
          SelectedItems.Clear();
          _converter.Cards = new Card[blanks];
          _converter.Cards[0] = c;
        }
      };
    }
  }
  

  public class CardOrderConverter : IValueConverter {
    public CardOrderConverter() {
        Cards = new Card[Game.Instance.CallCard.Blanks];
      }

      public Card[] Cards;

      public object Convert(object value, Type targetType, object parameter,
                            CultureInfo culture) {
        return Cards.IndexOf(value) + 1;
        //return "boo";
      }

      public object ConvertBack(object value, Type targetType, object parameter,
                                CultureInfo culture) {
        throw new NotImplementedException();
      }
    }
    
    public class DiameterToRadiusConverter : IValueConverter {
          public object Convert(object value, Type targetType, object parameter,
                                CultureInfo culture) {
            try {
              return (int) value / 2;
            } catch {
              return 0;
            }
          }
    
          public object ConvertBack(object value, Type targetType, object parameter,
                                    CultureInfo culture) {
            try {
              return (int) value * 2;
            } catch {
              return 0;
            }
          }
        }
}
