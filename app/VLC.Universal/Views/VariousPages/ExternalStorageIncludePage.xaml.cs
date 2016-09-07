using System;
using System.Collections.Generic;
using System.Linq;
using VLC.Model;
using VLC.Model.FileExplorer;
using VLC.UI.Views;
using VLC.ViewModels;
using VLC.ViewModels.Others.VlcExplorer;
using Windows.Storage;
using Windows.UI.Xaml;

namespace VLC.UI.UWP.Views.VariousPages
{
    public sealed partial class ExternalStorageIncludePage : IVLCModalFlyout
    {
        public ExternalStorageIncludePage()
        {
            this.InitializeComponent();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            Locator.NavigationService.GoBack_HideFlyout();

            if (Index.IsChecked == true)
                await Locator.ExternalDeviceService.AskExternalDeviceIndexing();
            else if (Select.IsChecked == true)
                await Locator.ExternalDeviceService.AskContentToCopy();
        }

        public bool ModalMode
        {
            get { return true; }
        }
    }
}
