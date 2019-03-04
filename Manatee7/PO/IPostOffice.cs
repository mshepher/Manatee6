using System.Threading.Tasks;

namespace Manatee5 {

  public interface IPostOffice {
    
    void Publish(string type, NMessage message);
    void Unpublish(NMessage message);

    void Subscribe();
    void Unsubscribe();

    void Ping();
    void DePing();

    bool Listening { get; }
    bool HasPermission { set; get; }
    
    //#event MessageHandler MessageReceived;
    event MessageReceivedHandler OnMessageReceived;
    event MessageLostHandler OnMessageLost;
    event ErrorHandler OnBluetoothPermissionsError;
    event ErrorHandler OnBluetoothPowerError;
    event EventHandler OnSubscriptionExpired;
    event BinaryEventHandler OnPermissionChanged;

    void Dispose();
  }

  public delegate void MessageReceivedHandler(string type, byte[] message);
  public delegate void MessageLostHandler(string type, byte[] message);
  public delegate void ErrorHandler(bool error);
  public delegate void EventHandler();
  public delegate void BinaryEventHandler(bool b);


}
