/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using VLC_WINRT.Common;
using VLC_WINRT.Utility.Services.RunTime;

namespace VLC_WINRT.Utility.Commands
{
    public class StopVideoCommand : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
            App.RootPage.MainFrame.GoBack();
        }
    }
}
