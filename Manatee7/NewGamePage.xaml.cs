﻿using System;
using System.Linq;
using Log = Serilog.Log;
using Manatee5.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xamarin.Forms;

namespace Manatee5 {
  public partial class NewGamePage {
    private static Preferences Preferences => Preferences.Instance;
    private readonly PostOffice _px = PostOffice.Instance;
    private readonly GameController _controller = GameController.Instance;

    private readonly Invitation _invitation = new Invitation(
        Preferences.PlayerName + "'s Game",
        Preferences.Me
    );

    public Dictionary<Deck, bool> CustomDecks { set; get; }

    public Collection<Player> JoinedPlayers { set; get; } =
      new Collection<Player>();
    
    public IEnumerable<Player> DistinctJoinedPlayers => JoinedPlayers?.Distinct();

    public NewGamePage() {
      InitializeComponent();
      BindingContext = this;
      _controller.SendInvite(_invitation);
      _controller.NewGame(_invitation);
      JoinedPlayers.Add(Preferences.Me);
    }

    protected override void OnAppearing() {
      base.OnAppearing();
      _px.OnJoinGameMessageSeen += AddPlayer;
      _px.OnLeaveGameMessageSeen += RemovePlayer;
    }

    protected override void OnDisappearing() {
      base.OnDisappearing();
      //don't stay subscribed to external events
      _px.OnJoinGameMessageSeen -= AddPlayer;
      _px.OnLeaveGameMessageSeen -= RemovePlayer;
    }

    private void AddPlayer(JoinGameMessage message) {
          JoinedPlayers.Add(message.Sender);
        OnPropertyChanged(nameof(DistinctJoinedPlayers));
    }

    private void RemovePlayer(LeaveGameMessage message) {
        JoinedPlayers.Remove(message.Sender);
        OnPropertyChanged(nameof(DistinctJoinedPlayers));
    }

    private void CancelButtonClicked(object sender, EventArgs e) {
      _controller.WithdrawInvite();
      _controller.ResetGame();
      Navigation.PopModalAsync();
    }


    private async void StartButtonClicked(object sender, EventArgs e) {
      _px.OnJoinGameMessageSeen -= AddPlayer;
      _px.OnLeaveGameMessageSeen -= RemovePlayer;
      try {
        StartGameButton.IsEnabled = false;
        CancelButton.IsEnabled = false;
        StartGameButton.Text = "Shuffling...";
        var playerList = new List<Player>();
        foreach (var item in VisiblePlayersListView.DataSource.Items.ToList())
          playerList.Add((Player)item);
        await _controller.StartGameAsHost(playerList);
        if (Navigation.ModalStack.Any())
          Navigation.PopModalAsync();
      } catch (GameException ex) {
        DisplayAlert(ex.Alert, ex.Detail, "Ugh, fine.");
      }
    }
  }
}