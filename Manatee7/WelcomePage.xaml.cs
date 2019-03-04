using System;
using System.Collections.Generic;
using Manatee5.Model;
using Xamarin.Forms;

namespace Manatee5 {
  public partial class WelcomePage : ContentPage {
    public WelcomePage() {
      InitializeComponent();
      BindingContext = Preferences.Instance;
      //make user click the 'start scanning' button
      //NameEntry.Completed += NextClicked;
    }

    private void NextClicked(object sender, EventArgs e) {
      Preferences.Save();
      App.NextPage(new NavigationPage(new MainPage()));
    }
  }
}
