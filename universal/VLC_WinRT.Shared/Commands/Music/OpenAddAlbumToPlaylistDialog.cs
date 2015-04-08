using VLC_WinRT.Model;
using VLC_WinRT.Utils;
using VLC_WinRT.ViewModels;

namespace VLC_WinRT.Commands.Music
{
    public class OpenAddAlbumToPlaylistDialog : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
            Locator.NavigationService.Go(VLCPage.AddAlbumToPlaylistDialog);
        }
    }
}
