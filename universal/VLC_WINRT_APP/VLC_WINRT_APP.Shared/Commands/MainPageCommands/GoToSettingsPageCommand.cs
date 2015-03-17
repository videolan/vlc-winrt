/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using VLC_WINRT.Common;
using VLC_WINRT_APP.Views.UserControls;
using VLC_WINRT_APP.Views.VariousPages;

namespace VLC_WINRT_APP.Commands.MainPageCommands
{
    public class GoToSettingsPageCommand : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
#if WINDOWS_PHONE_APP
            if (App.ApplicationFrame.CurrentSourcePageType != typeof(SettingsPage))
                App.ApplicationFrame.Navigate(typeof(SettingsPage));
#else
            var settingsFlyout = new SettingsFlyout();
            settingsFlyout.Show();
#endif
        }
    }
}
