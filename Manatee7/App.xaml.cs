using System;
using System.Collections.ObjectModel;
using Serilog;
using Xamarin.Forms;
using Plugin.Connectivity;
using Xamarin.Forms.Xaml;
using System.Reflection;
using System.Threading.Tasks;
using Manatee7.Model;
using Manatee7.PO;
using Plugin.SimpleAudioPlayer;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Crashes;
using Rg.Plugins.Popup.Services;
using Device = Xamarin.Forms.Device;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]

namespace Manatee7 {

    public partial class App : Application {

        private static readonly ISimpleAudioPlayer Jingle = CrossSimpleAudioPlayer.CreateSimpleAudioPlayer();
        private static readonly ISimpleAudioPlayer Whoosh = CrossSimpleAudioPlayer.CreateSimpleAudioPlayer();

        public App() {
            Log.Information("Entering App Constructor");

            //DEBUG ONLY

            //https://forums.xamarin.com/discussion/141808/system-dialogs-triggering-onsleep-ios
            // We need to associate OnSleep/OnResume behavior with non-default system events in
            // iOS.  By default, system dialogs triggering a sleep/wake cycle on iOS, and
            // users are trapped in an endless cycle of popups if they refuse network
            // permissions or don't switch on Bluetooth.  Oops.

            MessagingCenter.Subscribe<string>(this, "iOSSleep", (str) => { OnSleepSubroutine(); });
            MessagingCenter.Subscribe<string>(this, "iOSWake", (str) => { OnWakeSubroutine(); });


            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(
                    "ODUwMzRAMzEzNzJlMzEyZTMwZzFaeVhNYVVGSUI2UG1Ca3hHZU5XcVZwdzh6djloN3VqR2xqMXhMbnRlRT0=");
            Log.Information("Registered license");
      
            InitializeComponent();
            Log.Information("Initialized component");
            Log.Information("Starting with properties: {p}", Properties.Keys);

            //https://askxammy.com/playing-sounds-in-xamarin-forms/
            try {
                var assembly = typeof(App).GetTypeInfo().Assembly;
                Log.Information("Loading audio files");
                Whoosh.Load(assembly.GetManifestResourceStream("Manatee7.Resources.whoosh.wav"));
                Jingle.Load(assembly.GetManifestResourceStream("Manatee7.Resources.jingle.wav"));
                Log.Information("Loaded audio player");
            }
            catch (Exception e) {
                //Sound effects aren't vital, so we can continue without them
                Log.Error("Failed to load audio player; caught exception: {NewLine}{e}", e);
            }
      
            if (Preferences.Instance.PlayerName == "") {
                MainPage = new WelcomePage(); //FORCE the player to choose a username.
            }
            else {
                MainPage = new NavigationPage(new MainPage());
            }

            if (!Preferences.Instance.AutoConnect || !PostOffice.Instance.HasPermission) return;
            
            //Don't make users manually enable scanning more than once
            Log.Information("About to try to subscribe");
            PostOffice.Instance.SafeSubscribe();

        }

        public static void GetHelp() {
            PopupNavigation.Instance.PushAsync(new TroubleshootingPage());
        }
    
        public async void DeckCheck() {
            //Automatically load two decks for the user to start with
            if (!(Resources["DeckLibrary"] is DeckLibrary library) || !library.IsEmpty) return;
            try {
                var codes = new[] { "WKRXY", "LADY1"};
                foreach (var deck in codes) {
                    await library.AddDeckFromCode(deck);
                }
            }
            catch (Exception e) {
                //But fail silently if that doesn't work
                Log.Warning("Failed to download decks; exception is \n{e}", e);
            }
        }


        public static async Task<bool> InternetIsUp() {
          if (!CrossConnectivity.IsSupported) return true; //we have to assume it's all good
          return (await CrossConnectivity.Current.IsRemoteReachable("googleapis.com")); 
        }

        // handle the messy operation of adding a ContentPage to a NavigationPage to a
        // MasterDetailPage (if necessary)
        public static void NextPage(Page page) {
            if (page is BaseGamePage) {
                //can't set RootPage, must create entirely new NavigationPage
                if ((Current.MainPage as MasterDetailPage)?.Detail is NavigationPage) {
                    ((MasterDetailPage)Current.MainPage).Detail = new NavigationPage(page);
                } else {
                    var newRoot = new MasterDetailPage {
                            Master = new NavigationPage(new SidebarPage()) {
                                    BarBackgroundColor = (Color)Current.Resources["MainLight"],
                                    Title = "☰"},
                            Detail = new NavigationPage(page)
                    };
                    Current.MainPage = newRoot;
                }
            } else 
                Current.MainPage = page;
        }

        public static void PlayJingle() {
            if (Preferences.Instance.SoundEffects)
                Jingle?.Play();
        }
    
        public static void PlayWhoosh() {
            if (Preferences.Instance.SoundEffects)
                Whoosh?.Play();
        }
    
        protected override void OnSleep() {
            if (Device.RuntimePlatform == Device.Android)
                OnSleepSubroutine();
        }

        private void OnSleepSubroutine() {
            try {
                PostOffice.Instance.Hibernate();
            }
            catch (NullReferenceException) {
                Log.Error("No post office.  You're running in debug mode, right?");
            }
        }

        protected override void OnResume() {
            if (Device.RuntimePlatform == Device.Android)
                OnWakeSubroutine();
        }

        private void OnWakeSubroutine() {
            try {
                PostOffice.Instance.Thaw();
            }
            catch (NullReferenceException) {
                Log.Error("No post office.  You're running in debug mode, right?");
            }
        }

        protected override void OnStart() {
            base.OnStart();
            AppCenter.Start("ios=0a276ec4-8864-4b5b-8919-874b9868c2da;" +
                            "android=a85016c7-bd85-42eb-a9eb-ba2cd88caea8;", typeof(Crashes));
        }
    }
}
