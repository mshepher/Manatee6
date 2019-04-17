using System;
using System.Collections.Generic;
using Rg.Plugins.Popup.Services;
using System.Linq;
using Manatee7.Model;
using Manatee7.PO;
using Xamarin.Forms;

namespace Manatee7
{
    public partial class PermissionsPrimerPage : Rg.Plugins.Popup.Pages.PopupPage
    {
        public bool FirstPage { set; get; }
        public bool SecondPage { set; get; }
        public event BinaryEventHandler ChoiceMade;
        public PermissionsPrimerPage()
        {
            BindingContext = this;
            InitializeComponent();
            FirstMessage.IsVisible = true;
            SecondMessage.IsVisible = false;
        }

        private void NextPageClicked(object sender, EventArgs e)
        {
            FirstMessage.IsVisible = false;
            SecondMessage.IsVisible = true;
            Preferences.Instance.Strategy = NearbyStrategy.Ble;
        }

        private async void OKClicked(object sender, EventArgs e)
        {
            Preferences.Instance.AutoConnect = true;
            PostOffice.Instance.HasPermission = true;
            ChoiceMade?.Invoke(true);
            if (PopupNavigation.Instance.PopupStack.Any())
                await Rg.Plugins.Popup.Services.PopupNavigation.Instance.PopAsync();
        }

        private async void CancelClicked(object sender, EventArgs e)
        {
            ChoiceMade?.Invoke(false);
            Preferences.Instance.Strategy = NearbyStrategy.Default;
            if (PopupNavigation.Instance.PopupStack.Any())
                await Rg.Plugins.Popup.Services.PopupNavigation.Instance.PopAsync();
        }

    }

}