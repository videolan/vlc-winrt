using VLC.Model;
using VLC.Utils;
using VLC.ViewModels;

namespace VLC.Commands.MediaPlayback
{
    public class ChangePlaybackSpeedRateCommand : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
            if (Locator.MediaPlaybackViewModel.PlaybackService.PlayingType == PlayingType.NotPlaying)
                return;

            var request = parameter.ToString();
            switch (request)
            {
                case "faster":
                    if (Locator.MediaPlaybackViewModel.SpeedRate < 200)
                    {
                        Locator.MediaPlaybackViewModel.SpeedRate += 10;
                    }
                    break;
                case "slower":
                    if (Locator.MediaPlaybackViewModel.SpeedRate > 50)
                    {
                        Locator.MediaPlaybackViewModel.SpeedRate -= 10;
                    }
                    break;
                case "reset":
                    Locator.MediaPlaybackViewModel.SpeedRate = 100;
                    break;
            }
        }
    }
}
