using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Data.SqlTypes;
using Xamarin.Forms;
using Log = Serilog.Log;
using static Manatee5.Model.Card.Type;
using Syncfusion.DataSource.Extensions;

namespace Manatee5.Model {
  public class Game : INotifyPropertyChanged {

    public string GameName { internal set; get; }

    public Guid GameID {
      get => gameId;
      internal set {
        OnPropertyChanged("GameID");
        OnPropertyChanged("GameIDString");
        gameId = value;
      }
    }

    public string GameIDString => GameID.ToString();

    // these collections are NOT thread-safe and should only be accessed from the main thread

    //Rules
    public Rules GameRules { get; set; }

    //Players
    public Player Host { get; internal set; }
    public Player CurrentJudge { get; internal set; }
    public bool IAmJudge => (CurrentJudge.Equals(_me));
    public List<Player> Players { get; internal set; }
    public Dictionary<Player, int> Score { get; internal set; }
    private Player _me => Preferences.Instance.Me;


    //Deck
    //public List<Card> AllResponseCards { get; set; }
    //public List<Card> AllCallCards { get; set; }
    public Queue<Card> MyResponseCards { get; internal set; }
    public Queue<Card> MyCallCards { get; internal set; }
    public ObservableCollection<Card> Hand { get; internal set; }

    public Card CallCard {
      get => callCard;
      internal set {
        callCard = value;
        OnPropertyChanged("CallCard");
      }
    }

    //Current state
    public void AddSubmission(Player p, List<Card> c) {
      lock (Submissions) {
        Submissions[p] = c;
        OnPropertyChanged(nameof(Submissions));
      }
    }

    public Tuple<Player, string> LastWinningCombo { internal set; get; }

    public Dictionary<Player, List<Card>> Submissions { get; set; }
    public List<List<Card>> OrderedSubmissions { get; internal set; }

    public short Round {
      get => _round;
      internal set {
        _round = value;
        OnPropertyChanged("Round");
      }
    }

    private short _round;

    private Card callCard;
    private Guid gameId = Guid.Empty;


    private Game() {
    }

    public static Game Instance { get; internal set; } = new Game();

    public event PropertyChangedEventHandler PropertyChanged;

    public void ResetGame() {
      Players = null;
      MyResponseCards = null;
      MyCallCards = null;
      Score = null;
      CallCard = null;
      Hand = null;
      Round = 0;
      GameID = Guid.Empty;
    }

    public void PlayCards(List<Card> cards) {
      Log.Information("Removing cards {card} from hand: {hand}", cards.Select(x => x.Text),
          Hand.Select(card => card.Text).ToList());
      Debug.Assert(cards.All(c => Hand.Contains(c)));
      foreach (var c in cards) {
        try {
          Hand.Remove(c);
        }
        catch (Exception e) {
          Log.Information("Caught baffling exception: {NewLine}{@ex}", e);
        }
      }
      OnPropertyChanged(nameof(Hand));
    }

    public void RegisterWinningCard(Player judge, Player winner, Dictionary<Player, int> score, short round) {
      CurrentJudge = judge;
      foreach (var player in score.Keys.ToList()) {
        Score[player] = Score.ContainsKey(player)
            ? Math.Max(Score[player], score[player])
            : score[player];
      }
      if (Score.ContainsKey(winner))
        Score[winner]++;
      OnPropertyChanged(nameof(Score));

      Round = ++round;
    }
    
    public void SetUpGame(StartGameMessage message) {
      GameID = message.NewGameID;
      Players = message.Players;
      MyResponseCards = message.MyResponseCards;
      MyCallCards = message.MyCallCards;
      CurrentJudge = Players[0];
      //DEBUG OPTION
      //CurrentJudge = new Player();
      Score = new Dictionary<Player, int>();
      foreach (var p in Players) Score.Add(p, 0);
      GameRules = message.Rules;
      CallCard = message.CallCard;
      Hand = new ObservableCollection<Card>();
      for (var i = 0; i < GameRules.CardsPerHand; i++) Hand.Add(MyResponseCards.Dequeue());
    }

    public void SubmissionsArrived(SubmissionsFlippedMessage message) {
      Submissions = message.Submissions;
      OrderedSubmissions = message.OrderedSubmissions;
    }

    public void NewRound(Card nextCallCard) {
      CurrentJudge = Players[(Players.IndexOf(CurrentJudge) + 1) % Players.Count];
      Log.Information("Adding cards to hand...");
      while (Hand.Count < GameRules.CardsPerHand)
        Hand.Add(MyResponseCards.Dequeue());
      OnPropertyChanged(nameof(Hand));
      CallCard = nextCallCard;
    }

    //https://forums.xamarin.com/discussion/99191/xamarin-forms-mvvm-in-c-propertychanged-event-handler-is-always-null-when-onpropertychanged-call
    protected virtual void OnPropertyChanged(string propertyName) {
      Device.BeginInvokeOnMainThread(() =>
          PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)));
    }


  }

}
