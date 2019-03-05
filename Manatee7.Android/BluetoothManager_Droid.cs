using Android.Bluetooth;
using Android.Content.PM;
using Manatee7.Droid;
using Xamarin.Forms;

[assembly: Dependency(typeof(BluetoothManager_Droid))]
namespace Manatee7.Droid {
  public class BluetoothManager_Droid : IBluetoothManager{
    private readonly BluetoothAdapter _adapter = BluetoothAdapter.DefaultAdapter;

    public bool HasPermission => true; //Android doesn't do bluetooth permissions?  Apparently?
    public bool HasBluetooth => 
      MainActivity.MostRecentActivity.PackageManager.HasSystemFeature(
          PackageManager.FeatureBluetoothLe);
    public bool PoweredOn => _adapter.IsEnabled;
    public void EnableBluetooth() {
      _adapter.Enable();
    }
  }
}