using VLC_WinRT.Helpers.MusicLibrary;
using VLC_WinRT.Model.Music;
using VLC_WinRT.Utils;
using VLC_WinRT.ViewModels;

namespace VLC_WinRT.Commands.MusicLibrary
{
    public class AddToPlaylistCommand : AlwaysExecutableCommand
    {
        public override async void Execute(object parameter)
        {
            if (parameter is TrackItem)
            {
                await Locator.MusicLibraryVM.MusicLibrary.AddToPlaylist(parameter as TrackItem);
            }
            else if (parameter is AlbumItem)
            {
                await Locator.MusicLibraryVM.MusicLibrary.AddToPlaylist(parameter as AlbumItem);
            }
        }
    }
}
