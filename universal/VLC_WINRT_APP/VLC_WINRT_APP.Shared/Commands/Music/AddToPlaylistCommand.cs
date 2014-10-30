using VLC_WINRT.Common;
using VLC_WINRT_APP.Helpers.MusicLibrary;
using VLC_WINRT_APP.Model.Music;
using VLC_WINRT_APP.ViewModels;

namespace VLC_WINRT_APP.Commands.Music
{
    public class AddToPlaylistCommand : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
            if (parameter is TrackItem)
            {
                MusicLibraryManagement.AddToPlaylist(parameter as TrackItem);
            }
            else if (parameter is AlbumItem)
            {
                MusicLibraryManagement.AddToPlaylist(parameter as AlbumItem);
            }
            Locator.MusicLibraryVM.CurrentTrackCollection = null;
        }
    }
}
