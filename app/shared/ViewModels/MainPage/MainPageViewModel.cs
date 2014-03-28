/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.Storage.AccessCache;
using VLC_WINRT.Common;
using VLC_WINRT.Model;
using VLC_WINRT.Utility.Commands;
using VLC_WINRT.Utility.Commands.MainPage;
using VLC_WINRT.Utility.Helpers;
using Windows.UI.Xaml.Navigation;
#if WINDOWS_PHONE_APP
using VLC_WINPRT;
#endif
namespace VLC_WINRT.ViewModels.MainPage
{
    public class MainPageViewModel : NavigateableViewModel
    {
        private ObservableCollection<Panel> _panels = new ObservableCollection<Panel>();
        private ObservableCollection<Panel> _secondaryPanels = new ObservableCollection<Panel>();
        private ObservableCollection<VideoLibraryViewModel> _dlnaVMs =
            new ObservableCollection<VideoLibraryViewModel>();
        private ObservableCollection<BackItem> _backers = new ObservableCollection<BackItem>();
#if NETFX_CORE
        private ExternalStorageViewModel _externalStorageVM;
#endif
        private bool _isAppBarOpen;
        private bool _isNetworkAppBarShown;
        private LastViewedViewModel _lastViewedVM;
        private MusicLibraryViewModel _musicLibraryVm;
        private string _networkMRL = string.Empty;
        private PickVideoCommand _pickVideoCommand;
        private PlayNetworkMRLCommand _playNetworkMRL;
        private ActionCommand _showAppBarCommand;
        private ActionCommand _toggleNetworkAppBarCommand;
        private VideoLibraryViewModel _videoVM;
        private SelectDefaultFolderForIndexingVideoCommand _setDefaultFolderForIndexingVideoCommand;
        private GoToPanelCommand _goToPanelCommand;

        public MainPageViewModel()
        {
            LastViewedVM = new LastViewedViewModel();
            PickVideo = new PickVideoCommand();
            PlayNetworkMRL = new PlayNetworkMRLCommand();

            _toggleNetworkAppBarCommand =
                new ActionCommand(() => { IsNetworkAppBarShown = !IsNetworkAppBarShown; });

            _showAppBarCommand = new ActionCommand(() => { IsAppBarOpen = true; });

            // TODO: For Windows 8.1 build, use ResourceLoader.GetForCurrentView(); 

            var resourceLoader = new ResourceLoader();
            Panels.Add(new Panel(resourceLoader.GetString("Home"), 0, 1));
            Panels.Add(new Panel(resourceLoader.GetString("Videos"), 1, 0.4));
            Panels.Add(new Panel(resourceLoader.GetString("Music"), 2, 0.4));

            SecondaryPanels.Add(new Panel(resourceLoader.GetString("ExternalStorage"), 3, 0.4));
            SecondaryPanels.Add(new Panel(resourceLoader.GetString("MediaServers"), 4, 0.4));
            _setDefaultFolderForIndexingVideoCommand = new SelectDefaultFolderForIndexingVideoCommand();
            _goToPanelCommand = new GoToPanelCommand();
        }

        public override async Task OnNavigatedTo(NavigationEventArgs e)
        {
            // Make sure we're only initializing once.
            if (e.NavigationMode == NavigationMode.New)
            {
                await InitVideoVM();
                await _lastViewedVM.Initialize();

                var dlnaFolder = await KnownVLCLocation.MediaServers.GetFoldersAsync();
                var tasks = new List<Task>();
                DLNAVMs.Clear();
                foreach (StorageFolder storageFolder in dlnaFolder)
                {
                    StorageFolder newFolder = storageFolder;
                    var videoLib = new VideoLibraryViewModel(newFolder);
                    tasks.Add(videoLib.GetMedia());
                    DLNAVMs.Add(videoLib);
                }
                await Task.WhenAll(tasks);
            }
        }

        public async Task InitVideoVM()
        {
            // default video folder
            if (App.LocalSettings.ContainsKey("DefaultVideoFolder"))
            {
                StorageFolder customDefaultVideoFolder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(
                        App.LocalSettings["DefaultVideoFolder"].ToString());
                VideoVM = new VideoLibraryViewModel(customDefaultVideoFolder);
            }
            else
            {
                VideoVM = new VideoLibraryViewModel(KnownVLCLocation.VideosLibrary);
            }

            await VideoVM.GetMedia();

            MusicLibraryVm = Locator.MusicLibraryVM;
            await MusicLibraryVm.Initialize();

#if NETFX_CORE
            ExternalStorageVM = new ExternalStorageViewModel();
            await ExternalStorageVM.Initialize();
#endif
        }

        public SelectDefaultFolderForIndexingVideoCommand SetDefaultFolderForIndexingVideoCommand
        {
            get { return _setDefaultFolderForIndexingVideoCommand; }
            set { SetProperty(ref _setDefaultFolderForIndexingVideoCommand, value); }
        }

        public ObservableCollection<BackItem> Backers
        {
            get
            {
                return _backers;
            }
            set { SetProperty(ref _backers, value); }
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

        public ObservableCollection<Panel> SecondaryPanels
        {
            get { return _secondaryPanels; }
            set { SetProperty(ref _secondaryPanels, value); }
        }

        public VideoLibraryViewModel VideoVM
        {
            get { return _videoVM; }
            set { SetProperty(ref _videoVM, value); }
        }

#if NETFX_CORE
        public ExternalStorageViewModel ExternalStorageVM
        {
            get { return _externalStorageVM; }
            set { SetProperty(ref _externalStorageVM, value); }
        }
#endif

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
