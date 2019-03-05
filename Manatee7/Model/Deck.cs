using System;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Manatee7.Model {
[Serializable]
  public class Deck {
    public string Name { get; set; }

    public string Code { get; set; }
    public string Description { get; set; }
    public override string ToString() {
      return JsonConvert.SerializeObject(this).ToString();
    }
    //public IList<Card> cards { get; }
    public DateTimeOffset UpdatedAt { get; }
    public Deck() {
    }

    public bool Enabled {
      get => DeckLibrary.Instance.DeckIsEnabled(Code);
      set {
        if (value) 
          DeckLibrary.Instance.EnableDeck(Code);
        else
          DeckLibrary.Instance.DisableDeck(Code);
      }
    }

    [Newtonsoft.Json.JsonConstructor]
    public Deck(string name, string code, string description, string updated_at) { 
      Code = code;
      UpdatedAt = DateTimeOffset.Parse(updated_at);
      Name = name;
      Description = description;
    }
  }
}
