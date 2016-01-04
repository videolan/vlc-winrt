using VLC_WinRT.Model;
using VLC_WinRT.Utils;
using VLC_WinRT.ViewModels;

namespace VLC_WinRT.Commands.MediaPlayback
{
    public class ChangeSpuDelayCommand : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
            if (Locator.MediaPlaybackViewModel.PlayingType == PlayingType.NotPlaying) return;
            var request = parameter.ToString();
            switch (request)
            {
                case "faster":
                    if (Locator.MediaPlaybackViewModel.SpuDelay < 3000)
                    {
                        Locator.MediaPlaybackViewModel.SpuDelay += 50;
                    }
                    break;
                case "slower":
                    if (Locator.MediaPlaybackViewModel.SpuDelay > -3000)
                    {
                        Locator.MediaPlaybackViewModel.SpuDelay -= 50;
                    }
                    break;
                case "reset":
                    Locator.MediaPlaybackViewModel.SpuDelay = 0;
                    break;
            }
        }
    }
}