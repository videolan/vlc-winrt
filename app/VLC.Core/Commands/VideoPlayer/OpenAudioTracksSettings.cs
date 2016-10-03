using System;
using System.Collections.Generic;
using System.Text;
using VLC.Model;
using VLC.Utils;
using VLC.ViewModels;

namespace VLC.Commands.VideoPlayer
{
    public class ShowAudioTracksSettingsCommand : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
            Locator.NavigationService.Go(VLCPage.AudioTracksSettings);
        }
    }
}
