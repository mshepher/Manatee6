using System.Threading.Tasks;
using Xamarin.Forms;
using Rg.Plugins.Popup.Services;
using Plugin.Connectivity;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using Manatee7.Model;
using Syncfusion.ListView.XForms;
using Serilog;
using Log = Serilog.Log;
using Xamarin.Forms.Internals;
using Manatee7.PO;

namespace Manatee7 {
    public partial class TroubleshootingPage : Rg.Plugins.Popup.Pages.PopupPage {

        public string InternetConnected { set; get; } = "UNKNOWN";
        public bool BluetoothOn { set; get; }
        public bool MicPermissionOK { set; get; }
        private IHardwareManager _hardwareManager;


        public TroubleshootingPage() {
            _hardwareManager = DependencyService.Get<IHardwareManager>();
            BluetoothOn = _hardwareManager.BluetoothPoweredOn;
            MicPermissionOK = _hardwareManager.HasMicrophonePermission;
            InitializeComponent();
        }

        protected override async void OnAppearing() {
            base.OnAppearing();
            if (!CrossConnectivity.IsSupported) 
                InternetConnected="UNKNOWN";
            if (!await CrossConnectivity.Current.IsRemoteReachable("googleapis.com")) 
                InternetConnected="NO";
            InternetConnected="YES";
            OnPropertyChanged("InternetConnected");
        }

        private void BackButtonClicked(object sender, EventArgs e) {
            PopupNavigation.Instance.PopAsync(); 
        }
    

        protected override bool OnBackButtonPressed() {
            PopupNavigation.Instance.PopAsync();
            return true;
        } 

        protected override bool OnBackgroundClicked() {
            return false;
        }
    }
}
