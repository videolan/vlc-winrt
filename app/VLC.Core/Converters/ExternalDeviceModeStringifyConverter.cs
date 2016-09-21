using System;
using Windows.UI.Xaml.Data;
using VLC.Model;
using VLC.Utils;

namespace VLC.Converters
{
    public class ExternalDeviceModeStringifyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is ExternalDeviceMode)
            {
                switch ((ExternalDeviceMode)value)
                {
                    case ExternalDeviceMode.AskMe:
                        return Strings.AskMe;
                    case ExternalDeviceMode.IndexMedias:
                        return Strings.ReadFromExternalStorage;
                    case ExternalDeviceMode.SelectMedias:
                        return Strings.SelectContentToCopy;
                    case ExternalDeviceMode.DoNothing:
                        return Strings.DoNothing;
                    default:
                        throw new NotImplementedException();
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
