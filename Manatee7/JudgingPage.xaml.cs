using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Manatee5.Model;
using Xamarin.Forms;
using Serilog;

namespace Manatee5 {
  public partial class JudgingPage : BaseGamePage {

    public ObservableCollection<Player> WallOfShame { set; get; }
    public ObservableCollection<string> MyStrings { set; get; }

    private bool timeoutReached = false;
    public bool CanOverride => (timeoutReached && WallOfShame.Count < 2);

    public JudgingPage() {
      try {
        WallOfShame = new ObservableCollection<Player>(game.Players);
        InitializeComponent();
        WallOfShame.Remove(Preferences.Instance.Me);
        //WallOfShameDisplay.ItemsSource = WallOfShame;
        game.Submissions = new Dictionary<Player, List<Card>>();
        game.PropertyChanged += (sender, args) => {
          if (args.PropertyName != "Submissions") return;
          foreach (var p in game.Submissions.Keys)
            WallOfShame.Remove(p);
        };
        Task.Run(async () => {
          await Task.Delay(40 * 1000);
          timeoutReached = true;
          OnPropertyChanged(nameof(CanOverride));
        });
      }
      
      catch (Exception e) {
        Log.Fatal("Threw Exception {e}", e);
      }
      controller.StatusUpdated += (oldStatus, newStatus) => {
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