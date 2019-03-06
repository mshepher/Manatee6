using System;
using Xamarin.Forms;
using Manatee7.Model;
using Log = Serilog.Log;

namespace Manatee7 {
  public partial class SettingsPage {
    public SettingsPage() {
      InitializeComponent();
      foreach (var d in listView.ItemsSource) {
        Log.Information("{@d} is an item", d);
      }
      BindingContext = this;
      _library.PropertyChanged += (sender, e) => Log.Information("Saw change");
      CodeEntry.Completed += AddDeckFromEntry;
      LinkTapped.Tapped += (sender, e) => Device.OpenUri(
        new Uri("http://www.cardcastgame.com/browse"));
    }

    private readonly DeckLibrary _library = DeckLibrary.Instance;

    private void Delete(object sender, EventArgs e) {
      try {
        _library.RemoveDeck(((MenuItem)sender).CommandParameter as string);
      }
      catch (Exception ex) {
        Log.Error("could not delete deck {deck}; exception {Newline}{e}",
            ((MenuItem) sender).CommandParameter, ex);
      }
    }

    private void Handle_Unfocused(object sender, FocusEventArgs e) {
      Preferences.Save();
    }

    private async void AddDeckFromEntry(object sender, EventArgs e) {
      if (CodeEntry.Text.Length != 5) return;
      AddButton.IsEnabled = false;
      AddButton.Text = "Adding deck...";
      var code = CodeEntry.Text.ToUpper();
      try {
        await _library.AddDeckFromCode(code);
        CodeEntry.Text = "";
        CodeEntry.Placeholder = "Enter code";
      } catch (Exception ex) {
        Log.Error("Exception! {@ex}" + ex.Message);
        CodeEntry.Text = "";
        CodeEntry.Placeholder = "Not found!";
      }
      AddButton.IsEnabled = true;
      AddButton.Text = "Add Deck";
    }
  }
}
