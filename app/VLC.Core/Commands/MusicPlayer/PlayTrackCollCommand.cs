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
            Locator.NavigationService.GoOnPlaybackStarted(VLCPage.MusicPlayerPage);

            uint index = 0;
            if (parameter is uint)
            {
                index = (uint)parameter;
            }
            await Locator.PlaybackService.SetPlaylist(trackCollection.Playlist, index);
        }
    }
}
