/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System;
using VLC.ViewModels;
using VLC.Model;
using VLC.Utils;

namespace VLC.Commands.VideoPlayer
{
    public class StopVideoCommand : AlwaysExecutableCommand
    {
        public override async void Execute(object parameter)
        {
            Locator.MediaPlaybackViewModel.UpdatePosition();

            Locator.MediaPlaybackViewModel.PlaybackService.Stop();
        }
    }
}
