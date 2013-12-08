using System.Collections.ObjectModel;
using GalaSoft.MvvmLight.Command;
using VLC_WINRT.Common;
using VLC_WINRT.Utility.Commands;

namespace VLC_WINRT.ViewModels.MainPage
{
    public class MainPageViewModel : NavigateableViewModel
    {
        private bool _isAppBarVisible;
        private LastViewedViewModel _lastViewedVM;
        private LibraryViewModel _musicVM;
        private string _networkMRL = string.Empty;
        private PickVideoCommand _pickVideoCommand;
        private PlayNetworkMRLCommand _playNetworkMRL;

        private ObservableCollection<LibraryViewModel> _removableStorageVMs =
            new ObservableCollection<LibraryViewModel>();

        private RelayCommand _showAppBarCommand;

        private RelayCommand _showNetworkAppBarCommand;
        private bool _toggleNetworkAppBarVisibility;
        private LibraryViewModel _videoVM;

        public MainPageViewModel()
        {
            VideoVM = new LibraryViewModel(KnownVLCLocation.VideosLibrary);
            MusicVM = new LibraryViewModel(KnownVLCLocation.MusicLibrary);

            LastViewedVM = new LastViewedViewModel();
            PickVideo = new PickVideoCommand();
            PlayNetworkMRL = new PlayNetworkMRLCommand();

            _showNetworkAppBarCommand =
                new RelayCommand(() => { ToggleNetworkAppBarVisibility = !ToggleNetworkAppBarVisibility; });

            _showAppBarCommand = new RelayCommand(() => { IsAppBarVisible = true; });
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

        public bool ToggleNetworkAppBarVisibility
        {
            get { return _toggleNetworkAppBarVisibility; }
            set { SetProperty(ref _toggleNetworkAppBarVisibility, value); }
        }

        public PickVideoCommand PickVideo
        {
            get { return _pickVideoCommand; }
            set { SetProperty(ref _pickVideoCommand, value); }
        }

        public RelayCommand ShowAppBarCommand
        {
            get { return _showAppBarCommand; }
            set { SetProperty(ref _showAppBarCommand, value); }
        }

        public RelayCommand ShowNetworkAppBarCommand
        {
            get { return _showNetworkAppBarCommand; }
            set { SetProperty(ref _showNetworkAppBarCommand, value); }
        }

        public ObservableCollection<LibraryViewModel> RemovableStorageVMs
        {
            get { return _removableStorageVMs; }
            set { SetProperty(ref _removableStorageVMs, value); }
        }

        public bool IsAppBarVisible
        {
            get { return _isAppBarVisible; }
            set { SetProperty(ref _isAppBarVisible, value); }
        }

        public PlayNetworkMRLCommand PlayNetworkMRL
        {
            get { return _playNetworkMRL; }
            set { SetProperty(ref _playNetworkMRL, value); }
        }

        public string NetworkMRL
        {
            get { return _networkMRL; }
            set { SetProperty(ref _networkMRL, value); }
        }
    }
}