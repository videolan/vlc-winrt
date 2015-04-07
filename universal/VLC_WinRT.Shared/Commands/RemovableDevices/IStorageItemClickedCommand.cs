using Windows.Storage;
using Windows.UI.Xaml.Controls;
using VLC_WINRT.Common;
using VLC_WinRT.ViewModels;
using VLC_WinRT.Model;
using VLC_WinRT.Views.MainPages;

namespace VLC_WinRT.Commands.RemovableDevices
{
    public class IStorageItemClickedCommand : AlwaysExecutableCommand
    {
        public override async void Execute(object parameter)
        {
            IStorageItem storageItem = ((ItemClickEventArgs)parameter).ClickedItem as IStorageItem;
            if (Locator.MainVM.NavigationService.CurrentPage == VLCPage.MainPageFileExplorer)
                await Locator.ExternalStorageVM.CurrentStorageVM.NavigateTo(storageItem);
#if WINDOWS_APP
            else if(App.ApplicationFrame.CurrentSourcePageType == typeof(MainPageMediaServers))
                await Locator.DlnaVM.CurrentDlnaVm.NavigateTo(storageItem);
#endif
        }
    }
}
