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
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;
using VLC_WINRT.Model;
using Image = VLC_WINRT.Utility.Helpers.MusicLibrary.MusicEntities.Image;

namespace VLC_WINRT.Utility.Converters
{
    public class AlbumImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var topImage = value as List<Image>;
            return topImage == null ? null : topImage.LastOrDefault(image => !string.IsNullOrEmpty(image.Url)).Url;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
