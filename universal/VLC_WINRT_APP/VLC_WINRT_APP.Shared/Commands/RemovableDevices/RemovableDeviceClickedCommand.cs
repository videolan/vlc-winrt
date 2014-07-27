using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using VLC_WINRT.Common;
using VLC_WINRT_APP.ViewModels;
using VLC_WINRT_APP.ViewModels.Others.VlcExplorer;

namespace VLC_WINRT_APP.Commands.RemovableDevices
{
#if WINDOWS_APP
    public class RemovableDeviceClickedCommand : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
            if ((parameter as SelectionChangedEventArgs).AddedItems.Count != 0)
            {
                FileExplorerViewModel fileExplorer =
                    (parameter as SelectionChangedEventArgs).AddedItems[0] as FileExplorerViewModel;
                Locator.ExternalStorageVM.CurrentStorageVM = fileExplorer;
                Task.Run(() => Locator.ExternalStorageVM.CurrentStorageVM.GetFiles());
            }
        }
    }
#endif
}
