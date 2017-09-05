/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using Slide2D.Images;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Data;

namespace VLC.Converters
{
    public class AlbumImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var topImage = value as List<Img>;
            if (topImage == null)
                return null;

            var albumImage = topImage.LastOrDefault(image => !string.IsNullOrEmpty(image.Src));
            return albumImage?.Src;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
