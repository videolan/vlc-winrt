using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using VLC_WinRT.Utils;
using VLC_WinRT.ViewModels;
using VLC_WinRT.ViewModels.Others.VlcExplorer;

namespace VLC_WinRT.Commands.VLCFileExplorer
{
    public class RootFolderClickedCommand : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
            FileExplorer fileExplorer = null;
            if (parameter is SelectionChangedEventArgs)
            {
                fileExplorer = (parameter as SelectionChangedEventArgs).AddedItems[0] as FileExplorer;
            }
            else if (parameter is ItemClickEventArgs)
            {
                fileExplorer = (parameter as ItemClickEventArgs).ClickedItem as FileExplorer;
            }

            if (fileExplorer == null) return;
            Locator.FileExplorerVM.CurrentStorageVM = fileExplorer;
            Task.Run(() => Locator.FileExplorerVM.CurrentStorageVM.GetFiles());
        }
    }
}
