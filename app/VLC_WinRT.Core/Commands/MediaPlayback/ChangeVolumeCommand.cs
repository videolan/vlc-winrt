using System;
using System.Collections.Generic;
using System.Text;
using VLC.Model;
using VLC.Utils;
using VLC.ViewModels;

namespace VLC.Commands.MediaPlayback
{
    public class ChangeVolumeCommand : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
            if (Locator.MediaPlaybackViewModel.PlaybackService.PlayingType == PlayingType.NotPlaying)
                return;

            var request = parameter.ToString();
            switch (request)
            {
                case "higher":
                    if (Locator.MediaPlaybackViewModel.Volume < 100)
                    {
                        Locator.MediaPlaybackViewModel.Volume += 5;
                    }
                    break;
                case "lower":
                    if (Locator.MediaPlaybackViewModel.Volume > 0)
                    {
                        Locator.MediaPlaybackViewModel.Volume -= 5;
                    }
                    break;
                case "reset":
                    Locator.MediaPlaybackViewModel.Volume = 100;
                    break;
                case "mute":
                    Locator.MediaPlaybackViewModel.Volume = 1;
                    break;
            }
        }
    }
}
