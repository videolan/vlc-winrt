/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using Windows.Media;
using VLC_WINRT.Common;
using VLC_WINRT.ViewModels;

namespace VLC_WINRT.Utility.Commands.MusicPlayer
{
    public class MusicPlayOrPauseCommand : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
            if (MediaControl.IsPlaying)
                Locator.MusicPlayerVM.Pause();
            else
                Locator.MusicPlayerVM.Resume();
        }
    }
}
