using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using Manatee7.Model;
using Syncfusion.ListView.XForms;
using Serilog;
using Xamarin.Forms;
using Rg.Plugins.Popup.Services;
using Log = Serilog.Log;
using Xamarin.Forms.Internals;

namespace Manatee7
{
    public partial class FinalScorePage : Rg.Plugins.Popup.Pages.PopupPage {
        public int HighScore { get; }
        public List<KeyValuePair<Player,int>> OrderedPlayers { get; }
        public FinalScorePage() {
            OrderedPlayers = Game.Instance.Score.ToList();
            OrderedPlayers.Sort((kv1,kv2) => {
                if (kv1.Value < kv2.Value) return 1;
                if (kv1.Value > kv2.Value) return -1;
                return 0;
            });

            InitializeComponent();
            
            HighScore = OrderedPlayers.Count > 0 ? OrderedPlayers.First().Value : 0;
            var rank = 1;
            foreach (var kv in OrderedPlayers) {
                var player = kv.Key;
                var wins = kv.Value;
                ScoreGrid.Children.Add(new Label() {Text = player.Name, Style = (Style)Resources["NameStyle"]}, 0, rank - 1);
                ScoreGrid.Children.Add(new Label() {Text = wins.ToString(), Style = (Style)Resources["ScoreStyle"]}, 1, rank - 1);
                rank++;
            }
        }

        private async void ExitClicked(object sender, EventArgs e)
        {
            if (PopupNavigation.Instance.PopupStack.Any())
                await PopupNavigation.Instance.PopAsync();
            GameController.Instance.ExitGame();
        }

        protected override bool OnBackButtonPressed()
        {
            return true;
        }

        protected override bool OnBackgroundClicked()
        {
            return false;
        }
    }
}