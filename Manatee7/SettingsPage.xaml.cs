using System;
using System.Linq;
using Xamarin.Forms;
using Manatee7.Model;
using System.Collections.Generic;
using Log = Serilog.Log;
using System.Threading.Tasks;

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
            //listView.CachingStrategy = ListViewCachingStrategy.RecycleElement;
            listView.SizeChanged += (sender, e) =>
            {
                if (listView.Height > 0 && listView.Height < ReferenceGrid.Height * 3 && !_smallScreen)
                    SwitchToSmallScreen();
            };
        }
        private void SwitchToSmallScreen()
        {
            if (_smallScreen) return;
            //CardsPerHandGrid.IsVisible = false;
            //BaseFontSize = StepFontSize; 


            SmallNameLayout.IsVisible = true;

            ScreenNameLabelLarge.IsVisible = false;
            EntryLarge.IsVisible = false;

            /*NearbyLargeGrid.IsVisible = false;
            NearbySmallGrid.IsVisible = true;
            MicSmallGrid.IsVisible = true;*/

            //ParentGrid.Margin = new Thickness { Bottom = 5, Left = 5, Right = 5, Top = 2 };
           /* Resources.TryGetValue("Scale", out object o);
            PageScale = o is double d ? d : PageScale;
            //DeckInstructionLabel.FontSize *= Scale;

            var stepHeight = ReferenceStepper.Height;
            var margin = ReferenceGrid.Height - stepHeight;
            ReferenceStepper.HeightRequest = stepHeight * PageScale;
            ReferenceGrid.MinimumHeightRequest = (stepHeight * PageScale) + margin;

            _smallScreen = true;*/
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

        private void Handle_Unfocused(object sender, FocusEventArgs e)
        {
            Preferences.Save();
            if (listView.ItemsSource == _library.Decks)
                listView.ItemsSource = new Dictionary<string, Deck>();
            else
                listView.ItemsSource = _library.Decks;
        }

        private async void AddDeckFromEntry(object sender, EventArgs e)
        {
            AddButton.IsEnabled = false;
            AddButton.Text = "Adding deck...";
            try
            {
                if (CodeEntry.Text.Length != 5)
                    throw new ArgumentException("Input is wrong length!");
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