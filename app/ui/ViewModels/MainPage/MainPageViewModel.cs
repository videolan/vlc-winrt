using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.Storage;
using VLC_WINRT.Common;
using VLC_WINRT.Utility.Commands;

namespace VLC_WINRT.ViewModels.MainPage
{
    public class MainPageViewModel : NavigateableViewModel
    {
        private ObservableCollection<VideoLibraryViewModel> _dlnaVMs =
            new ObservableCollection<VideoLibraryViewModel>();

        private ExternalStorageViewModel _externalStorageVM;

        private bool _isAppBarOpen;
        private bool _isNetworkAppBarShown;
        private LastViewedViewModel _lastViewedVM;
        private VideoLibraryViewModel _musicVM;
        private string _networkMRL = string.Empty;
        private PickVideoCommand _pickVideoCommand;
        private PlayNetworkMRLCommand _playNetworkMRL;
        private ActionCommand _showAppBarCommand;
        private ActionCommand _toggleNetworkAppBarCommand;
        private VideoLibraryViewModel _videoVM;

        public MainPageViewModel()
        {
            VideoVM = new VideoLibraryViewModel(KnownVLCLocation.VideosLibrary);
            MusicVM = new VideoLibraryViewModel(KnownVLCLocation.MusicLibrary);
            ExternalStorageVM = new ExternalStorageViewModel();

            Task<IReadOnlyList<StorageFolder>> dlnaFolders = KnownVLCLocation.MediaServers.GetFoldersAsync().AsTask();
            dlnaFolders.ContinueWith(t =>
            {
                IReadOnlyList<StorageFolder> folders = t.Result;
                foreach (StorageFolder storageFolder in folders)
                {
                    StorageFolder newFolder = storageFolder;
                    DispatchHelper.Invoke(() => DLNAVMs.Add(new VideoLibraryViewModel(newFolder)));
                }
            });

            LastViewedVM = new LastViewedViewModel();
            PickVideo = new PickVideoCommand();
            PlayNetworkMRL = new PlayNetworkMRLCommand();

            _toggleNetworkAppBarCommand =
                new ActionCommand(() => { IsNetworkAppBarShown = !IsNetworkAppBarShown; });

            _showAppBarCommand = new ActionCommand(() => { IsAppBarOpen = true; });
        }

        public VideoLibraryViewModel VideoVM
        {
            get { return _videoVM; }
            set { SetProperty(ref _videoVM, value); }
        }

        public ExternalStorageViewModel ExternalStorageVM
        {
            get { return _externalStorageVM; }
            set { SetProperty(ref _externalStorageVM, value); }
        }

        public VideoLibraryViewModel MusicVM
        {
            get { return _musicVM; }
            set { SetProperty(ref _musicVM, value); }
        }

        public ObservableCollection<VideoLibraryViewModel> DLNAVMs
        {
            get { return _dlnaVMs; }
            set { SetProperty(ref _dlnaVMs, value); }
        }

        public LastViewedViewModel LastViewedVM
        {
            get { return _lastViewedVM; }
            set { SetProperty(ref _lastViewedVM, value); }
        }

        public bool IsNetworkAppBarShown
        {
            get { return _isNetworkAppBarShown; }
            set { SetProperty(ref _isNetworkAppBarShown, value); }
        }

        public PickVideoCommand PickVideo
        {
            get { return _pickVideoCommand; }
            set { SetProperty(ref _pickVideoCommand, value); }
        }

        public ActionCommand ShowAppBarCommand
        {
            get { return _showAppBarCommand; }
            set { SetProperty(ref _showAppBarCommand, value); }
        }

        public ActionCommand ToggleNetworkAppBarCommand
        {
            get { return _toggleNetworkAppBarCommand; }
            set { SetProperty(ref _toggleNetworkAppBarCommand, value); }
        }


        public bool IsAppBarOpen
        {
            get { return _isAppBarOpen; }
            set
            {
                SetProperty(ref _isAppBarOpen, value);
                if (value == false)
                {
                    // hide open network portion of appbar whenever app bar is dissmissed.
                    IsNetworkAppBarShown = false;
                }
            }
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