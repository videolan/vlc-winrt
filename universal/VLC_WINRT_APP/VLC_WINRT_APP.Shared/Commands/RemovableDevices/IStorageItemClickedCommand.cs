using Windows.Storage;
using Windows.UI.Xaml.Controls;
using VLC_WINRT.Common;
using VLC_WINRT_APP.ViewModels;
using VLC_WINRT_APP.Views.MainPages;

namespace VLC_WINRT_APP.Commands.RemovableDevices
{
    public class IStorageItemClickedCommand : AlwaysExecutableCommand
    {
        public override async void Execute(object parameter)
        {
            IStorageItem storageItem = ((ItemClickEventArgs) parameter).ClickedItem as IStorageItem;
#if WINDOWS_APP
            if(App.ApplicationFrame.CurrentSourcePageType == typeof(MainPageRemovables))
                await Locator.ExternalStorageVM.CurrentStorageVM.NavigateTo(storageItem);
            else if(App.ApplicationFrame.CurrentSourcePageType == typeof(MainPageMediaServers))
                await Locator.DlnaVM.CurrentDlnaVm.NavigateTo(storageItem);
#else
            if (App.ApplicationFrame.CurrentSourcePageType == typeof(MainPageHome) && (App.ApplicationFrame.Content as MainPageHome).MainPivot.SelectedIndex == 3)
                await Locator.ExternalStorageVM.CurrentStorageVM.NavigateTo(storageItem);
#endif
        }
    }
}
