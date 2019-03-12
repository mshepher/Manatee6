using System;
using System.Collections.Concurrent;
using Serilog;
using Foundation;
using NearbyMessages;
using Xamarin.Forms;
using Manatee7.iOS;

[assembly: Dependency(typeof(PostOffice_iOS))]

namespace Manatee7.iOS {
  // ReSharper disable once InconsistentNaming
  public class PostOffice_iOS : IPostOffice, IDisposable {
    private const string ApiKey = "AIzaSyDo-3hY1qVya1oeutJ44JyJAx8ICMr85AA";
    private GNSMessageManager _manager;
    private GNSSubscription _listener;

    private ConcurrentDictionary<NMessage, GNSPublication> _publications =
        new ConcurrentDictionary<NMessage, GNSPublication>();

    private GNSMessage _ping;
    // ReSharper disable once NotAccessedField.Local
    private readonly GNSPermission _permission;
    
    public bool HasPermission {
      get => GNSPermission.IsGranted;
      set => GNSPermission.SetGranted(value);
    }

    public bool Listening => _listener != null;

    public NearbyStrategy CurrentStrategy {
      set {
        _mediumStrategy = value == NearbyStrategy.Ble
            ? GNSDiscoveryMediums.Ble
            : GNSDiscoveryMediums.Default;
        _strategy = GNSStrategy.StrategyWithParamsBlock(
            strategyParams => strategyParams.DiscoveryMediums =
                _mediumStrategy);
      }
      get => _mediumStrategy == GNSDiscoveryMediums.Ble
          ? NearbyStrategy.Ble
          : NearbyStrategy.Default;
    }

    private static GNSDiscoveryMediums _mediumStrategy = GNSDiscoveryMediums.Default;

    private GNSStrategy _strategy = GNSStrategy.StrategyWithParamsBlock(
        strategyParams => strategyParams.DiscoveryMediums = 
            _mediumStrategy);

    public event MessageReceivedHandler OnMessageReceived;
    public event MessageLostHandler OnMessageLost;
    public event ErrorHandler OnBluetoothPermissionsError;
    public event ErrorHandler OnBluetoothPowerError;
    public event EventHandler OnSubscriptionExpired;
    public event BinaryEventHandler OnPermissionChanged;

    public PostOffice_iOS() {

      GNSMessageManager.SetDebugLoggingEnabled(true);

      _permission = new GNSPermission((permissionGranted) => {
        Log.Information("Permission changed: {p}", permissionGranted);
        OnPermissionChanged?.Invoke(permissionGranted);
      });

      //fixme 
      Log.Information("Is Nearby permission granted? {p}", GNSPermission.IsGranted);

      Log.Logger.Information("In PostOffice_iOS constructor");

      _manager = new GNSMessageManager(ApiKey, (messageManagerParams) => {
        messageManagerParams.BluetoothPermissionErrorHandler = (error) => {
          OnBluetoothPermissionsError?.Invoke(error);
        };
        messageManagerParams.BluetoothPowerErrorHandler = (error) => {
          OnBluetoothPowerError?.Invoke(error);
        };
      });

    }

    public void Dispose() {
      _permission.Dispose();
      _manager = null;
      _listener = null;
      _strategy = null;
      foreach (var p in _publications.Values) p.Dispose();
      _publications = null;
    }

    public void Publish(string type, NMessage message) {
      Log.Information("Entering with permission: {p}", GNSPermission.IsGranted);
      var bytes = NSData.FromArray(MessageFormatter.ToBytes(message));
      var encodedMessage = GNSMessage.MessageWithContent(bytes, type);
      Log.Information("Publishing message of size {size}", bytes.Length);
      _publications[message] = _manager.PublicationWithMessage(encodedMessage, 
      publicationParams => publicationParams.Strategy = _strategy);
    }

    public void Unpublish(NMessage message) {
      // ReSharper disable once NotAccessedVariable
      GNSPublication ignoreMe;
      if (!_publications.ContainsKey(message)) return;
      _publications[message].Dispose();
      _publications.TryRemove(message, out ignoreMe);
    }

    private void MessageFoundHandler(GNSMessage message) {
      OnMessageReceived?.Invoke(message.Type, message.Content.ToArray());
    }

    private void MessageLostHandler(GNSMessage message) {
      OnMessageLost?.Invoke(message.Type, message.Content.ToArray());
    }

    public void Subscribe() {
      Log.Information("Entering with permission: {p}", GNSPermission.IsGranted);
      _listener?.Dispose();
      _listener = null;
      _listener = _manager.SubscriptionWithMessageFoundHandler(
          MessageFoundHandler, MessageLostHandler, (subscriptionParams) => {
            subscriptionParams.DeviceTypesToDiscover = GNSDeviceTypes.UsingNearby;
            subscriptionParams.Strategy = _strategy;
          });
    }

    public void Unsubscribe() {
      _listener?.Dispose();
      _listener = null;
    }

    public void Ping() {
      Log.Information("Entering with permission: {p}", GNSPermission.IsGranted);
      _ping?.Dispose(); // prevent multiplication of pings
      _ping = GNSMessage.MessageWithContent(NSData.FromString("ping"), "");
    }

    public void DePing() {
      _ping.Dispose();
    }
  }
}
