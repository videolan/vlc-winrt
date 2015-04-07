using VLC_WinRT.Model;
using VLC_WinRT.ViewModels;
using VLC_WINRT.Common;

namespace VLC_WinRT.Commands.Music
{
    public class OpenAddAlbumToPlaylistDialog : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
            Locator.MainVM.NavigationService.Go(VLCPage.AddAlbumToPlaylistDialog);
        }
    }
}
