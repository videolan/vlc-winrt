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
using VLC_WINRT_APP.Model;
using VLC_WINRT_APP.Commands;
using VLC_WINRT_APP.ViewModels.Others;
using VLC_WINRT_APP.ViewModels.Others.VlcExplorer;

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
        private PickVideoCommand _pickVideoCommand;
        private PlayNetworkMRLCommand _playNetworkMRL;
        #endregion
        #region public fields
        #endregion
        #region public props
        #endregion

#if WINDOWS_APP
        private ExternalStorageViewModel _externalStorageVM;
#endif

        public MainVM()
        {
            PickVideo = new PickVideoCommand();
            PlayNetworkMRL = new PlayNetworkMRLCommand();

            // TODO: For Windows 8.1 build, use ResourceLoader.GetForCurrentView(); 

            var resourceLoader = new ResourceLoader();
            Panels.Add(new Panel(resourceLoader.GetString("Home"), 0, 1));
            Panels.Add(new Panel(resourceLoader.GetString("Videos"), 1, 0.4));
            Panels.Add(new Panel(resourceLoader.GetString("Music"), 2, 0.4));

            //SecondaryPanels.Add(new Panel(resourceLoader.GetString("ExternalStorage"), 3, 0.4));
            //SecondaryPanels.Add(new Panel(resourceLoader.GetString("MediaServers"), 4, 0.4));
            
            Initialize();
        }

        public async Task Initialize()
        {
            await Locator.SettingsVM.PopulateCustomFolders();
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

#if WINDOWS_APP
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

        public PickVideoCommand PickVideo
        {
            get { return _pickVideoCommand; }
            set { SetProperty(ref _pickVideoCommand, value); }
        }


        public PlayNetworkMRLCommand PlayNetworkMRL
        {
            get { return _playNetworkMRL; }
            set { SetProperty(ref _playNetworkMRL, value); }
        }
    }
}
