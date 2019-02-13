using VLC.Model;
using VLC.Utils;
using VLC.ViewModels;

namespace VLC.Commands.MediaPlayback
{
    public class ChangeAudioDelayCommand : AlwaysExecutableCommand
    {
        private int limit = 3000;
        private int step = 50;
        public override void Execute(object parameter)
        {
            if (Locator.MediaPlaybackViewModel.PlaybackService.PlayingType == PlayingType.NotPlaying)
                return;

            var request = parameter.ToString();
            switch (request)
            {
                case "faster":
                    Locator.MediaPlaybackViewModel.AudioDelay += step;
                    if (Locator.MediaPlaybackViewModel.AudioDelay > limit)
                    {
                        Locator.MediaPlaybackViewModel.AudioDelay = limit;
                    }
                    break;
                case "slower":
                    Locator.MediaPlaybackViewModel.AudioDelay -= step;
                    if (Locator.MediaPlaybackViewModel.AudioDelay < -limit)
                    {
                        Locator.MediaPlaybackViewModel.AudioDelay = -limit;
                    }
                    break;
                case "reset":
                    Locator.MediaPlaybackViewModel.AudioDelay = 0;
                    break;
            }
        }
    }
}
