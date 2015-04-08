/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System;
using VLC_WinRT.Utils;

namespace VLC_WinRT.Commands
{
    public class ActionCommand : AlwaysExecutableCommand
    {
        private readonly Action _action;
        public ActionCommand(Action action)
        {
            _action = action;
        }

        public override void Execute(object parameter)
        {
            _action();
        }
    }
}
