/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using Autofac;
using VLC_WINRT.Common;
using VLC_WINRT_APP.Services.Interface;

namespace VLC_WINRT_APP.Commands.MediaPlayback
{
    public class PlayPauseCommand : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
            var playerService = App.Container.Resolve<IMediaService>();
            playerService.Pause();
        }
    }
}
