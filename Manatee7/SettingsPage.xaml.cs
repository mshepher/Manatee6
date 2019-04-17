using System;
using System.Linq;
using Xamarin.Forms;
using Manatee7.Model;
using System.Collections.Generic;
using Log = Serilog.Log;
using System.Threading.Tasks;
using Manatee7.PO;

namespace Manatee7
{
    public partial class SettingsPage : ContentPage
    {
        private readonly PostOffice _px = PostOffice.Instance;

        //public double MediumLabelFont { set; get; } = Device.GetNamedSize(NamedSize.Medium, typeof(Label));

        public bool DisplayMicAllowed
        {
            set
            {
                if (_px.HasPermission)
                    _px.CurrentStrategy = value ? NearbyStrategy.Default : NearbyStrategy.Ble;
            }
            get => _px.HasPermission && _px.CurrentStrategy == NearbyStrategy.Default;
        }


        private bool _smallScreen = false;
        public SettingsPage()
        {
            InitializeComponent();
            foreach (var d in listView.ItemsSource)
            {
                Log.Information("{@d} is an item", d);
            }

            BindingContext = this;
            _library.PropertyChanged += (sender, e) => Log.Information("Saw change");
            NearbyPermissionSwitch.Toggled +=
                    (sender, e) => OnPropertyChanged(nameof(DisplayMicAllowed));
            CodeEntry.Completed += AddDeckFromEntry;
            LinkTapped.Tapped += async (sender, e) =>
            {
                Link.TextColor = Color.Accent;
                await Task.Delay(50);
                Device.OpenUri(
                        new Uri("http://www.cardcastgame.com/browse"));
                Link.TextColor = Color.Blue;

            };
            SmallNameLayout.IsVisible = false;
            listView.SizeChanged += (sender, e) =>
            {
                if (listView.Height > 0 && listView.Height < ReferenceGrid.Height * 3 && !_smallScreen)
                    SwitchToSmallScreen();
            };
        }
        
        //Move screen name prompt to a single line on smaller screens
        private void SwitchToSmallScreen()
        {
            if (_smallScreen) return;

            SmallNameLayout.IsVisible = true;

            ScreenNameLabelLarge.IsVisible = false;
            EntryLarge.IsVisible = false;
        }

        protected override void OnDisappearing()
        {
            Preferences.Save();
            base.OnDisappearing();
        }
        private readonly DeckLibrary _library = DeckLibrary.Instance;

        private void Delete(object sender, EventArgs e)
        {
            try
            {
                _library.RemoveDeck(((MenuItem)sender).CommandParameter as string);
                if (Device.RuntimePlatform == Device.iOS)
                {
                    listView.ItemsSource = new Dictionary<string, Deck>();

                    listView.ItemsSource = _library.Decks;
                }

            }
            catch (Exception ex)
            {
                Log.Error("could not delete deck {deck}; exception {Newline}{e}",
                          ((MenuItem)sender).CommandParameter, ex);
            }
        }

        //Save when user exits 'name' text entry field
        private void Handle_Unfocused(object sender, FocusEventArgs e) {
            Preferences.Save();
        }

        private async void AddDeckFromEntry(object sender, EventArgs e)
        {
            AddButton.IsEnabled = false;
            AddButton.Text = "Adding deck...";
            try
            {
                if (CodeEntry.Text.Length != 5)
                    throw new ArgumentException("Input is the wrong length!");
                //https://stackoverflow.com/questions/3061662/how-to-find-out-if-string-contains-non-alpha-numeric-characters-in-c-net-2-0
                if (!CodeEntry.Text.All(char.IsLetterOrDigit))
                    throw new ArgumentException("Contains non-alphanumeric chars");

                var code = CodeEntry.Text.ToUpper();
                await _library.AddDeckFromCode(code);
                //listView.BeginRefresh();
                CodeEntry.Placeholder = "Enter 5-letter code";
            }
            catch (Exception ex)
            {
                Log.Error("Exception! {@ex}" + ex.Message);
                CodeEntry.Placeholder = "Not found!";
            }
            CodeEntry.Text = "";
            AddButton.Text = "Add Deck";
        }
    }

    public class MicValueToStrategy : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
                              System.Globalization.CultureInfo culture)
        {
            return (value is NearbyStrategy strategy &&
                    strategy == NearbyStrategy.Default); //don't leave the toggle on if user has switched off nearby all together
        }

        public object ConvertBack(object value, Type targetType, object parameter,
                                  System.Globalization.CultureInfo culture)
        {
            return (bool)value ? NearbyStrategy.Default : NearbyStrategy.Ble;
        }
    }
}