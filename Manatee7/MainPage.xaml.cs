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
using System.Threading;

namespace Manatee7
{
    public partial class MainPage
    {
        private static readonly PostOffice _px = PostOffice.Instance;

        public bool Scanning { get => _px.HasPermission && _px.Listening;
        }

        public MainPage()
        {
            InitializeComponent();
            _updateInvitations = (sender, args) => OnPropertyChanged(nameof(Invitations));
            GameController.Instance.VisibleInvitations.CollectionChanged += _updateInvitations;

            //stuff that doesn't need to happen before the user sees a screen, or every time 
            //a modal window gets temporarily loaded and unloaded
            Task.Run(async () =>
            {
                if (CrossConnectivity.IsSupported && !await CrossConnectivity.Current.IsRemoteReachable("googleapis.com"))
                    await DisplayAlert("Couldn't reach the messaging server!",
                        "Manatee can't talk to other players without an internet connection", "OK");
                else
                    ((App)Application.Current).DeckCheck();
            });


            _px.OnPermissionChanged += b => OnPropertyChanged("Scanning");
            _px.DidSubscribe += () => OnPropertyChanged("Scanning");
            _px.DidUnsubscribe += () => OnPropertyChanged("Scanning");
        }

        // ReSharper disable once MemberCanBeMadeStatic.Global
        public IEnumerable Invitations => GameController.Instance.VisibleInvitations.Distinct();

        private NotifyCollectionChangedEventHandler _updateInvitations;

        private readonly IBluetoothManager _bluetoothManager = DependencyService.Get<IBluetoothManager>();

        private async void CreateGameButtonClicked(object sender, EventArgs e)
        {
            if (!_px.Listening)
            {
                if (_px.HasPermission || Preferences.Instance.AutoConnect)
                {
                    _px.HasPermission = true; //they have already seen the permissions dialog (AutoConnect) and are 
                        //requesting a broadcast
                    
                }
                _px.SafeSubscribe();
            }

            if (DeckLibrary.Instance.IsEmpty)
                await DisplayAlert("You cannot start a game without at least one deck.",
                    "Make sure you're connected to the internet and go to settings to download more decks.",
                    "Ugh, fine.");
            else if (!_px.Listening) return;
            else
              await Navigation.PushModalAsync(new NewGamePage());
        }

        private void SettingsButtonClicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new SettingsPage());
        }

        //https://www.syncfusion.com/kb/8634/how-to-handle-the-button-event-when-itemtapped-event-is-triggered-in-sflistview-for-android-platform
        private void GameSelected(object sender, EventArgs e)
        {
            if (!(sender is Button button) || button.BindingContext == null) return;
            var inviteMessage = (Invitation)button.BindingContext;
            Rg.Plugins.Popup.Services.PopupNavigation.Instance.PushAsync(new JoinGamePage(inviteMessage));
        }

        private async void StartScanning(object sender, EventArgs e)
        {
            if (_px.HasPermission || Preferences.Instance.AutoConnect)
            {
                _px.HasPermission = true; //they've seen the permission dialog (AutoConnect or RequestPermission) 
                    //and are requesting a scan
            }
            _px.SafeSubscribe();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            OnPropertyChanged("Scanning"); // value may have changed in Settings
        }

        public async Task<bool> RequestPermission()
        {
            return true; /*
            //https://stackoverflow.com/questions/39652909/await-for-a-pushmodalasync-form-to-closed-in-xamarin-forms
            var waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
            var primerPage = new PermissionsPrimerPage();
            bool waitOutput = false;
            primerPage.ChoiceMade += (b) =>
            {
                waitHandle.Set();
                waitOutput = b;
            };
            await Rg.Plugins.Popup.Services.PopupNavigation.Instance.PushAsync(primerPage);
            await Task.Run(() => waitHandle.WaitOne());
            return waitOutput;*/
        }
    }


}
