using VLC.Model;
using VLC.Utils;
using VLC.ViewModels;

namespace VLC.Commands.MusicLibrary
{
    public class OpenAddAlbumToPlaylistDialog : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
            Locator.NavigationService.Go(VLCPage.AddAlbumToPlaylistDialog);
        }
    }
}
