using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VLC.Model;
using VLC.Model.FileExplorer;
using VLC.UI.Views;
using VLC.ViewModels;
using VLC.ViewModels.Others.VlcExplorer;
using Windows.Devices.Enumeration;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace VLC.UI.UWP.Views.VariousPages
{
    public sealed partial class ExternalStorageIncludePage : Page, IVLCModalFlyout
    {
        private string _deviceID;
        public ExternalStorageIncludePage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var device = e.Parameter as DeviceInformation;
            _deviceID = device.Id;
            DeviceNamePlaceHolder.Text = device.Name;
        }

        private async void Ok_Click(object sender, RoutedEventArgs e)
        {
            Locator.NavigationService.GoBack_HideFlyout();
            await performAction();
        }

        private async void Remember_Click(object sender, RoutedEventArgs e)
        {
            Locator.NavigationService.GoBack_HideFlyout();

            if (Index.IsChecked == true)
                Locator.SettingsVM.ExternalDeviceMode = ExternalDeviceMode.IndexMedias;
            else if (Select.IsChecked == true)
                Locator.SettingsVM.ExternalDeviceMode = ExternalDeviceMode.SelectMedias;

            await performAction();
        }

        private async Task performAction()
        {
            if (Index.IsChecked == true)
                await Locator.ExternalDeviceService.AskExternalDeviceIndexing(_deviceID);
            else if (Select.IsChecked == true)
                await Locator.ExternalDeviceService.AskContentToCopy(_deviceID);
        }

        public bool ModalMode
        {
            get { return true; }
        }

        private void Ignore_Click(object sender, RoutedEventArgs e)
        {
            Locator.NavigationService.GoBack_HideFlyout();
        }
    }
}
