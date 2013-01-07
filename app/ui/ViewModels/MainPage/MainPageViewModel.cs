using VLC_WINRT.Common;

namespace VLC_WINRT.ViewModels.MainPage
{
    public class MainPageViewModel : BindableBase
    {
        private LibraryViewModel _videoVM;

        public LibraryViewModel VideoVM
        {
            get { return _videoVM; }
            set { SetProperty(ref _videoVM, value); }
        }

        private LibraryViewModel _musicVM;

        public LibraryViewModel MusicVM
        {
            get { return _musicVM; }
            set { SetProperty(ref _musicVM, value); }
        } 
    }
}