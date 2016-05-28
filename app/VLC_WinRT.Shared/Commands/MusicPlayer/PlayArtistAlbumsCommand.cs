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
            if (parameter is ArtistItem)
            {
                Locator.NavigationService.Go(VLCPage.MusicPlayerPage);
                var artist = parameter as ArtistItem;
                var tracks = await Locator.MediaLibrary.LoadTracksByArtistId(artist.Id).ToObservableAsync();

                await Locator.MediaPlaybackViewModel.TrackCollection.Add(tracks, false, true, tracks[0]);
            }
        }
    }
}