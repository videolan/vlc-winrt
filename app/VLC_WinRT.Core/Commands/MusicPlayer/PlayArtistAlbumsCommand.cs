using VLC.Model.Music;
using VLC.ViewModels;
using VLC.Model;
using VLC.Utils;
using VLC.Helpers;

namespace VLC.Commands.MusicPlayer
{
    public class PlayArtistAlbumsCommand: AlwaysExecutableCommand
    {
        public override async void Execute(object parameter)
        {
            if (parameter is ArtistItem)
            {
                Locator.NavigationService.Go(VLCPage.MusicPlayerPage);
                var artist = parameter as ArtistItem;
                var tracks = await Locator.MediaLibrary.LoadTracksByArtistId(artist.Id).ToObservableAsync();

                await Locator.MediaPlaybackViewModel.PlaybackService.SetPlaylist(tracks, false, true, tracks[0]);
            }
        }
    }
}