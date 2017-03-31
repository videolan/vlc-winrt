using VLC.Helpers.MusicLibrary;
using VLC.Model.Music;
using VLC.Utils;
using VLC.ViewModels;

namespace VLC.Commands.MusicLibrary
{
    public class AddToPlaylistCommand : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
            if (parameter is TrackItem)
            {
                Locator.MediaLibrary.AddToPlaylist(parameter as TrackItem);
            }
            else if (parameter is AlbumItem)
            {
                Locator.MediaLibrary.AddToPlaylist(parameter as AlbumItem);
            }
        }
    }
}
