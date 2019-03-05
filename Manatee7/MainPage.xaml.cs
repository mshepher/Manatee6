using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Serilog;
using Xamarin.Forms;
using Plugin.Connectivity;
using Manatee7.Model;
using System.Threading.Tasks;

namespace Manatee7 {
  public partial class MainPage {
    public MainPage() {
      InitializeComponent();
      _updateInvitations = (sender, args) => OnPropertyChanged(nameof(Invitations));
      
      //stuff that doesn't need to happen before the user sees a screen, or every time 
      //a modal window gets temporarily loaded and unloaded
      Task.Run(async () => {
        if (CrossConnectivity.IsSupported && !await CrossConnectivity.Current.IsRemoteReachable("googleapis.com")) 
          await DisplayAlert("Could not reach googleapis.com!",
              "Google Nearby won't work without an internet connection", "OK");
        else 
          ((App) Application.Current).DeckCheck();

        if (_bluetoothManager.HasBluetooth && !_bluetoothManager.PoweredOn)
          await DisplayBluetoothPowerAlert();

        PostOffice.Instance.SafeSubscribe();
      });
    }

    // ReSharper disable once MemberCanBeMadeStatic.Global
    public IEnumerable Invitations => GameController.Instance.VisibleInvitations.Distinct();
    
    private NotifyCollectionChangedEventHandler _updateInvitations; 

    private readonly IBluetoothManager _bluetoothManager = DependencyService.Get<IBluetoothManager>();
    
    protected override async void OnAppearing() {
      base.OnAppearing();
      GameController.Instance.VisibleInvitations.CollectionChanged += _updateInvitations;
      OnPropertyChanged(nameof(Invitations));
      }
      
    protected override void OnDisappearing() {
      base.OnDisappearing();
      GameController.Instance.VisibleInvitations.CollectionChanged -= _updateInvitations;
    }
    private async Task DisplayBluetoothPowerAlert() {
      if (await DisplayAlert("Bluetooth is powered off!",
          "Google Nearby needs either microphone access or Bluetooth (and works much better with both).",
          "Enable Bluetooth", "Leave Disabled"))
        _bluetoothManager.EnableBluetooth();
    }

    private void CreateGameButtonClicked(object sender, EventArgs e) {
      if (DeckLibrary.Instance.IsEmpty)
        DisplayAlert("You cannot start a game without at least one deck.",
            "Make sure you're connected to the internet and go to settings to download more decks.",
            "Ugh, fine.");
      else
        Navigation.PushModalAsync(new NewGamePage());
    }

    private void SettingsButtonClicked(object sender, EventArgs e) {
      Navigation.PushAsync(new SettingsPage());
    }

    //https://www.syncfusion.com/kb/8634/how-to-handle-the-button-event-when-itemtapped-event-is-triggered-in-sflistview-for-android-platform
    private void GameSelected(object sender, EventArgs e) {
      if (!(sender is Button button) || button.BindingContext == null) return;
      var inviteMessage = (Invitation)button.BindingContext;
      Rg.Plugins.Popup.Services.PopupNavigation.Instance.PushAsync(new JoinGamePage(inviteMessage));
    }
  }
}
