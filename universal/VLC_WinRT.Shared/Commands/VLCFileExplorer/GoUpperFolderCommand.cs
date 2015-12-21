using VLC_WinRT.Model;
using VLC_WinRT.Utils;
using VLC_WinRT.ViewModels;
using VLC_WinRT.Views.MainPages;

namespace VLC_WinRT.Commands.VLCFileExplorer
{
    public class GoUpperFolderCommand : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
            if (Locator.NavigationService.CurrentPage == VLCPage.MainPageFileExplorer)
            {
                if (Locator.FileExplorerVM.CurrentStorageVM != null &&
                    Locator.FileExplorerVM.CurrentStorageVM.CanGoBack)
                    Locator.FileExplorerVM.CurrentStorageVM.GoBack();
            }
        }
    }
}
