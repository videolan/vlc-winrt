using System.Collections.ObjectModel;
using GalaSoft.MvvmLight.Command;
using VLC_WINRT.Common;
using VLC_WINRT.Utility.Commands;

namespace VLC_WINRT.ViewModels.MainPage
{
    public class MainPageViewModel : NavigateableViewModel
    {
        private LastViewedViewModel _lastViewedVM;
        private LibraryViewModel _musicVM;
        private bool _toggleNetworkAppBarVisibility;
        private PickVideoCommand _pickVideoCommand;

        private ObservableCollection<LibraryViewModel> _removableStorageVMs =
            new ObservableCollection<LibraryViewModel>();

        private LibraryViewModel _videoVM;
        private RelayCommand _showNetworkAppBarCommand;

        public MainPageViewModel()
        {
            VideoVM = new LibraryViewModel(KnownVLCLocation.VideosLibrary);
            MusicVM = new LibraryViewModel(KnownVLCLocation.MusicLibrary);

            LastViewedVM = new LastViewedViewModel();
            PickVideo = new PickVideoCommand();

            _showNetworkAppBarCommand = new RelayCommand(() =>
            {
                this.ToggleNetworkAppBarVisibility = !this.ToggleNetworkAppBarVisibility;
            });
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
    }
}