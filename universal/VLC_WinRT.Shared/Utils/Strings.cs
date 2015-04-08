using System.IO;
using Windows.Storage;

namespace VLC_WinRT.Utils
{
    public static class Strings
    {
        public static readonly string ModernFont = "ms-appx:SEGMDL2.TTF#Segoe MDL2 Assets";
        public static readonly string MusicDatabase = Path.Combine(ApplicationData.Current.LocalFolder.Path, "mediavlc.sqlite");
        public static readonly string VideoDatabase = Path.Combine(ApplicationData.Current.LocalFolder.Path, "mediavlcVideos.sqlite");
        public static readonly string SettingsDatabase = Path.Combine(ApplicationData.Current.LocalFolder.Path, "settings.sqlite");
    }
}
