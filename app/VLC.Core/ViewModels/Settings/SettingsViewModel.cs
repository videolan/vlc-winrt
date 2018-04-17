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
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources.Core;
using Windows.UI.Xaml;
using VLC.Helpers;
using VLC.Model;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Core;
using VLC.Commands.Navigation;
using VLC.Commands.Settings;
using VLC.Utils;
using Windows.UI.Xaml.Navigation;

namespace VLC.ViewModels.Settings
{
    public class SettingsViewModel : BindableBase
    {
        private List<StorageFolder> _musicFolders;
        private List<StorageFolder> _videoFolders;
        private bool musicFoldersLoaded;
        private bool videoFoldersLoaded;
        private bool _notificationOnNewSong;
        private ApplicationTheme applicationTheme;
        private List<VLCAccentColor> _accentColors = new List<VLCAccentColor>();
        private VLCAccentColor _accentColor;
        private bool _continueVideoPlaybackInBackground;
        private OrderType _albumsOrderType;
        private OrderListing _albumsOrderListing;
        private VLCPage _homePage;
        private ExternalDeviceMode _externalDeviceMode;
        private Languages _selectedLanguage;
        private string _lastFmUserName;
        private string _lastFmPassword;
        private string _subtitlesEncodingValue;
        private bool _lastFmIsConnected = false;
        private bool _hardwareAcceleration;
        private bool _forceLandscape;
        private List<KeyboardAction> _keyboardActions;
        private List<string> _subtitlesEncodingValues;
        private VLCEqualizer _vlcEqualizer;
        private IList<VLCEqualizer> _equalizerPresets;
        private bool _useCompactOverlayPiP;

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

        public double ExtraMarginValue { get; } = 20;
        /// <summary>
        /// https://code.videolan.org/videolan/vlc-winrt/issues/166
        /// </summary>
        public bool ExtraMargin
        {
            get
            {
                var r = ApplicationSettingsHelper.ReadSettingsValue(nameof(ExtraMargin), false);
                return r as bool? ?? false;
            } 
            set
            {
                ApplicationSettingsHelper.SaveSettingsValue(nameof(ExtraMargin), value, false);
                App.RootPage.AdjustMargin(value);
            }
        }

        public List<VLCAccentColor> AccentColors
        {
            get
            {
                if (!_accentColors.Any())
                {
                    _accentColors.Add(new VLCAccentColor(Color.FromArgb(255, 0xff, 0x88, 0x00)));
                    _accentColors.Add(new VLCAccentColor(Color.FromArgb(255, 241, 13, 162)));
                    _accentColors.Add(new VLCAccentColor(Color.FromArgb(255, 240, 67, 98)));
                    _accentColors.Add(new VLCAccentColor(Color.FromArgb(255, 239, 95, 65)));
                    _accentColors.Add(new VLCAccentColor(Color.FromArgb(255, 46, 204, 113)));
                    _accentColors.Add(new VLCAccentColor(Color.FromArgb(255, 52, 152, 219)));
                    _accentColors.Add(new VLCAccentColor(Color.FromArgb(255, 155, 89, 182)));
                    _accentColors.Add(new VLCAccentColor(Color.FromArgb(255, 52, 73, 94)));
                    _accentColors.Add(new VLCAccentColor(Color.FromArgb(255, 22, 160, 133)));
                    _accentColors.Add(new VLCAccentColor(Color.FromArgb(255, 39, 174, 96)));
                    _accentColors.Add(new VLCAccentColor(Color.FromArgb(255, 41, 128, 185)));
                    _accentColors.Add(new VLCAccentColor(Color.FromArgb(255, 142, 68, 173)));
                    _accentColors.Add(new VLCAccentColor(Color.FromArgb(255, 44, 62, 80)));
                    _accentColors.Add(new VLCAccentColor(Color.FromArgb(255, 241, 196, 15)));
                    _accentColors.Add(new VLCAccentColor(Color.FromArgb(255, 230, 126, 34)));
                    _accentColors.Add(new VLCAccentColor(Color.FromArgb(255, 231, 76, 60)));
                    _accentColors.Add(new VLCAccentColor(Color.FromArgb(255, 243, 156, 18)));
                    _accentColors.Add(new VLCAccentColor(Color.FromArgb(255, 211, 84, 0)));
                    _accentColors.Add(new VLCAccentColor(Color.FromArgb(255, 192, 57, 43)));
                }
                return _accentColors;
            }
        }

        public VLCAccentColor AccentColor
        {
            get
            {
                var color = ApplicationSettingsHelper.ReadSettingsValue(nameof(AccentColor), false);
                if (color == null || string.IsNullOrEmpty(color.ToString()) || color.ToString() == "#00000000")
                {
                    _accentColor = AccentColors[0];
                }
                else
                {
                    var str = color.ToString();
                    var c = new Color();
                    c.A = byte.Parse(str.Substring(1, 2), NumberStyles.AllowHexSpecifier);
                    c.R = byte.Parse(str.Substring(3, 2), NumberStyles.AllowHexSpecifier);
                    c.G = byte.Parse(str.Substring(5, 2), NumberStyles.AllowHexSpecifier);
                    c.B = byte.Parse(str.Substring(7, 2), NumberStyles.AllowHexSpecifier);
                    _accentColor = AccentColors.FirstOrDefault(x => x.Color == c);
                }
                return _accentColor;
            }
            set
            {
                if (_accentColor == value || value == null) return;
                ApplicationSettingsHelper.SaveSettingsValue(nameof(AccentColor), value.Color.ToString(), false);
                SetProperty(ref _accentColor, value);
            }
        }

        public bool MediaCenterMode => DeviceHelper.IsMediaCenterModeCompliant;

        public bool DesktopMode => !MediaCenterMode;

        public static ApplicationTheme GetApplicationTheme()
        {
            var appTheme = ApplicationSettingsHelper.ReadSettingsValue(nameof(ApplicationTheme), false);
            ApplicationTheme applicationTheme;
            if (appTheme == null)
            {
                applicationTheme = App.Current.RequestedTheme;
            }
            else
            {
                applicationTheme = (ApplicationTheme)appTheme;
            }
            return applicationTheme;
        }

        public static Languages GetSelectedLanguage()
        {
            var language = ApplicationSettingsHelper.ReadSettingsValue("Languages", false);
            if (language == null)
            {
                return Languages.English;
            }
            return (Languages)language;
        }

        public bool ContinueVideoPlaybackInBackground
        {
            get
            {
                var continuePlaybackInBackground = ApplicationSettingsHelper.ReadSettingsValue(nameof(ContinueVideoPlaybackInBackground));
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
                ApplicationSettingsHelper.SaveSettingsValue(nameof(ContinueVideoPlaybackInBackground), value);
            }
        }

        public bool CompactOverlayPiP
        {
            get
            {
                var useCompactOverlayPiP = ApplicationSettingsHelper.ReadSettingsValue(nameof(CompactOverlayPiP));
                if (useCompactOverlayPiP == null)
                {
                    if (Locator.VideoPlayerVm.IsCompactOverlaySupported)
                    {
                        _useCompactOverlayPiP = true;
                    }
                    else
                    {
                        _useCompactOverlayPiP = false;
                    }
                }
                else
                {
                    _useCompactOverlayPiP = (bool)useCompactOverlayPiP;
                }

                return _useCompactOverlayPiP;
            }
            set
            {
                SetProperty(ref _useCompactOverlayPiP, value);
                ApplicationSettingsHelper.SaveSettingsValue(nameof(CompactOverlayPiP), value);
            }
        }

        public List<KeyboardAction> KeyboardActions => Locator.MainVM.KeyboardListenerService.Shortcuts;
    
        public IList<VLCEqualizer> Presets => _equalizerPresets ?? (_equalizerPresets = Locator.PlaybackService.GetEqualizerPresets());

        public VLCEqualizer Equalizer
        {
            get
            {
                var eq = ApplicationSettingsHelper.ReadSettingsValue(nameof(Equalizer));
                if (eq == null)
                {
                    _vlcEqualizer = Presets[0];
                }
                else
                {
                    _vlcEqualizer = Presets[Convert.ToInt32((uint)eq)];
                }
                return _vlcEqualizer;
            }
            set
            {
                if (value == null)
                    return;
                SetProperty(ref _vlcEqualizer, value);
                ApplicationSettingsHelper.SaveSettingsValue(nameof(Equalizer), value.Index);
                Locator.PlaybackService.SetEqualizer(value);
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

        public List<ExternalDeviceMode> ExternalDeviceModeCollection { get; set; }
            = Enum.GetValues(typeof(ExternalDeviceMode)).Cast<ExternalDeviceMode>().ToList();

        public List<Languages> LanguageCollection { get; } = Enum.GetValues(typeof(Languages)).Cast<Languages>().ToList();

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
                var notificationOnNewSong = ApplicationSettingsHelper.ReadSettingsValue(nameof(NotificationOnNewSong));
                _notificationOnNewSong = notificationOnNewSong as bool? ?? true;
                return _notificationOnNewSong;
            }
            set
            {
                ApplicationSettingsHelper.SaveSettingsValue(nameof(NotificationOnNewSong), value);
                SetProperty(ref _notificationOnNewSong, value);
            }
        }

        public OrderType AlbumsOrderType
        {
            get
            {
                var albumsOrderType = ApplicationSettingsHelper.ReadSettingsValue(nameof(AlbumsOrderType), false);
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
                ApplicationSettingsHelper.SaveSettingsValue(nameof(AlbumsOrderType), (int)value, false);
                if ((int)value == 0 || value != _albumsOrderType)
                    Task.Run(() => Locator.MusicLibraryVM.OrderAlbums()).ConfigureAwait(false);
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
                    Task.Run(() => Locator.MusicLibraryVM.OrderAlbums());
                SetProperty(ref _albumsOrderListing, value);
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
                    _forceLandscape = (bool)force;
                }
                return _forceLandscape;
            }
            set
            {
                ApplicationSettingsHelper.SaveSettingsValue(nameof(ForceLandscape), value);
                SetProperty(ref _forceLandscape, value);
            }
        }
        
        public Languages SelectedLanguage
        {
            get { return GetSelectedLanguage(); }
            set
            {
                ApplicationSettingsHelper.SaveSettingsValue("Languages", (int)value, false);
                SwitchLanguage(value);
                SetProperty(ref _selectedLanguage, value);
            }
        }

        public VLCPage HomePage
        {
            get
            {
                var homePage = ApplicationSettingsHelper.ReadSettingsValue(nameof(HomePage), false);
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
                ApplicationSettingsHelper.SaveSettingsValue(nameof(HomePage), (int)value, false);
                SetProperty(ref _homePage, value);
            }
        }

        public ExternalDeviceMode ExternalDeviceMode
        {
            get
            {
                var externalDeviceMode = ApplicationSettingsHelper.ReadSettingsValue(nameof(ExternalDeviceMode), false);
                if (externalDeviceMode == null)
                    _externalDeviceMode = ExternalDeviceMode.AskMe;
                else
                    _externalDeviceMode = (ExternalDeviceMode)externalDeviceMode;
                return _externalDeviceMode;
            }
            set
            {
                ApplicationSettingsHelper.SaveSettingsValue(nameof(ExternalDeviceMode), (int)value, false);
                SetProperty(ref _externalDeviceMode, value);
            }
        }

        public string SubtitleEncodingValue
        {
            get
            {
                var subtitleEncodingValue = ApplicationSettingsHelper.ReadSettingsValue(nameof(SubtitleEncodingValue), false);
                if (string.IsNullOrEmpty((string)subtitleEncodingValue))
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

        public static void SwitchLanguage(Languages language)
        {
            var currentCulture = "en-US";
            switch (language)
            {
                case Languages.English:
                    currentCulture = "en-US";
                    break;
                case Languages.Japanese:
                    currentCulture = "ja-JP";
                    break;
                case Languages.French:
                    currentCulture = "fr-FR";
                    break;
                case Languages.German:
                    currentCulture = "de-DE";
                    break;
                case Languages.Polish:
                    currentCulture = "pl-PL";
                    break;
                case Languages.Slovak:
                    currentCulture = "sk-SK";
                    break;
                case Languages.Danish:
                    currentCulture = "da-DK";
                    break;
                case Languages.Spanish:
                    currentCulture = "es-ES";
                    break;
                case Languages.Hungarian:
                    currentCulture = "hu-HU";
                    break;
                case Languages.Italian:
                    currentCulture = "it-IT";
                    break;
                case Languages.Korean:
                    currentCulture = "ko-KR";
                    break;
                case Languages.Malay:
                    currentCulture = "ms-MY";
                    break;
                case Languages.Norwegian:
                    currentCulture = "nb-NO";
                    break;
                case Languages.Dutch:
                    currentCulture = "nl-NL";
                    break;
                case Languages.Russian:
                    currentCulture = "ru-RU";
                    break;
                case Languages.Swedish:
                    currentCulture = "sv-SE";
                    break;
                case Languages.Turkish:
                    currentCulture = "tr-TR";
                    break;
                case Languages.Ukrainian:
                    currentCulture = "uk-UA";
                    break;
                case Languages.Chinese:
                    currentCulture = "zh-CN";
                    break;
                case Languages.Portuguese:
                    currentCulture = "pt-PT";
                    break;
                case Languages.Czech:
                    currentCulture = "cs-CZ";
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(language), language, null);
            }
            var culture = new CultureInfo(currentCulture);
            Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = culture.Name;
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
            ResourceContext.GetForCurrentView().Reset();
        }

        public async Task GetMusicLibraryFolders()
        {
            var musicLib = await StorageLibrary.GetLibraryAsync(KnownLibraryId.Music);
            await DispatchHelper.InvokeInUIThread(CoreDispatcherPriority.Low, () => MusicFolders = musicLib.Folders.ToList());
        }

        public async Task GetVideoLibraryFolders()
        {
            var videosLib = await StorageLibrary.GetLibraryAsync(KnownLibraryId.Videos);
            await DispatchHelper.InvokeInUIThread(CoreDispatcherPriority.Low, () => VideoFolders = videosLib.Folders.ToList());
        }

        /// <summary>
        /// TODO: This needs to be moved to a service.
        /// </summary>
        public void ClearKeystore()
        {
            Task.Run(async () =>
            {
                try
                {
                    var file = await StorageFile.GetFileFromPathAsync(string.Format("{0}\\keystore",
                        ApplicationData.Current.LocalFolder.Path)).AsTask().ConfigureAwait(false);
                    await FileIO.WriteTextAsync(file, "").AsTask().ConfigureAwait(false);
                }
                catch (Exception e)
                {

                }
            }).ConfigureAwait(false);
        }

        #region navigation
        public void OnNavigatedFrom(NavigationEventArgs e)
        {
            var settingsPanel = Locator.MainVM.Panels.FirstOrDefault(x => x.Target == VLCPage.SettingsPage);
            if (settingsPanel != null)
                Locator.MainVM.Panels.Remove(settingsPanel);
        }
        #endregion

        public List<string> Profiles { get; } = new List<string>
        {
            "VP9",
            "720p",
            "1080p"
        };

        public string VLCTranscoded => "-VLC-transcoded-";
    }
}