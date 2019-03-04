﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using Manatee5.Model;
using Syncfusion.ListView.XForms;
using Serilog;
using Xamarin.Forms;
using Rg.Plugins.Popup.Services;
using Log = Serilog.Log;
using Xamarin.Forms.Internals;

namespace Manatee5 {
  public partial class WinningCardPage : Rg.Plugins.Popup.Pages.PopupPage {
    public Player WinningPlayer { get; }
    public String WinningString { get; }


    public WinningCardPage(Player p, Card callCard, List<Card> response) {
      WinningPlayer = p;
      InitializeComponent();
      var callCardStrings = callCard.TextArray;
      Debug.Assert(callCardStrings.Count == response.Count + 1);
      int i;
      for (i = 0; i < response.Count(); i++) {
        FormattedWinningString.Spans.Add(new Span {Text = callCardStrings[i]});
        FormattedWinningString.Spans.Add(new Span
            {Text = response[i].Text, TextColor = Color.Red});
      }
      FormattedWinningString.Spans.Add(new Span {Text = callCardStrings[i]});
    }

    Game game = Game.Instance;
        
    private async void NextRound(object sender, EventArgs e) {
      Button.IsEnabled = false;
      GameController.Instance.NewRound();
      if (PopupNavigation.Instance.PopupStack.Any())
        await PopupNavigation.Instance.PopAsync();
    }
    
    protected override bool OnBackButtonPressed() {
      return true;
    } 

    protected override bool OnBackgroundClicked() {
      return false;
    }
  }
}