using System;
using System.Collections.Generic;
using Manatee7.Model;
using Xamarin.Forms;

namespace Manatee7 {
    public partial class WelcomePage : ContentPage {
        public WelcomePage() {
            InitializeComponent();
            BindingContext = Preferences.Instance;
        }

        private void NextClicked(object sender, EventArgs e) {
            if (NameEntry.Text.Length <= 0) return;
            Preferences.Save();
            App.NextPage(new NavigationPage(new MainPage()));
        }
    }
}