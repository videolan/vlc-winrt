﻿using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using VLC_WINRT_APP.Views.UserControls;

namespace VLC_WINRT_APP.Converters
{
    public class IsVisibleDependingOnSidebarState : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (App.RootPage.ColumnGrid.SidebarState == SidebarState.Minimized)
            {
                return Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}