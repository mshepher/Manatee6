using System;
using Serilog;
using System.Threading.Tasks;
using System.Linq;
using Newtonsoft.Json;
using Xamarin.Forms;
using System.Collections.Generic;
using System.ComponentModel;
using Medallion;
using static Manatee5.Model.Card.Type;
#pragma warning disable 4014

namespace Manatee5.Model {
  public sealed class DeckLibrary : INotifyPropertyChanged {

    //the list of decks is small enough that it can be read and written as a single object

    private readonly IDictionary<string, object> _properties =
        Application.Current.Properties;

    private IEnumerable<string> Codes {
      get {
        return _properties.Keys.Where(s => s.EndsWith(CardSuffix)).
            Select(s => s.Substring(0, s.IndexOf(CardSuffix, StringComparison.Ordinal))).
            Where(s => _properties.ContainsKey(s) && _properties.ContainsKey(EnabledKey(s)));
      }
    }
    private const string CardSuffix = "-cards";
    private const string EnabledSuffix = "-enabled";
    private static string CardKey(string code) {
      return code + "-cards";
    }
    
    private static string EnabledKey(string code) {
      return code + "-enabled";
    }

    private readonly System.Net.Http.HttpClient _client = new System.Net.Http.HttpClient() {
        Timeout = TimeSpan.FromMilliseconds(5000)
    };

    public event PropertyChangedEventHandler PropertyChanged;

    public Dictionary<string, Deck> Decks {
      get {
        var retVal = new Dictionary<string, Deck>();
        foreach (var code in Codes) {
          try {
            retVal[code] = JsonConvert.DeserializeObject<Deck>(_properties[code] as string);
          }
          catch (Exception exception) {
            Log.Error("Failed to add deck {code} to Decks property: {exception}{Newline} Wiping {code} from the face of the earth just to be safe.",
                code, exception.Message, code);
            RemoveDeck(code);
          }
        }
        return retVal;
      }
    }

    //this should gracefully ignore non-deck-code keys
    private IEnumerable<string> EnabledDecks => _properties.Keys.Where(DeckIsEnabled);

    public bool DeckIsEnabled(string code) {
      return (_properties.ContainsKey(code) && 
              _properties.ContainsKey(EnabledKey(code)) && 
              _properties[EnabledKey(code)] is bool b && 
              b);
    }

    public void EnableDeck(string code) {
      _properties[EnabledKey(code)] = true;
      Application.Current.SavePropertiesAsync();
    }
    
    public void DisableDeck(string code) {
      _properties[EnabledKey(code)] = false;
      Application.Current.SavePropertiesAsync();
    }
    
    public async Task<Dictionary<Card.Type,List<Card>>> Cards(string code)
    {
      if (!_properties.ContainsKey(CardKey(code))) return null;
      try {
        var cardList = await Task.Run(() =>
            JsonConvert.DeserializeObject<Dictionary<string, List<Card>>>(
                (string) _properties[CardKey(code)]));
        return new Dictionary<Card.Type, List<Card>> {
            [Call] = cardList["calls"], [Response] = cardList["responses"]
        };
      }
      catch (Exception e) {
        Log.Error("Failed to fetch cards for {code}: {exception}{Newline} Wiping {code} from the face of the earth just to be safe.",
            code, e.Message, code);
        RemoveDeck(code);
        return new Dictionary<Card.Type, List<Card>> {
            [Call] = new List<Card>(), [Response] = new List<Card>()
        };
      }
    }

    public static DeckLibrary Instance { get; } = new DeckLibrary();

    private DeckLibrary() {
      //clean leftovers of previous deletions
      foreach (var code in Codes) {
        if (_properties[code] == null || _properties[EnabledKey(code)] == null)
          RemoveDeck(code);
      }
    }

    /*
    public void AddDeck(Deck d) {
      decks.Put(d.Code, d);
    }*/

    private async Task<string> FetchDeck(string code) {
      var response = await _client.GetAsync(
          "https://api.cardcastgame.com/v1/decks/" + code);
      response.EnsureSuccessStatusCode();
      var jsonText = await response.Content.ReadAsStringAsync();
      return jsonText;
    }

    private async Task<string> FetchCards(string code) {
      var response = await _client.GetAsync(
          $"https://api.cardcastgame.com/v1/decks/{code}/cards");
      response.EnsureSuccessStatusCode();
      var jsonText = await response.Content.ReadAsStringAsync();
      return jsonText;
    }
    
    public bool IsEmpty => !Codes.Any();

    public async Task AddDeckFromCode(string code) {
      var deckTask = await FetchDeck(code);
      var cardsTask = await FetchCards(code);
      _properties[code] = deckTask;
      _properties[CardKey(code)] = cardsTask;
      _properties[EnabledKey(code)] = true;
      
      Application.Current.SavePropertiesAsync();
      OnPropertyChanged(nameof(Decks));
      OnPropertyChanged(nameof(Codes));
      OnPropertyChanged(nameof(EnabledDecks));
      OnPropertyChanged(nameof(IsEmpty));
    }


    public void RemoveDeck(string code) {
      if (!_properties.ContainsKey(code))
        Log.Error("key {} not found in Properties", code);
      else _properties.Remove(code);
      
      if (!_properties.ContainsKey(CardKey(code)))
        Log.Error("key {} not found in Properties", CardKey(code));
      else _properties.Remove(CardKey(code));

      if (!_properties.ContainsKey(EnabledKey(code)))
        Log.Error("key {} not found in Properties", EnabledKey(code));
      else _properties.Remove(EnabledKey(code));
      
      Application.Current.SavePropertiesAsync();
      OnPropertyChanged(nameof(Decks));
      OnPropertyChanged(nameof(EnabledDecks));
      OnPropertyChanged(nameof(IsEmpty));
    }

    //https://forums.xamarin.com/discussion/99191/xamarin-forms-mvvm-in-c-propertychanged-event-handler-is-always-null-when-onpropertychanged-call
    //https://forums.xamarin.com/discussion/129292/is-firing-propertychanged-event-from-a-non-ui-thread-safe
    private void OnPropertyChanged(string propertyName) {
      Device.BeginInvokeOnMainThread(() =>
          PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)));
    }

    public async Task<Dictionary<Card.Type, List<Card>>> Shuffle(IEnumerable<string> codes) {
      var retVal = new Dictionary<Card.Type, List<Card>> {
          [Response] = new List<Card>(), [Call] = new List<Card>()
      };
      foreach (var code in codes) {
        var cards = await Cards(code);
        try {
          retVal[Call].AddRange(cards[Call]); 
          retVal[Response].AddRange(cards[Response]); 
        }
        catch (Exception e) {
          Log.Error("Could not add deck {code} to shuffled deck; exception: {e}",
              code, e);
        }
      }

      retVal[Call].Shuffle();
      retVal[Response].Shuffle();
      return retVal;
    }
    
    public async Task<Dictionary<Card.Type, List<Card>>> Shuffle() {
      return await Shuffle(EnabledDecks);
    }
  }
}