using System;
using System.Collections.Generic;
using System.ComponentModel;
using Xamarin.Forms;

namespace Manatee5.Model {
  public class Preferences : INotifyPropertyChanged {
    private Preferences() {
    }

    public static async void Save() {
      await Application.Current.SavePropertiesAsync();
    }

    public void Save(object sender, EventArgs e) {
      Save();
    }

    private readonly IDictionary<string, object> properties = Application.Current.Properties;

    public static Preferences Instance { get; } = new Preferences();

    public event PropertyChangedEventHandler PropertyChanged;

    public static Preferences GetPreferences() {
      return Instance;
    }

    private object GetAppProperty(string key, object value) {
      if (!properties.ContainsKey(key) || properties[key].GetType() != value.GetType()) 
        properties[key] = value;
      return properties[key];
    }
    
    private void SetAppProperty(string key, object value) {
      properties[key] = value;
      OnPropertyChanged("PlayerName");
    }

    public Guid PlayerID => new Guid(
        (string)GetAppProperty("PlayerID", Guid.NewGuid().ToString()));

    public string PlayerName {
      get => (string) GetAppProperty("PlayerName", "");
      set {
        SetAppProperty("PlayerName",value);
      }
    }

    public int CardsPerHand {
      get => (int) GetAppProperty("CardsPerHand", 7);
      set => SetAppProperty("CardsPerHand",value);
    }

    public int Robots {
      get => (int) GetAppProperty("Robots", 1);
      set => SetAppProperty("Robots",value);
    }

    public bool SoundEffects {
      get => (bool) GetAppProperty("SoundEffects", true);
      set => SetAppProperty("SoundEffects",value);
    }

    public bool UseNearby {
      get => (bool)GetAppProperty("UseNearby", false);
      set => SetAppProperty("UseNearby",value);
    }

    public bool NSFWAllowed {
      get => (bool)GetAppProperty("NSFWAllowed", true);
      set => SetAppProperty("NSFWAllowed",value);
    }
    public bool DiscardsAllowed {
      get =>(bool)GetAppProperty("AllowDiscards", true);
      set => SetAppProperty("AllowDiscards",value);
    }

    public Player Me =>
        new Player(PlayerName, PlayerID);

    //https://forums.xamarin.com/discussion/99191/xamarin-forms-mvvm-in-c-propertychanged-event-handler-is-always-null-when-onpropertychanged-call
    protected virtual void OnPropertyChanged(string propertyName) {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}
