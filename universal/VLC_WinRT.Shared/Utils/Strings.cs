using System.IO;
using Windows.ApplicationModel.Resources;
using Windows.Storage;

namespace VLC_WinRT.Utils
{
    /// <summary>
    /// Magic strings belong here
    /// </summary>
    public static class Strings
    {
        public static readonly string ModernFont = "ms-appx:SEGMDL2.TTF#Segoe MDL2 Assets";
        public static readonly string MusicDatabase = Path.Combine(ApplicationData.Current.LocalFolder.Path, "mediavlc.sqlite");
        public static readonly string VideoDatabase = Path.Combine(ApplicationData.Current.LocalFolder.Path, "mediavlcVideos.sqlite");
        public static readonly string SettingsDatabase = Path.Combine(ApplicationData.Current.LocalFolder.Path, "settings.sqlite");

        public static readonly char UnknownChar = '#';
        public static readonly string UnknownString = UnknownChar.ToString();

        private static ResourceLoader _resourcesLoader = ResourceLoader.GetForViewIndependentUse();

        public static readonly string Home = _resourcesLoader.GetString("Home");
        public static readonly string Music = _resourcesLoader.GetString("Music");
        public static readonly string RemovableStorage = _resourcesLoader.GetString("RemovableStorage");
        public static readonly string FileExplorer = "file explorer";
        
        public static readonly string UnknownArtist = _resourcesLoader.GetString("UnknownArtist");
        public static readonly string UnknownAlbum = _resourcesLoader.GetString("UnknownAlbum");
        public static readonly string UnknownTrack = _resourcesLoader.GetString("UnknownTrack");
        public static readonly string UnknownShow = _resourcesLoader.GetString("UntitledShow");

        public static readonly string Albums = _resourcesLoader.GetString("Albums");
        public static readonly string Artists = _resourcesLoader.GetString("Artists");
        public static readonly string Songs = _resourcesLoader.GetString("Songs");
        public static readonly string Playlists = _resourcesLoader.GetString("Playlists");

        public static readonly string Videos = _resourcesLoader.GetString("Videos");
        public static readonly string Shows = _resourcesLoader.GetString("Shows");
        public static readonly string CameraRoll = _resourcesLoader.GetString("CameraRoll/Text");


        public static readonly string OrderByArtist = _resourcesLoader.GetString("OrderByArtist");
        public static readonly string OrderByDate = _resourcesLoader.GetString("OrderByDate");
        public static readonly string OrderByAlbum = "by album";

        public static readonly string OrderAscending = _resourcesLoader.GetString("OrderAscending");
        public static readonly string OrderDescending = _resourcesLoader.GetString("OrderDescending");

        public static readonly string AlbumsFound = _resourcesLoader.GetString("AlbumsFound");

        public static readonly string PlaylistAlreadyExists = _resourcesLoader.GetString("PlaylistAlreadyExists");
        public static readonly string TrackAlreadyExistsInPlaylist = _resourcesLoader.GetString("TrackAlreadyExistsInPlaylist");
        public static readonly string TrackAddedToYourPlaylist = _resourcesLoader.GetString("TrackAddedToYourPlaylist");
        public static readonly string HaveToSelectPlaylist = _resourcesLoader.GetString("HaveToSelectPlaylist");

        public static readonly string CrashReport = _resourcesLoader.GetString("CrashReport");
        public static readonly string WeNeedYourHelp = _resourcesLoader.GetString("WeNeedYourHelp");

        public static readonly string Yes = _resourcesLoader.GetString("Yes");
        public static readonly string No = _resourcesLoader.GetString("No");

        public static readonly string NoBiographyFound = _resourcesLoader.GetString("NoBiographyFound");

        public static readonly string PrivacyStatement = _resourcesLoader.GetString("PrivacyStatement");
        public static readonly string SpecialThanks = _resourcesLoader.GetString("SpecialThanks");
        public static readonly string Settings = _resourcesLoader.GetString("Settings");
        public static readonly string License = "License";

        static Strings()
        {
            PlaylistAlreadyExists = "This playlist already exists";
            TrackAddedToYourPlaylist = "Track added to the playlist";
            TrackAlreadyExistsInPlaylist = "This track is already in the playlist";
            HaveToSelectPlaylist = "You need to select a playlist";
        }
    }
}
