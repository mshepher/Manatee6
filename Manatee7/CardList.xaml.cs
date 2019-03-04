using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Manatee5.Model;
using Syncfusion.ListView.XForms;
using Xamarin.Forms;

namespace Manatee5 {
  public partial class CardList : SfListView {
    private string[] SelectedCards;
    
    public CardList() {
      InitializeComponent();
    }
    
  }
}
