using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using Manatee7.Model;
using Syncfusion.ListView.XForms;
using Serilog;
using Xamarin.Forms;
using Rg.Plugins.Popup.Services;
using Log = Serilog.Log;
using Xamarin.Forms.Internals;

namespace Manatee7
{
    public partial class FinalScorePage : Rg.Plugins.Popup.Pages.PopupPage {
        public FinalScorePage()
        {
            InitializeComponent();
        }

        private async void ExitClicked(object sender, EventArgs e)
        {
            if (PopupNavigation.Instance.PopupStack.Any())
                await PopupNavigation.Instance.PopAsync();
            GameController.Instance.ExitGame();
        }


        protected override bool OnBackButtonPressed()
        {
            return true;
        }

        protected override bool OnBackgroundClicked()
        {
            return false;
        }
    }
}