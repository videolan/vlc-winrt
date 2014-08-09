/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
using VLC_WINRT.Common;
using VLC_WINRT_APP.ViewModels;
using VLC_WINRT_APP.Views.MainPages;

namespace VLC_WINRT_APP.Commands.MainPageCommands
{
    public class GoToPanelCommand : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
            App.RootPage.MainFrameThemeTransition.Edge = EdgeTransitionLocation.Bottom;
            Model.Panel panel = (parameter as ItemClickEventArgs).ClickedItem as Model.Panel;
            foreach (Model.Panel panel1 in Locator.MainVM.Panels)
                panel1.Color = new SolidColorBrush(Colors.DimGray);
            panel.Color = App.Current.Resources["MainColor"] as SolidColorBrush;
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
#if WINDOWS_APP
                case 3:
                    if (App.ApplicationFrame.CurrentSourcePageType != typeof(MainPageRemovables))
                        App.ApplicationFrame.Navigate(typeof(MainPageRemovables));
                    break;
                case 4:
                    if (App.ApplicationFrame.CurrentSourcePageType != typeof(MainPageMediaServers))
                        App.ApplicationFrame.Navigate(typeof(MainPageMediaServers));
                    break;
#endif
            }
        }
    }
}
