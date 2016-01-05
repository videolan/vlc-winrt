/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Xaml;
using VLC_WinRT.Database;
using VLC_WinRT.Helpers;
using VLC_WinRT.Helpers.MusicLibrary;
using VLC_WinRT.Model.Music;
using VLC_WinRT.Model.Video;
using VLC_WinRT.Model;
using VLC_WinRT.Views.MusicPages;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Core;
using VLC_WinRT.Commands.Navigation;
using VLC_WinRT.Commands.Settings;
using VLC_WinRT.Utils;

namespace VLC_WinRT.ViewModels.Settings
{
    public class SettingsViewModel : BindableBase
    {
#if WINDOWS_APP
        private List<StorageFolder> _musicFolders;
        private List<StorageFolder> _videoFolders;
        private bool musicFoldersLoaded;
        private bool videoFoldersLoaded;
        private bool _notificationOnNewSong;
        private bool _notificationOnNewSongForeground;
#endif
        private ApplicationTheme applicationTheme;
        private List<Color> _accentColors;
        private Color _accentColor;
        private bool _accentColorTitleBar;
        private bool _continueVideoPlaybackInBackground;
        private OrderType _albumsOrderType;
        private OrderListing _albumsOrderListing;
        private MusicView _musicView;
        private VideoView _videoView;
        private VLCPage _homePage;
        private string _lastFmUserName;
        private string _lastFmPassword;
        private string _subtitlesEncodingValue;
        private bool _lastFmIsConnected = false;
        private bool _hardwareAcceleration;
        private bool _forceLandscape;
        private bool _richAnimations;
        private List<KeyboardAction> _keyboardActions;
        private List<string> _subtitlesEncodingValues;

        public ApplicationTheme ApplicationTheme
        {
            get
            {
                return GetApplicationTheme();
            }
            set
            {
                ApplicationSettingsHelper.SaveSettingsValue(nameof(ApplicationTheme), (int)value, false);
                SetProperty(ref applicationTheme, value);
                App.SetShellDecoration();
            }
        }

        public List<Color> AccentColors
        {
            get
            {
                if (_accentColors != null && _accentColors.Any())
                    return _accentColors;
                _accentColors = new List<Color>()
                {
                    Color.FromArgb(255, 0xff, 0x88, 0x00),
                    Color.FromArgb(255, 241, 13, 162),
                    Color.FromArgb(255, 240, 67, 98),
                    Color.FromArgb(255, 239, 95, 65),
                    Color.FromArgb(255, 46, 204, 113),
                    Color.FromArgb(255, 52, 152, 219),
                    Color.FromArgb(255, 155, 89, 182),
                    Color.FromArgb(255, 52, 73, 94),
                    Color.FromArgb(255, 22, 160, 133),
                    Color.FromArgb(255, 39, 174, 96),
                    Color.FromArgb(255, 41, 128, 185),
                    Color.FromArgb(255, 142, 68, 173),
                    Color.FromArgb(255, 44, 62, 80),
                    Color.FromArgb(255, 241, 196, 15),
                    Color.FromArgb(255, 230, 126, 34),
                    Color.FromArgb(255, 231, 76, 60),
                    Color.FromArgb(255, 243, 156, 18),
                    Color.FromArgb(255, 211, 84, 0),
                    Color.FromArgb(255, 192, 57, 43)
                };
                return _accentColors;
            }
        }

        public Color AccentColor
        {
            get
            {
                var color = ApplicationSettingsHelper.ReadSettingsValue(nameof(AccentColor), false);
                if (color == null || string.IsNullOrEmpty(color.ToString()))
                {
                    _accentColor = AccentColors[0];
                }
                else
                {
                    var str = color.ToString();
                    _accentColor = new Color();
                    _accentColor.A = byte.Parse(str.Substring(1, 2), NumberStyles.AllowHexSpecifier);
                    _accentColor.R = byte.Parse(str.Substring(3, 2), NumberStyles.AllowHexSpecifier);
                    _accentColor.G = byte.Parse(str.Substring(5, 2), NumberStyles.AllowHexSpecifier);
                    _accentColor.B = byte.Parse(str.Substring(7, 2), NumberStyles.AllowHexSpecifier);
                }
                return _accentColor;
            }
            set
            {
                if (_accentColor == value) return;
                ApplicationSettingsHelper.SaveSettingsValue(nameof(AccentColor), value.ToString(), false);
                SetProperty(ref _accentColor, value);
                App.SetShellDecoration();
            }
        }

        public bool AccentColorTitleBar
        {
            get
            {
                var accentColorTitleBar = ApplicationSettingsHelper.ReadSettingsValue(nameof(AccentColorTitleBar), false);
                if (accentColorTitleBar == null)
                {
                    _accentColorTitleBar = false;
                }
                else
                {
                    _accentColorTitleBar = (bool)accentColorTitleBar;
                }
                return _accentColorTitleBar;
            }
            set
            {
                if (_accentColorTitleBar == value) return;
                ApplicationSettingsHelper.SaveSettingsValue(nameof(AccentColorTitleBar), value, false);
                SetProperty(ref _accentColorTitleBar, value);
                App.SetShellDecoration();
            }
        }

        public static ApplicationTheme GetApplicationTheme()
        {
            var appTheme = ApplicationSettingsHelper.ReadSettingsValue(nameof(ApplicationTheme), false);
            ApplicationTheme applicationTheme;
            if (appTheme == null)
            {
#if WINDOWS_APP
                applicationTheme = ApplicationTheme.Light;
#else
                    applicationTheme = App.Current.RequestedTheme;
#endif
            }
            else
            {
                applicationTheme = (ApplicationTheme)appTheme;
            }
            return applicationTheme;
        }

#if WINDOWS_APP
        public bool ContinueVideoPlaybackInBackground
        {
            get
            {
                var continuePlaybackInBackground = ApplicationSettingsHelper.ReadSettingsValue("ContinueVideoPlaybackInBackground");
                if (continuePlaybackInBackground == null)
                {
                    _continueVideoPlaybackInBackground = true;
                }
                else
                {
                    _continueVideoPlaybackInBackground = (bool)continuePlaybackInBackground;
                }
                return _continueVideoPlaybackInBackground;
            }
            set
            {
                SetProperty(ref _continueVideoPlaybackInBackground, value);
                ApplicationSettingsHelper.SaveSettingsValue("ContinueVideoPlaybackInBackground", value);
            }
        }
#else
        public bool ContinueVideoPlaybackInBackground { get; } = false;
#endif
        public List<KeyboardAction> KeyboardActions
        {
            get
            {
                return _keyboardActions ?? (_keyboardActions = Locator.MainVM.KeyboardListenerService._keyboardActionDatabase.GetAllKeyboardActions());
            }
        }

        public List<string> SubtitlesEncodingValues
        {
            get
            {
                if (_subtitlesEncodingValues != null && _subtitlesEncodingValues.Any())
                {
                    return _subtitlesEncodingValues;
                }
                _subtitlesEncodingValues = new List<string>
                {
                    "System",
                    "UTF-8",
                    "UTF-16",
                    "UTF-16BE",
                    "UTF-16LE",
                    "GB18030",
                    "ISO-8859-15",
                    "Windows-1252",
                    "IBM850",
                    "ISO-8859-2",
                    "Windows-1250",
                    "ISO-8859-3",
                    "ISO-8859-10",
                    "Windows-1251",
                    "KOI8-R",
                    "KOI8-U",
                    "ISO-8859-6",
                    "Windows-1256",
                    "ISO-8859-7",
                    "Windows-1253",
                    "ISO-8859-8",
                    "Windows-1255",
                    "ISO-8859-9",
                    "Windows-1254",
                    "ISO-8859-11",
                    "Windows-874",
                    "ISO-8859-13",
                    "Windows-1257",
                    "ISO-8859-14",
                    "ISO-8859-16",
                    "ISO-2022-CN-EXT",
                    "EUC-CN",
                    "ISO-2022-JP-2",
                    "EUC-JP",
                    "Shift_JIS",
                    "CP949",
                    "ISO-2022-KR",
                    "Big5",
                    "ISO-2022-TW",
                    "Big5-HKSCS",
                    "VISCII",
                    "Windows-1258"
                };
                return _subtitlesEncodingValues;
            }
        }

        public List<VLCPage> HomePageCollection { get; set; } = new List<VLCPage>()
        {
            VLCPage.MainPageVideo,
            VLCPage.MainPageMusic,
            VLCPage.MainPageFileExplorer
        };

        public List<OrderType> AlbumsOrderTypeCollection
        { get; set; }
        = new List<OrderType>()
        {
            OrderType.ByArtist,
            OrderType.ByDate,
            OrderType.ByAlbum,
        };

        public List<OrderListing> AlbumsListingTypeCollection
        { get; set; }
        = new List<OrderListing>()
        {
            OrderListing.Ascending,
            OrderListing.Descending
        };

        public List<VideoView> VideoViewCollection
        { get; set; }
        = new List<VideoView>()
        {
            VideoView.Videos,
            VideoView.Shows,
            VideoView.CameraRoll
        };
#if WINDOWS_APP
        public List<StorageFolder> MusicFolders
        {
            get
            {
                if (!musicFoldersLoaded)
                {
                    musicFoldersLoaded = true;
                    Task.Run(() => GetMusicLibraryFolders());
                }
                return _musicFolders;
            }
            set { SetProperty(ref _musicFolders, value); }
        }

        public List<StorageFolder> VideoFolders
        {
            get
            {
                if (!videoFoldersLoaded)
                {
                    videoFoldersLoaded = true;
                    Task.Run(() => GetVideoLibraryFolders());
                }
                return _videoFolders;
            }
            set { SetProperty(ref _videoFolders, value); }
        }

        public AddFolderToLibrary AddFolderToLibrary { get; set; } = new AddFolderToLibrary();
        public RemoveFolderFromVideoLibrary RemoveFolderFromVideoLibrary { get; set; } = new RemoveFolderFromVideoLibrary();
        public RemoveFolderFromMusicLibrary RemoveFolderFromMusicLibrary { get; set; } = new RemoveFolderFromMusicLibrary();
        public KnownLibraryId MusicLibraryId { get; set; } = KnownLibraryId.Music;
        public KnownLibraryId VideoLibraryId { get; set; } = KnownLibraryId.Videos;

        public bool NotificationOnNewSong
        {
            get
            {
                var notificationOnNewSong = ApplicationSettingsHelper.ReadSettingsValue("NotificationOnNewSong");
                _notificationOnNewSong = notificationOnNewSong as bool? ?? false;
                return _notificationOnNewSong;
            }
            set
            {
                ApplicationSettingsHelper.SaveSettingsValue("NotificationOnNewSong", value);
                SetProperty(ref _notificationOnNewSong, value);
            }
        }

        public bool NotificationOnNewSongForeground
        {
            get
            {
                var notificationOnNewSongForeground = ApplicationSettingsHelper.ReadSettingsValue("NotificationOnNewSongForeground");
                _notificationOnNewSongForeground = notificationOnNewSongForeground as bool? ?? false;
                return _notificationOnNewSongForeground;
            }
            set
            {
                ApplicationSettingsHelper.SaveSettingsValue("NotificationOnNewSongForeground", value);
                SetProperty(ref _notificationOnNewSongForeground, value);
            }
        }
#endif

        public OrderType AlbumsOrderType
        {
            get
            {
                var albumsOrderType = ApplicationSettingsHelper.ReadSettingsValue("AlbumsOrderType", false);
                if (albumsOrderType == null)
                {
                    _albumsOrderType = OrderType.ByAlbum;
                }
                else
                {
                    _albumsOrderType = (OrderType)albumsOrderType;
                }
                return _albumsOrderType;
            }
            set
            {
                ApplicationSettingsHelper.SaveSettingsValue("AlbumsOrderType", (int)value, false);
                if ((int)value == 0 || value != _albumsOrderType)
                    Locator.MusicLibraryVM.OrderAlbums();
                SetProperty(ref _albumsOrderType, value);
            }
        }

        public OrderListing AlbumsOrderListing
        {
            get
            {
                var albumsOrderListing = ApplicationSettingsHelper.ReadSettingsValue("AlbumsOrderListing", false);
                if (albumsOrderListing == null)
                {
                    _albumsOrderListing = OrderListing.Ascending;
                }
                else
                {
                    _albumsOrderListing = (OrderListing)albumsOrderListing;
                }
                return _albumsOrderListing;
            }
            set
            {
                ApplicationSettingsHelper.SaveSettingsValue("AlbumsOrderListing", (int)value, false);
                if (value != _albumsOrderListing)
                    Locator.MusicLibraryVM.OrderAlbums();
                SetProperty(ref _albumsOrderListing, value);
            }
        }

        public VideoView VideoView
        {
            get
            {
                var videoView = ApplicationSettingsHelper.ReadSettingsValue("VideoView", false);
                if (videoView == null)
                {
                    _videoView = VideoView.Videos;
                }
                else
                {
                    _videoView = (VideoView)videoView;
                }
                return _videoView;
            }
            set
            {
                ApplicationSettingsHelper.SaveSettingsValue("VideoView", (int)value, false);
                if (value != _videoView)
                {
                    Locator.MainVM.ChangeMainPageVideoViewCommand.Execute((int)value);
                }
                SetProperty(ref _videoView, value);
            }
        }

        public string LastFmUserName
        {
            get
            {
                var username = ApplicationSettingsHelper.ReadSettingsValue("LastFmUserName");
                _lastFmUserName = username == null ? "" : username.ToString();
                return _lastFmUserName;
            }
            set
            {
                ApplicationSettingsHelper.SaveSettingsValue("LastFmUserName", value);
                SetProperty(ref _lastFmUserName, value);
            }
        }

        public string LastFmPassword
        {
            get
            {
                var password = ApplicationSettingsHelper.ReadSettingsValue("LastFmPassword");
                _lastFmPassword = password == null ? "" : password.ToString();
                return _lastFmPassword;
            }
            set
            {
                ApplicationSettingsHelper.SaveSettingsValue("LastFmPassword", value);
                SetProperty(ref _lastFmPassword, value);
            }
        }

        public bool LastFmIsConnected
        {
            get
            {
                var lastFmIsConnected = ApplicationSettingsHelper.ReadSettingsValue("LastFmIsConnected");
                if (lastFmIsConnected == null)
                {
                    _lastFmIsConnected = false;
                }
                else
                {
                    _lastFmIsConnected = (bool)lastFmIsConnected;
                }
                return _lastFmIsConnected;
            }
            set
            {
                ApplicationSettingsHelper.SaveSettingsValue("LastFmIsConnected", value);
                SetProperty(ref _lastFmIsConnected, value);
            }
        }

        public bool HardwareAccelerationEnabled
        {
            get
            {
                var hardwareAccelerationEnabled = ApplicationSettingsHelper.ReadSettingsValue("HardwareAccelerationEnabled");
                if (hardwareAccelerationEnabled == null)
                {
                    _hardwareAcceleration = true;
                }
                else
                {
                    _hardwareAcceleration = (bool)hardwareAccelerationEnabled;
                }
                return _hardwareAcceleration;
            }
            set
            {
                ApplicationSettingsHelper.SaveSettingsValue("HardwareAccelerationEnabled", value);
                SetProperty(ref _hardwareAcceleration, value);
            }
        }

        public bool ForceLandscape
        {
            get
            {
                var force = ApplicationSettingsHelper.ReadSettingsValue(nameof(ForceLandscape));
                if (force == null)
                {
                    _forceLandscape = true;
                }
                else
                {
                    _forceLandscape = (bool) force;
                }
                return _forceLandscape;
            }
            set
            {
                ApplicationSettingsHelper.SaveSettingsValue(nameof(ForceLandscape), value);
                SetProperty(ref _forceLandscape, value);
            }
        }

        public bool RichAnimations
        {
            get
            {
                var richAnims = ApplicationSettingsHelper.ReadSettingsValue("RichAnimationsEnabled");
                _richAnimations = (richAnims is bool) ? (bool)richAnims : true;
                return _richAnimations;
            }
            set
            {
                ApplicationSettingsHelper.SaveSettingsValue("RichAnimationsEnabled", value);
                SetProperty(ref _richAnimations, value);
                Locator.Slideshow.RichAnimations = value;
            }
        }

        public VLCPage HomePage
        {
            get
            {
                var homePage = ApplicationSettingsHelper.ReadSettingsValue("Homepage", false);
                if (homePage == null)
                {
                    _homePage = VLCPage.MainPageVideo;
                }
                else
                {
                    _homePage = (VLCPage)homePage;
                }
                return _homePage;
            }
            set
            {
                ApplicationSettingsHelper.SaveSettingsValue("Homepage", (int)value, false);
                SetProperty(ref _homePage, value);
            }
        }

        public string SubtitleEncodingValue
        {
            get
            {
                var subtitleEncodingValue = ApplicationSettingsHelper.ReadSettingsValue(nameof(SubtitleEncodingValue), false);
                if (string.IsNullOrEmpty((string) subtitleEncodingValue))
                {
                    _subtitlesEncodingValue = "System";
                }
                else
                {
                    _subtitlesEncodingValue = subtitleEncodingValue.ToString();
                    if (_subtitlesEncodingValue == "")
                        _subtitlesEncodingValue = "System";
                }
                return _subtitlesEncodingValue;
            }
            set
            {
                if (value == "System")
                    value = "";
                ApplicationSettingsHelper.SaveSettingsValue(nameof(SubtitleEncodingValue), value, false);
                SetProperty(ref _subtitlesEncodingValue, value);
            }
        }

        public ChangeSettingsViewCommand ChangeSettingsViewCommand { get; } = new ChangeSettingsViewCommand();

#if WINDOWS_APP
        public async Task GetMusicLibraryFolders()
        {
            var musicLib = await StorageLibrary.GetLibraryAsync(KnownLibraryId.Music);
            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () => MusicFolders = musicLib.Folders.ToList());
        }

        public async Task GetVideoLibraryFolders()
        {
            var videosLib = await StorageLibrary.GetLibraryAsync(KnownLibraryId.Videos);
            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () => VideoFolders = videosLib.Folders.ToList());
        }
#endif
    }
}
