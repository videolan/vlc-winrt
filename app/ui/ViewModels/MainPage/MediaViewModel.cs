using VLC_WINRT.Utility.Commands;
using Windows.Storage;

namespace VLC_WINRT.ViewModels.MainPage
{
    public class MediaViewModel : ThumbnailViewModel
    {
        private OpenVideoCommand _openVideo;
        private string _subtitle = string.Empty;
        private string _title = string.Empty;

        public MediaViewModel(StorageFile storageFile) : base(storageFile)
        {
            if (storageFile != null)
            {
                Title = storageFile.DisplayName;
                Subtitle = storageFile.FileType.ToUpper() + " File";
            
                OpenVideo = new OpenVideoCommand();
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

        public OpenVideoCommand OpenVideo
        {
            get { return _openVideo; }
            set { SetProperty(ref _openVideo, value); }
        }
    }
}