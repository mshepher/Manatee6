using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Serilog;
using Xamarin.Forms;

namespace Manatee7.Model {
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

    private object GetAppProperty( object value,[CallerMemberName] string key = "") {
      Log.Debug("Fetching property {key}", key);
      if (!properties.ContainsKey(key) || properties[key].GetType() != value.GetType()) 
        properties[key] = value;
      return properties[key];
    }
    
    private void SetAppProperty(object value,[CallerMemberName] string key = "") {
      Log.Debug("Setting property {key} to {value}", key, value);
      properties[key] = value;
      OnPropertyChanged(key);
    }

    private Guid PlayerID => new Guid(
        (string)GetAppProperty(Guid.NewGuid().ToString()));

    public NearbyStrategy Strategy {
      get => (NearbyStrategy) GetAppProperty((int) NearbyStrategy.Default);
      set => SetAppProperty((int) value);
    }

    public string PlayerName {
      get => (string) GetAppProperty("");
      set => SetAppProperty(value);
    }
    
    public bool AutoConnect {
      get => (bool) GetAppProperty(false);
      set => SetAppProperty(value);
    }

    public int CardsPerHand {
      get => (int) GetAppProperty(7);
      set => SetAppProperty(value);
    }

    public int Robots {
      get => (int) GetAppProperty(1);
      set => SetAppProperty(value);
    }

    public bool SoundEffects {
      get => (bool) GetAppProperty(true);
      set => SetAppProperty(value);
    }

    public bool UseNearby {
      get => (bool)GetAppProperty(false);
      set => SetAppProperty(value);
    }

    public bool NSFWAllowed {
      get => (bool)GetAppProperty(true);
      set => SetAppProperty(value);
    }
    public bool DiscardsAllowed {
      get =>(bool)GetAppProperty(true);
      set => SetAppProperty(value);
    }

    public Player Me =>
        new Player(PlayerName, PlayerID);

    //https://forums.xamarin.com/discussion/99191/xamarin-forms-mvvm-in-c-propertychanged-event-handler-is-always-null-when-onpropertychanged-call
    protected virtual void OnPropertyChanged(string propertyName) {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}