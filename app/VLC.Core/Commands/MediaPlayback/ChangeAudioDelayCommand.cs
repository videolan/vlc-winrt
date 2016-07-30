using VLC.Model;
using VLC.Utils;
using VLC.ViewModels;

namespace VLC.Commands.MediaPlayback
{
    public class ChangeAudioDelayCommand : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
            if (Locator.MediaPlaybackViewModel.PlaybackService.PlayingType == PlayingType.NotPlaying)
                return;

            var request = parameter.ToString();
            switch (request)
            {
                case "faster":
                    if (Locator.MediaPlaybackViewModel.AudioDelay < 3000)
                    {
                        Locator.MediaPlaybackViewModel.AudioDelay += 50;
                    }
                    break;
                case "slower":
                    if (Locator.MediaPlaybackViewModel.AudioDelay > -3000)
                    {
                        Locator.MediaPlaybackViewModel.AudioDelay -= 50;
                    }
                    break;
                case "reset":
                    Locator.MediaPlaybackViewModel.AudioDelay = 0;
                    break;
            }
        }
    }
}
