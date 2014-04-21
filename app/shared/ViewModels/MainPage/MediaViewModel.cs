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
using Windows.Storage.AccessCache;
using Windows.Storage.FileProperties;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Autofac;
using SQLite;
using VLC_WINRT.Common;
using VLC_WINRT.Utility.Commands;
using Windows.Storage;
using VLC_WINRT.Utility.Commands.VideoPlayer;
using System.Threading.Tasks;
using VLC_WINRT.Utility.Services.Interface;
using VLC_WINRT.ViewModels.Settings;

namespace VLC_WINRT.ViewModels.MainPage
{
    public class MediaViewModel : BindableBase
    {
        private OpenVideoCommand _openVideo;
        private FavoriteVideoCommand _favoriteVideo;
        private string _subtitle = string.Empty;
        private string _title = string.Empty;
        private char _alphaKey;
        private bool _favorite;
        private TimeSpan _duration;
        private string _type;
        private TimeSpan _timeWatched;
        private StorageFile _file;
        private ImageSource _imageBrush;
        private readonly IThumbnailService _thumbsService;
        private string _token;
        public MediaViewModel()
        {
            OpenVideo = new OpenVideoCommand();
            FavoriteVideo = new FavoriteVideoCommand();
            _thumbsService = App.Container.Resolve<IThumbnailService>();
        }

        public void Initialize(StorageFile storageFile)
        {
            if (storageFile != null)
            {
                File = storageFile;
                Title = storageFile.DisplayName;
                AlphaKey = Title.ToUpper()[0];
                Subtitle = storageFile.FileType.ToUpper() + " File";
                Type = storageFile.FileType.Replace(".", "").ToLower();
                Token = StorageApplicationPermissions.FutureAccessList.Add(File);
            }
        }

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

        private async void GenerateThumbnail()
        {
            try
            {
                if (File.FileType == ".mkv")
                {
                    WriteableBitmap thumb = await _thumbsService.GetScreenshot(File);
                    await DispatchHelper.InvokeAsync(async () =>
                    {
                        Image = thumb;
                    });
                }
                else
                {
                    StorageItemThumbnail thumb = await _thumbsService.GetThumbnail(File);
                    await DispatchHelper.InvokeAsync(async () =>
                    {
                        var image = new BitmapImage();
                        await image.SetSourceAsync(thumb);
                        Image = image;
                    });
                }
               
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
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

        public Task Initialize()
        {
            return GetTimeInformation();
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

        [Ignore]
        public OpenVideoCommand OpenVideo
        {
            get { return _openVideo; }
            set { SetProperty(ref _openVideo, value); }
        }

        [Ignore]
        public FavoriteVideoCommand FavoriteVideo
        {
            get { return _favoriteVideo; }
            set { SetProperty(ref _favoriteVideo, value); }
        }

        public VideoProperties VideoProperties;
    }
}
