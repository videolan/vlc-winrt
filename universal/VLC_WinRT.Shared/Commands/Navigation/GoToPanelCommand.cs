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
using VLC_WinRT.Model;
using VLC_WinRT.Utils;
using VLC_WinRT.ViewModels;
using VLC_WinRT.Views.MainPages;

namespace VLC_WinRT.Commands.Navigation
{

    public class GoToPanelCommand : AlwaysExecutableCommand
    {
        public override async void Execute(object parameter)
        {
            Model.Panel panel = null;
            if (parameter is Model.Panel)
            {
                panel = parameter as Model.Panel;
            }
            else if (parameter is SelectionChangedEventArgs)
            {
                panel = (parameter as SelectionChangedEventArgs).AddedItems[0] as Model.Panel;
            }
            else if (parameter is string || parameter is int)
            {
                panel = Locator.MainVM.Panels.First(x => x.Index == int.Parse(parameter.ToString()));
            }

#if WINDOWS_PHONE_APP
            var iPreviousView = App.RootPage.ShellContent.CurrentViewIndex;
            var iNewView = Locator.MainVM.Panels.IndexOf(panel);
            App.RootPage.ShellContent.SetPivotAnimation(iNewView > iPreviousView);
#endif

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
            }
            
            if (App.SplitShell.IsRightFlyoutOpen)
            {
                Locator.NavigationService.GoBack_HideFlyout();
            }
        }
    }
}