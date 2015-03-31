/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using VLC_WINRT.Common;
using VLC_WinRT.ViewModels;

namespace VLC_WinRT.Commands.MediaPlayback
{
    public class PlayPreviousCommand : AlwaysExecutableCommand
    {
        public override async void Execute(object parameter)
        {
#if WINDOWS_APP
            if (Locator.MediaPlaybackViewModel.IsPlaying)
            {
                // Music Logic
                await Locator.MediaPlaybackViewModel.PlayPrevious();
            }
            else
            {
                // Video Logic
            }
#else
            await Locator.MediaPlaybackViewModel.PlayPrevious();
#endif
        }
    }
}
