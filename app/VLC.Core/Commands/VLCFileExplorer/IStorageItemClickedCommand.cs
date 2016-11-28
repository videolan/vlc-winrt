using Windows.Storage;
using Windows.UI.Xaml.Controls;
using VLC.ViewModels;
using VLC.Model;
using VLC.Model.FileExplorer;
using VLC.Utils;

namespace VLC.Commands.VLCFileExplorer
{
    public class IStorageItemClickedCommand : AlwaysExecutableCommand
    {
        public override async void Execute(object parameter)
        {
            var clickedItem = ((ItemClickEventArgs)parameter).ClickedItem;
            // For some reason, when clicking early while the page is loading, it seems we can click "null items"
            if (clickedItem == null)
                return;
            IVLCStorageItem storageItem = clickedItem as IVLCStorageItem;
            if (Locator.NavigationService.CurrentPage == VLCPage.MainPageFileExplorer)
                await Locator.FileExplorerVM.CurrentStorageVM.NavigateTo(storageItem);
        }
    }
}
