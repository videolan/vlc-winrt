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
using VLC_WINRT_APP.Commands.Settings;
using VLC_WINRT_APP.Common;
using VLC_WINRT_APP.Helpers;
using VLC_WINRT_APP.Helpers.MusicLibrary;
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
#if WINDOWS_PHONE_APP
        private bool _enableSidebar;
        private bool _searchArtist;
        private bool _searchAlbum;
        private bool _searchTrack;
        private bool _searchVideo;
#endif
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

        public bool EnableSidebar
        {
            get
            {
                var enableSide = ApplicationSettingsHelper.ReadSettingsValue("EnableSidebar");
                if (enableSide != null && (bool) enableSide)
                {
                    _enableSidebar = true;
                }
                else
                {
                    _enableSidebar = false;
                }
                return _enableSidebar;
            }
            set
            {
                SetProperty(ref _enableSidebar, value);
                ApplicationSettingsHelper.SaveSettingsValue("EnableSidebar", (bool)value);
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
                SetProperty(ref _albumsOrderType, value);
                MusicLibraryManagement.OrderAlbums();
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
                SetProperty(ref _albumsOrderListing, value);
                MusicLibraryManagement.OrderAlbums();
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
            var enableSide = ApplicationSettingsHelper.ReadSettingsValue("EnableSidebar");
            if (enableSide == null)
            {
                EnableSidebar = false;
            }

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
