using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml.Data;
using VLC_WinRT.Model;
using VLC_WinRT.Model.Video;
using VLC_WinRT.Utils;

namespace VLC_WinRT.Converters
{
    public class VLCSurfaceZoomStringifyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is VLCSurfaceZoom)
            {
                switch ((VLCSurfaceZoom)value)
                {
                    case VLCSurfaceZoom.SURFACE_BEST_FIT:
                        return Strings.SURFACE_BEST_FIT.ToUpperFirstChar();
                    case VLCSurfaceZoom.SURFACE_FIT_HORIZONTAL:
                        return Strings.SURFACE_FIT_HORIZONTAL.ToUpperFirstChar();
                    case VLCSurfaceZoom.SURFACE_FIT_VERTICAL:
                        return Strings.SURFACE_FIT_VERTICAL.ToUpperFirstChar();
                    case VLCSurfaceZoom.SURFACE_STRETCH:
                        return Strings.SURFACE_STRETCH.ToUpperFirstChar();
                    default:
                        throw new ArgumentOutOfRangeException(nameof(value), value, null);
                }
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
