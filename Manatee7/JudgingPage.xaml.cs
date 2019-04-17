using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Manatee7.Model;
using Manatee7.PO;
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
        public bool CanOverride => (timeoutReached && _wallOfShameCount < 2);
        private int _wallOfShameCount;

        public JudgingPage()
        {
            try
            {
                var humanPlayers = game.HumanPlayers.ToList();
                humanPlayers.Remove(Preferences.Instance.Me);
                var humanCards = humanPlayers.Select((x) => new KeyValuePair<Player, bool>(x, false));
                var robotCards = game.RobotPlayers.Select((x) => new KeyValuePair<Player, bool>(x, true));
                WallOfShame = new ObservableCollection<KeyValuePair<Player, bool>>(humanCards.Concat(robotCards));
                _wallOfShameCount = humanPlayers.Count;
                InitializeComponent();
                
                //make sure to catch any card selections that came in before NewRound()
                PostOffice.Instance.Hibernate();
                PostOffice.Instance.Thaw();
                
                game.PropertyChanged += (sender, args) =>
                {
                    if (args.PropertyName != "Submissions") return;
                    foreach (var kv in WallOfShame.ToList()) {
                        if (kv.Value) continue;
                        foreach (var p in game.Submissions.Keys)
                            if (kv.Key == p)
                            {
                                WallOfShame.Remove(kv);
                                _wallOfShameCount--;
                                WallOfShame.Add(new KeyValuePair<Player, bool>(p, true));
                            }
                    }
                    OnPropertyChanged(nameof(CanOverride));
                };
                
                Task.Run(async () =>
                {
                    await Task.Delay(30 * 1000);
                    PO.PostOffice.Instance.ClearMessageHistory();
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
            App.NextPage(new FlippedCardsPage());
        }

        private void JustGo(object sender, EventArgs e) {
            controller.FlipCards();
            FlipCards();
        }
    }
}
