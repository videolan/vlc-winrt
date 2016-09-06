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
                Locator.ExternalDeviceService.AskExternalDeviceIndexing();
            else if (Select.IsChecked == true)
            {
                // Display the folder of the first external storage device detected.
                Locator.MainVM.CurrentPanel = Locator.MainVM.Panels.FirstOrDefault(x => x.Target == VLCPage.MainPageFileExplorer);

                var devices = KnownFolders.RemovableDevices;
                IReadOnlyList<StorageFolder> rootFolders = await devices.GetFoldersAsync();

                var rootFolder = rootFolders.First();
                if (rootFolder == null)
                    return;

                var storageItem = new VLCStorageFolder(rootFolder);
                Locator.FileExplorerVM.CurrentStorageVM = new LocalFileExplorerViewModel(
                    rootFolder, RootFolderType.ExternalDevice);
                await Locator.FileExplorerVM.CurrentStorageVM.GetFiles();
            }
        }

        public bool ModalMode
        {
            get { return true; }
        }
    }
}
