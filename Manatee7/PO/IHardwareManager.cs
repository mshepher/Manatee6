namespace Manatee7 {
  public interface IHardwareManager {
    bool HasBluetoothPermission { get; }
    bool HasMicrophonePermission { get; }
    bool HasBluetoothAdapter { get; }
    bool BluetoothPoweredOn { get; }
    void EnableBluetooth(); // On Android we can switch bluetooth on programatically;
                            // On iOS we can send the user to settings.

  }
}