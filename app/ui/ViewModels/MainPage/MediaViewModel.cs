using VLC_WINRT.Utility.Commands;
using Windows.Storage;

namespace VLC_WINRT.ViewModels.MainPage
{
    public class MediaViewModel : ThumbnailViewModel
    {
        private OpenFileCommand _openFile;
        private string _subtitle = string.Empty;
        private string _title = string.Empty;

        public MediaViewModel(StorageFile storageFile) : base(storageFile)
        {
            Title = storageFile.Name;
            Subtitle = storageFile.FileType.ToUpper() + " File";
            
            OpenFile = new OpenFileCommand();
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

        public OpenFileCommand OpenFile
        {
            get { return _openFile; }
            set { SetProperty(ref _openFile, value); }
        }
    }
}