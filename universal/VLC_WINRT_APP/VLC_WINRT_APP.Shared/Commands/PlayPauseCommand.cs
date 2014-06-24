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
using VLC_WINRT_APP.Utility.Services.Interface;
#if WINDOWS_PHONE_APP
using VLC_WINPRT;
#endif
using VLC_WINRT_APP;

namespace VLC_WINRT_APP.Commands
{
    public class PlayPauseCommand : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
            var playerService = App.Container.Resolve<IMediaService>();
            if (playerService.IsPlaying)
            {
                playerService.Pause();
            }
            else
            {
                playerService.Play();
            }
        }
    }
}
