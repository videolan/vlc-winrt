using System;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Data;
using VLC_WinRT.Model.Music;
using VLC_WinRT.Utils;

namespace VLC_WinRT.Converters
{
    public class MusicViewStringifyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is MusicView)
            {
                switch ((MusicView)value)
                {
                    case MusicView.Albums:
                        return Strings.Albums.ToUpperFirstChar();
                    case MusicView.Artists:
                        return Strings.Artists.ToUpperFirstChar();
                    case MusicView.Playlists:
                        return Strings.Playlists.ToUpperFirstChar();
                    case MusicView.Songs:
                        return Strings.Songs.ToUpperFirstChar();
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
