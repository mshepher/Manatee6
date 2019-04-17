using System.Threading.Tasks;

namespace Manatee7.PO {

    public interface IPostOffice {
    
        void Publish(string type, NMessage message);
        void Unpublish(NMessage message);

        void Subscribe();
        void Unsubscribe();

        bool Listening { get; }
        bool HasPermission { set; get; }
        NearbyStrategy CurrentStrategy { set; get; }
    
        //#event MessageHandler MessageReceived;
        event MessageReceivedHandler OnMessageReceived;
        event MessageLostHandler OnMessageLost;
        event BinaryEventHandler OnPermissionChanged;

        void Dispose();
    }

    public enum NearbyStrategy
    {
        Audio = 1 << 0,
        Ble = 1 << 1,
        Default = Audio | Ble
    }

    public delegate void MessageReceivedHandler(string type, byte[] message);
    public delegate void MessageLostHandler(string type, byte[] message);
    public delegate void ErrorHandler(bool error);
    public delegate void EventHandler();
    public delegate void BinaryEventHandler(bool b);


}