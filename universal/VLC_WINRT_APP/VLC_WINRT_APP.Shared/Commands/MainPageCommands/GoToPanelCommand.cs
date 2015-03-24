/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
using VLC_WINRT.Common;
using VLC_WINRT_APP.Model;
using VLC_WINRT_APP.ViewModels;
using VLC_WINRT_APP.Views.MainPages;

namespace VLC_WINRT_APP.Commands.MainPageCommands
{

    public class GoToPanelCommand : AlwaysExecutableCommand
    {
        public override async void Execute(object parameter)
        {
            Model.Panel panel = null;
            if (parameter is ItemClickEventArgs)
            {
                panel = (parameter as ItemClickEventArgs).ClickedItem as Model.Panel;
            }
            else if (parameter is SelectionChangedEventArgs)
            {
                panel = (parameter as SelectionChangedEventArgs).AddedItems[0] as Model.Panel;
            }
            else if (parameter is string || parameter is int)
            {
                panel = Locator.MainVM.Panels.First(x => x.Index == int.Parse(parameter.ToString()));
            }

            foreach (Model.Panel panel1 in Locator.MainVM.Panels)
            {
                panel1.IsCurrent = false;
            }
            if (panel != null)
            {
                panel.IsCurrent = true;
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

#if WINDOWS_APP
                    case 4:
                        if (App.ApplicationFrame.CurrentSourcePageType != typeof(MainPageMediaServers))
                            App.ApplicationFrame.Navigate(typeof(MainPageMediaServers));
                        break;
#endif
                }
            }
        }
    }
}