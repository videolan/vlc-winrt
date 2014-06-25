using Windows.Storage;
using Windows.UI.Xaml.Controls;
using VLC_WINRT.Common;

namespace VLC_WINRT_APP.Commands.RemovableDevices
{
    public class StorageFolderClickedCommand : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
            StorageFolder folder = ((ItemClickEventArgs) parameter).ClickedItem as StorageFolder;
            
        }
    }
}
