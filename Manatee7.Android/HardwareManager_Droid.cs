using Android.Bluetooth;
using Android.Content.PM;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Manatee7.Droid;
using Xamarin.Forms;
using Perm = Android.Manifest.Permission;

[assembly: Dependency(typeof(HardwareManager_Droid))]
namespace Manatee7.Droid {
    public class HardwareManager_Droid : IHardwareManager {
        private readonly BluetoothAdapter _adapter = BluetoothAdapter.DefaultAdapter;

        public bool HasBluetoothPermission =>
            (ContextCompat.CheckSelfPermission(
                MainActivity.MostRecentActivity, Perm.Bluetooth) == (int) Permission.Granted);
        
        public bool HasMicrophonePermission =>
           (ContextCompat.CheckSelfPermission(
               MainActivity.MostRecentActivity, Perm.RecordAudio) == (int)Permission.Granted);

        public bool HasBluetoothAdapter =>
          MainActivity.MostRecentActivity.PackageManager.HasSystemFeature(
              PackageManager.FeatureBluetoothLe);

        public bool BluetoothPoweredOn => _adapter.IsEnabled;

        public void EnableMicrophone() {
            ActivityCompat.RequestPermissions(MainActivity.MostRecentActivity, 
                                              new string[] {Perm.RecordAudio}, 
                                              0);
        }
        
        public void EnableBluetooth() {
            _adapter.Enable();
        }
    }
}