﻿using System;
using System.IO;
using System.Linq;
using Windows.ApplicationModel;
using VLC.Model.Music;
using Windows.ApplicationModel.Resources;
using Windows.Security.ExchangeActiveSyncProvisioning;
using Windows.Storage;

namespace VLC.Utils
{
    /// <summary>
    /// Magic strings belong here
    /// </summary>
    public class Strings
    {
        public static readonly string AlreadyLaunched = "AlreadyLaunched";
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

        /// <summary>
        /// Appends the current memory usage and limits on Windows Phone to the <paramref name="stringBuilder"/>
        /// </summary>
        public static string MemoryUsage()
        {
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
            return null;
        }

        public static readonly string VideoThumbsFolderPath = "ms-appdata:///local/videoThumbs";
        public static readonly string MoviePicFolderPath = "ms-appdata:///local/moviePic";
        public static readonly string MovieSubFolderPath = "ms-appdata:///local/movieSub";
        public static readonly string TemporaryFolderPath = "ms-appdata:///temp";

        public static readonly string ModernFont = "ms-appx:VLC.ttf#VLC";
        public static readonly string MusicDatabase = Path.Combine(ApplicationData.Current.LocalFolder.Path, "mediavlc.sqlite");
        public static readonly string MusicBackgroundDatabase = Path.Combine(ApplicationData.Current.LocalFolder.Path, "background.sqlite");
        public static readonly string VideoDatabase = Path.Combine(ApplicationData.Current.LocalFolder.Path, "mediavlcVideos.sqlite");
        public static readonly string SettingsDatabase = Path.Combine(ApplicationData.Current.LocalFolder.Path, "settings.sqlite");

        public static string PodcastFolderName => "Podcasts";

        public static readonly char UnknownChar = '#';
        public static readonly string UnknownString = UnknownChar.ToString();

        private static readonly ResourceLoader _resourcesLoader = ResourceLoader.GetForViewIndependentUse();

        public static string Dash => "­­­ — ";

        public static string Home => _resourcesLoader.GetString(nameof(Home));
        public static string Music => _resourcesLoader.GetString(nameof(Music));
        public static string RemovableStorage => _resourcesLoader.GetString(nameof(RemovableStorage));
        public static string FileExplorer => _resourcesLoader.GetString(nameof(FileExplorer));
        public static string Network => _resourcesLoader.GetString(nameof(Network));
        public static string Local => _resourcesLoader.GetString(nameof(Local));
        public static string Search => _resourcesLoader.GetString(nameof(Search));
        public static string CopyHelpDesktop => _resourcesLoader.GetString(nameof(CopyHelpDesktop));
        public static string CopyHelpMediaCenter => _resourcesLoader.GetString(nameof(CopyHelpMediaCenter));
        public static string CopyHelpPhone => _resourcesLoader.GetString(nameof(CopyHelpPhone));

        public static string YourMusic => _resourcesLoader.GetString(nameof(YourMusic));
        public static string MostPlayedArtists => _resourcesLoader.GetString(nameof(MostPlayedArtists));
        public static string Favorites => _resourcesLoader.GetString(nameof(Favorites));
        public static string Loading => _resourcesLoader.GetString(nameof(Loading));
        public static string LoadingMusic => _resourcesLoader.GetString(nameof(LoadingMusic));
        public static string Download => "Download";
        public static string NoInternetConnection => _resourcesLoader.GetString(nameof(NoInternetConnection));

        // MUSIC ITEMS
        public static string UnknownArtist => _resourcesLoader.GetString(nameof(UnknownArtist));
        public static string UnknownAlbum => _resourcesLoader.GetString(nameof(UnknownAlbum));
        public static string UnknownTrack => _resourcesLoader.GetString(nameof(UnknownTrack));
        public static string UnknownShow => _resourcesLoader.GetString(nameof(UnknownShow));

        public static string Albums => _resourcesLoader.GetString(nameof(Albums));
        public static string Artists => _resourcesLoader.GetString(nameof(Artists));
        public static string Songs => _resourcesLoader.GetString(nameof(Songs));
        public static string Playlists => _resourcesLoader.GetString(nameof(Playlists));
        public static string MyPlaylists => _resourcesLoader.GetString(nameof(MyPlaylists));
        public static string Tracks => _resourcesLoader.GetString(nameof(Tracks));
        public static string MusicShows => _resourcesLoader.GetString(nameof(MusicShows));

        public static string SimilarArtists => _resourcesLoader.GetString(nameof(SimilarArtists));
        public static string MoreInfo => "more";

        // MUSIC MICS
        public static string Audio => _resourcesLoader.GetString(nameof(Audio));
        public static string MusicFolders => _resourcesLoader.GetString(nameof(MusicFolders));
        public static string MusicFoldersDescription => _resourcesLoader.GetString(nameof(MusicFoldersDescription));
        public static string OrderByArtist => _resourcesLoader.GetString(nameof(OrderByArtist));
        public static string OrderByDate => _resourcesLoader.GetString(nameof(OrderByDate));
        public static string OrderByAlbum => _resourcesLoader.GetString(nameof(OrderByAlbum));
        public static string Biography => "Biography";

        // VIDEOS ITEMS
        public static string Videos => _resourcesLoader.GetString(nameof(Videos));
        public static string Shows => _resourcesLoader.GetString(nameof(Shows));
        public static string CameraRoll => _resourcesLoader.GetString(nameof(CameraRoll));
        public static string Season => _resourcesLoader.GetString(nameof(Season));
        public static string Episode => _resourcesLoader.GetString(nameof(Episode));

        // VIDEO MISC
        public static string Video => _resourcesLoader.GetString(nameof(Video));
        public static string VideoFolders => _resourcesLoader.GetString(nameof(VideoFolders));
        public static string VideoFoldersDescription => _resourcesLoader.GetString(nameof(VideoFoldersDescription));
        public static string PlayerSettings => _resourcesLoader.GetString(nameof(PlayerSettings));
        public static string AudioDelay => _resourcesLoader.GetString(nameof(AudioDelay));
        public static string SubtitleDelay => _resourcesLoader.GetString(nameof(SubtitleDelay));
        public static string Zoom => _resourcesLoader.GetString(nameof(Zoom));
        public static string SURFACE_FIT_SCREEN => _resourcesLoader.GetString(nameof(SURFACE_FIT_SCREEN));
        public static string SURFACE_BEST_FIT => _resourcesLoader.GetString(nameof(SURFACE_BEST_FIT));
        public static string SURFACE_FILL => _resourcesLoader.GetString(nameof(SURFACE_FILL));
        public static string SURFACE_16_9 => _resourcesLoader.GetString(nameof(SURFACE_16_9));
        public static string SURFACE_4_3 => _resourcesLoader.GetString(nameof(SURFACE_4_3));
        public static string SURFACE_ORIGINAL => _resourcesLoader.GetString(nameof(SURFACE_ORIGINAL));
        public static string SURFACE_2_35_1 => _resourcesLoader.GetString(nameof(SURFACE_2_35_1));


        // PLAYBACK
        public static string Playlist => _resourcesLoader.GetString(nameof(Playlist));
        public static string NowPlaying => _resourcesLoader.GetString(nameof(NowPlaying));
        public static string Speed => _resourcesLoader.GetString(nameof(Speed));
        public static string ResetSpeed => _resourcesLoader.GetString(nameof(ResetSpeed));
        public static string IncreaseSpeed => _resourcesLoader.GetString(nameof(IncreaseSpeed));
        public static string DecreaseSpeed => _resourcesLoader.GetString(nameof(DecreaseSpeed));
        public static string AudioTracks => _resourcesLoader.GetString(nameof(AudioTracks));
        public static string Subtitles => _resourcesLoader.GetString(nameof(Subtitles));
        public static string NoSubtitles => _resourcesLoader.GetString(nameof(NoSubtitles));
        public static string Chapters => _resourcesLoader.GetString(nameof(Chapters));
        public static string Volume => _resourcesLoader.GetString(nameof(Volume));
        public static string IncreaseVolume => _resourcesLoader.GetString(nameof(IncreaseVolume));
        public static string DecreaseVolume => _resourcesLoader.GetString(nameof(DecreaseVolume));
        public static string Mute => _resourcesLoader.GetString(nameof(Mute));
        public static string Equalizer => "Equalizer";

        public static string OrderAscending => _resourcesLoader.GetString(nameof(OrderAscending));
        public static string OrderDescending => _resourcesLoader.GetString(nameof(OrderDescending));

        public static string AlbumsFound => _resourcesLoader.GetString(nameof(AlbumsFound));
        public static string NewVideos => _resourcesLoader.GetString(nameof(NewVideos));

        public static string PlaylistAlreadyExists => _resourcesLoader.GetString(nameof(PlaylistAlreadyExists));
        public static string TrackAlreadyExistsInPlaylist => _resourcesLoader.GetString(nameof(TrackAlreadyExistsInPlaylist));
        public static string TrackAddedToYourPlaylist => _resourcesLoader.GetString(nameof(TrackAddedToYourPlaylist));
        public static string HaveToSelectPlaylist => _resourcesLoader.GetString(nameof(HaveToSelectPlaylist));
        public static string YourPlaylistWontBeAccessible => _resourcesLoader.GetString(nameof(YourPlaylistWontBeAccessible));


        // ERRORS
        public static string Sorry => _resourcesLoader.GetString(nameof(Sorry));
        public static string CrashReport => _resourcesLoader.GetString(nameof(CrashReport));
        public static string WeNeedYourHelp => _resourcesLoader.GetString(nameof(WeNeedYourHelp));
        public static string MediaCantBeRead => _resourcesLoader.GetString(nameof(MediaCantBeRead));
        public static string ConnectionLostPleaseCheck => _resourcesLoader.GetString(nameof(ConnectionLostPleaseCheck));

        // MISC
        public static string Yes => _resourcesLoader.GetString(nameof(Yes));
        public static string No => _resourcesLoader.GetString(nameof(No));
        public static string AreYouSure => _resourcesLoader.GetString(nameof(AreYouSure));
        public static string Others => "Others";
        public static string Ok => _resourcesLoader.GetString(nameof(Ok));
        public static string Cancel => _resourcesLoader.GetString(nameof(Cancel));
        public static string Ignore => _resourcesLoader.GetString(nameof(Ignore));
        public static string Remember => _resourcesLoader.GetString(nameof(Remember));

        // ACTIONS
        public static string Back => _resourcesLoader.GetString(nameof(Back));
        public static string Add => _resourcesLoader.GetString(nameof(Add));
        public static string CopyToLocalStorage => _resourcesLoader.GetString(nameof(CopyToLocalStorage));

        // PLAY ..
        public static string Play => _resourcesLoader.GetString(nameof(Play));
        public static string Playback => _resourcesLoader.GetString(nameof(Playback));
        public static string Pause => _resourcesLoader.GetString(nameof(Pause));
        public static string Stop => _resourcesLoader.GetString(nameof(Stop));

        public static string PlayAlbum => _resourcesLoader.GetString(nameof(PlayAlbum));
        public static string PlayTrack => _resourcesLoader.GetString(nameof(PlayTrack));
        public static string PlayVideo => _resourcesLoader.GetString(nameof(PlayVideo));
        public static string PlayAll => _resourcesLoader.GetString(nameof(PlayAll));
        public static string PlayFolder => _resourcesLoader.GetString(nameof(PlayFolder));
        public static string Shuffle => _resourcesLoader.GetString(nameof(Shuffle));


        // OPEN ...
        public static string OpenFile => _resourcesLoader.GetString(nameof(OpenFile));
        public static string OpenStream => _resourcesLoader.GetString(nameof(OpenStream));
        public static string OpenSubtitle => _resourcesLoader.GetString(nameof(OpenSubtitle));

        // DELETE
        public static string DeleteSelected => _resourcesLoader.GetString(nameof(DeleteSelected));
        public static string DeletePlaylist => _resourcesLoader.GetString(nameof(DeletePlaylist));

        // FAVORITE
        public static string Favorite => "Favorite";


        // PIN ...
        public static string PinAlbum => _resourcesLoader.GetString(nameof(PinAlbum));
        public static string PinArtist => _resourcesLoader.GetString(nameof(PinArtist));
        public static string TileRemoved => _resourcesLoader.GetString(nameof(TileRemoved));

        public static string EditMetadata => _resourcesLoader.GetString(nameof(EditMetadata));
        public static string ChangeAlbumCover => _resourcesLoader.GetString(nameof(ChangeAlbumCover));


        // PLAYLISTS
        public static string NewPlaylist => _resourcesLoader.GetString(nameof(NewPlaylist));
        public static string AddToPlaylist => _resourcesLoader.GetString(nameof(AddToPlaylist));
        public static string AddToCurrentPlaylist => _resourcesLoader.GetString(nameof(AddToCurrentPlaylist));
        public static string ShowPlaylist => _resourcesLoader.GetString(nameof(ShowPlaylist));

        // VIEW ...
        public static string ViewArtist => _resourcesLoader.GetString(nameof(ViewArtist));
        public static string ViewAlbum => _resourcesLoader.GetString(nameof(ViewAlbum));

        public static string AddToCollection => _resourcesLoader.GetString(nameof(AddToCollection));
        public static string RemoveFolder => _resourcesLoader.GetString(nameof(RemoveFolder));
        public static string RemoveVideoFolderDescription => _resourcesLoader.GetString(nameof(RemoveVideoFolderDescription));
        public static string RemoveMusicFolderDescription => _resourcesLoader.GetString(nameof(RemoveMusicFolderDescription));
        public static string AddFolder => _resourcesLoader.GetString(nameof(AddFolder));
        public static string Connect => _resourcesLoader.GetString(nameof(Connect));
        public static string Disconnect => _resourcesLoader.GetString(nameof(Disconnect));
        public static string Reset => _resourcesLoader.GetString(nameof(Reset));
        public static string ResetMusicDatabase => _resourcesLoader.GetString(nameof(ResetMusicDatabase));
        public static string RefreshMusicLibrary => _resourcesLoader.GetString(nameof(RefreshMusicLibrary));
        public static string RefreshVideoLibrary => _resourcesLoader.GetString(nameof(RefreshVideoLibrary));
        public static string RefreshLanguage => _resourcesLoader.GetString(nameof(RefreshLanguage));

        // PLACEHOLDERS
        public static string PlaylistNamePlaceholder => _resourcesLoader.GetString(nameof(PlaylistNamePlaceholder));
        public static string NoBiographyFound => _resourcesLoader.GetString(nameof(NoBiographyFound));
        public static string Username => _resourcesLoader.GetString(nameof(Username));
        public static string Password => _resourcesLoader.GetString(nameof(Password));
        public static string EnterURL => _resourcesLoader.GetString(nameof(EnterURL));

        // SEVERAL WAYS TO SAY WE FOUND NOTHING
        public static string ItsEmpty => _resourcesLoader.GetString(nameof(ItsEmpty));
        public static string NothingToSeeHere => _resourcesLoader.GetString(nameof(NothingToSeeHere));
        public static string ElementsNotFound => _resourcesLoader.GetString(nameof(ElementsNotFound));

        public static string NoVideosFound => _resourcesLoader.GetString(nameof(NoVideosFound));
        public static string AddMediaHelp => _resourcesLoader.GetString(nameof(AddMediaHelp));
        public static string AddMediaHelpWithIP => _resourcesLoader.GetString(nameof(AddMediaHelpWithIP));
        public static string NoResults => _resourcesLoader.GetString(nameof(NoResults));
        public static string HoweverYouMayFindWhatYouWantHere => _resourcesLoader.GetString(nameof(HoweverYouMayFindWhatYouWantHere));
        public static string NoCameraVideosFound => _resourcesLoader.GetString(nameof(NoCameraVideosFound));
        public static string NoFavoriteAlbums => _resourcesLoader.GetString(nameof(NoFavoriteAlbums));
        public static string NoPlaylists => _resourcesLoader.GetString(nameof(NoPlaylists));
        public static string NoShowsFound => _resourcesLoader.GetString(nameof(NoShowsFound));
        public static string HowToFavoriteAlbum1 => _resourcesLoader.GetString(nameof(HowToFavoriteAlbum1));
        public static string HowToFavoriteAlbum2 => _resourcesLoader.GetString(nameof(HowToFavoriteAlbum2));
        public static string NoArtistShowsFound => _resourcesLoader.GetString(nameof(NoArtistShowsFound));


        public static string PrivacyStatement => _resourcesLoader.GetString(nameof(PrivacyStatement));

        // SETTINGS
        public static string Settings => _resourcesLoader.GetString(nameof(Settings));
        public static string License => _resourcesLoader.GetString(nameof(License));
        public static string UserInterface => _resourcesLoader.GetString(nameof(UserInterface));
        public static string VideoSettings => _resourcesLoader.GetString(nameof(VideoSettings));
        public static string MusicSettings => _resourcesLoader.GetString(nameof(MusicSettings));
        public static string AboutTheApp => _resourcesLoader.GetString(nameof(AboutTheApp));
        public static string Notifications => _resourcesLoader.GetString(nameof(Notifications));
        public static string NotificationWhenSongStarts => _resourcesLoader.GetString(nameof(NotificationWhenSongStarts));
        public static string VideoPlaybackInBackground => _resourcesLoader.GetString(nameof(VideoPlaybackInBackground));
        public static string CompactOverlayPiP => _resourcesLoader.GetString(nameof(CompactOverlayPiP));
        public static string Restart => _resourcesLoader.GetString(nameof(Restart));
        public static string EvenIfBackground => _resourcesLoader.GetString(nameof(EvenIfBackground));
        public static string EvenIfNotBackground => _resourcesLoader.GetString(nameof(EvenIfNotBackground));
        public static string NeedRestart => _resourcesLoader.GetString(nameof(NeedRestart));
        public static string ClearKeystore => _resourcesLoader.GetString(nameof(ClearKeystore));
        public static string Credentials => _resourcesLoader.GetString(nameof(Credentials));
        public static string TvUnsafeArea => _resourcesLoader.GetString(nameof(TvUnsafeArea));
        public static string AddMarginExplanation => _resourcesLoader.GetString(nameof(AddMarginExplanation));
        public static string ExtraMargin => _resourcesLoader.GetString(nameof(ExtraMargin));
        public static string NoExtraMargin => _resourcesLoader.GetString(nameof(NoExtraMargin));

        // LastFM
        public static string ConnectToLastFM => _resourcesLoader.GetString(nameof(ConnectToLastFM));
        public static string ConnectedToLastFM => _resourcesLoader.GetString(nameof(ConnectedToLastFM));
        public static string CheckCredentials => _resourcesLoader.GetString(nameof(CheckCredentials));
        public static string Connecting => _resourcesLoader.GetString(nameof(Connecting));


        public static string Theme => _resourcesLoader.GetString(nameof(Theme));
        public static string AppTheme => _resourcesLoader.GetString(nameof(AppTheme));
        public static string Languages => _resourcesLoader.GetString(nameof(Languages));
        public static string SelectLanguageDescription => _resourcesLoader.GetString(nameof(SelectLanguageDescription));

        public static string EnglishLanguage => _resourcesLoader.GetString(nameof(EnglishLanguage));
        public static string FrenchLanguage => _resourcesLoader.GetString(nameof(FrenchLanguage));
        public static string JapaneseLanguage => _resourcesLoader.GetString(nameof(JapaneseLanguage));
        public static string GermanLanguage => _resourcesLoader.GetString(nameof(GermanLanguage));
        public static string PolishLanguage => _resourcesLoader.GetString(nameof(PolishLanguage));
        public static string SlovakLanguage => _resourcesLoader.GetString(nameof(SlovakLanguage));
        public static string DanishLanguage => _resourcesLoader.GetString(nameof(DanishLanguage));
        public static string SpanishLanguage => _resourcesLoader.GetString(nameof(SpanishLanguage));
        public static string HungarianLanguage => _resourcesLoader.GetString(nameof(HungarianLanguage));
        public static string ItalianLanguage => _resourcesLoader.GetString(nameof(ItalianLanguage));
        public static string KoreanLanguage => _resourcesLoader.GetString(nameof(KoreanLanguage));
        public static string MalayLanguage => _resourcesLoader.GetString(nameof(MalayLanguage));
        public static string NorwegianLanguage => _resourcesLoader.GetString(nameof(NorwegianLanguage));
        public static string DutchLanguage => _resourcesLoader.GetString(nameof(DutchLanguage));
        public static string RussianLanguage => _resourcesLoader.GetString(nameof(RussianLanguage));
        public static string SwedishLanguage => _resourcesLoader.GetString(nameof(SwedishLanguage));
        public static string TurkishLanguage => _resourcesLoader.GetString(nameof(TurkishLanguage));
        public static string UkrainianLanguage => _resourcesLoader.GetString(nameof(UkrainianLanguage));
        public static string ChineseLanguage => _resourcesLoader.GetString(nameof(ChineseLanguage));
        public static string PortugueseLanguage => _resourcesLoader.GetString(nameof(PortugueseLanguage));
        public static string CzechLanguage => _resourcesLoader.GetString(nameof(CzechLanguage));
        public static string IcelandicLanguage => _resourcesLoader.GetString(nameof(IcelandicLanguage));
        
        public static string HomePage => _resourcesLoader.GetString(nameof(HomePage));
        public static string HomePageDescription => _resourcesLoader.GetString(nameof(HomePageDescription));
        public static string Animations => _resourcesLoader.GetString(nameof(Animations));
        public static string RichAnimationsDescription => _resourcesLoader.GetString(nameof(RichAnimationsDescription));
        public static string KeyboardShortcuts => _resourcesLoader.GetString(nameof(KeyboardShortcuts));
        public static string VideoPlayback => _resourcesLoader.GetString(nameof(VideoPlayback));
        public static string HardwareDecoding => _resourcesLoader.GetString(nameof(HardwareDecoding));
        public static string HardwareDecodingDescription => _resourcesLoader.GetString(nameof(HardwareDecodingDescription));
        public static string ForceLandscape => _resourcesLoader.GetString(nameof(ForceLandscape));
        public static string SubtitlesEncoding => _resourcesLoader.GetString(nameof(SubtitlesEncoding));
        
        // ERRORS
        public static string FailOpenVideo => _resourcesLoader.GetString(nameof(FailOpenVideo));
        public static string FailNavigateVideoPlayerPage => _resourcesLoader.GetString(nameof(FailNavigateVideoPlayerPage));
        public static string FailStartVLCEngine => _resourcesLoader.GetString(nameof(FailStartVLCEngine));
        public static string FailFilePlayBackground => _resourcesLoader.GetString(nameof(FailFilePlayBackground));

        // COLORS AND THEMES
        public static string Light => _resourcesLoader.GetString(nameof(Light));
        public static string Dark => _resourcesLoader.GetString(nameof(Dark));
        public static string AccentColor => _resourcesLoader.GetString(nameof(AccentColor));
        public static string BackgroundColor => _resourcesLoader.GetString(nameof(BackgroundColor));
        public static string TitleBar => _resourcesLoader.GetString(nameof(TitleBar));

        // EXTERNAL STORAGE DEVICES
        public static string ExternalStorage => _resourcesLoader.GetString(nameof(ExternalStorage));
        public static string ExternalStorageDeviceDetected => _resourcesLoader.GetString(nameof(ExternalStorageDeviceDetected));
        public static string WhatToDo => _resourcesLoader.GetString(nameof(WhatToDo));
        public static string WhatToDoWhenExternalStorage => _resourcesLoader.GetString(nameof(WhatToDoWhenExternalStorage));
        public static string ReadFromExternalStorage => _resourcesLoader.GetString(nameof(ReadFromExternalStorage));
        public static string SelectContentToCopy => _resourcesLoader.GetString(nameof(SelectContentToCopy));
        public static string DoNothing => _resourcesLoader.GetString(nameof(DoNothing));
        public static string AskMe => _resourcesLoader.GetString(nameof(AskMe));

        public Strings()
        {
        }

        public static string HumanizedArtistName(string artistName) { return string.IsNullOrEmpty(artistName) ? Strings.UnknownArtist : artistName; }
        public static string HumanizedAlbumName(string albumName) { return string.IsNullOrEmpty(albumName) ? Strings.UnknownAlbum : albumName; }

        public static string HumanizedArtistFirstLetter(string artist)
        {
            return (string.IsNullOrEmpty(artist) ? Strings.UnknownArtist[0].ToString() : (char.IsLetter(artist.ElementAt(0)) ? artist.ToUpper().ElementAt(0).ToString() : Strings.UnknownString));
        }

        public static string HumanizeSeconds(double value)
        {
            if (double.IsNaN(value))
            {
                return "";
            }
            var time = TimeSpan.FromSeconds(value);
            if (time.Hours > 0)
            {
                return String.Format("{0:hh\\:mm\\:ss}", time);
            }
            else
            {
                return String.Format("{0:mm\\:ss}", time);
            }
        }

        public static string HumanizedTimeSpan(TimeSpan ts)
        {
            if (ts.Hours > 0)
            {

                return String.Format("{0:hh\\:mm\\:ss}", ts);
            }
            else
            {
                return String.Format("{0:mm\\:ss}", ts);
            }
        }
    }
}
