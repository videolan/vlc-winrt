/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using VLC_WINRT.Common;
#if WINDOWS_PHONE_APP

#endif
using VLC_WINRT_APP;
using VLC_WINRT_APP.ViewModels;

namespace VLC_WINRT_APP.Commands.MediaPlayback
{
    public class StopCommand : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
            Locator.VideoVm.UnRegisterMediaControlEvents();
#if NETFX_CORE
            App.RootPage.MainFrame.GoBack();
#endif
        }
    }
}
