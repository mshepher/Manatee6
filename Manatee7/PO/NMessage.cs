using System;
using System.Collections.Generic;
using System.IO;
using Log = Serilog.Log;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using Manatee7.Model;
using Medallion;
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

//Serializable objects cannot have read-only properties, or most of these would 
//be get-only or private

namespace Manatee7 {
  public static class GlobalConstants {
    public const double MessageVersion = 1.0;
  }

  [Serializable]
  public abstract class NMessage : IEquatable<NMessage> {
    public double MessageVersion { get; set; }
    public string MessageType { get; set; }
    public Player Sender { get; set; }
    public Guid MessageID { get; }
    public Guid GameID { get; set; }
    public short Round { get; set; }

    protected NMessage() {
      MessageVersion = GlobalConstants.MessageVersion;
      Sender = Preferences.Instance.Me;
      MessageID = Guid.NewGuid();
      GameID = Game.Instance.GameID;
      MessageType = GetType().ToString().Split('.').Last();
      Round = Game.Instance.Round;
    }

    public bool Equals(NMessage m) {
      return m != null && m.MessageID == MessageID;
    }

    public override bool Equals(object obj) {
      return (obj is NMessage message) && Equals(message);
    }

    public override int GetHashCode() {
      return MessageID.GetHashCode();
    }
  }

  public static class MessageFormatter {
    private static readonly BinaryFormatter bf = new BinaryFormatter();

    public static byte[] ToBytes(NMessage message) {
      using (var m = new MemoryStream()) {
        bf.Serialize(m, message);
        return m.ToArray();
      }
    }

    public static NMessage FromBytes(byte[] bytes) {
      try {
        using (MemoryStream m = new MemoryStream()) {
          m.Write(bytes, 0, bytes.Length);
          m.Seek(0, SeekOrigin.Begin);
          return (NMessage) bf.Deserialize(m);
        }
      }
      catch (Exception e) {
        Log.Warning("Saw invalid message: {e}", e.Message);
        return null;
      }
    }
  }

  [Serializable]
  public class ProposeGameMessage : NMessage {
    public Invitation Invitation { get; set; }
    public ProposeGameMessage(Invitation invitation) {
      Invitation = invitation;
    }
  }

  [Serializable]
  public class JoinGameMessage : NMessage {
    public Player Host { set; get; }

    public JoinGameMessage(Player host) {
      Host = host;
    }
  }

  [Serializable]
  public class LeaveGameMessage : NMessage {
    public LeaveGameMessage(Guid gameID) {
      GameID = gameID;
    }
  }

  [Serializable]
  public class StartGameMessage : NMessage {
    public Player Recipient { get; set; }
    public List<Player> HumanPlayers { get; set; }
    public List<Player> RobotPlayers { get; set; }

    public Queue<Card> MyResponseCards { get; set; }
    public Queue<Card> MyCallCards { get; set; }
    public Card CallCard { get; set; }
    public Rules Rules { get; set; }
    public Guid NewGameID { get; set; }

    public StartGameMessage(Player recipient, List<Player> players, List<Player> robots, 
                            Queue<Card> myResponseCards, Queue<Card> myCallCards, Card callCard, int cardsPerHand,
                            int handsPerGame, bool nsfwAllowed, Guid newGameId) {
      Rules = new Rules(robots.Count, cardsPerHand, handsPerGame, nsfwAllowed);
      RobotPlayers = robots;
      Recipient = recipient;
      HumanPlayers = players;
      MyResponseCards = myResponseCards;
      MyCallCards = myCallCards;
      CallCard = callCard;
      NewGameID = newGameId;
    }
  }

  [Serializable]
  public class SubmissionsFlippedMessage : NMessage {
    public Dictionary<Player, List<Card>> Submissions { get; set; }
    public List<List<Card>> OrderedSubmissions { get; set; }

    public SubmissionsFlippedMessage(Dictionary<Player, List<Card>> submissions, short round) {
      Submissions = submissions;
      var l = submissions.Values.ToList();
      OrderedSubmissions = l.Shuffled().ToList();
      Round = round;
    }
  }

  [Serializable]
  public class PlayedCardMessage : CardMessage {
    public PlayedCardMessage(List<Card> cards, short round) {
      Cards = cards;
      Round = round;
    }
  }


  [Serializable]
  public class WinningCardSelectedMessage : CardMessage {
    public Card NextCallCard { get; set; }
    public Dictionary<Player, int> Score { get; set; }
    public Player Winner { get; set; }

    public WinningCardSelectedMessage(List<Card> cards, Player player, Card nextCard,
                                      Dictionary<Player, int> score, short round) {
      Winner = player;
      Cards = cards;
      NextCallCard = nextCard;
      Score = new Dictionary<Player, int>(score);
      Round = round;
    }
  }


  [Serializable]
  public abstract class CardMessage : NMessage {
    public List<Card> Cards { get; set; }
  }
}
