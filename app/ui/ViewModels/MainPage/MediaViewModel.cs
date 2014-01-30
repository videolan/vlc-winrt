using System;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.Storage.FileProperties;
using Windows.UI.Xaml.Automation;
using VLC_WINRT.Utility.Commands;
using Windows.Storage;
using VLC_WINRT.Utility.Commands.VideoPlayer;

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

        public MediaViewModel(StorageFile storageFile)
            : base(storageFile)
        {
            if (storageFile != null)
            {
                Title = storageFile.DisplayName;
                AlphaKey = Title.ToUpper()[0];
                Subtitle = storageFile.FileType.ToUpper() + " File";
                OpenVideo = new OpenVideoCommand();
                FavoriteVideo = new FavoriteVideoCommand();
                SetDuration();
            }
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

        public TimeSpan Duration
        {
            get { return _duration; }
            set { SetProperty(ref _duration, value); }
        }

        async void SetDuration()
        {
            try
            {
                //var p = await File.Properties.GetVideoPropertiesAsync();
                //if (p == null) return;
                //Duration = p.Duration;
            }
            catch
            {
            }
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