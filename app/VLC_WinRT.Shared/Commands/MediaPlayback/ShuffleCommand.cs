using System.Linq;
using VLC_WinRT.Utils;
using VLC_WinRT.ViewModels;

namespace VLC_WinRT.Commands.MediaPlayback
{
    public class ShuffleCommand : AlwaysExecutableCommand
    {
        public override async void Execute(object parameter)
        {
            if (Locator.MediaPlaybackViewModel.PlaybackService == null
                || Locator.MediaPlaybackViewModel.PlaybackService.Playlist == null
                || !Locator.MediaPlaybackViewModel.PlaybackService.Playlist.Any()
                || Locator.MediaPlaybackViewModel.PlaybackService.Playlist.Count < 3) return;
            await Locator.MediaPlaybackViewModel.PlaybackService.Shuffle();
        }
    }
}
