using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using VLC.Utils;
using VLC.ViewModels;
using VLC.ViewModels.Others.VlcExplorer;

namespace VLC.Commands.VLCFileExplorer
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
