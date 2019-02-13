using VLC.Model;
using VLC.Utils;
using VLC.ViewModels;

namespace VLC.Commands.MediaPlayback
{
    public class ChangeSpuDelayCommand : AlwaysExecutableCommand
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
                    Locator.MediaPlaybackViewModel.SpuDelay += step;
                    if (Locator.MediaPlaybackViewModel.SpuDelay > limit)
                    {
                        Locator.MediaPlaybackViewModel.SpuDelay = limit;
                    }
                    break;
                case "slower":
                    Locator.MediaPlaybackViewModel.SpuDelay -= step;
                    if (Locator.MediaPlaybackViewModel.SpuDelay < -limit)
                    {
                        Locator.MediaPlaybackViewModel.SpuDelay = -limit;
                    }
                    break;
                case "reset":
                    Locator.MediaPlaybackViewModel.SpuDelay = 0;
                    break;
            }
        }
    }
}