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
using VLC_WinRT.ViewModels;
using VLC_WinRT.Model;

namespace VLC_WinRT.Commands.Video
{
    public class StopVideoCommand : AlwaysExecutableCommand
    {
        public override async void Execute(object parameter)
        {
            await Locator.MediaPlaybackViewModel.UpdatePosition();

            if (parameter is Type)
            {
                App.ApplicationFrame.Navigate((Type)parameter);
            }
            else if (App.ApplicationFrame.CanGoBack)
                Locator.NavigationService.GoBack_Default();
            else
                Locator.NavigationService.Go(VLCPage.MainPageHome);
            await Locator.MediaPlaybackViewModel.CleanViewModel();
            Locator.MediaPlaybackViewModel.IsPlaying = false;
        }
    }
}
