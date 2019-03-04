using System;
using System.Collections.Generic;
using Manatee5.Model;

namespace Manatee5 {
  public partial class FlippedCardsPage : BaseGamePage {

    public List<List<Card>> SubmittedCards { get; } = game.OrderedSubmissions;

    public FlippedCardsPage() {
      InitializeComponent();
      //SubmissionListView.RefreshListViewItem(-1, -1, true);
      AwaitingLabel.IsVisible = !game.IAmJudge;
    }

    private void WinningCardButtonClicked(object sender, EventArgs e)
    {
      controller.PickWinningResponse((List<Card>) SubmissionListView.CurrentItem);
    }
  }
}