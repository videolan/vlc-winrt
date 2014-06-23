/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using Windows.UI.Xaml.Controls;
using VLC_WINRT.Common;
#if WINDOWS_PHONE_APP
using VLC_WINPRT;
#endif
using VLC_WINRT.ViewModels;
using VLC_WINRT_APP;
using VLC_WINRT_APP.ViewModels;
using VLC_WINRT_APP.Views.MainPages;

namespace VLC_WINRT_APP.Utility.Commands.MainPage
{
    public class GoToPanelCommand : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
            Model.Panel panel = ((ItemClickEventArgs)parameter).ClickedItem as Model.Panel;
            foreach (Model.Panel panel1 in Locator.MainVM.Panels)
            {
                panel1.Opacity = 0.4;
            }
            panel.Opacity = 1;
            switch (panel.Title)
            {
                case "home":
                    if (App.ApplicationFrame.CurrentSourcePageType != typeof(MainPageHome))
                        App.ApplicationFrame.Navigate(typeof(MainPageHome));
                    break;
                case "videos":
                    if (App.ApplicationFrame.CurrentSourcePageType != typeof(MainPageVideos))
                        App.ApplicationFrame.Navigate(typeof(MainPageVideos));
                    break;
                case "music":
                    if (App.ApplicationFrame.CurrentSourcePageType != typeof(MainPageMusic))
                        App.ApplicationFrame.Navigate(typeof(MainPageMusic));
                    break;
            }
        }
    }
}
