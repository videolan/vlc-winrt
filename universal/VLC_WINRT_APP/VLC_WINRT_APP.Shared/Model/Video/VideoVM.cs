/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Autofac;
using SQLite;
using VLC_WINRT_APP.Commands.Video;
using VLC_WINRT_APP.Common;
using VLC_WINRT_APP.Services.Interface;
using WinRTXamlToolkit.Imaging;
using WinRTXamlToolkit.IO.Extensions;

namespace VLC_WINRT_APP.Model.Video
{
    public class VideoItem : BindableBase
    {
        #region private props
        private char _alphaKey;
        private string _token;
        private string _type;
        private string _title = string.Empty;
        private string _subtitle = string.Empty;
        private bool _favorite;
        private TimeSpan _duration;
        private TimeSpan _timeWatched;
        private String _thumbnailPath = "ms-appx:///Assets/NoCover.jpg";
        private StorageFile _file;
        private FavoriteVideoCommand _favoriteVideo;
        private readonly IThumbnailService _thumbsService;
        private DateTime _lastWatched;
        // TVShows related
        private int _season = -1;
        private int _episode;

        #endregion

        #region private fields

        #endregion

        #region public props
        #region tvshows related
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

        public String ThumbnailPath
        {
            get
            {
                return _thumbnailPath;
            }
            set { SetProperty(ref _thumbnailPath, value); }
        }

        [Ignore]
        public StorageFile File
        {
            get { return _file; }
            set
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
        public string FilePath
        {
            get { return _filePath; }
            set { SetProperty(ref _filePath, value); }
        }

        public string Type
        {
            get { return _type; }
            set { SetProperty(ref _type, value); }
        }

        [Ignore]
        public char AlphaKey
        {
            get { return _alphaKey; }
            set { SetProperty(ref _alphaKey, value); }
        }

        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        public string Subtitle
        {
            get { return _subtitle; }
            set { SetProperty(ref _subtitle, value); }
        }

        public bool Favorite
        {
            get { return _favorite; }
            set { SetProperty(ref _favorite, value); }
        }

        public TimeSpan TimeWatched
        {
            get { return _timeWatched; }
            set
            {
                SetProperty(ref _timeWatched, value);
                OnPropertyChanged("PortionWatched");
            }
        }

        public TimeSpan Duration
        {
            get { return _duration; }
            set { SetProperty(ref _duration, value); }
        }

        [Ignore]
        public FavoriteVideoCommand FavoriteVideo
        {
            get { return _favoriteVideo; }
            set { SetProperty(ref _favoriteVideo, value); }
        }

        public VideoProperties VideoProperties;

        public DateTime LastWatched
        {
            get { return _lastWatched; }
            set { SetProperty(ref _lastWatched, value); }
        }

        public Boolean HasThumbnail { get; set; }

        public Boolean IsCameraRoll { get; set; }

        private string _filePath;

        public bool IsTvShow
        {
            get { return _season != -1; }
        }
        #endregion

        #region public fields

        #endregion

        #region constructors
        public VideoItem()
        {
            FavoriteVideo = new FavoriteVideoCommand();
            _thumbsService = App.Container.Resolve<IThumbnailService>();
        }

        public VideoItem(string season, string episode)
        {
            FavoriteVideo = new FavoriteVideoCommand();
            _thumbsService = App.Container.Resolve<IThumbnailService>();
            Season = int.Parse(season);
            Episode = int.Parse(episode);
        }

        public async Task Initialize(StorageFile storageFile)
        {
            if (storageFile != null)
            {
                File = storageFile;
                Title = storageFile.DisplayName;
                AlphaKey = Title.ToUpper()[0];
                Subtitle = storageFile.FileType.ToUpper() + " File";
                Type = storageFile.FileType.Replace(".", "").ToLower();
                Token = StorageApplicationPermissions.FutureAccessList.Add(File);
                FilePath = storageFile.Path;
                await GetTimeInformation();
            }
        }

        public async Task InitializeFromFilePath()
        {
            try
            {
                StorageFile file = await StorageFile.GetFileFromPathAsync(FilePath);
                await Initialize(file);
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        #region methods
        // Returns false is no snapshot generation was required, true otherwise
        public async Task<Boolean> GenerateThumbnail()
        {
            if (HasThumbnail)
                return false;
            try
            {
                WriteableBitmap image = null;
                StorageItemThumbnail thumb = null;
                // If file is a mkv, we save the thumbnail in a VideoPic folder so we don't consume CPU and resources each launch
                if (VLCFileExtensions.MFSupported.Contains(File.FileType.ToLower()))
                {
                    thumb = await _thumbsService.GetThumbnail(File).ConfigureAwait(false);
                }
                // If MF thumbnail generation failed or wasn't supported:
                if (thumb == null)
                { 
                    var res = await _thumbsService.GetScreenshot(File).ConfigureAwait(false);
                    if (res == null)
                        return true;
                    image = res.Bitmap();
                    await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => Duration = TimeSpan.FromMilliseconds(res.Length()));
                }
                if (thumb != null || image != null)
                {
                    // RunAsync won't await on the lambda it receives, so we need to do it ourselves
                    var tcs = new TaskCompletionSource<bool>();
                    await App.Dispatcher.RunAsync(CoreDispatcherPriority.Low, new Windows.UI.Core.DispatchedHandler(async () =>
                    {
                        if (thumb != null)
                        {
                            image = new WriteableBitmap((int)thumb.OriginalWidth, (int)thumb.OriginalHeight);
                            await image.SetSourceAsync(thumb);
                        }
                        StorageFolder videoPic = await ApplicationData.Current.LocalFolder.CreateFolderAsync("videoPic", CreationCollisionOption.OpenIfExists);
                        var thumbnailFile = await image.SaveToFile(videoPic, Id + ".jpg");
                        ThumbnailPath = String.Format("ms-appdata:///local/videoPic/{0}.jpg", Id);
                        tcs.SetResult(true);
                    }));
                    await tcs.Task;
                }
                HasThumbnail = true;
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
            return false;
        }

        private async Task GetTimeInformation()
        {
            if (VideoProperties == null)
            {
                await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    VideoProperties = await _file.Properties.GetVideoPropertiesAsync();
                    TimeSpan duration = VideoProperties != null ? VideoProperties.Duration : TimeSpan.FromSeconds(0);
#if WINDOWS_PHONE_APP
                    // Absolutely totally bad workaround
                    if(duration.Seconds < 1)
                        duration = TimeSpan.FromTicks(duration.Ticks * 10000);
#endif
                    Duration = duration;
                });
            }

            //App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            //{
            //});
        }
        #endregion
    }
}
