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

        public static string PodcastFolderName => "Podcasts";

        public static readonly char UnknownChar = '#';
        public static readonly string UnknownString = UnknownChar.ToString();

        private static readonly ResourceLoader _resourcesLoader = ResourceLoader.GetForViewIndependentUse();

        public static string Home => _resourcesLoader.GetString(nameof(Home));
        public static string Music => _resourcesLoader.GetString(nameof(Music));
        public static string RemovableStorage => _resourcesLoader.GetString(nameof(RemovableStorage));
        public static string FileExplorer => _resourcesLoader.GetString(nameof(FileExplorer));
        public static string Network => _resourcesLoader.GetString(nameof(Network));
        public static string Library => _resourcesLoader.GetString(nameof(Library));

        public static string TopVideos => _resourcesLoader.GetString(nameof(TopVideos));
        public static string YourMusic => _resourcesLoader.GetString(nameof(YourMusic));
        public static string MostPlayedArtists => _resourcesLoader.GetString(nameof(MostPlayedArtists));
        public static string RecommendedForYou => _resourcesLoader.GetString(nameof(RecommendedForYou));


        // MUSIC ITEMS
        public static string UnknownArtist => _resourcesLoader.GetString(nameof(UnknownArtist));
        public static string UnknownAlbum => _resourcesLoader.GetString(nameof(UnknownAlbum));
        public static string UnknownTrack => _resourcesLoader.GetString(nameof(UnknownTrack));
        public static string UnknownShow => _resourcesLoader.GetString(nameof(UnknownShow));

        public static string Albums => _resourcesLoader.GetString(nameof(Albums));
        public static string Artists => _resourcesLoader.GetString(nameof(Artists));
        public static string Songs => _resourcesLoader.GetString(nameof(Songs));
        public static string Playlists => _resourcesLoader.GetString(nameof(Playlists));
        public static string Tracks => _resourcesLoader.GetString(nameof(Tracks));

        // MUSIC MICS
        public static string MusicFolders => _resourcesLoader.GetString(nameof(MusicFolders));
        public static string OrderByArtist => _resourcesLoader.GetString(nameof(OrderByArtist));
        public static string OrderByDate => _resourcesLoader.GetString(nameof(OrderByDate));
        public static string OrderByAlbum => _resourcesLoader.GetString(nameof(OrderByAlbum));


        // VIDEOS ITEMS
        public static string Videos => _resourcesLoader.GetString(nameof(Videos));
        public static string Shows => _resourcesLoader.GetString(nameof(Shows));
        public static string CameraRoll => _resourcesLoader.GetString(nameof(CameraRoll));



        public static string OrderAscending => _resourcesLoader.GetString(nameof(OrderAscending));
        public static string OrderDescending => _resourcesLoader.GetString(nameof(OrderDescending));

        public static string AlbumsFound => _resourcesLoader.GetString(nameof(AlbumsFound));
        public static string NewVideos => _resourcesLoader.GetString(nameof(NewVideos));
        
        public static string PlaylistAlreadyExists => _resourcesLoader.GetString(nameof(PlaylistAlreadyExists));
        public static string TrackAlreadyExistsInPlaylist => _resourcesLoader.GetString(nameof(TrackAlreadyExistsInPlaylist));
        public static string TrackAddedToYourPlaylist => _resourcesLoader.GetString(nameof(TrackAddedToYourPlaylist));
        public static string HaveToSelectPlaylist => _resourcesLoader.GetString(nameof(HaveToSelectPlaylist));

        public static string CrashReport => _resourcesLoader.GetString(nameof(CrashReport));
        public static string WeNeedYourHelp => _resourcesLoader.GetString(nameof(WeNeedYourHelp));

        public static string Yes => _resourcesLoader.GetString(nameof(Yes));
        public static string No => _resourcesLoader.GetString(nameof(No));



        // ACTIONS
        public static string PlayAlbum => _resourcesLoader.GetString(nameof(PlayAlbum));
        public static string PlayAll => _resourcesLoader.GetString(nameof(PlayAll));
        public static string NewPlaylist => _resourcesLoader.GetString(nameof(NewPlaylist));
        public static string AddToPlaylist => _resourcesLoader.GetString(nameof(AddToPlaylist));
        public static string ViewArtist => _resourcesLoader.GetString(nameof(ViewArtist));
        public static string AddToCollection => _resourcesLoader.GetString(nameof(AddToCollection));
        public static string RemoveFolder => _resourcesLoader.GetString(nameof(RemoveFolder));
        public static string AddFolder => _resourcesLoader.GetString(nameof(AddFolder));
        public static string Connect => _resourcesLoader.GetString(nameof(Connect));
        public static string Reset => _resourcesLoader.GetString(nameof(Reset));
        public static string ResetMusicDatabase => _resourcesLoader.GetString(nameof(ResetMusicDatabase));

        // PLACEHOLDERS
        public static string PlaylistNamePlaceholder => _resourcesLoader.GetString(nameof(PlaylistNamePlaceholder));
        public static string NoBiographyFound => _resourcesLoader.GetString(nameof(NoBiographyFound));
        public static string Username => _resourcesLoader.GetString(nameof(Username));
        public static string Password => _resourcesLoader.GetString(nameof(Password));


        // SETTINGS

        public static string PrivacyStatement => _resourcesLoader.GetString(nameof(PrivacyStatement));
        public static string SpecialThanks => _resourcesLoader.GetString(nameof(SpecialThanks));
        public static string Settings => _resourcesLoader.GetString(nameof(Settings));
        public static string License => _resourcesLoader.GetString(nameof(License));
        public static string UserInterface => _resourcesLoader.GetString(nameof(UserInterface));
        public static string VideoSettings => _resourcesLoader.GetString(nameof(VideoSettings));
        public static string MusicSettings => _resourcesLoader.GetString(nameof(MusicSettings));
        public static string AboutTheApp => _resourcesLoader.GetString(nameof(AboutTheApp));
        public static string Notifications => _resourcesLoader.GetString(nameof(Notifications));
        public static string NotificationWhenSongStarts => _resourcesLoader.GetString(nameof(NotificationWhenSongStarts));
        public static string EvenIfBackground => _resourcesLoader.GetString(nameof(EvenIfBackground));
        public static string EvenIfNotBackground => _resourcesLoader.GetString(nameof(EvenIfNotBackground));
        public static string ConnectToLastFM => _resourcesLoader.GetString(nameof(ConnectToLastFM));
        public static string ConnectedToLastFM => _resourcesLoader.GetString(nameof(ConnectedToLastFM));

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
