using System;
using System.Collections;
using System.Collections.Specialized;
using System.Linq;
using Xamarin.Forms;
using Plugin.Connectivity;
using Manatee7.Model;
using System.Threading.Tasks;
using Manatee7.PO;
using Serilog;

namespace Manatee7
{
    public partial class MainPage {
        private static readonly PostOffice _px = PostOffice.Instance;

        public bool Scanning => _px.HasPermission && _px.Listening;

        public MainPage() {
            InitializeComponent();
            _updateInvitations = (sender, args) => OnPropertyChanged(nameof(Invitations));
            GameController.Instance.VisibleInvitations.CollectionChanged += _updateInvitations;

            //stuff that doesn't need to happen before the user sees a screen, or every time 
            //a modal window gets temporarily loaded and unloaded
            Task.Run(async () => {
                if (CrossConnectivity.IsSupported &&
                    !await CrossConnectivity.Current.IsRemoteReachable("googleapis.com"))
                    await DisplayAlert("Couldn't reach the messaging server!",
                                       "Manatee can't talk to other players without an internet connection",
                                       "OK");
                else ((App) Application.Current).DeckCheck();
            });


            _px.OnPermissionChanged += b => OnPropertyChanged("Scanning");
            _px.DidSubscribe += () => OnPropertyChanged("Scanning");
            _px.DidUnsubscribe += () => OnPropertyChanged("Scanning");
        }

        // ReSharper disable once MemberCanBeMadeStatic.Global
        public IEnumerable Invitations => GameController.Instance.VisibleInvitations.Distinct();

        private readonly NotifyCollectionChangedEventHandler _updateInvitations;

        private async void CreateGameButtonClicked(object sender, EventArgs e) {
            if (!_px.Listening) {
                if (_px.HasPermission || Preferences.Instance.AutoConnect) {
                    //they have already seen the permissions dialog (AutoConnect) and are 
                    //requesting a broadcast
                    _px.HasPermission = true; 

                } else if (await RequestPermission())
                    _px.SafeSubscribe();
                OnPropertyChanged("Scanning");
            }

            if (DeckLibrary.Instance.IsEmpty)
                await DisplayAlert("You cannot start a game without at least one deck.",
                                   "Make sure you're connected to the internet and go to settings to download more decks.",
                                   "Ugh, fine.");
            if (_px.Listening) await Navigation.PushModalAsync(new NewGamePage());
        }

        private void SettingsButtonClicked(object sender, EventArgs e) {
            Navigation.PushAsync(new SettingsPage());
        }

        //https://www.syncfusion.com/kb/8634/how-to-handle-the-button-event-when-itemtapped-event-is-triggered-in-sflistview-for-android-platform
        private void GameSelected(object sender, EventArgs e) {
            if (!(sender is Button button) || button.BindingContext == null) return;
            var inviteMessage = (Invitation) button.BindingContext;
            Rg.Plugins.Popup.Services.PopupNavigation.Instance.PushAsync(
                    new JoinGamePage(inviteMessage));
        }

        private async void StartScanning(object sender, EventArgs e) {
            if (_px.HasPermission || Preferences.Instance.AutoConnect) {
                _px.HasPermission =
                        true; //they've seen the permission dialog (AutoConnect or RequestPermission) 
                //and are requesting a scan
            }
            else if (await RequestPermission())
                _px.SafeSubscribe();
            OnPropertyChanged("Scanning");
        }

        protected override void OnAppearing() {
            base.OnAppearing();
            OnPropertyChanged(propertyName: "Scanning"); // value may have changed in Settings
        }

        private async Task<bool> RequestPermission() {
            return Device.RuntimePlatform != Device.iOS || 
                   await DisplayAlert("Manatee uses Google Nearby to connect to nearby devices",
                                      "Google Nearby sends and receives messages via Bluetooth, internet, and ultrasonic (and near ultrasonic) audio.",
                                      "Allow", "Cancel")
                    ;

        }

        public async void GetHelp(object sender, EventArgs e) {
            await Rg.Plugins.Popup.Services.PopupNavigation.Instance.PushAsync(
                    new TroubleshootingPage());

        }
    }
}
