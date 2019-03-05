namespace Manatee7 {
  public interface IBluetoothManager {
    bool HasPermission { get; }
    bool HasBluetooth { get; }
    bool PoweredOn { get; }
    void EnableBluetooth(); // On Android we can switch bluetooth on programatically;
                            // On iOS we can send the user to settings.

  }
}