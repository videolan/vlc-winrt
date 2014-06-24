/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using VLC_WINRT.Common;
using VLC_WINRT_APP.ViewModels;

namespace VLC_WINRT_APP.Commands.MediaPlayback
{
    public class PlayPreviousCommand : AlwaysExecutableCommand
    {
        public override async void Execute(object parameter)
        {
            if (Locator.MusicPlayerVM.IsPlaying)
            {
                // Music Logic
                Locator.MusicPlayerVM.PlayPrevious();
            }
            else
            {
                // Video Logic
            }
        }
    }
}
