using VLC.Model;
using VLC.Utils;
using VLC.ViewModels;
using VLC.UI.Views.MainPages;

namespace VLC.Commands.VLCFileExplorer
{
    public class GoUpperFolderCommand : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
            if (Locator.NavigationService.CurrentPage == VLCPage.MainPageFileExplorer)
            {
                if (Locator.FileExplorerVM.CurrentStorageVM != null && Locator.FileExplorerVM.CanGoBack)
                    Locator.FileExplorerVM.CurrentStorageVM.GoBack();
                else
                {
                    Locator.FileExplorerVM.GoBackToRootFolders();
                }
            }
        }
    }
}
