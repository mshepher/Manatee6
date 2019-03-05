using System;
using System.Linq;

namespace Manatee7 {
  public partial class SelectCardPage {

    public SelectCardPage() {
      InitializeComponent();
      BindingContext = game;
      HandListView.RefreshListViewItem(-1, -1, true);
      HandListView.SelectionChanged += ToggleButton;
    }

    private void PlayCardButtonClicked(object sender, EventArgs e) {
      var selectedCards = HandListView.SelectedCards.ToList();
      controller.PlayCards(selectedCards);
      PlayCardButton.IsVisible = false;
      WaitingLabel.IsVisible = true;
      HandListView.SelectionMode = Syncfusion.ListView.XForms.SelectionMode.None;
    }

    private void ToggleButton(object sender, Syncfusion.ListView.XForms.ItemSelectionChangedEventArgs e) {
      if (HandListView.SelectedItems.Count == game.CallCard.Blanks && !WaitingLabel.IsVisible) {
        PlayCardButton.IsEnabled = true;
      } else {
        PlayCardButton.IsEnabled = false;
      }
    }

    protected override void OnDisappearing() {
      base.OnDisappearing();
      //I don't know if this actually creates a loop or not
      HandListView.SelectionChanged -= ToggleButton;
    }
  }
}