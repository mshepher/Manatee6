using AVFoundation;
using CoreBluetooth;
using Foundation;
using Manatee7.iOS;
using UIKit;
using Xamarin.Forms;

[assembly: Dependency(typeof(HardwareManager_iOS))]
namespace Manatee7.iOS {
    public class HardwareManager_iOS : IHardwareManager {
        // https://stackoverflow.com/questions/43752085/prompt-user-to-enable-bluetooth-in-xamarin-forms
        private readonly CBCentralManager _manager = new CBCentralManager();

        public bool HasBluetoothAdapter => _manager.State != CBCentralManagerState.Unsupported;
        public bool HasBluetoothPermission => _manager.State != CBCentralManagerState.Unauthorized;
        public bool BluetoothPoweredOn => _manager.State != CBCentralManagerState.PoweredOff;

        public bool HasMicrophonePermission {
            get {
                var authStatus =
                        AVCaptureDevice.GetAuthorizationStatus(AVMediaType.Audio);
                return authStatus == AVAuthorizationStatus.Authorized;
            }
        }

        public void EnableMicrophone() {
            UIApplication.SharedApplication.OpenUrl(new NSUrl(UIApplication.OpenSettingsUrlString));
        }

        public void EnableBluetooth() {
            UIApplication.SharedApplication.OpenUrl(new NSUrl("App-Prefs:root=Bluetooth")); 
        }
    }
}