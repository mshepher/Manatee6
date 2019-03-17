using System;
using System.Collections.ObjectModel;
using Serilog;
using Xamarin.Forms;
using Plugin.Connectivity;
using Xamarin.Forms.Xaml;
using System.Reflection;
using System.Threading.Tasks;
using Manatee7.Model;
using Plugin.SimpleAudioPlayer;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Crashes;
using Device = Xamarin.Forms.Device;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]

namespace Manatee7 {

  public partial class App : Application {
    public static ObservableCollection<string> AppAlerts { private set; get; } =
      new ObservableCollection<string>();

    public static bool DidAskAboutNearby = false;

    /*
    public static SettingsPage SettingsPage => _settingsPage ?? (_settingsPage = new SettingsPage());
    public static FlippedCardsPage FlippedCardsPage => _flippedCardsPage ?? (_flippedCardsPage = new FlippedCardsPage());
    public static SelectCardPage SelectCardPage => _selectCardPage ?? (_selectCardPage = new SelectCardPage());
    public static JudgingPage JudgingPage => _judgingPage ?? (_judgingPage = new JudgingPage());
    private static SettingsPage _settingsPage;
    private static SelectCardPage _selectCardPage;
    private static FlippedCardsPage _flippedCardsPage;
    private static FlippedCardsPage _judgingPage;
    */

    //public static IBluetoothManager BluetoothManager;
    private static readonly ISimpleAudioPlayer Jingle = CrossSimpleAudioPlayer.CreateSimpleAudioPlayer();
    private static readonly ISimpleAudioPlayer Whoosh = CrossSimpleAudioPlayer.CreateSimpleAudioPlayer();

    public static class Constants {
      public const string BluetoothPermissionErrorString = "Can't access Bluetooth!";
      public const string NearbyPermissionErrorString = "With Nearby disabled, your device can't talk to other devices.";
      public const string BluetoothPowerErrorString = "Bluetooth is switched off!";
      public const string InternetErrorString = "Can't reach googleapis.com!  Are you connected to the internet?";
    }
    
    public App() {
      Log.Information("Entering App Constructor");
      
      //https://forums.xamarin.com/discussion/141808/system-dialogs-triggering-onsleep-ios
      MessagingCenter.Subscribe<string>(this, "iOSSleep", (str) => { OnSleepSubroutine(); });

      MessagingCenter.Subscribe<string>(this, "iOSWake", (str) => { OnWakeSubroutine(); });


        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(
            "NTk3MTNAMzEzNjJlMzQyZTMwTFhmYWZpR3g1NU83bTNjZldpektRamVhUVRwc2pRYUQ2QXhqODJ0UE9wMD0=");
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
        MainPage = new WelcomePage();
      }
      else {
          MainPage = new NavigationPage(new MainPage());
      }

      if (Preferences.Instance.AutoConnect && PostOffice.Instance.HasPermission) {
        PostOffice.Instance.SafeSubscribe();
      }
      
      /* PostOffice.Instance.OnPermissionChanged += b => {
         if (!b)
           AppAlerts.Add(Constants.BluetoothPermissionErrorString);
         else
           AppAlerts.Remove(Constants.BluetoothPermissionErrorString);
       };
       
      CrossConnectivity.Current.ConnectivityChanged += async (sender, args) => {
         if (await InternetIsUp()) {
           AppAlerts.Remove(Constants.InternetErrorString);
           DeckCheck(); 
        } else {
           AppAlerts.Add(Constants.InternetErrorString);
         }
       };
      }
 */
    }

    public async void DeckCheck() {
      if (!(Resources["DeckLibrary"] is DeckLibrary library) || !library.IsEmpty) return;
        try {
          var codes = new[] {"6VVUN", "LADY1"};
          foreach (var deck in codes) {
            await library.AddDeckFromCode(deck);
          }
        }
        catch (Exception e) {
          Log.Warning("Failed to download decks; exception is \n{e}", e);
        }
      }

    /*
    public static async Task<bool> InternetIsUp() {
      if (!CrossConnectivity.IsSupported) return true; //we have to assume it's all good
      return (await CrossConnectivity.Current.IsRemoteReachable("googleapis.com")); 
    }
    
    */
    //The default Nearby permissions dialog crashes the app, so we need to head it off at the pass
    public static async Task<bool> RequestNearbyPermission() {
      Log.Logger.Information("Asking for Nearby permission");
      var output = await Current.MainPage.DisplayAlert(
          "Allow Manatee to use Nearby?",
          "Google Nearby uses ultrasonic audio and low-energy Bluetooth to communicate with nearby devices.",
          "Allow", "Don't Allow");
      Log.Logger.Information("Nearby permission granted? {o}",output);
      return output;
    }
    
    public static void NextPage(Page page) {

      if (page is BaseGamePage) {
        if ((Current.MainPage as MasterDetailPage)?.Detail is NavigationPage) {
          ((MasterDetailPage)Current.MainPage).Detail = new NavigationPage(page);
        } else {
          var newRoot = new MasterDetailPage {
              Master = new SidebarPage(),
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
