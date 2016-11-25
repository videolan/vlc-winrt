using System.Linq;
using Windows.UI.Xaml.Controls;
using VLC.Model;
using VLC.Model.Music;
using VLC.ViewModels;
using System.Collections.ObjectModel;
using VLC.Utils;
using VLC.Helpers;

namespace VLC.Commands.MusicPlayer
{
    public class PlayAllSongsCommand : AlwaysExecutableCommand
    {
        public override async void Execute(object parameter)
        {
            var tracks = Locator.MediaLibrary.LoadTracks();
            if (tracks == null || !tracks.Any()) return;
            Locator.NavigationService.GoOnPlaybackStarted(VLCPage.MusicPlayerPage);
            var itemClickArgs = parameter as ItemClickEventArgs;
            var index = 0;
            if (itemClickArgs != null)
            {
                var selectedTrack = itemClickArgs.ClickedItem as TrackItem;
                index = tracks.IndexOf(tracks.FirstOrDefault(x => x.Id == selectedTrack.Id));
            }
            if (index > -1)
            {
                await Locator.PlaybackService.SetPlaylist(tracks, index);
            }
        }
    }
}
