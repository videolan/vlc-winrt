/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System;
using System.Collections.ObjectModel;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Resources;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Autofac;
using VLC_WinRT.Commands.MainPageCommands;
using VLC_WinRT.Helpers;
using VLC_WinRT.Commands;
using VLC_WinRT.Model.Search;
using VLC_WinRT.Services.RunTime;
using Panel = VLC_WinRT.Model.Panel;
using Windows.UI.Popups;
using VLC_WinRT.Model;
using libVLCX;
using VLC_WinRT.Utils;

namespace VLC_WinRT.ViewModels
{
    public class MainVM : BindableBase
    {
        #region private fields
        private ObservableCollection<Panel> _panels = new ObservableCollection<Panel>();
        #endregion
        #region private props
        private NetworkListenerService networkListenerService;
        private KeyboardListenerService keyboardListenerService;
        private bool _isInternet;
        private string _searchTag = "";
        private bool _preventAppExit = false;
        private string _informationText;
        private bool _isBackground = false;

        // Navigation props
        private VLCPage currentPage;
        private bool canGoBack;
        #endregion

        #region public props
        public VLCPage CurrentPage
        {
            get { return currentPage; }
            set { SetProperty(ref currentPage, value); }
        }

        public bool CanGoBack
        {
            get
            {
                return canGoBack;
            }
            set { SetProperty(ref canGoBack, value); }
        }

        public KeyboardListenerService KeyboardListenerService { get { return keyboardListenerService; } }
        public bool IsInternet
        {
            get { return _isInternet; }
            set
            {
                InformationText = !value ? "No Internet connection" : "";
                SetProperty(ref _isInternet, value);
            }
        }

        public GoToPanelCommand GoToPanelCommand { get; } = new GoToPanelCommand();

        public GoToSettingsPageCommand GoToSettingsPageCommand { get; } = new GoToSettingsPageCommand();

        public GoToThanksPageCommand GoToThanksPageCommand { get; } = new GoToThanksPageCommand();

        public ChangeMainPageMusicViewCommand ChangeMainPageMusicViewCommand { get; } = new ChangeMainPageMusicViewCommand();
        
        public ChangeMainPageVideoViewCommand ChangeMainPageVideoViewCommand { get; } = new ChangeMainPageVideoViewCommand();

        public SearchClickedCommand SearchClickedCommand { get; }= new SearchClickedCommand();
        
        public bool PreventAppExit
        {
            get { return _preventAppExit; }
            set { SetProperty(ref _preventAppExit, value); }
        }

        public string InformationText
        {
            get { return _informationText; }
            set { SetProperty(ref _informationText, value); }
        }

        public bool IsBackground
        {
            get { return _isBackground; }
            private set { SetProperty(ref _isBackground, value); }
        }

        #endregion


        public MainVM()
        {
            keyboardListenerService = App.Container.Resolve<KeyboardListenerService>();
            networkListenerService = App.Container.Resolve<NetworkListenerService>();
            networkListenerService.InternetConnectionChanged += networkListenerService_InternetConnectionChanged;
            _isInternet = NetworkListenerService.IsConnected;
            
            Panels.Add(new Panel(Strings.Home, 0, App.Current.Resources["HomeSymbol"].ToString(), true));
            Panels.Add(new Panel(Strings.Videos, 1, App.Current.Resources["VideoSymbol"].ToString()));
            Panels.Add(new Panel(Strings.Music, 2, App.Current.Resources["MusicSymbol"].ToString()));
            Panels.Add(new Panel(Strings.RemovableStorage, 3, App.Current.Resources["FolderSymbol"].ToString()));
            Initialize();

            CoreWindow.GetForCurrentThread().Activated += ApplicationState_Activated;
            Locator.NavigationService.ViewNavigated += (sender, page) =>
            {
                CurrentPage = page;
                CanGoBack = Locator.NavigationService.CanGoBack();
            };
        }

        private void ApplicationState_Activated(object sender, WindowActivatedEventArgs e)
        {
            if (e.WindowActivationState == CoreWindowActivationState.Deactivated)
            {
                IsBackground = true;
                if (Locator.MediaPlaybackViewModel.CurrentMedia == null) return;
                if (!Locator.MediaPlaybackViewModel.IsPlaying) return;
                // If we're playing a video, just pause.
                if (Locator.MediaPlaybackViewModel.PlayingType == PlayingType.Video)
                {
                    // TODO: Route Video Player calls through Media Service
                    if (!(bool)ApplicationSettingsHelper.ReadSettingsValue("ContinueVideoPlaybackInBackground"))
                        Locator.MediaPlaybackViewModel._mediaService.Pause();
                }
            }
            else
            {
                IsBackground = false;
                if (Locator.MediaPlaybackViewModel.CurrentMedia == null) return;
                if (Locator.MediaPlaybackViewModel.PlayingType != PlayingType.Video)
                    return;
                if (Locator.MediaPlaybackViewModel.MediaState == MediaState.Paused)
                    Locator.MediaPlaybackViewModel._mediaService.Pause();
            }
        }

        async void networkListenerService_InternetConnectionChanged(object sender, Model.Events.InternetConnectionChangedEventArgs e)
        {
            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                IsInternet = e.IsConnected;
                if (!IsInternet)
                {
                    if (Locator.MediaPlaybackViewModel.IsPlaying == true && Locator.MediaPlaybackViewModel.IsStream)
                    {
                        var lostStreamDialog = new MessageDialog("Connection to the server was stopped, please check your Internet connection");
                        await lostStreamDialog.ShowAsync();
                    }
                }
            });
        }

        void Initialize()
        {
            if (ApplicationSettingsHelper.ReadSettingsValue("ContinueVideoPlaybackInBackground") == null)
                ApplicationSettingsHelper.SaveSettingsValue("ContinueVideoPlaybackInBackground", true);
        }

        public void CloseStreamFlyout()
        {
            var streamFLyout = App.Current.Resources["PhoneOpenStreamFlyout"] as Flyout;
            streamFLyout?.Hide();
        }

        public void OpenStreamFlyout()
        {
            var streamFLyout = App.Current.Resources["PhoneOpenStreamFlyout"] as Flyout;
            streamFLyout?.ShowAt(App.ApplicationFrame);
        }

        public ObservableCollection<Panel> Panels
        {
            get { return _panels; }
            set
            {
                SetProperty(ref _panels, value);
            }
        }

        private bool NeedsToDrop()
        {
            Package thisPackage = Package.Current;
            PackageVersion version = thisPackage.Id.Version;
            string appVersion = string.Format("{0}.{1}.{2}.{3}",
                version.Major, version.Minor, version.Build, version.Revision);
            if (ApplicationSettingsHelper.Contains("CurrentVersion") && ApplicationSettingsHelper.ReadSettingsValue("CurrentVersion").ToString() == appVersion)
            {
                return false;
            }
            else
            {
                ApplicationSettingsHelper.SaveSettingsValue("CurrentVersion", appVersion);
                return true;
            }
        }

        public void DropTablesIfNeeded()
        {
            if (!NeedsToDrop()) return;
            Locator.MusicLibraryVM.TrackCollectionRepository.Drop();
            Locator.MusicLibraryVM.TracklistItemRepository.Drop();
            Locator.MusicLibraryVM._albumDataRepository.Drop();
            Locator.MusicLibraryVM._artistDataRepository.Drop();
            Locator.MusicLibraryVM._trackDataRepository.Drop();
            Locator.VideoLibraryVM.VideoRepository.Drop();
            Locator.MusicLibraryVM.TrackCollectionRepository.Initialize();
            Locator.MusicLibraryVM.TracklistItemRepository.Initialize();
            Locator.MusicLibraryVM._albumDataRepository.Initialize();
            Locator.MusicLibraryVM._artistDataRepository.Initialize();
            Locator.MusicLibraryVM._trackDataRepository.Initialize();
            Locator.VideoLibraryVM.VideoRepository.Initialize();
            LogHelper.SignalUpdate();
        }
    }
}
