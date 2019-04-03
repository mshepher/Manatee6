using System;
using Manatee7.Model;
using Xamarin.Forms;

namespace Manatee7 {
  public partial class SidebarPage : ContentPage {
    public SidebarPage() {
      InitializeComponent();
      //todo: Get cross-platform navigation bar height to set title height
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
