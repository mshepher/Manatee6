using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Manatee7.Model;
using Xamarin.Forms;
using Serilog;

namespace Manatee7
{
    public partial class JudgingPage : BaseGamePage
    {

        public ObservableCollection<KeyValuePair<Player, bool>> WallOfShame { set; get; }
        public ObservableCollection<string> MyStrings { set; get; }

        private List<string> Threadlock = new List<string>();
        private bool timeoutReached = false;
        public bool CanOverride => (timeoutReached && WallOfShame.Count < 2);

        public JudgingPage()
        {
            try
            {
                var humanPlayers = game.HumanPlayers.ToList();
                humanPlayers.Remove(Preferences.Instance.Me);
                var humanCards = humanPlayers.Select((x) => new KeyValuePair<Player, bool>(x, false));
                var robotCards = game.RobotPlayers.Select((x) => new KeyValuePair<Player, bool>(x, true));
                WallOfShame = new ObservableCollection<KeyValuePair<Player, bool>>(humanCards.Concat(robotCards));
                InitializeComponent();
                PostOffice.Instance.Hibernate();
                PostOffice.Instance.Thaw();
                //WallOfShameDisplay.ItemsSource = WallOfShame;
                game.PropertyChanged += (sender, args) =>
                {
                        if (args.PropertyName != "Submissions") return;
                        foreach (var kv in WallOfShame.ToList())
                        {
                            if (!kv.Value)
                                foreach (var p in game.Submissions.Keys)
                                    if (kv.Key == p)
                                    {
                                        WallOfShame.Remove(kv);
                                        WallOfShame.Add(new KeyValuePair<Player, bool>(p, true));
                                    }
                        }
                };
                Task.Run(async () =>
                {
                    await Task.Delay(5 * 1000);
                    timeoutReached = true;
                    OnPropertyChanged(nameof(CanOverride));
                });
            }

            catch (Exception e)
            {
                Log.Fatal("Threw Exception {e}", e);
            }
            controller.StatusUpdated += (oldStatus, newStatus) =>
            {
                if (newStatus == GameController.GameStatus.SUBMISSION_FLIPPED)
                    FlipCards();
            };
        }

    private void FlipCards() {
      var FlipPage = new FlippedCardsPage();
      App.NextPage(FlipPage);
    }

    private void JustGo(object sender, EventArgs e) {
      controller.FlipCards();
      FlipCards();
    }
  }
}