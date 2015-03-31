/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using VLC_WINRT.Common;
using VLC_WinRT.Views.VariousPages;

namespace VLC_WinRT.Commands.MainPageCommands
{
    public class GoToThanksPageCommand: AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
            if (App.ApplicationFrame.CurrentSourcePageType != typeof (SpecialThanks))
            {
                App.ApplicationFrame.Navigate(typeof (SpecialThanks));
            }
        }
    }
}
