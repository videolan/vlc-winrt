using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace VLC.Converters
{
    public class PlayingStateToGlyphConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var isPlaying = (bool)value;
            if (isPlaying)
                return Application.Current.Resources["PauseSymbol"];
            return Application.Current.Resources["PlaySymbol"];
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
