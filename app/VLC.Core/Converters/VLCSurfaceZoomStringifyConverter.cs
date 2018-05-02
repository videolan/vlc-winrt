using System;
using Windows.UI.Xaml.Data;
using VLC.Model.Video;
using VLC.Utils;

namespace VLC.Converters
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
                    case VLCSurfaceZoom.SURFACE_FIT_SCREEN:
                        return Strings.SURFACE_FIT_SCREEN.ToUpperFirstChar();
                    case VLCSurfaceZoom.SURFACE_FILL:
                        return Strings.SURFACE_FILL.ToUpperFirstChar();
                    case VLCSurfaceZoom.SURFACE_16_9:
                        return Strings.SURFACE_16_9.ToUpperFirstChar();
                    case VLCSurfaceZoom.SURFACE_4_3:
                        return Strings.SURFACE_4_3.ToUpperFirstChar();
                    case VLCSurfaceZoom.SURFACE_ORIGINAL:
                        return Strings.SURFACE_ORIGINAL.ToUpperFirstChar();
                    case VLCSurfaceZoom.SURFACE_2_35_1:
                        return Strings.SURFACE_2_35_1.ToUpperFirstChar();
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
