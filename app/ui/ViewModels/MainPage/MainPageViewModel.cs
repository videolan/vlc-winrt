using VLC_WINRT.Common;

namespace VLC_WINRT.ViewModels.MainPage
{
    public class MainPageViewModel : BindableBase
    {
        private LastViewedViewModel _lastViewedVM;
        private LibraryViewModel _musicVM;
        private LibraryViewModel _videoVM;

        public MainPageViewModel()
        {
            VideoVM = new LibraryViewModel(KnownVLCLocation.VideosLibrary);
            MusicVM = new LibraryViewModel(KnownVLCLocation.MusicLibrary);
            LastViewedVM = new LastViewedViewModel();
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

        public LastViewedViewModel LastViewedVM
        {
            get { return _lastViewedVM; }
            set { SetProperty(ref _lastViewedVM, value); }
        }
    }
}