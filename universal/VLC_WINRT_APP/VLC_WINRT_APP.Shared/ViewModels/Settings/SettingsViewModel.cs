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
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using VLC_WINRT_APP.Commands.Settings;
using VLC_WINRT_APP.Common;
using VLC_WINRT_APP.Helpers;
using XboxMusicLibrary.Models;

namespace VLC_WINRT_APP.ViewModels.Settings
{
    public class SettingsViewModel : BindableBase
    {
        private List<StorageFolder> _musicFolders;
        private List<StorageFolder> _videoFolders;
        private bool _notificationOnNewSong;
        private bool _notificationOnNewSongForeground;

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

        public async Task Initialize()
        {
            MusicLibraryId = KnownLibraryId.Music;
            VideoLibraryId = KnownLibraryId.Videos;

            AddFolderToLibrary = new AddFolderToLibrary();
            RemoveFolderFromMusicLibrary = new RemoveFolderFromMusicLibrary();
            RemoveFolderFromVideoLibrary = new RemoveFolderFromVideoLibrary();

            var notificationOnNewSong = ApplicationSettingsHelper.ReadSettingsValue("NotificationOnNewSong");
            NotificationOnNewSong = notificationOnNewSong != null && (bool)notificationOnNewSong;

            var notificationOnNewSongForeground = ApplicationSettingsHelper.ReadSettingsValue("NotificationOnNewSongForeground");
            NotificationOnNewSongForeground = notificationOnNewSongForeground != null && (bool)notificationOnNewSongForeground;
            await GetLibrariesFolders();
        }

        public async Task GetLibrariesFolders()
        {
            var musicLib = await StorageLibrary.GetLibraryAsync(KnownLibraryId.Music);
            MusicFolders = musicLib.Folders.ToList();

            var videosLib = await StorageLibrary.GetLibraryAsync(KnownLibraryId.Videos);
            VideoFolders = videosLib.Folders.ToList();   
        }
    }
}
