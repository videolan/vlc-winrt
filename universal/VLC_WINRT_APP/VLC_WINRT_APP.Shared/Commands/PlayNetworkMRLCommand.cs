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
using VLC_WINRT_APP.Views.VideoPages;

namespace VLC_WINRT_APP.Commands
{
    public class PlayNetworkMRLCommand : AlwaysExecutableCommand
    {
        public override async void Execute(object parameter)
        {
            var mrl = parameter as string;
            if (string.IsNullOrEmpty(mrl))
                return;

            if (App.ApplicationFrame.CurrentSourcePageType != typeof(VideoPlayerPage))
            {
                App.ApplicationFrame.Navigate(typeof(VideoPlayerPage));
            }

#if WINDOWS_PHONE_APP
            Locator.MainVM.CloseStreamFlyout();
#endif

            //TODO: pass MRL to vlc
            await Locator.VideoVm.SetActiveVideoInfo(null, mrl);
        }
    }
}
