using VLC_WinRT.Model.Music;
using VLC_WinRT.ViewModels;
using VLC_WinRT.Model;
using VLC_WinRT.Utils;
using VLC_WinRT.Helpers;

namespace VLC_WinRT.Commands.MusicPlayer
{
    public class PlayArtistAlbumsCommand: AlwaysExecutableCommand
    {
        public override async void Execute(object parameter)
        {
            Locator.NavigationService.Go(VLCPage.MusicPlayerPage);
            if (parameter is ArtistItem)
            {
                var artist = parameter as ArtistItem;
                var tracks = await Locator.MusicLibraryVM._trackDatabase.LoadTracksByArtistId(artist.Id).ToObservableAsync();
                await PlaylistHelper.AddTrackCollectionToPlaylistAndPlay(tracks.ToPlaylist());
            }
        }
    }
}