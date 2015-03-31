﻿using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
﻿using VLC_WinRT.Model;
﻿using VLC_WinRT.Views.UserControls;

namespace VLC_WinRT.Converters
{
    public class IsVisibleDependingOnSidebarState : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if ((SidebarState)value == SidebarState.Minimized)
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