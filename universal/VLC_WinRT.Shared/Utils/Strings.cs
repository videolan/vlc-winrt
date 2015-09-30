using System;
using System.IO;
using System.Linq;
using Windows.ApplicationModel;
using VLC_WinRT.Model.Music;
using Windows.ApplicationModel.Resources;
using Windows.Security.ExchangeActiveSyncProvisioning;
using Windows.Storage;

namespace VLC_WinRT.Utils
{
    /// <summary>
    /// Magic strings belong here
    /// </summary>
    public class Strings
    {
        public static readonly string DatabaseVersion = "DatabaseVersion";

        /// <summary>
        /// Returns the current App Version
        /// </summary>
        public static string AppVersion
        {
            get
            {
                PackageVersion version = Package.Current.Id.Version;
                return $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
            }
        }

        public static string DeviceModel
        {
            get
            {
                var deviceInfo = new EasClientDeviceInformation();
                if (string.IsNullOrEmpty(deviceInfo.SystemManufacturer) ||
                    string.IsNullOrEmpty(deviceInfo.SystemProductName))
                {
                    return "Unknown model";
                }
                return $"{deviceInfo?.SystemManufacturer ?? "Unknown OEM"} - {deviceInfo?.SystemProductName ?? "Unknown model"}";
            }
        }

        public static string FeedbackAzureURL => "https://vlc.azure-mobile.net/tables/feedback";

        /// <summary>
         /// Appends the current memory usage and limits on Windows Phone to the <paramref name="stringBuilder"/>
         /// </summary>
        public static string MemoryUsage()
        {
#if WINDOWS_PHONE_APP
            try
            {
                // Gets the app's current memory usage    
                ulong AppMemoryUsageUlong = Windows.System.MemoryManager.AppMemoryUsage;
                // Gets the app's memory usage limit    
                ulong AppMemoryUsageLimitUlong = Windows.System.MemoryManager.AppMemoryUsageLimit;

                AppMemoryUsageUlong /= 1024 * 1024;
                AppMemoryUsageLimitUlong /= 1024 * 1024;
                return "UsedRAM : " + AppMemoryUsageUlong + " - MaxRAM : " + AppMemoryUsageLimitUlong;
            }
            catch { }
#endif
            return null;
        }

        public static readonly string VideoPicFolderPath = "ms-appdata:///local/videoPic/";

        public static readonly string FeedbackMailAdress = "modernvlc@outlook.com";

        public static readonly string ModernFont = "ms-appx:VLC.ttf#W10";
        public static readonly string MusicDatabase = Path.Combine(ApplicationData.Current.LocalFolder.Path, "mediavlc.sqlite");
        public static readonly string VideoDatabase = Path.Combine(ApplicationData.Current.LocalFolder.Path, "mediavlcVideos.sqlite");
        public static readonly string SettingsDatabase = Path.Combine(ApplicationData.Current.LocalFolder.Path, "settings.sqlite");

        public static readonly string PodcastFolderName = "Podcasts";

        public static readonly char UnknownChar = '#';
        public static readonly string UnknownString = UnknownChar.ToString();

        private static readonly ResourceLoader _resourcesLoader = ResourceLoader.GetForViewIndependentUse();

        public static readonly string Home = _resourcesLoader.GetString("Home");
        public static readonly string Music = _resourcesLoader.GetString("Music");
        public static readonly string RemovableStorage = _resourcesLoader.GetString("RemovableStorage");
        public static readonly string FileExplorer = _resourcesLoader.GetString("Files");
        public static readonly string Network = _resourcesLoader.GetString("network");
        public static readonly string Library = _resourcesLoader.GetString("library");

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
        public static readonly string OrderByAlbum = _resourcesLoader.GetString("OrderByAlbum");

        public static readonly string OrderAscending = _resourcesLoader.GetString("OrderAscending");
        public static readonly string OrderDescending = _resourcesLoader.GetString("OrderDescending");

        public static readonly string AlbumsFound = _resourcesLoader.GetString("AlbumsFound");
        public static readonly string NewVideo = _resourcesLoader.GetString("VideosFound");
        
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
        public static readonly string License = _resourcesLoader.GetString("License");

        public Strings()
        {
        }

        public static string HumanizedArtistName(string artistName) { return string.IsNullOrEmpty(artistName) ? Strings.UnknownArtist : artistName; }
        public static string HumanizedAlbumName(string albumName) { return string.IsNullOrEmpty(albumName) ? Strings.UnknownAlbum : albumName; }
        public static string HumanizedYear(int year) { return year == 0 ? Strings.UnknownString : year.ToString(); }
        public static string HumanizedAlbumFirstLetter(string albumName)
        {
            return string.IsNullOrEmpty(albumName) ? Strings.UnknownString : (char.IsLetter(albumName.ElementAt(0)) ? albumName.ElementAt(0).ToString().ToUpper() : Strings.UnknownString);
        }
    }
}
