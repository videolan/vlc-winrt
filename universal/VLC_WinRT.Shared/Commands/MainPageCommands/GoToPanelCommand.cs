/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System.Linq;
using Windows.UI.Xaml.Controls;
using VLC_WINRT.Common;
using VLC_WinRT.Model;
using VLC_WinRT.Utils;
using VLC_WinRT.ViewModels;
using VLC_WinRT.Views.MainPages;

namespace VLC_WinRT.Commands.MainPageCommands
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
                        Locator.NavigationService.Go(VLCPage.MainPageHome);
                        break;
                    case 1:
                        Locator.NavigationService.Go(VLCPage.MainPageVideo);
                        break;
                    case 2:
                        Locator.NavigationService.Go(VLCPage.MainPageMusic);
                        break;
                    case 3:
                        Locator.NavigationService.Go(VLCPage.MainPageFileExplorer);
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