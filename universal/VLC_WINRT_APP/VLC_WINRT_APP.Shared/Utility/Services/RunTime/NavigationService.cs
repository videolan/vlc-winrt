/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System;
using Windows.UI.Xaml;
using VLC_WINRT.Views;

namespace VLC_WINRT.Utility.Services.RunTime
{
    public class NavigationService
    {
        public static void NavigateTo(Type page)
        {
            ((RootPage)Window.Current.Content).MainFrame.Navigate(page);
        }
    }
}
