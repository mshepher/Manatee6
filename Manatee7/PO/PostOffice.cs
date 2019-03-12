using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using Manatee7.Model;
using Serilog;
using Xamarin.Forms;


namespace Manatee7 {
  public class PostOffice {
    private readonly Preferences _preferences = Preferences.Instance;
    private readonly HashSet<Guid> _processed = new HashSet<Guid>();

    public NearbyStrategy CurrentStrategy {
      get => _postOffice.CurrentStrategy;
      set {
        if (_postOffice == null || _postOffice.CurrentStrategy == value) return;
        _postOffice.CurrentStrategy = value;
        if (!Listening) return;
        Hibernate();
        Thaw();
      }
    }

    private readonly IPostOffice _postOffice = DependencyService.Get<IPostOffice>();
    private HashSet<NMessage> _publications;

    private int _counter;
    private readonly Game _game = Game.Instance;

    public bool Listening => (_postOffice != null && _postOffice.Listening);

    public bool HasPermission {
      get => _postOffice.HasPermission;
      set => _postOffice.HasPermission = value;
    }

    //I should be creating EventArgs classes for these, but I'm not going to
    public event HandleProposeGameMessage OnProposeGameMessageSeen;
    public event HandleJoinGameMessage OnJoinGameMessageSeen;
    public event HandleLeaveGameMessage OnLeaveGameMessageSeen;
    public event HandleStartGameMessage OnStartGameMessageSeen;
    public event HandleSubmissionsFlippedMessage OnSubmissionsFlippedMessageSeen;
    public event HandlePlayedCardMessage OnPlayedCardMessageSeen;
    public event HandleWinningCardSelectedMessage OnWinningCardSelectedMessageSeen;

    public event HandleProposeGameMessage OnProposeGameMessageWithdrawn;
    public event HandleJoinGameMessage OnJoinGameMessageWithdrawn;
/*
    public event MessageReceivedHandler OnMessageReceived;
    public event MessageLostHandler OnMessageLost;
    public event ErrorHandler OnBluetoothPermissionsError;
    public event ErrorHandler OnBluetoothPowerError;
    public event EventHandler OnSubscriptionExpired;
    public event EventHandler OnPublicationExpired;
*/
    public event BinaryEventHandler OnPermissionChanged;
    public event EventHandler DidSubscribe;
    public event EventHandler DidUnsubscribe;

    private PostOffice() {
      _publications = new HashSet<NMessage>();

      _preferences.PropertyChanged += (sender, args) => {
        if (args.PropertyName != "Strategy" || _postOffice == null ||
            _postOffice.CurrentStrategy == _preferences.Strategy) return;
        CurrentStrategy = _preferences.Strategy; // this setter includes a refresh
      }; // unlike permissions,
         // strategy values won't be preserved between reboots, hence incorporating preferences

      _postOffice.CurrentStrategy = _preferences.Strategy;

      _postOffice.OnMessageLost += (type, message) => {
        Log.Logger.Information("Lost {message}", type);
        RetractMessage(type, message);
      };
      _postOffice.OnMessageReceived += (type, message) => {
        Log.Logger.Information("Saw message of type {type} and length {length}", type, message.Length);
        SortMessage(type, message);
      };
      
      _postOffice.OnPermissionChanged += (hasPermission) => {
        Log.Information("in non-arch-specific post office; Permission changed: {p}", hasPermission);
        if (hasPermission) {
          if (_preferences.AutoConnect && !Listening) Thaw();
          else _preferences.AutoConnect = true; // only worry about permissions the first time
          }
        };

      _postOffice.OnBluetoothPowerError += error => {
        Log.Logger.Error("Bluetooth power error!");
      };
      _postOffice.OnSubscriptionExpired += () => {
        Log.Logger.Information("Subscription expired!");
      };
      _postOffice.OnBluetoothPermissionsError += error => {
        Log.Logger.Error("Bluetooth permissions error!");
      };
    }

    public static PostOffice Instance { get; } = new PostOffice();

    public void ProcessMessage(NMessage message) {
      Log.Information("Processed and archived message: {@message}", message);
      _processed.Add(message.MessageID);
    }
    
    public bool MessageProcessed(NMessage message) {
      return _processed.Contains(message.MessageID);
    }

    public void ClearMessageHistory() {
        if (!_postOffice.Listening) return;
        Unsubscribe();
        SafeSubscribe();
        Log.Information("Unsubscribed and resubscribed");
      }
    
    public void SafeSubscribe() {
      if (_postOffice == null) return;
      //Subscription must be done on main thread in case it invokes a permissions prompt
      Device.BeginInvokeOnMainThread(() => _postOffice.Subscribe());
      Log.Information("Subscribed");
      //Some Android devices won't scan for others unless publish()
      //has been called
      _postOffice.Ping();
      DidSubscribe?.Invoke();
      Debug.Assert(Listening);
    }

    public void Unsubscribe() {
      if (_postOffice != null) {
        _postOffice.Unsubscribe();
        Log.Information("Unsubscribed");
        _postOffice.DePing();
        DidUnsubscribe?.Invoke();
        Debug.Assert(!Listening);
      }
      else {
        Log.Information("Arch-specific PostOffice is null");
      }
    }

    public void ClearOldPublications() {
      if (_postOffice != null) {
        foreach (var message in _publications.Where(m => m.Round < _game.Round).ToList()) {
          Unpublish(message);
        }
      }
      else {
        Log.Information("Arch-specific PostOffice is null");
      }
    }

    public void ClearPublications() {
      if (_postOffice != null) {
        foreach (var message in _publications) {
          _postOffice.Unpublish(message);
        }
      }
      else {
        Log.Information("Arch-specific PostOffice is null");
      }

      _publications = new HashSet<NMessage>();
    }

    public void Hibernate() {
      Log.Information("Hibernating");
      if (_postOffice != null) {
        foreach (NMessage message in _publications) {
          _postOffice.Unpublish(message);
        }
        _postOffice.Unsubscribe();
        Log.Information("Unsubscribed");
      }
      else {
        Log.Information("Arch-specific PostOffice is null");
      }
    }

    public void Thaw() {
      Log.Information("Thawing");
      lock(_publications) {
        //not prompting the user every time they turn their screen on
      if (_postOffice != null && HasPermission) {
        foreach (var message in _publications) {
          _postOffice.Publish(message.MessageType, message);
        }
        _postOffice.Subscribe();
        Log.Information("Resubscribed");
      }
      else {
        Log.Information("Arch-specific PostOffice is null (or no permissions)");
      }
    }
    }

    //There has to be a better string -> type conversion for C#.  I just don't know what it is.
    //So, as long as I can't declare a variable without a type: this weirdness.  
    private void SortMessage(string type, byte[] message) {
      if (type.Length == 0) return;
      if (type == "ping") return;
      try {
        var payload = MessageFormatter.FromBytes(message);
        if (Math.Abs(payload.MessageVersion - GlobalConstants.MessageVersion) > 0.001) {
          Log.Warning("Saw incompatible message version {version} from {player}",
              payload.MessageVersion,payload.Sender);
        }

        lock(_processed) { 
          var iteration = _counter++;
          Log.Information("Entering SortMessage {i}", iteration);
          if (_processed.Contains(payload.MessageID)) {
            Log.Information("Saw previously seen message: {id}", payload.MessageID);
            return;
          }  else {
            Log.Information("Saw message: \n{@payload}", payload);
          }

          if (payload.GameID != _game.GameID) {
            Log.Information("GUID {a} and {b} don't match", payload.GameID, _game.GameID);
          } else if (payload.Round < _game.Round) {
            Log.Information("Message for round {r1}, but on round {r2}",
                payload.Round, _game.Round);
          } else {
            switch (type) {
              case nameof(ProposeGameMessage):
                OnProposeGameMessageSeen?.Invoke((ProposeGameMessage)payload);
                break;
              case nameof(JoinGameMessage):
                if (((JoinGameMessage) payload).Host.Equals(Preferences.Instance.Me))
                  OnJoinGameMessageSeen?.Invoke((JoinGameMessage)payload);
                break;
              case nameof(LeaveGameMessage):
                OnLeaveGameMessageSeen?.Invoke((LeaveGameMessage)payload);
                break;
              case (nameof(StartGameMessage)):
                if (((StartGameMessage) payload).Recipient.Equals(Preferences.Instance.Me))
                  OnStartGameMessageSeen?.Invoke((StartGameMessage)payload);
                break;
              case (nameof(SubmissionsFlippedMessage)):
                OnSubmissionsFlippedMessageSeen?.Invoke((SubmissionsFlippedMessage)payload);
                break;
              case (nameof(PlayedCardMessage)):
                OnPlayedCardMessageSeen?.Invoke((PlayedCardMessage)payload);
                break;
              case (nameof(WinningCardSelectedMessage)):
                OnWinningCardSelectedMessageSeen?.Invoke((WinningCardSelectedMessage)payload);
                break;
              default:
                Log.Information("Types: {a} {b}", type, payload.GetType());
                break;
            }
            Log.Information("Exiting  SortMessage {i}", iteration);
          }
        }
      }
      catch (Exception e) {
        Log.Error("caught exception : {@e} while trying to parse an incoming message.", e);
      }
    }



    private void RetractMessage(string type, byte[] message) {
      if (type == "" || type == "ping") return;
      var payload = MessageFormatter.FromBytes(message);
      Log.Information("Lost message of type {type}; sorting", type);
      if (payload.GetType() == typeof(ProposeGameMessage)) {
        OnProposeGameMessageWithdrawn?.Invoke((ProposeGameMessage) payload);
      }
      if (payload.GetType() == typeof(JoinGameMessage) && payload.GameID == _game.GameID) {
        OnJoinGameMessageWithdrawn?.Invoke((JoinGameMessage) payload);
      }
    }

    public void Publish(NMessage m) {
      var type = m.MessageType;
      lock (_publications) {
        if (_postOffice == null) {
          Log.Information("Arch-specific PostOffice is null");
          return;
        }
        _publications.Add(m);
        //Publication has to happen on main thread in case it triggers a permissions prompt
        Device.BeginInvokeOnMainThread(() => _postOffice.Publish(type, m));
        Log.Information("Sending message type {type} with GameID {gid}", type, m.GameID);
        Log.Information("content of publications is: {Newline}{@publications}", _publications);
      }
    }

    public void Unpublish(NMessage m) {
      _postOffice?.Unpublish(m);
        if (_publications.Contains(m)) _publications.Remove(m);
        Log.Information("UnPublishing message {message} of type {type}", m.MessageID,
            m.MessageType);
      }
      }


  //Technically these could all be HandleNMessage, but the IDE produces more useful error checking
  //this way
  public delegate void HandleNMessage(NMessage message);

  public delegate void HandleProposeGameMessage(ProposeGameMessage message);

  public delegate void HandleLeaveGameMessage(LeaveGameMessage message);

  public delegate void HandleJoinGameMessage(JoinGameMessage message);

  public delegate void HandleStartGameMessage(StartGameMessage message);

  public delegate void HandleSubmissionsFlippedMessage(SubmissionsFlippedMessage message);

  public delegate void HandlePlayedCardMessage(PlayedCardMessage message);

  public delegate void HandleWinningCardSelectedMessage(WinningCardSelectedMessage message);
  
}
