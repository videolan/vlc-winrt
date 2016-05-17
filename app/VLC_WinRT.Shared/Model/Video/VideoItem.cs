/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.UI.Core;
using SQLite;
using VLC_WinRT.Commands.VideoLibrary;
using VLC_WinRT.Helpers.VideoLibrary;
using VLC_WinRT.Utils;
using VLC_WinRT.ViewModels;
using System.Diagnostics;
using Windows.Storage.AccessCache;
using libVLCX;
using VLC_WinRT.Helpers;
using Windows.UI.Xaml.Media.Imaging;

namespace VLC_WinRT.Model.Video
{
    public class VideoItem : BindableBase, IMediaItem
    {
        #region private props
        private char _alphaKey;
        private string _token;
        private string _type;
        private string _title = string.Empty;
        private string _subtitle = string.Empty;
        private bool _favorite;
        private TimeSpan _duration;
        private int _timeWatchedSeconds;
        private StorageFile _file;
        private DateTime _lastWatched;

        private BitmapImage _videoImage;
        private LoadingState _videosImageLoadingState = LoadingState.NotLoaded;

        
        #region tvshows props // TVShows related // Todo create a class ShowEpisodeItem that inherits from VideoItem
        private string _showTitle;
        private int _season = -1;
        private int _episode;
        #endregion

        #endregion

        #region private fields

        #endregion

        #region public props
        #region tvshows related
        public bool IsTvShow { get; set; }

        public string ShowTitle
        {
            get { return _showTitle; }
            set { SetProperty(ref _showTitle, value); }
        }

        public int Season
        {
            get { return _season; }
            private set { SetProperty(ref _season, value); }
        }

        public int Episode
        {
            get { return _episode; }
            private set { SetProperty(ref _episode, value); }
        }
        #endregion

        [PrimaryKey, AutoIncrement, Column("_id")]
        public int Id { get; set; }

        public string Name
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }
        public string Type
        {
            get { return _type; }
            set { SetProperty(ref _type, value); }
        }
        public uint Width { get; set; }
        public uint Height { get; set; }

        public bool Favorite
        {
            get { return _favorite; }
            set { SetProperty(ref _favorite, value); }
        }
        public TimeSpan Duration
        {
            get { return _duration; }
            set { SetProperty(ref _duration, value); }
        }

        public DateTime LastWatched
        {
            get { return _lastWatched; }
            set { SetProperty(ref _lastWatched, value); }
        }

        public Boolean IsPictureLoaded { get; set; }

        public Boolean HasMoviePicture { get; set; }

        public Boolean IsCameraRoll { get; set; }

        public bool IsCurrentPlaying { get; set; }
        
        public int TimeWatchedSeconds
        {
            get { return _timeWatchedSeconds; }
            set
            {
                SetProperty(ref _timeWatchedSeconds, value);
                OnPropertyChanged(nameof(TimeWatched));
                OnPropertyChanged(nameof(HasBeenSeen));
            }
        }
        
        [Ignore]
        public BitmapImage VideoImage
        {
            get
            {
                if (_videoImage == null && _videosImageLoadingState == LoadingState.NotLoaded)
                {
                    _videosImageLoadingState = LoadingState.Loading;
                    ResetVideoPicture();
                }

                return _videoImage;
            }
            set { SetProperty(ref _videoImage, value); }
        }

        public Task ResetVideoPicture()
        {
            return Task.Factory.StartNew(() => LoadImageToMemoryHelper.LoadImageToMemory(this));
        }

        [Ignore]
        public string PictureUri
        {
            get
            {
                if (HasMoviePicture)
                {
                    return $"{Strings.MoviePicFolderPath}/{Id}.jpg";
                }
                else if (IsPictureLoaded)
                {
                    return $"{Strings.VideoPicFolderPath}/{Id}.jpg";
                }
                return string.Empty;
            }
        }
        
        [Ignore]
        public StorageFile File
        {
            get { return _file; }
            private set
            {
                SetProperty(ref _file, value);
            }
        }

        [Ignore]
        public String Token
        {
            get { return _token; }
            set { SetProperty(ref _token, value); }
        }

        [Unique]
        public string Path
        {
            get { return _filePath; }
            set { SetProperty(ref _filePath, value); }
        }

        [Ignore]
        public char AlphaKey
        {
            get { return _alphaKey; }
            set { SetProperty(ref _alphaKey, value); }
        }
        
        [Ignore]
        public TimeSpan TimeWatched
        {
            get { return TimeSpan.FromSeconds(_timeWatchedSeconds); }
        }
 
        [Ignore]
        public bool HasBeenSeen
        {
            get
            {
                return TimeWatched.Seconds > 0 && Duration.Seconds > 30 && ((double)TimeWatched.Seconds / Duration.Seconds) < 0.98;
            }
        }
        
        [Ignore]
        public FavoriteVideoCommand FavoriteVideo { get; } = new FavoriteVideoCommand();
        
        private string _filePath;

        [Ignore]
        public Media VlcMedia { get; set; }

        #endregion

        #region public fields

        #endregion

        #region constructors
        public VideoItem()
        {
        }

        public VideoItem(string showName, int season, int episode)
        {
            ShowTitle = showName;
            Season = season;
            Episode = episode;
        }

        public async Task Initialize(StorageFile storageFile)
        {
            if (storageFile != null)
            {
                File = storageFile;
                Name = (string.IsNullOrEmpty(storageFile.DisplayName)) ? storageFile.Name : storageFile.DisplayName;
                AlphaKey = Name.ToUpper()[0];
                Type = storageFile.FileType.ToLower();
                Path = storageFile.Path;
                await GetTimeInformation();
            }
        }

        public async Task InitializeFromFilePath()
        {
            try
            {
                StorageFile file = await StorageFile.GetFileFromPathAsync(Path);
                await Initialize(file);
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        #region methods
        private async Task GetTimeInformation()
        {
            if (_duration != TimeSpan.Zero) return;
            var media = await Locator.VLCService.GetMediaFromPath(_filePath);
            var duration = await Locator.VLCService.GetDuration(media);
            if (duration == null || duration == TimeSpan.Zero)
            {
                var mP = await File.Properties.GetVideoPropertiesAsync();
                duration = mP.Duration;
            }

            await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Normal, () =>
            {
                Duration = duration;
            });
        }

        public Tuple<FromType, string> GetMrlAndFromType(bool preferToken)
        {
            if (!string.IsNullOrEmpty(_token))
            {
                // Using an already created token
                return new Tuple<FromType, string>(FromType.FromLocation, "winrt://" + _token);
            }
            if (File != null && (string.IsNullOrEmpty(Path) || preferToken))
            {
                // Using a Token
                // FromLocation : 1
                return new Tuple<FromType, string>(FromType.FromLocation, "winrt://" + StorageApplicationPermissions.FutureAccessList.Add(File));
            }
            if (!string.IsNullOrEmpty(Path))
                return new Tuple<FromType, string>(FromType.FromPath, Path);
            return null;
        }

        public async Task<bool> LoadFileFromPath()
        {
            try
            {
                if (File == null)
                {
                    File = await StorageFile.GetFileFromPathAsync(Path);
                }
            }
            catch (Exception e)
            {
                return false;
            }
            return true;
        }
        #endregion
    }
}
