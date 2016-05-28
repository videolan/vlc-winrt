using System.Linq;
using Windows.UI.Xaml.Controls;
using VLC_WinRT.Model.Music;
using VLC_WinRT.ViewModels;
using VLC_WinRT.Model;
using VLC_WinRT.Utils;
using VLC_WinRT.Helpers;

namespace VLC_WinRT.Commands.MusicPlayer
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
                await Locator.MediaPlaybackViewModel.TrackCollection.Add(trackCollection.Playlist, false, true, trackCollection.Playlist[(int)parameter]);
            }
            else if (parameter is ItemClickEventArgs)
            {
                var track = (TrackItem)((ItemClickEventArgs)parameter).ClickedItem;

                await Locator.MediaPlaybackViewModel.TrackCollection.Add(trackCollection.Playlist, true, true, track);
            }
            else
            {
                await Locator.MediaPlaybackViewModel.TrackCollection.Add(trackCollection.Playlist, false, true, trackCollection.Playlist[0]);
            }
        }
    }
}
