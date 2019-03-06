using System;
using System.Linq;
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
      AddButton.IsEnabled = false;
      AddButton.Text = "Adding deck...";
      try {
        if (CodeEntry.Text.Length != 5) 
          throw new ArgumentException("Input is wrong length!");
        //https://stackoverflow.com/questions/3061662/how-to-find-out-if-string-contains-non-alpha-numeric-characters-in-c-net-2-0
        if (!CodeEntry.Text.All(char.IsLetterOrDigit)) 
          throw new ArgumentException("Contains non-alphanumeric chars");
        
        var code = CodeEntry.Text.ToUpper();
        await _library.AddDeckFromCode(code);
        CodeEntry.Placeholder = "Enter 5-letter code";
      } catch (Exception ex) {
        Log.Error("Exception! {@ex}" + ex.Message);
        CodeEntry.Placeholder = "Not found!";
      }
      CodeEntry.Text = "";
      AddButton.Text = "Add Deck";
    }
  }
}