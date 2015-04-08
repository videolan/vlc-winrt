using VLC_WinRT.Helpers.MusicLibrary;
using VLC_WinRT.Model.Music;
using VLC_WinRT.Utils;

namespace VLC_WinRT.Commands.Music
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
