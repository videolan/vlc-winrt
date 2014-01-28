using System;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.Storage.FileProperties;
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
        private bool _favorite;

        public MediaViewModel(StorageFile storageFile)
            : base(storageFile)
        {
            if (storageFile != null)
            {
                Title = storageFile.DisplayName;
                Subtitle = storageFile.FileType.ToUpper() + " File";
                OpenVideo = new OpenVideoCommand();
                FavoriteVideo = new FavoriteVideoCommand();
            }
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