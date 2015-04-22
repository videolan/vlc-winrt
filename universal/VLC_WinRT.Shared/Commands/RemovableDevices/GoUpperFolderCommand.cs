using VLC_WinRT.Utils;
using VLC_WinRT.ViewModels;
using VLC_WinRT.Views.MainPages;

namespace VLC_WinRT.Commands.RemovableDevices
{
    public class GoUpperFolderCommand : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
            if (App.ApplicationFrame.CurrentSourcePageType == typeof (MainPageFileExplorer))
            {
                if (Locator.FileExplorerVM.CurrentStorageVM != null &&
                    Locator.FileExplorerVM.CurrentStorageVM.CanGoBack)
                    Locator.FileExplorerVM.CurrentStorageVM.GoBack();
            }
        }
    }
}
