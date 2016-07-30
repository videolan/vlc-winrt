using System.Linq;
using Windows.UI.Xaml.Controls;
using VLC.Model.Music;
using VLC.ViewModels;
using VLC.Model;
using VLC.Utils;
using VLC.Helpers;

namespace VLC.Commands.MusicPlayer
{
    public class PlayTrackCollCommand : AlwaysExecutableCommand
    {
        public override async void Execute(object parameter)
        {
            var trackCollection = Locator.MusicLibraryVM.CurrentTrackCollection;
            if (trackCollection == null || trackCollection.Playlist == null || !trackCollection.Playlist.Any()) return;
            Locator.NavigationService.Go(VLCPage.MusicPlayerPage);

            if (parameter is int)
            {
                await Locator.MediaPlaybackViewModel.PlaybackService.SetPlaylist(trackCollection.Playlist, false, true, trackCollection.Playlist[(int)parameter]);
            }
            else if (parameter is ItemClickEventArgs)
            {
                var track = (TrackItem)((ItemClickEventArgs)parameter).ClickedItem;

                await Locator.MediaPlaybackViewModel.PlaybackService.SetPlaylist(trackCollection.Playlist, true, true, track);
            }
            else
            {
                await Locator.MediaPlaybackViewModel.PlaybackService.SetPlaylist(trackCollection.Playlist, false, true, trackCollection.Playlist[0]);
            }
        }
    }
}
