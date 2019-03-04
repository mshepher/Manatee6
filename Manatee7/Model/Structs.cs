using System;
using System.Data.SqlTypes;

namespace Manatee5.Model {
  [Serializable]
  public struct Invitation : IEquatable<Invitation> {
    
    public string GameName { get; }
    public Player Host { get; }

    public Invitation(string gameName, Player host) {
      GameName = gameName ?? throw new ArgumentNullException(nameof(gameName));
      Host = host;
    }

    public override bool Equals(object obj) {
      return (obj is Invitation i && i.Host.Equals(Host) && i.GameName == GameName);
    }

    public bool Equals(Invitation i) { 
      return (i.Host.Equals(Host) && i.GameName == GameName);
    }

    public override int GetHashCode() {
      return (Host.GetHashCode() ^ GameName.GetHashCode());
    }

    public static bool operator !=(Invitation a, Invitation b) {
      return !(a==b);
    } 
    
    public static bool operator ==(Invitation a, Invitation b) {
      return a.Equals(b);
    }
  }
  

  [Serializable]
  public struct Player : IEquatable<Player> {
    public string Name { get; }
    private Guid ID { get; }
    public Player(string name, Guid id) {
      Name = name;
      ID = id;
    }

    public bool Equals(Player p) { 
      return (p.Name == Name && p.ID == ID);
    }

    public override bool Equals(object obj) {
      return (obj is Player p && Equals(p));
    }

    public static bool operator !=(Player a, Player b) {
      return !(a==b);
    }

    public override int GetHashCode() {
      return (Name.GetHashCode() ^ ID.GetHashCode());
    }

    public static bool operator ==(Player a, Player b) {
      return a.Equals(b);
    }
  }

  [Serializable]
  public struct Rules {
    public int RobotPlayers { get; }
    public int CardsPerHand { get; }
    public bool NSFWAllowed { get; }

    public Rules(int robotPlayers, int cardsPerHand, bool nsfwAllowed) {
      RobotPlayers = robotPlayers;
      CardsPerHand = cardsPerHand;
      NSFWAllowed = nsfwAllowed;
    }
  }
}