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
using VLC_WINPRT;
#endif
using VLC_WINRT_APP;

namespace VLC_WINRT.Utility.Commands.MainPage
{
    public class GoToPanelCommand : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
        }
    }
}
