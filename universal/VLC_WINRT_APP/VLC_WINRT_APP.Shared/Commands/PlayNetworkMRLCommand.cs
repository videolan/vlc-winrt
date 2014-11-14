/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System;
using VLC_WINRT.Common;
using VLC_WINRT_APP.Services.RunTime;
using VLC_WINRT_APP;
using VLC_WINRT_APP.ViewModels;
using VLC_WINRT_APP.Views.VideoPages;
#if WINDOWS_PHONE_APP

#endif

namespace VLC_WINRT_APP.Commands
{
    public class PlayNetworkMRLCommand : AlwaysExecutableCommand
    {
        public override async void Execute(object parameter)
        {
            var mrl = parameter as string;
            if (string.IsNullOrEmpty(mrl))
                throw new ArgumentException("Expecting to see a string mrl for this command");

            //TODO: pass MRL to vlc
            await Locator.VideoVm.SetActiveVideoInfo(mrl, true);
            #if WINDOWS_APP
            App.ApplicationFrame.Navigate(typeof(VideoPlayerPage));
            #endif
        }
    }
}
