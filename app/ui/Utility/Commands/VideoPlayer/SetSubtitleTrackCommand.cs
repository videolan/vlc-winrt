using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VLC_WINRT.Common;
using VLC_WINRT.ViewModels;

namespace VLC_WINRT.Utility.Commands.VideoPlayer
{
    public class SetSubtitleTrackCommand : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
            Locator.PlayVideoVM.SetSubtitleTrack((int)parameter);
        }
    }
}
