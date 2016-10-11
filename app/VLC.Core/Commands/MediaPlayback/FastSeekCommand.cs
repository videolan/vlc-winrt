using VLC.Model;
using VLC.Utils;
using VLC.ViewModels;

namespace VLC.Commands.MediaPlayback
{
    public class FastSeekCommand : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
            if (Locator.MediaPlaybackViewModel.PlaybackService.PlayingType == PlayingType.NotPlaying ||
                parameter is int == false)
                return;

            int offset = (int)parameter;
            Locator.MediaPlaybackViewModel.Time += offset;
        }
    }
}
