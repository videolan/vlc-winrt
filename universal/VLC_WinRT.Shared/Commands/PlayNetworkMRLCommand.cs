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
using VLC_WinRT.Helpers;
using VLC_WinRT.Model.Stream;
using VLC_WinRT.ViewModels;
using VLC_WinRT.Model;

namespace VLC_WinRT.Commands
{
    public class PlayNetworkMRLCommand : AlwaysExecutableCommand
    {
        public override async void Execute(object parameter)
        {
            var mrl = parameter as string;
            if (string.IsNullOrEmpty(mrl))
            {
                ToastHelper.Basic("Please enter a valid URL.");
                return;
            }
            mrl = mrl.Trim();
            //TODO: pass MRL to vlc
            try
            {
                var stream = new StreamMedia(mrl);
                await Locator.MediaPlaybackViewModel.SetMedia(stream);
            }
            catch (Exception ex)
            {
                ExceptionHelper.CreateMemorizedException("PlayNetworkMRLCommand.Execute", ex);
                return;
            }
            Locator.NavigationService.Go(VLCPage.VideoPlayerPage);
            Locator.MainVM.CloseStreamFlyout();
        }
    }
}
