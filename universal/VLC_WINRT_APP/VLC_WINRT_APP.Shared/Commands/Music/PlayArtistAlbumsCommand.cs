using VLC_WINRT.Common;
using VLC_WINRT_APP.Helpers.MusicPlayer;
using VLC_WINRT_APP.Model.Music;
using VLC_WINRT_APP.ViewModels;
using VLC_WINRT_APP.ViewModels.MusicVM;
using VLC_WINRT_APP.Views.MusicPages;

namespace VLC_WINRT_APP.Commands.Music
{
    public class PlayArtistAlbumsCommand: AlwaysExecutableCommand
    {
        public override async void Execute(object parameter)
        {
            if (App.ApplicationFrame.CurrentSourcePageType != typeof (MusicPlayerPage))
                App.ApplicationFrame.Navigate(typeof (MusicPlayerPage));
            Locator.MusicLibraryVM.IsAlbumPageShown = false;
            if (parameter is ArtistItem)
            {
                var artist = parameter as ArtistItem;
                var tracks = await MusicLibraryVM._trackDataRepository.LoadTracksByArtistId(artist.Id);
                await PlayMusicHelper.AddTrackCollectionToPlaylistAndPlay(tracks);
            }
        }
    }
}