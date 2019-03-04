using System;
using Manatee5.Model;
using Xamarin.Forms;

namespace Manatee5 {
  public partial class SidebarPage : ContentPage {
    public SidebarPage() {
      InitializeComponent();
      GameController.Instance.StatusUpdated += (oldStatus, newStatus) => {
        if (newStatus != GameController.GameStatus.WINNER_CHOSEN) return;
        ScoreBoard.ItemsSource = null;
        ScoreBoard.ItemsSource = Game.Instance.Score;
      };
    }
    private void ExitButtonClicked(object sender, EventArgs e) {
      GameController.Instance.ExitGame();
    }
  }
}
