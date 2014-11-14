using VLC_WINRT.Common;
using VLC_WINRT_APP.Helpers.MusicLibrary;
using VLC_WINRT_APP.Model.Music;

namespace VLC_WINRT_APP.Commands.Music
{
    public class AddToPlaylistCommand : AlwaysExecutableCommand
    {
        public override async void Execute(object parameter)
        {
            if (parameter is TrackItem)
            {
                await MusicLibraryManagement.AddToPlaylist(parameter as TrackItem);
            }
            else if (parameter is AlbumItem)
            {
                await MusicLibraryManagement.AddToPlaylist(parameter as AlbumItem);
            }
        }
    }
}
