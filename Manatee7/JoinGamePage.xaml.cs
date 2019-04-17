using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Manatee7.Model;
using Syncfusion.ListView.XForms;
using Serilog;
using Xamarin.Forms;
using Rg.Plugins.Popup.Services;
using Log = Serilog.Log;
using static Manatee7.GameController.GameStatus;

namespace Manatee7 {
    public partial class JoinGamePage : Rg.Plugins.Popup.Pages.PopupPage {

        public JoinGamePage(Invitation invite) {

            this.invite = invite;
            InitializeComponent();
            controller.RSVP(this.invite);
            controller.StatusUpdated += (oldStatus, newStatus) => {
                if ((oldStatus == WAIT_AS_HOST || oldStatus == WAIT_AS_GUEST) &&
                    (newStatus == CHOOSING_SUBMISSION || newStatus == WAIT_AS_JUDGE)) StartGame();
            };
        }

        private readonly GameController controller = GameController.Instance;

        private async void CancelClicked(object sender, EventArgs e) {
            Button.IsEnabled = false;
            controller.UnRSVP();
            if (PopupNavigation.Instance.PopupStack.Any())
                await PopupNavigation.Instance.PopAsync();
        }

        private async void Cancelled(object sender, EventArgs e) {
            Log.Information("EventArgs e for VisibleInvitations.CollectionChanged: {Newline}{e}", e);
            
            // If another game was cancelled, we don't care.  If one of two overlapping invites
            // from the same host disappeared, we also don't care.
            if (controller.VisibleInvitations.Contains(invite)) return;

            WindowLabel.Text = "Game cancelled!";
            Button.Text = "OK";
            ActivityIndicator.IsVisible = false;
            await Task.Delay(4000);
            if (PopupNavigation.Instance.PopupStack.Contains(this)) {
                CancelClicked(sender, e);
            }
        }
    
        protected override bool OnBackButtonPressed() {
            return true;
        } 

        protected override bool OnBackgroundClicked() {
            return false;
        }

        private readonly Invitation invite;

        private async void StartGame() {
            try {
                if (PopupNavigation.Instance.PopupStack.Any())
                    await PopupNavigation.Instance.PopAsync();
            } catch {
                Log.Information("JoinGamePage.StartGame: Tried to pop popup, but no popup popped");
            }
        }
    }
}