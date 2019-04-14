using System;
using Log = Serilog.Log;
using Manatee7.Model;

using Xamarin.Forms;
// ReSharper disable InconsistentNaming -- protected classes should count as private


namespace Manatee7 {
    public abstract partial class BaseGamePage : ContentPage {

        protected static readonly Game game = Game.Instance;
        protected static readonly GameController controller = GameController.Instance;

        protected BaseGamePage() {
            Title = $"{game.Round + 1}/{game.GameRules.HandsPerGame}: {game.CurrentJudge.Name}'s Deal";
            InitializeComponent();
        }

        protected override bool OnBackButtonPressed() {
            return true;
        }
        
        public void GetHelp(object sender, EventArgs e) {
            Rg.Plugins.Popup.Services.PopupNavigation.Instance.PushAsync(
                    new TroubleshootingPage());
        }
    }
}