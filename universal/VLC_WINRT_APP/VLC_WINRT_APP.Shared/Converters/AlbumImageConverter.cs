/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Data;
using VLC_WINRT_APP.Helpers.MusicLibrary.MusicEntities;

namespace VLC_WINRT_APP.Converters
{
    public class AlbumImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var topImage = value as List<Image>;
            if (topImage == null)
                return null;

            var albumImage = topImage.LastOrDefault(image => !string.IsNullOrEmpty(image.Url));
            if (albumImage == null)
                return null;
            else
                return albumImage.Url;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
