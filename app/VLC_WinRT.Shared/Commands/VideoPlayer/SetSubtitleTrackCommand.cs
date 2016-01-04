/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using VLC_WinRT.Utils;
using VLC_WinRT.ViewModels;

namespace VLC_WinRT.Commands.VideoPlayer
{
    public class SetSubtitleTrackCommand : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
            Locator.MediaPlaybackViewModel.SetSubtitleTrack((int)parameter);
        }
    }
}
