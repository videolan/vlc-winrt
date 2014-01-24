using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.Storage;
using VLC_WINRT.Common;
using VLC_WINRT.Model;
using VLC_WINRT.Utility.Commands;
using VLC_WINRT.Utility.Commands.MainPage;

namespace VLC_WINRT.ViewModels.MainPage
{
    public class MainPageViewModel : NavigateableViewModel
    {
        private ObservableCollection<Panel> _panels = new ObservableCollection<Panel>();
        private ObservableCollection<VideoLibraryViewModel> _dlnaVMs =
            new ObservableCollection<VideoLibraryViewModel>();

        private ExternalStorageViewModel _externalStorageVM;

        private bool _isAppBarOpen;
        private bool _isNetworkAppBarShown;
        private LastViewedViewModel _lastViewedVM;
        private MusicLibraryViewModel _musicLibraryVm;
        private VideoLibraryViewModel _musicVM;
        private string _networkMRL = string.Empty;
        private PickVideoCommand _pickVideoCommand;
        private PlayNetworkMRLCommand _playNetworkMRL;
        private ActionCommand _showAppBarCommand;
        private ActionCommand _toggleNetworkAppBarCommand;
        private VideoLibraryViewModel _videoVM;
        private GoToPanelCommand _goToPanelCommand;

        public MainPageViewModel()
        {
            VideoVM = new VideoLibraryViewModel(KnownVLCLocation.VideosLibrary);
            MusicLibraryVm = Locator.MusicLibraryVM;
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

            Panels.Add(new Panel("home", 0, 1));
            Panels.Add(new Panel("videos", 1, 0.4));
            Panels.Add(new Panel("music", 2, 0.4));
            Panels.Add(new Panel("removable storage", 3, 0.4));
            Panels.Add(new Panel("dlna", 4, 0.4));
            _goToPanelCommand = new GoToPanelCommand();
        }

        public GoToPanelCommand GoToPanel
        {
            get { return _goToPanelCommand; }
            set { SetProperty(ref _goToPanelCommand, value); }
        }

        public ObservableCollection<Panel> Panels
        {
            get { return _panels; }
            set
            {
                SetProperty(ref _panels, value);
            }
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
        
        public MusicLibraryViewModel MusicLibraryVm
        {
            get { return _musicLibraryVm; }
            set { SetProperty(ref _musicLibraryVm, value); }
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