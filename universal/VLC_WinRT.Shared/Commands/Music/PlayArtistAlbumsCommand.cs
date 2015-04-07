using VLC_WINRT.Common;
using VLC_WinRT.Helpers.MusicPlayer;
using VLC_WinRT.Model.Music;
using VLC_WinRT.ViewModels;
using VLC_WinRT.Model;

namespace VLC_WinRT.Commands.Music
{
    public class PlayArtistAlbumsCommand: AlwaysExecutableCommand
    {
        public override async void Execute(object parameter)
        {
            Locator.MainVM.NavigationService.Go(VLCPage.MusicPlayerPage);
            Locator.MusicLibraryVM.IsAlbumPageShown = false;
            if (parameter is ArtistItem)
            {
                var artist = parameter as ArtistItem;
                var tracks = await Locator.MusicLibraryVM._trackDataRepository.LoadTracksByArtistId(artist.Id).ToObservableAsync();
                await PlayMusicHelper.AddTrackCollectionToPlaylistAndPlay(tracks);
            }
        }
    }
}