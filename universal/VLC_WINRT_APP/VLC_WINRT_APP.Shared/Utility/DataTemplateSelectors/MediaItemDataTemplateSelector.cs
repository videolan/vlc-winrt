/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace VLC_WINRT_APP.Utility.DataTemplateSelectors
{
    public class MediaItemDataTemplateSelector : DataTemplateSelector
    {
        // Full Size Video Template
        public DataTemplate WideMovieTemplate { get; set; }

        // Normal Size Video Template
        public DataTemplate NormalMovieTemplate { get; set; }

        //private int i = -1;
        
        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            return WideMovieTemplate;
            //i++;
            //return i == 0 ? WideMovieTemplate : NormalMovieTemplate;
        }
    }
}
