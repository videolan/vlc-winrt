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
using System.IO;
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
using VLC_WINRT.Common;
using VLC_WINRT_APP.Commands.Video;
using VLC_WINRT_APP.Common;
using VLC_WINRT_APP.Helpers.MusicLibrary.EchoNest;
using VLC_WINRT_APP.Services.Interface;
using WinRTXamlToolkit.Imaging;

namespace VLC_WINRT_APP.ViewModels.VideoVM
{
    public class VideoVM : BindableBase
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
        private ImageSource _imageBrush;
        private StorageFile _file;
        private FavoriteVideoCommand _favoriteVideo;
        private readonly IThumbnailService _thumbsService;
        #endregion

        #region private fields

        #endregion

        #region public props
        [Ignore]
        public ImageSource Image
        {
            get { return _imageBrush; }
            set { SetProperty(ref _imageBrush, value); }
        }

        [Ignore]
        public StorageFile File
        {
            get { return _file; }
            set
            {
                SetProperty(ref _file, value);
                Task.Run(() => GenerateThumbnail());
            }
        }

        [PrimaryKey, Column("_id")]
        public String Token
        {
            get { return _token; }
            set { SetProperty(ref _token, value); }
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
        #endregion

        #region public fields

        #endregion

        #region constructors
        public VideoVM()
        {
            FavoriteVideo = new FavoriteVideoCommand();
            _thumbsService = App.Container.Resolve<IThumbnailService>();
        }

        public void Initialize(StorageFile storageFile)
        {
            if (storageFile != null)
            {
                if (Image == null)
                {
                    App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        var img = new BitmapImage(new Uri("ms-appx:/Assets/NoCoverWide.jpg", UriKind.RelativeOrAbsolute));
                        Image = img;
                    });
                }
                File = storageFile;
                Title = storageFile.DisplayName;
                AlphaKey = Title.ToUpper()[0];
                Subtitle = storageFile.FileType.ToUpper() + " File";
                Type = storageFile.FileType.Replace(".", "").ToLower();
                Token = StorageApplicationPermissions.FutureAccessList.Add(File);
                GetTimeInformation();
            }
        }
        #endregion

        #region methods
        private async void GenerateThumbnail()
        {
            try
            {
                // If file is a mkv, we save the thumbnail in a VideoPic folder so we don't consume CPU and resources each launch
                StorageFolder videoPic = await ApplicationData.Current.LocalFolder.CreateFolderAsync("videoPic", CreationCollisionOption.OpenIfExists);
                if (File.FileType == ".mkv")
                {
                    StorageFile videoPicFile = null;
#if WINDOWS_APP
                    videoPicFile = await videoPic.TryGetItemAsync(Title + ".jpg") as StorageFile;
#else
                    try
                    {
                        videoPicFile = await videoPic.GetFileAsync(Title + ".jpg");
                    }
                    catch
                    {
                        
                    }
#endif
                    if (videoPicFile != null)
                    {
                        IRandomAccessStream stream = await videoPicFile.OpenReadAsync();
                        App.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
                        {
                            var img = new BitmapImage();
                            img.SetSource(stream);
                            Image = img;
                        });
                    }
                    else
                    {
                        WriteableBitmap thumb = await _thumbsService.GetScreenshot(File);
                        App.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
                        {
                            Image = thumb;
                            thumb.SaveToFile(videoPic, Title + ".jpg");
                        });
                        
                    }
                }
                else
                {
                    StorageItemThumbnail thumb = await _thumbsService.GetThumbnail(File);
                    App.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
                    {
                        var image = new BitmapImage();
                        image.SetSourceAsync(thumb);
                        Image = image;
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        private async Task GetTimeInformation()
        {
            if (VideoProperties == null)
                VideoProperties = await _file.Properties.GetVideoPropertiesAsync();

            TimeSpan duration = VideoProperties != null ? VideoProperties.Duration : TimeSpan.FromSeconds(0);

            await DispatchHelper.InvokeAsync(() =>
            {
                Duration = duration;
            });
        }
        #endregion
    }
}
