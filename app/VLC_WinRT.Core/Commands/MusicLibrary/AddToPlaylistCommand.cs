using VLC.Helpers.MusicLibrary;
using VLC.Model.Music;
using VLC.Utils;
using VLC.ViewModels;

namespace VLC.Commands.MusicLibrary
{
    public class AddToPlaylistCommand : AlwaysExecutableCommand
    {
        public override async void Execute(object parameter)
        {
            if (parameter is TrackItem)
            {
                await Locator.MediaLibrary.AddToPlaylist(parameter as TrackItem);
            }
            else if (parameter is AlbumItem)
            {
                await Locator.MediaLibrary.AddToPlaylist(parameter as AlbumItem);
            }
        }
    }
}
