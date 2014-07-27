using Windows.Storage;
using Windows.UI.Xaml.Controls;
using VLC_WINRT.Common;
using VLC_WINRT_APP.ViewModels;

namespace VLC_WINRT_APP.Commands.RemovableDevices
{
    public class IStorageItemClickedCommand : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
            IStorageItem storageItem = ((ItemClickEventArgs) parameter).ClickedItem as IStorageItem;
#if WINDOWS_APP
            Locator.ExternalStorageVM.CurrentStorageVM.NavigateTo(storageItem);
#endif
        }
    }
}
