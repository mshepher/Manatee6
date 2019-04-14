using System.Text;
using Serilog;
using Android.Gms.Nearby;
using Android.Gms.Nearby.Messages;
using System.Collections.Generic;
using Message = Android.Gms.Nearby.Messages.Message;
using Xamarin.Forms;

[assembly: Dependency(typeof(Manatee7.Droid.PostOffice_Droid))]
namespace Manatee7.Droid {
    public class PostOffice_Droid : IPostOffice {

        private bool _hasSystemPermission;
        private bool HasUserPermission
        {
            get => (bool)Application.Current.Properties["UserNearbyPermission"];
            set => Application.Current.Properties["UserNearbyPermission"] = value;
        }
        
        public bool HasPermission
        {
            get => _hasSystemPermission && HasUserPermission;
            set
            {
                if (HasPermission && !value)
                {
                    Unsubscribe();
                }

                if (_hasSystemPermission && value )
                {
                    Subscribe();
                }

                HasUserPermission = value;
            }
        }

        private static ManateeListener _listener;
        private static MessagesClient _client;
        public bool Listening { get; private set; }

        public event MessageReceivedHandler OnMessageReceived;
        public event MessageLostHandler OnMessageLost;
        public event ErrorHandler OnBluetoothPermissionsError;
        public event ErrorHandler OnBluetoothPowerError;
        public event BinaryEventHandler OnPermissionChanged;
        public event EventHandler OnSubscriptionExpired;

        public NearbyStrategy CurrentStrategy {
            set {
                _strategy = value == NearbyStrategy.Ble ? Strategy.BleOnly : Strategy.Default;
                _publishOptions = new PublishOptions.Builder().SetStrategy(_strategy).Build();
            }
            get => _strategy == Strategy.BleOnly ? NearbyStrategy.Ble : NearbyStrategy.Default;
        } 

        private static Strategy _strategy = Strategy.Default;

        private PublishOptions _publishOptions; 
    
        private readonly Dictionary<NMessage, Message> _publications 
                = new Dictionary<NMessage, Message>();

        public PostOffice_Droid() {
            OnPermissionChanged += (b) => _hasSystemPermission = b;

            if (!Application.Current.Properties.ContainsKey("UserNearbyPermission"))
                Application.Current.Properties["UserNearbyPermission"] = false;

            _publishOptions = new PublishOptions.Builder().SetStrategy(_strategy).Build();
       
            _listener = new ManateeListener();

            _listener.OnMessageLost += (type, content) => {
                OnMessageLost?.Invoke(type, content);
            };

            _listener.OnMessageReceived += (type, content) => {
                OnMessageReceived?.Invoke(type, content);
            };
      
            OnSubscriptionExpired += Subscribe;
            Log.Information("About to create client");
            _client = NearbyClass.GetMessagesClient(
                    MainActivity.MostRecentActivity);
            _client.RegisterStatusCallback(new StatCallback(
                                                   permissionChanged => {
                                                       OnPermissionChanged?.Invoke(permissionChanged);
                                                   }
                                           ));
            Log.Information("Created client");
        }

        public void Subscribe() {
            Log.Information("Subscribing");

            _client.Subscribe(_listener, new SubscribeOptions.
                                              Builder().SetStrategy(_strategy).
                                                        SetCallback(new SubCallback(RaiseSubscriptionExpired)).
                                                        Build());
            Listening = true;
            //some android devices don't start listening until they publish a message
            Ping();
        }

        private void RaiseSubscriptionExpired() {
            Log.Logger.Information("Subscription expired");
            Listening = false;
            OnSubscriptionExpired?.Invoke();
        }

        public void Unsubscribe() {
            Listening = false;
            Log.Information("Unsubscribing");
            _client.Unsubscribe(_listener);
            DePing();
        }

        public void Dispose() {
            Listening = false;
            _listener.Dispose();
            _client.Dispose();
        }

        public void Publish(string type, NMessage message) {
            var bytes = MessageFormatter.ToBytes(message);
            _publications[message] = new Message(bytes, type);
            Log.Information("Publishing message of size {size}", bytes.Length);
            _client.Publish(_publications[message],_publishOptions);
        }

        public void Unpublish(NMessage message) {
            if (!_publications.ContainsKey(message))
                Log.Error("Trying to unpublish message, but message not found in " +
                          "droid-specific publication list: {message}", message);
            else { 
                _client.Unpublish(_publications[message]);
                _publications.Remove(message);
            }
        }

        private readonly Message _ping = new Message(Encoding.ASCII.GetBytes("one ping to rule them all"), "ping");

        private void Ping() {
            _client.Publish(_ping);
        }

        private void DePing() {
            _client.Unpublish(_ping);
        }
    }

    // Helper methods/classes

    public class ManateeListener : MessageListener {
        public event MessageReceivedHandler OnMessageReceived;
        public event MessageLostHandler OnMessageLost;

        public override void OnLost(Message message) {
            OnMessageLost?.Invoke(message.Type, message.GetContent());
        }

        public override void OnFound(Message message) {
            OnMessageReceived?.Invoke(message.Type, message.GetContent()); 
        }
    }

  
    // Android.Nearby requires me to create a new object class for every type of callback
    // it's so inelegant

    public class SubCallback : SubscribeCallback {
        private readonly EventHandler _handleEvent;
        public SubCallback(EventHandler e) {
            _handleEvent = e;
        }

        public override void OnExpired() {
            _handleEvent();
        }
    }

    public class PubCallback : PublishCallback {
        private readonly EventHandler _handleEvent;
        public PubCallback(EventHandler e) {
            _handleEvent = e;
        }

        public override void OnExpired() {
            _handleEvent();
        }
    }

    public class StatCallback : StatusCallback {
        private readonly BinaryEventHandler _handleEvent;
        public StatCallback(BinaryEventHandler e) {
            _handleEvent = e;
        }

        public override void OnPermissionChanged(bool permissionGranted) {
            _handleEvent(permissionGranted);
        }
    }
}