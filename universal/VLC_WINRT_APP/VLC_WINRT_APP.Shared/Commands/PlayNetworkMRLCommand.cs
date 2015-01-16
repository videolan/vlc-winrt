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
using VLC_WINRT_APP.Helpers;
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

            //TODO: pass MRL to vlc
            try
            {
                await Locator.VideoVm.SetActiveVideoInfo(null, mrl);
            }
            catch (Exception ex)
            {
                ExceptionHelper.CreateMemorizedException("PlayNetworkMRLCommand.Execute", ex);
                return;
            }

            if (App.ApplicationFrame.CurrentSourcePageType != typeof(VideoPlayerPage))
            {
                App.ApplicationFrame.Navigate(typeof(VideoPlayerPage));
            }

#if WINDOWS_PHONE_APP
            Locator.MainVM.CloseStreamFlyout();
#endif
        }
    }
}
