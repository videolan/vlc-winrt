using VLC_WINRT.Common;
using Windows.Storage;

namespace VLC_WINRT.ViewModels.MainPage
{
    public class MainPageViewModel : BindableBase
    {
        private LibraryViewModel _musicVM;
        private LibraryViewModel _videoVM;

        public MainPageViewModel()
        {
            VideoVM = new LibraryViewModel("Videos", "", KnownVLCLocation.VideosLibrary);
            MusicVM = new LibraryViewModel("Music", "", KnownVLCLocation.MusicLibrary);
        }

        public LibraryViewModel VideoVM
        {
            get { return _videoVM; }
            set { SetProperty(ref _videoVM, value); }
        }

        public LibraryViewModel MusicVM
        {
            get { return _musicVM; }
            set { SetProperty(ref _musicVM, value); }
        }
    }
}