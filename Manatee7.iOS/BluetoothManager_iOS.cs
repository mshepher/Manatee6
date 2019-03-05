using CoreBluetooth;
using Foundation;
using Manatee7.iOS;
using UIKit;
using Xamarin.Forms;

[assembly: Dependency(typeof(BluetoothManager_iOS))]
namespace Manatee7.iOS {
  public class BluetoothManager_iOS : IBluetoothManager {
    // https://stackoverflow.com/questions/43752085/prompt-user-to-enable-bluetooth-in-xamarin-forms
    private readonly CBCentralManager _manager = new CBCentralManager();

    public bool HasBluetooth => _manager.State != CBCentralManagerState.Unsupported;
    public bool HasPermission => _manager.State != CBCentralManagerState.Unauthorized;
    public bool PoweredOn => _manager.State != CBCentralManagerState.PoweredOff;
    public void EnableBluetooth() {
      UIApplication.SharedApplication.OpenUrl(new NSUrl("App-Prefs:root=Bluetooth")); 
    }
  }
}