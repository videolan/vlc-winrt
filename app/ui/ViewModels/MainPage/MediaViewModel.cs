/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System;
using Windows.Storage.FileProperties;
using VLC_WINRT.Common;
using VLC_WINRT.Utility.Commands;
using Windows.Storage;
using VLC_WINRT.Utility.Commands.VideoPlayer;
using System.Threading.Tasks;

namespace VLC_WINRT.ViewModels.MainPage
{
    public class MediaViewModel : ThumbnailViewModel
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

        public MediaViewModel(StorageFile storageFile)
            : base(storageFile)
        {
            if (storageFile != null)
            {
                Title = storageFile.DisplayName;
                AlphaKey = Title.ToUpper()[0];
                Subtitle = storageFile.FileType.ToUpper() + " File";
                Type = File.FileType.Replace(".", "").ToLower();
                OpenVideo = new OpenVideoCommand();
                FavoriteVideo = new FavoriteVideoCommand();
            }
        }

        public string Type
        {
            get { return _type; }
            set { SetProperty(ref _type, value); }
        }
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
                VideoProperties = await File.Properties.GetVideoPropertiesAsync();

            TimeSpan duration = VideoProperties != null ? VideoProperties.Duration : TimeSpan.FromSeconds(0);

            await DispatchHelper.InvokeAsync(() =>
            {
                Duration = duration;
            });
        }

        public OpenVideoCommand OpenVideo
        {
            get { return _openVideo; }
            set { SetProperty(ref _openVideo, value); }
        }

        public FavoriteVideoCommand FavoriteVideo
        {
            get { return _favoriteVideo; }
            set { SetProperty(ref _favoriteVideo, value); }
        }

        public VideoProperties VideoProperties;
    }
}
