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
using VLC_WINRT_APP.ViewModels;
using VLC_WINRT_APP.Views.MainPages;

namespace VLC_WINRT_APP.Commands.MainPage
{
    public class GoToPanelCommand : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
            Model.Panel panel = e.ClickedItem as Model.Panel;
            foreach (Model.Panel panel1 in Locator.MainVM.Panels)
                panel1.Opacity = 0.4;
            panel.Opacity = 1;
            switch (panel.Index)
            {
                case 0:
                    if (App.ApplicationFrame.CurrentSourcePageType != typeof(MainPageHome))
                        App.ApplicationFrame.Navigate(typeof(MainPageHome));
                    break;
                case 1:
                    if (App.ApplicationFrame.CurrentSourcePageType != typeof(MainPageVideos))
                        App.ApplicationFrame.Navigate(typeof(MainPageVideos));
                    break;
                case 2:
                    if (App.ApplicationFrame.CurrentSourcePageType != typeof(MainPageMusic))
                        App.ApplicationFrame.Navigate(typeof(MainPageMusic));
                    break;
                case 3:
                    if (App.ApplicationFrame.CurrentSourcePageType != typeof(MainPageRemovables))
                        App.ApplicationFrame.Navigate(typeof(MainPageRemovables));
                    break;
                case 4:
                    if (App.ApplicationFrame.CurrentSourcePageType != typeof(MainPageMediaServers))
                        App.ApplicationFrame.Navigate(typeof(MainPageMediaServers));
                    break;
            }
        }
    }
}
