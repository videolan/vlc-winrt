/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using VLC.Utils;
using VLC.ViewModels;

namespace VLC.Commands.MediaPlayback
{
    public class PlayPreviousCommand : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
            Locator.PlaybackService.Previous();
        }
    }
}
