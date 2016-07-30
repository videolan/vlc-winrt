using Windows.Storage;
using Windows.UI.Xaml.Controls;
using VLC_WinRT.ViewModels;
using VLC_WinRT.Model;
using VLC_WinRT.Model.FileExplorer;
using VLC_WinRT.Utils;

namespace VLC_WinRT.Commands.VLCFileExplorer
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
