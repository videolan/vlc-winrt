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
using VLC_WINRT.Common;
using VLC_WINRT.Model;
using VLC_WINRT.Utility.Commands;
using VLC_WINRT.Utility.Commands.MainPage;
#if WINDOWS_PHONE_APP
using VLC_WINPRT;
#endif
using VLC_WINRT.ViewModels;
using VLC_WINRT.ViewModels.MainPage;
using VLC_WINRT.ViewModels.MainPage.VlcExplorer;
using VLC_WINRT_APP;
using VLC_WINRT_APP.Views.MainPages;

namespace VLC_WINRT_APP.ViewModels
{
    public class MainVM : BindableBase
    {
        #region private fields
        private ObservableCollection<Panel> _panels = new ObservableCollection<Panel>();
        private ObservableCollection<Panel> _secondaryPanels = new ObservableCollection<Panel>();
        private ObservableCollection<FileExplorerViewModel> _dlnaVMs = new ObservableCollection<FileExplorerViewModel>();
        #endregion

        #region private props
        private bool _isAppBarOpen;
        private bool _isNetworkAppBarShown;
        private string _networkMRL = string.Empty;
        private PickVideoCommand _pickVideoCommand;
        private PlayNetworkMRLCommand _playNetworkMRL;
        private ActionCommand _showAppBarCommand;
        private ActionCommand _toggleNetworkAppBarCommand;
        private GoToPanelCommand _goToPanelCommand;
        #endregion
        #region public fields
        #endregion
        #region public props
        public GoToPanelCommand GoToPanel
        {
            get { return _goToPanelCommand; }
            set { SetProperty(ref _goToPanelCommand, value); }
        }
        #endregion

#if NETFX_CORE
        private ExternalStorageViewModel _externalStorageVM;
#endif

        public MainVM()
        {
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
            _goToPanelCommand = new GoToPanelCommand();
            Initialize();
        }

        public async Task Initialize()
        {
            await Locator.SettingsVM.PopulateCustomFolders();
            await InitVideoVM();
            await InitMusicM();
        }

        public async Task InitVideoVM()
        {
            await Locator.VideoLibraryVM.GetMedia();
        }

        public async Task InitMusicM()
        {
            await Locator.MusicLibraryVM.Initialize();
        }

        public async Task InitRemovableStorageVM()
        {
#if NETFX_CORE
            if (ExternalStorageVM != null) return;

            ExternalStorageVM = new ExternalStorageViewModel();
            await ExternalStorageVM.Initialize();
#endif
        }

        public async Task InitDLNAVM()
        {
            if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {
                var dlnaFolder = await KnownVLCLocation.MediaServers.GetFoldersAsync();
                var tasks = new List<Task>();
                DLNAVMs.Clear();
                foreach (StorageFolder storageFolder in dlnaFolder)
                {
                    StorageFolder newFolder = storageFolder;
                    var videoLib = new FileExplorerViewModel(newFolder);
                    tasks.Add(videoLib.GetFiles());
                    DLNAVMs.Add(videoLib);
                }
                await Task.WhenAll(tasks);
            }
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

#if NETFX_CORE
        public ExternalStorageViewModel ExternalStorageVM
        {
            get { return _externalStorageVM; }
            set { SetProperty(ref _externalStorageVM, value); }
        }
#endif

        public ObservableCollection<FileExplorerViewModel> DLNAVMs
        {
            get { return _dlnaVMs; }
            set { SetProperty(ref _dlnaVMs, value); }
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
