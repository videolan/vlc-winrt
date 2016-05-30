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
using System.IO;

namespace VLC_WinRT.Model.Video
{
    public class VideoItem : BindableBase, IMediaItem
    {
        #region private props
        private string _filePath;
        private string _token;
        private string _name = string.Empty;
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
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }

        public string Type => System.IO.Path.GetExtension(Path);

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
        
        public int TimeWatchedSeconds
        {
            get
            {
                if (_timeWatchedSeconds == 0)
                    _timeWatchedSeconds = (int)TimeWatched.TotalSeconds;
                return _timeWatchedSeconds;
            }
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
        public char AlphaKey => Name.ToUpper()[0];

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

        [Ignore]
        public Media VlcMedia { get; set; }

        #endregion

        #region public fields

        #endregion

        #region constructors
        public VideoItem()
        {
        }
        
        public VideoItem(string name, string path, TimeSpan duration, uint width, uint height, string showTitle, int season, int episode)
        {
            this._name = name;
            this._filePath = path;
            this._duration = duration;
            this._showTitle = showTitle;
            this._season = season;
            this._episode = episode;
            this.Height = height;
            this.Width = width;

            IsTvShow = !string.IsNullOrEmpty(showTitle) && season >= 0 && episode >= 0;
        }

        public async Task InitializeFromFilePath()
        {
            try
            {
                File = await StorageFile.GetFileFromPathAsync(Path);
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        #region methods
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

        public bool IsCurrentPlaying()
        {
            return Path == Locator.MediaPlaybackViewModel.PlaybackService.Playlist[Locator.MediaPlaybackViewModel.PlaybackService.CurrentMedia].Path;
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
