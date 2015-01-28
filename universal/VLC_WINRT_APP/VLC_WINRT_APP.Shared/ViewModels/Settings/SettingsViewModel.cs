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
using Windows.Storage;
using Windows.UI.Xaml;
using VLC_WINRT_APP.Commands.Music;
using VLC_WINRT_APP.Commands.Settings;
using VLC_WINRT_APP.Common;
using VLC_WINRT_APP.Helpers;
using VLC_WINRT_APP.Helpers.MusicLibrary;
using VLC_WINRT_APP.Model.Music;
using VLC_WINRT_APP.Model.Video;
using XboxMusicLibrary.Models;
using VLC_WINRT_APP.Model;

namespace VLC_WINRT_APP.ViewModels.Settings
{
    public class SettingsViewModel : BindableBase
    {
#if WINDOWS_APP
        private bool _isSidebarAlwaysMinimized = false;
        private List<StorageFolder> _musicFolders;
        private List<StorageFolder> _videoFolders;
        private bool _notificationOnNewSong;
        private bool _notificationOnNewSongForeground;
        private bool _continueVideoPlaybackInBackground;
#endif
        private OrderType _albumsOrderType;
        private OrderListing _albumsOrderListing;
        private MusicView _musicView;
        private VideoView _videoView;
        private bool _searchArtist;
        private bool _searchAlbum;
        private bool _searchTrack;
        private bool _searchVideo;
        private string _lastFmUserName;
        private string _lastFmPassword;
        private bool _lastFmIsConnecting = false;
        private ApplicationTheme _applicationTheme;
        private bool _forceAppTheme;
#if WINDOWS_APP
        public bool ContinueVideoPlaybackInBackground
        {
            get { return _continueVideoPlaybackInBackground; }
            set
            {
                SetProperty(ref _continueVideoPlaybackInBackground, value);
                ApplicationSettingsHelper.SaveSettingsValue("ContinueVideoPlaybackInBackground", value);
            }
        }
        public bool IsSidebarAlwaysMinimized
        {
            get { return _isSidebarAlwaysMinimized; }
            set
            {
                SetProperty(ref _isSidebarAlwaysMinimized, value);
                ApplicationSettingsHelper.SaveSettingsValue("IsSidebarAlwaysMinimized", value);
                if(value)
                    App.RootPage.ColumnGrid.MinimizeSidebar();
                else
                    App.RootPage.ColumnGrid.RestoreSidebar();
            }
        }
#endif
        public ObservableCollection<OrderType> AlbumsOrderTypeCollection { get; set; }
        public ObservableCollection<OrderListing> AlbumsListingTypeCollection { get; set; }
        public ObservableCollection<MusicView> MusicViewCollection { get; set; }
        public ObservableCollection<VideoView> VideoViewCollection { get; set; }
#if WINDOWS_APP
        public List<StorageFolder> MusicFolders
        {
            get { return _musicFolders; }
            set { SetProperty(ref _musicFolders, value); }
        }

        public List<StorageFolder> VideoFolders
        {
            get { return _videoFolders; }
            set { SetProperty(ref _videoFolders, value); }
        }

        public AddFolderToLibrary AddFolderToLibrary { get; set; }
        public RemoveFolderFromVideoLibrary RemoveFolderFromVideoLibrary { get; set; }
        public RemoveFolderFromMusicLibrary RemoveFolderFromMusicLibrary { get; set; }
        public KnownLibraryId MusicLibraryId { get; set; }
        public KnownLibraryId VideoLibraryId { get; set; }

        public bool NotificationOnNewSong
        {
            get { return _notificationOnNewSong; }
            set
            {
                SetProperty(ref _notificationOnNewSong, value); 
                ApplicationSettingsHelper.SaveSettingsValue("NotificationOnNewSong", value);
            }
        }

        public bool NotificationOnNewSongForeground
        {
            get { return _notificationOnNewSongForeground; }
            set
            {
                SetProperty(ref _notificationOnNewSongForeground, value);
                ApplicationSettingsHelper.SaveSettingsValue("NotificationOnNewSongForeground", value);
            }
        }
#endif

#if WINDOWS_PHONE_APP
        public bool SearchArtists
        {
            get
            {
                var searchArtist = ApplicationSettingsHelper.ReadSettingsValue("SearchArtists");
                if (searchArtist != null && (bool)searchArtist)
                {
                    _searchArtist = true;
                }
                else
                {
                    _searchArtist = false;
                }
                return _searchArtist;
            }
            set
            {
                SetProperty(ref _searchArtist, value);
                ApplicationSettingsHelper.SaveSettingsValue("SearchArtists", (bool)value);
                if (!string.IsNullOrEmpty(Locator.MainVM.SearchTag))
                {
                    SearchHelpers.Search();
                }
            }
        }

        public bool SearchAlbums
        {
            get
            {
                var searchAlbum = ApplicationSettingsHelper.ReadSettingsValue("SearchAlbums");
                if (searchAlbum != null && (bool)searchAlbum)
                {
                    _searchAlbum = true;
                }
                else
                {
                    _searchAlbum = false;
                }
                return _searchAlbum;
            }
            set
            {
                SetProperty(ref _searchAlbum, value);
                ApplicationSettingsHelper.SaveSettingsValue("SearchAlbums", (bool)value);
                if (!string.IsNullOrEmpty(Locator.MainVM.SearchTag))
                {
                    SearchHelpers.Search();
                }
            }
        }

        public bool SearchTracks
        {
            get
            {
                var searchTrack = ApplicationSettingsHelper.ReadSettingsValue("SearchTracks");
                if (searchTrack != null && (bool)searchTrack)
                {
                    _searchTrack = true;
                }
                else
                {
                    _searchTrack = false;
                }
                return _searchTrack;
            }
            set
            {
                SetProperty(ref _searchAlbum, value);
                ApplicationSettingsHelper.SaveSettingsValue("SearchTracks", (bool)value);
                if (!string.IsNullOrEmpty(Locator.MainVM.SearchTag))
                {
                    SearchHelpers.Search();
                }
            }
        }

        public bool SearchVideos
        {
            get
            {
                if (!ApplicationSettingsHelper.Contains("SearchVideos")) SearchVideos = true;
                var searchVideos = ApplicationSettingsHelper.ReadSettingsValue("SearchVideos");

                if (searchVideos != null && (bool)searchVideos)
                {
                    _searchVideo = true;
                }
                else
                {
                    _searchVideo = false;
                }
                return _searchVideo;
            }
            set
            {
                SetProperty(ref _searchVideo, value);
                ApplicationSettingsHelper.SaveSettingsValue("SearchVideos", (bool)value);
                if (!string.IsNullOrEmpty(Locator.MainVM.SearchTag))
                {
                    SearchHelpers.Search();
                }
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
                    _albumsOrderType = OrderType.ByArtist;
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
                if (value != _albumsOrderType)
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

#if WINDOWS_PHONE_APP
        public MusicView MusicView
        {
            get
            {
                var musicView = ApplicationSettingsHelper.ReadSettingsValue("MusicView");
                if (musicView == null)
                {
                    _musicView = MusicView.Albums;
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
#endif

#if WINDOWS_PHONE_APP
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
#endif

        public string LastFmUserName
        {
            get
            {
                var username = ApplicationSettingsHelper.ReadSettingsValue("LastFmUserName");
                if (username == null)
                {
                    _lastFmUserName = "";
                }
                else
                {
                    _lastFmUserName = username.ToString();
                }
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
                if (password == null)
                {
                    _lastFmPassword = "";
                }
                else
                {
                    _lastFmPassword = password.ToString();
                }
                return _lastFmPassword;
            }
            set
            {
                ApplicationSettingsHelper.SaveSettingsValue("LastFmPassword", value);
                SetProperty(ref _lastFmPassword, value);
            }
        }

        public bool LastFmIsConnecting
        {
            get { return _lastFmIsConnecting; }
            set { SetProperty(ref _lastFmIsConnecting, value); }
        }

        public NavToLastFmPage NavToLastFmPage
        {
            get { return new NavToLastFmPage(); }
        }

        public bool ForceAppTheme
        {
            get
            {
                var manualTheme = ApplicationSettingsHelper.ReadSettingsValue("ForceAppTheme");
                if (manualTheme == null)
                {
                    _forceAppTheme = false;
                }
                else
                {
                    _forceAppTheme = (bool)manualTheme;
                }
                return _forceAppTheme;
            }
            set
            {
                ApplicationSettingsHelper.SaveSettingsValue("ForceAppTheme", value);
                SetProperty(ref _forceAppTheme, value);
                if (value) ApplicationTheme = App.Current.RequestedTheme;
                OnPropertyChanged("ApplicationTheme");
            }
        }
        public ApplicationTheme ApplicationTheme
        {
            get
            {
                if (ForceAppTheme)
                {
                    var appTheme = ApplicationSettingsHelper.ReadSettingsValue("ApplicationTheme");
                    if (appTheme == null)
                    {
                        _applicationTheme = App.Current.RequestedTheme;
                    }
                    else
                    {
                        _applicationTheme = (ApplicationTheme) appTheme;
                    }
                }
                else
                {
                    _applicationTheme = App.Current.RequestedTheme;
                }
                return _applicationTheme;
            }
            set
            {
                ApplicationSettingsHelper.SaveSettingsValue("ApplicationTheme", (int)value);
                SetProperty(ref _applicationTheme, value);
            }
        }

        public SettingsViewModel()
        {
            AlbumsOrderTypeCollection = new ObservableCollection<OrderType>();
            AlbumsOrderTypeCollection.Add(OrderType.ByArtist);
            AlbumsOrderTypeCollection.Add(OrderType.ByDate);

            AlbumsListingTypeCollection = new ObservableCollection<OrderListing>();
            AlbumsListingTypeCollection.Add(OrderListing.Ascending);
            AlbumsListingTypeCollection.Add(OrderListing.Descending);

            MusicViewCollection = new ObservableCollection<MusicView>();
            MusicViewCollection.Add(MusicView.Albums);
            MusicViewCollection.Add(MusicView.Artists);
            MusicViewCollection.Add(MusicView.Songs);
            MusicViewCollection.Add(MusicView.Playlists);

            VideoViewCollection = new ObservableCollection<VideoView>();
            VideoViewCollection.Add(VideoView.Videos);
            VideoViewCollection.Add(VideoView.Shows);
            VideoViewCollection.Add(VideoView.CameraRoll);
        }


        public async Task Initialize()
        {
#if WINDOWS_APP
            MusicLibraryId = KnownLibraryId.Music;
            VideoLibraryId = KnownLibraryId.Videos;

            AddFolderToLibrary = new AddFolderToLibrary();
            RemoveFolderFromMusicLibrary = new RemoveFolderFromMusicLibrary();
            RemoveFolderFromVideoLibrary = new RemoveFolderFromVideoLibrary();

            var notificationOnNewSong = ApplicationSettingsHelper.ReadSettingsValue("NotificationOnNewSong");
            NotificationOnNewSong = notificationOnNewSong != null && (bool)notificationOnNewSong;

            var notificationOnNewSongForeground = ApplicationSettingsHelper.ReadSettingsValue("NotificationOnNewSongForeground");
            NotificationOnNewSongForeground = notificationOnNewSongForeground != null && (bool)notificationOnNewSongForeground;
            IsSidebarAlwaysMinimized = (bool)ApplicationSettingsHelper.ReadSettingsValue("IsSidebarAlwaysMinimized");
            ContinueVideoPlaybackInBackground =
                (bool) ApplicationSettingsHelper.ReadSettingsValue("ContinueVideoPlaybackInBackground");

            await GetLibrariesFolders();
#else
            var searchArtist = ApplicationSettingsHelper.ReadSettingsValue("SearchArtists");
            if (searchArtist == null)
            {
                SearchArtists = false;
            }

            var searchVideo = ApplicationSettingsHelper.ReadSettingsValue("SearchVideos");
            if (searchVideo == null)
            {
                SearchVideos = true;
            }

            var searchAlbum = ApplicationSettingsHelper.ReadSettingsValue("SearchAlbums");
            if (searchAlbum == null)
            {
                SearchAlbums = false;
            }

            var searchTrack = ApplicationSettingsHelper.ReadSettingsValue("SearchTracks");
            if (searchTrack == null)
            {
                SearchTracks = false;
            }
#endif
        }
#if WINDOWS_APP
        public async Task GetLibrariesFolders()
        {
            var musicLib = await StorageLibrary.GetLibraryAsync(KnownLibraryId.Music);
            MusicFolders = musicLib.Folders.ToList();

            var videosLib = await StorageLibrary.GetLibraryAsync(KnownLibraryId.Videos);
            VideoFolders = videosLib.Folders.ToList();   
        }
#endif
    }
}
