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
            IVLCStorageItem storageItem = ((ItemClickEventArgs)parameter).ClickedItem as IVLCStorageItem;
            if (Locator.NavigationService.CurrentPage == VLCPage.MainPageFileExplorer)
                await Locator.FileExplorerVM.CurrentStorageVM.NavigateTo(storageItem);
        }
    }
}
