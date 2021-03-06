﻿using System;
using static Manatee7.GameController.GameStatus;
using static Manatee7.Model.Card.Type;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Medallion;
using System.Diagnostics;
using System.Linq;
using Manatee7.Model;
using Xamarin.Forms;
using Log = Serilog.Log;
using System.ComponentModel;
using System.Threading.Tasks;
using Manatee7.PO;

namespace Manatee7 {
    public class GameController : INotifyPropertyChanged {
        
        public ObservableCollection<Invitation> VisibleInvitations { get; } = new
                ObservableCollection<Invitation>();
    
        private readonly PostOffice _px = PostOffice.Instance;
        private readonly Game _game = Game.Instance;
        private Player _me => Preferences.Instance.Me;
    
        public static GameController Instance { get; } = new GameController();

        private ProposeGameMessage MyInviteMessage { get; set; }

        private JoinGameMessage MyRSVPMessage { get; set; }

        private readonly List<string> _robotNames = new List<string> { "Helen", "Karen", 
                "Ted", "Harold", "Chad", "Eliza" };

        private Card _nextCallCard;

        private GameController() {
            _px = PostOffice.Instance;
            _px.OnProposeGameMessageSeen += message => {
                if (!VisibleInvitations.Contains(message.Invitation))
                  VisibleInvitations.Add(message.Invitation);
                Log.Information("Added {game} to Visible Invitations",
                                message.Invitation.GameName);
                //Don't process announce game messages; we might want to be 'reminded' of them
                //px.ProcessMessage(message);
            };

            _px.OnProposeGameMessageWithdrawn += message => {
                Device.BeginInvokeOnMainThread(() =>
                                                       VisibleInvitations.Remove(message.Invitation));
            };
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public GameStatus Status {
            private set { 
                //'new card played' is the only really state-dependent command
                var oldValue = _status;
                if (_status == WAIT_AS_JUDGE && value != WAIT_AS_JUDGE)
                    _px.OnPlayedCardMessageSeen -= SawCard;
                if (value == WAIT_AS_JUDGE && _status != WAIT_AS_JUDGE)
                    _px.OnPlayedCardMessageSeen += SawCard;
                _status = value;
                StatusUpdated?.Invoke(oldValue, value);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StatusString)));
            }
            get => _status;
        }

        //DEBUG purposes only
        public string StatusString => _status.ToString();

        private GameStatus _status = SCANNING;

        private void CheckStatus(GameStatus expectedStatus) {
            CheckStatus(new[] { expectedStatus });
        }

        private void CheckStatus(GameStatus[] expectedStatuses) {
            if (!expectedStatuses.Contains(_status)) {
                Log.Error("Status is supposed to be one of {@es}, but is {s}!",
                          expectedStatuses, _status);
            }
            Debug.Assert(expectedStatuses.Contains(_status));
        }


        public event StatusHandler StatusUpdated;
        public event PropertyChangedEventHandler PropertyChanged;

        public void ResetGame() {
            _game.ResetGame();
        }
        public void NewGame(Invitation invite) {
            _game.ResetGame();
            _game.GameName = invite.GameName;
            _game.Host = invite.Host;
        }

        // WINNER_CHOSEN -> (WAIT_AS_JUDGE, CHOOSING_SUBMISSION)
        public void NewRound() {
            CheckStatus(WINNER_CHOSEN);
            if (Status != WINNER_CHOSEN) return;
            _game.NewRound(_nextCallCard);
            Status = _game.CurrentJudge.Equals(_me)
                    ? WAIT_AS_JUDGE
                    : CHOOSING_SUBMISSION;
            var newPage = _game.IAmJudge
                    ? new JudgingPage()
                    : (BaseGamePage)new SelectCardPage();
            App.NextPage(newPage);
        }

        // (WAIT_AS_HOST, WAIT_AS_GUEST) -> (WAIT_AS_JUDGE, CHOOSING_SUBMISSION)
        private void SetUpGame(StartGameMessage message) {
            CheckStatus(new[] { WAIT_AS_HOST, WAIT_AS_GUEST });
            _game.SetUpGame(message);

            //https://forums.xamarin.com/discussion/112676/eventhandler-not-being-removed-despite-being-unsubscribed
            _px.OnWinningCardSelectedMessageSeen -= WinningCardPicked;
            _px.OnWinningCardSelectedMessageSeen += WinningCardPicked;
            _px.OnSubmissionsFlippedMessageSeen -= SubmissionsArrived;
            _px.OnSubmissionsFlippedMessageSeen += SubmissionsArrived;
            Status = _game.IAmJudge
                    ? WAIT_AS_JUDGE
                    : CHOOSING_SUBMISSION;
            var nextPage = _game.IAmJudge
                    ? new JudgingPage()
                    : (BaseGamePage)new SelectCardPage();
            App.NextPage(nextPage);
            CheckStatus(new[] { WAIT_AS_JUDGE, CHOOSING_SUBMISSION });
        }

        public void ExitGame() {
            _px.OnWinningCardSelectedMessageSeen -= WinningCardPicked;
            _px.OnSubmissionsFlippedMessageSeen -= SubmissionsArrived;
            if (_game.Host.Equals(_me)) {
                WithdrawInvite();
            }
            ResetGame();
            PostOffice.Instance.ClearPublications();

            Status = SCANNING;
            Application.Current.MainPage = new NavigationPage(new MainPage());
        }

        //WAIT_AS_GUEST -> CHOOSING_SUBMISSION, WAIT_AS_JUDGE
        private void StartGameAsGuest(StartGameMessage message) {
            CheckStatus(WAIT_AS_GUEST);
            if (message.Sender != _game.Host) return;
            _px.ProcessMessage(message);
            if (!message.Recipient.Equals(_me)) return;

            SetUpGame(message);

            if (MyRSVPMessage != null) {
                _px.Unpublish(MyRSVPMessage);
                MyRSVPMessage = null;
            }
            CheckStatus(new[] { WAIT_AS_JUDGE, CHOOSING_SUBMISSION });
        }

        //  CHOOSING_SUBMISSION -> SUBMISSION_CHOSEN
        public void PlayCards(List<Card> cards) {
            _px.Publish(new PlayedCardMessage(cards, _game.Round));
            _game.PlayCards(cards);
            App.PlayWhoosh();
            Status = SUBMISSION_CHOSEN;
        }


        // SCANNING -> WAIT_AS_HOST
        public void SendInvite(Invitation i) {
            WithdrawInvite();
            _px.ClearPublications();
            MyInviteMessage = new ProposeGameMessage(i);
            _px.Publish(MyInviteMessage);

            Status = WAIT_AS_HOST;
        }


        // WAIT_AS_HOST -> SCANNING
        public void WithdrawInvite() {
            if (MyInviteMessage == null) return;
            try {
                _px.Unpublish(MyInviteMessage);
                MyInviteMessage = null;
                Status = SCANNING;
            } catch (Exception e) {
                Log.Error("Game:WithdrawInvite: Exception {@e}", e);
            }
        }

        // SCANNING -> WAIT_AS_GUEST
        // ReSharper disable once InconsistentNaming
        public void RSVP(Invitation i) {
            _px.ClearPublications();
            NewGame(i);
            Debug.Assert(MyRSVPMessage == null);
            if (MyRSVPMessage != null)
                _px.Unpublish(MyRSVPMessage);
            MyRSVPMessage = new JoinGameMessage(i.Host);
            Status = WAIT_AS_GUEST;
            _px.OnStartGameMessageSeen += StartGameAsGuest;
            _px.Publish(MyRSVPMessage);
        }

        // WAIT_AS_GUEST -> SCANNING
        public void UnRSVP() {
            CheckStatus(WAIT_AS_GUEST);
            if (MyRSVPMessage == null) return;
            try {
                _px.Unpublish(MyRSVPMessage);
                MyRSVPMessage = null;
                Status = SCANNING;
            } catch (Exception e) {
                Log.Error("Game:WithdrawInvite: Exception {@e}", e);
            }
        }


        public List<Player> NewRobotLineup()
        {
            return _robotNames.Shuffled().ToList().GetRange(1, Preferences.Instance.Robots).
                               Select(name => new Player(name + " the Robot", Guid.Empty)).ToList();
        }


        // WAIT_AS_HOST -> WAIT_AS_JUDGE, WAIT_AS_GUEST
        public async Task StartGameAsHost(List<Player> players, List<Player> robotPlayers) {
            CheckStatus(WAIT_AS_HOST);

            var library = DeckLibrary.Instance;
      
            //making this 'async' is probably pointless, but hey, why not.
            var shuffleTask = library.Shuffle();
      
            var prefs = Preferences.Instance;
      
            //We want our own hand processed last
            var loopList = players.ToList();
            Debug.Assert(loopList.Contains(_me));
            loopList.Remove(_me);
            loopList.Add(_me);
        
            var shuffled = await shuffleTask;
            var responseCards = shuffled[Response];
            var callCards = shuffled[Call];
            if (!prefs.NSFWAllowed) {
                responseCards.RemoveAll(x => x.NSFW);
                callCards.RemoveAll(x => x.NSFW);
            }


            if (responseCards.Count <=
                (_game.GameRules.HandsPerGame * 2) * (players.Count + robotPlayers.Count)
                + ( _game.GameRules.CardsPerHand) * players.Count)  {
                throw new GameException("Not enough response cards for a full game!",
                                        "Add more decks, or kick someone out of the game");
            } else if (callCards.Count < _game.GameRules.HandsPerGame) {
                throw new GameException("You have zero call cards!",
                                        "Add more (or different) decks.");
            } else {
                var newGuid = Guid.NewGuid();
                var numPlayers = players.Count;
                var responseStart = 0;
                var callStart = 0;

                //at least 1 call card exists
                var currentCard = callCards[0];
                callCards.RemoveAt(0);
                while (currentCard.Blanks == 1)
                {
                    callCards.Add(currentCard);
                    currentCard = callCards[0];
                    callCards.RemoveAt(0);
                }

                var responseIncrement = responseCards.Count / numPlayers;
                var callIncrement = callCards.Count / numPlayers;
                responseIncrement = (responseIncrement < 350) ? responseIncrement : 350;
                callIncrement = (callIncrement < 60) ? callIncrement : 60;

                foreach (var p in loopList) {
                    try {
                        var myResponseCards =
                                new Queue<Card>(
                                        responseCards.GetRange(responseStart, responseIncrement));
                        var myCallCards =
                                new Queue<Card>(callCards.GetRange(callStart, callIncrement));

                        callStart += callIncrement;
                        responseStart += responseIncrement;

                        var message = new StartGameMessage(p, players, robotPlayers, myResponseCards, myCallCards,
                                                           currentCard, prefs.CardsPerHand, prefs.HandsPerGame, prefs.NSFWAllowed,
                                                           newGuid);

                        //Deal our hand last to maintain null gameID on outgoing 
                        //messages until all hands are dealt
                        Log.Information("Created/updated message {m}", message);
                        if (p == _me) {
                            SetUpGame(message);
                        } else {
                            PostOffice.Instance.Publish(message);
                        }
                    } catch (Exception e) {
                        Log.Error("exception: {@e}", e);
                    }
                }
                Status = _game.CurrentJudge.Equals(_me)
                        ? WAIT_AS_JUDGE
                        : CHOOSING_SUBMISSION;
            }
        }

        // WAIT_AS_JUDGE -> SUBMISSIONS_FLIPPED, WAIT_AS_JUDGE
        private void SawCard(PlayedCardMessage message) {
            _px.ProcessMessage(message);

            if (!_game.HumanPlayers.Contains(message.Sender)) {
                Log.Error("Got a card submission from a player who isn't in the game???");
                return;
            }

            _game.AddSubmission(message.Sender, message.Cards);

            if (Status != WAIT_AS_JUDGE) return;
            if (_game.Submissions.Count == (_game.HumanPlayers.Count - 1)) {
                FlipCards();
                CheckStatus(SUBMISSION_FLIPPED);
            }
        }

        public void FlipCards() {
            //if all cards have been submitted
            _px.OnPlayedCardMessageSeen -= SawCard;
            foreach (var robot in _game.RobotPlayers) {
                try {
                    var sublist = new List<Card>();
                    for (var j = 0; j < _game.CallCard.Blanks; j++)
                        sublist.Add(_game.MyResponseCards.Dequeue());
                    _game.AddSubmission(robot, sublist);
                } catch {
                    Log.Error("Ran out of cards!");
                }
            }

            _px.ClearPublications(); // at this point, nobody needs anything from us
            var submissionsMessage = new SubmissionsFlippedMessage(_game.Submissions, _game.Round);
            _px.Publish(submissionsMessage);
            SubmissionsArrived(submissionsMessage);
            MyInviteMessage = null;
            MyRSVPMessage = null;
            CheckStatus(SUBMISSION_FLIPPED);
        }

        // (SUBMISSION_CHOSEN, WAIT_AS_JUDGE) -> SUBMISSION_FLIPPED
        private void SubmissionsArrived(SubmissionsFlippedMessage message) {
            CheckStatus(new[] { WAIT_AS_JUDGE, SUBMISSION_CHOSEN });
            _px.ProcessMessage(message);
            _game.SubmissionsArrived(message);
            Status = SUBMISSION_FLIPPED;
            App.NextPage(new FlippedCardsPage());
            CheckStatus(SUBMISSION_FLIPPED);
        }

        public void GameOver()
        {
            _game.GameID = Guid.Empty;
            _px.OnWinningCardSelectedMessageSeen -= WinningCardPicked;
            _px.OnSubmissionsFlippedMessageSeen -= SubmissionsArrived;

        }
        public void PickWinningResponse(List<Card> response) {
            if (response == null) return;
            var nextCard = _game.MyCallCards.Dequeue();
            var winner = _game.Submissions.FirstOrDefault(x => x.Value.SequenceEqual(response)).Key;
            var message = new WinningCardSelectedMessage(
                    response, winner, nextCard, _game.Score, _game.Round);
            _px.Publish(message);
            App.PlayWhoosh();
            WinningCardPicked(message);
        }

        // CHOOSING_SUBMISSION, SUBMISSION_CHOSEN, SUBMISSION_FLIPPED -> WINNER_CHOSEN
        private async void WinningCardPicked(WinningCardSelectedMessage message) {
            CheckStatus(new[] { CHOOSING_SUBMISSION, SUBMISSION_CHOSEN, SUBMISSION_FLIPPED });
            _px.ProcessMessage(message);

            _game.RegisterWinningCard(message.Sender, message.Winner, message.Score, message.Round);
            _nextCallCard = message.NextCallCard;
            //The 'await' in the popup means we can get duplicate alerts sometimes
            if (Status != WINNER_CHOSEN || message.Round > _game.Round) {
                //there should be no way to reach this point unless you are currently participating in a game
                if (message.Winner.Equals(_me))
                    App.PlayJingle();
                await Rg.Plugins.Popup.Services.PopupNavigation.Instance.PushAsync(new WinningCardPage
                                                                                           (message.Winner, _game.CallCard, message.Cards));
                Status = WINNER_CHOSEN;
            }
            CheckStatus(WINNER_CHOSEN);
        }

        public enum GameStatus {
            SCANNING, WAIT_AS_HOST, WAIT_AS_GUEST, WAIT_AS_JUDGE, CHOOSING_SUBMISSION, SUBMISSION_CHOSEN, SUBMISSION_FLIPPED, WINNER_CHOSEN
        }
    }

    public class GameException : Exception {
        public string Alert { get; }
        public string Detail { get; }
        public GameException(string alert, string detail) {
            Alert = alert;
            Detail = detail;
        }
    }

    public delegate void StatusHandler(GameController.GameStatus oldStatus,
                                       GameController.GameStatus newStatus);
}
