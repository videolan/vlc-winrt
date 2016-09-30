using System.Linq;
using VLC.Utils;
using VLC.ViewModels;

namespace VLC.Commands.MediaPlayback
{
    public class ShuffleCommand : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
            if (Locator.MediaPlaybackViewModel.PlaybackService == null
                || Locator.MediaPlaybackViewModel.PlaybackService.Playlist == null
                || !Locator.MediaPlaybackViewModel.PlaybackService.Playlist.Any()
                || Locator.MediaPlaybackViewModel.PlaybackService.Playlist.Count < 3) return;
            Locator.MediaPlaybackViewModel.PlaybackService.Shuffle();
        }
    }
}
