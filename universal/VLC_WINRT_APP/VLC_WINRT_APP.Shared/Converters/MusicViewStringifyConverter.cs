using System;
using System.Collections.Generic;
using System.Text;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Data;
using VLC_WINRT_APP.Model;
using VLC_WINRT_APP.Model.Music;

namespace VLC_WINRT_APP.Converters
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
                        return "albums";
                    case MusicView.Artists:
                        return "artists";
                    case MusicView.Playlists:
                        return "playlists";
                    case MusicView.Songs:
                        return "songs";
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
