using System;
using Serilog;
using System.Collections.Generic;
using Xamarin.Forms;
using System.Threading.Tasks;
using Manatee5.Model;

namespace Manatee5 {
  public partial class StarterDecksPage : ContentPage {
    public StarterDecksPage() {
      InitializeComponent();
      BindingContext = this;
      //DecksListView.ItemsSource = SuggestedDecks;
    }

    private void Pause(object sender, System.EventArgs e) {
      Log.Information("Paused");
    }

    async void Handle_Clicked(object sender, System.EventArgs e) {
      Log.Information("Clicked {sender} {args}", sender, e);
      Preferences.GetPreferences().NotMyFirstTime = true;
      Preferences.GetPreferences().Save();
      Navigation.PopModalAsync(false);
      foreach (KeyValuePair<string, string> item in (DecksListView.SelectedItems)) {  
        await DeckLibrary.Instance.AddDeckFromCode(item.Value  );
      }
    }

    public Dictionary<String, String> SuggestedDecks {
      get {
        return new Dictionary<string, string>() {
        {"ALL of Cards Against Humanity", "7RTVN"},
        {"Squeaky Clean Cards Against Humanity", "6VVUN"},
        {"Crabs Adjust Humidity", "BVPNZ"},
        {"Ladies Against Humanity", "LADY1"},
        {"A Game for Good Christians", "272QG"},
        {"Cards Against Downtime", "YT8HB"} };
      }

      //public Dictionary<string,string> SuggestedDecks { get { return suggestedDecks; } }
    }
  }

}