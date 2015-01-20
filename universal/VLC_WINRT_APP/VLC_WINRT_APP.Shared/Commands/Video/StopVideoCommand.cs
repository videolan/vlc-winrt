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
using VLC_WINRT_APP.Views.MainPages;

namespace VLC_WINRT_APP.Commands.Video
{
    public class StopVideoCommand : AlwaysExecutableCommand
    {
        public override async void Execute(object parameter)
        {
            await Locator.VideoVm.UpdatePosition();
            if(App.ApplicationFrame.CanGoBack)
                App.ApplicationFrame.GoBack();
            else
                App.ApplicationFrame.Navigate(typeof (MainPageHome));
            await Locator.VideoVm.CleanViewModel();
            Locator.VideoVm.IsRunning = false;
        }
    }
}
