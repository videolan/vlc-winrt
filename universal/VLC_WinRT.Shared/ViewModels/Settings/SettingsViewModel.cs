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
        private bool _continueVideoPlaybackInBackground;
        private OrderType _albumsOrderType;
        private OrderListing _albumsOrderListing;
        private MusicView _musicView;
        private VideoView _videoView;
        private VLCPage _homePage;
        private string _lastFmUserName;
        private string _lastFmPassword;
        private bool _lastFmIsConnected = false;
        private bool _hardwareAcceleration;
        private bool _richAnimations;
        private KeyboardActionDatabase _keyboardActionDatabase;

        public ApplicationTheme ApplicationTheme
        {
            get
            {
                return GetApplicationTheme();
            }
            set
            {
                ApplicationSettingsHelper.SaveSettingsValue(nameof(ApplicationTheme), (int)value);
                SetProperty(ref applicationTheme, value);
                App.SetShellDecoration();
            }
        }

        public static ApplicationTheme GetApplicationTheme()
        {
            var appTheme = ApplicationSettingsHelper.ReadSettingsValue(nameof(ApplicationTheme));
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

        public KeyboardActionDatabase KeyboardActionDatabase
        {
            get
            {
                _keyboardActionDatabase = new KeyboardActionDatabase();

                var actions = _keyboardActionDatabase.GetAllKeyboardActions();
                if (!actions.Any())
                {
                    // never set before, we need to do it ...
                    var actionsToSet = new List<KeyboardAction>()
                    {
                        new KeyboardAction()
                        {
                            Action = VLCAction.FullscreenToggle,
                            MainKey = VirtualKey.F
                        },
                        new KeyboardAction()
                        {
                            Action = VLCAction.LeaveFullscreen,
                            MainKey = VirtualKey.Escape,
                        },
                        new KeyboardAction()
                        {
                            Action = VLCAction.PauseToggle,
                            MainKey = VirtualKey.Space
                        },
                        new KeyboardAction()
                        {
                            Action = VLCAction.Faster,
                            MainKey = VirtualKey.Add
                        },
                        new KeyboardAction()
                        {
                            Action = VLCAction.Slow,
                            MainKey = VirtualKey.Subtract,
                        },
                        new KeyboardAction()
                        {
                            Action = VLCAction.NormalRate,
                            MainKey = VirtualKey.Execute,
                        },
                        new KeyboardAction()
                        {
                            Action = VLCAction.Next,
                            MainKey = VirtualKey.N,
                        },
                        new KeyboardAction()
                        {
                            Action = VLCAction.Previous,
                            MainKey = VirtualKey.P,
                        },
                        new KeyboardAction()
                        {
                            Action = VLCAction.Stop,
                            MainKey = VirtualKey.S,
                        },
                        new KeyboardAction()
                        {
                            Action = VLCAction.Quit,
                            MainKey = VirtualKey.Q,
                        },
                        new KeyboardAction()
                        {
                            Action = VLCAction.VolumeUp,
                            MainKey = VirtualKey.Control,
                            SecondKey = VirtualKey.Add,
                        },
                        new KeyboardAction()
                        {
                            Action = VLCAction.VolumeDown,
                            MainKey = VirtualKey.Control,
                            SecondKey = VirtualKey.Subtract,
                        },
                        new KeyboardAction()
                        {
                            Action = VLCAction.Mute,
                            MainKey = VirtualKey.M,
                        },
                        new KeyboardAction()
                        {
                            Action = VLCAction.ChangeAudioTrack,
                            MainKey = VirtualKey.B,
                        },
                        new KeyboardAction()
                        {
                            Action = VLCAction.ChangeSubtitle,
                            MainKey = VirtualKey.V
                        },
                        new KeyboardAction()
                        {
                            Action = VLCAction.OpenFile,
                            MainKey = VirtualKey.Control,
                            SecondKey = VirtualKey.O,
                        },
                        new KeyboardAction()
                        {
                            Action = VLCAction.OpenNetwork,
                            MainKey = VirtualKey.Control,
                            SecondKey = VirtualKey.N
                        }
                    };
                    _keyboardActionDatabase.AddKeyboardActions(actionsToSet);
                }
                return _keyboardActionDatabase;
            }
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
                    _continueVideoPlaybackInBackground = (bool) continuePlaybackInBackground;
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

        public List<VLCPage> HomePageCollection { get; set; } = new List<VLCPage>()
        {
            VLCPage.MainPageHome,
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


        public List<MusicView> MusicViewCollection
        { get; set; }
        = new List<MusicView>()
        {
            MusicView.Artists,
            MusicView.Albums,
            MusicView.Songs,
            MusicView.Playlists
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
                    Task.Run(()=>GetMusicLibraryFolders());
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
                var albumsOrderType = ApplicationSettingsHelper.ReadSettingsValue("AlbumsOrderType");
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
                ApplicationSettingsHelper.SaveSettingsValue("AlbumsOrderType", (int)value);
                if ((int)value == 0 || value != _albumsOrderType)
                    MusicLibraryManagement.OrderAlbums();
                SetProperty(ref _albumsOrderType, value);
            }
        }

        public OrderListing AlbumsOrderListing
        {
            get
            {
                var albumsOrderListing = ApplicationSettingsHelper.ReadSettingsValue("AlbumsOrderListing");
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
                ApplicationSettingsHelper.SaveSettingsValue("AlbumsOrderListing", (int)value);
                if (value != _albumsOrderListing)
                    MusicLibraryManagement.OrderAlbums();
                SetProperty(ref _albumsOrderListing, value);
            }
        }

        public MusicView MusicView
        {
            get
            {
                var musicView = ApplicationSettingsHelper.ReadSettingsValue("MusicView");
                if (musicView == null)
                {
                    _musicView = MusicView.Artists;
                }
                else
                {
                    _musicView = (MusicView)musicView;
                }
                return _musicView;
            }
            set
            {
                ApplicationSettingsHelper.SaveSettingsValue("MusicView", (int)value);
                if (value != _musicView)
                {
                    Locator.MainVM.ChangeMainPageMusicViewCommand.Execute((int)value);
                }
                SetProperty(ref _musicView, value);
            }
        }

        public VideoView VideoView
        {
            get
            {
                var videoView = ApplicationSettingsHelper.ReadSettingsValue("VideoView");
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
                ApplicationSettingsHelper.SaveSettingsValue("VideoView", (int)value);
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
                var homePage = ApplicationSettingsHelper.ReadSettingsValue("Homepage");
                if (homePage == null)
                {
                    _homePage = VLCPage.MainPageHome;
                }
                else
                {
                    _homePage = (VLCPage)homePage;
                }
                return _homePage;
            }
            set
            {
                ApplicationSettingsHelper.SaveSettingsValue("Homepage", (int)value);
                SetProperty(ref _homePage, value);
            }
        }

        public ChangeSettingsViewCommand ChangeSettingsViewCommand { get; } = new ChangeSettingsViewCommand();

#if WINDOWS_APP
        public async Task GetMusicLibraryFolders()
        {
            var musicLib = await StorageLibrary.GetLibraryAsync(KnownLibraryId.Music);
            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Low, ()=> MusicFolders = musicLib.Folders.ToList());
        }

        public async Task GetVideoLibraryFolders()
        {
            var videosLib = await StorageLibrary.GetLibraryAsync(KnownLibraryId.Videos);
            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () => VideoFolders = videosLib.Folders.ToList());
        }
#endif
    }
}
