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
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.Resources;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Autofac;
using VLC_WINRT.Common;
using VLC_WINRT_APP.Commands.MainPageCommands;
using VLC_WINRT_APP.Common;
using VLC_WINRT_APP.Helpers;
using VLC_WINRT_APP.Commands;
using VLC_WINRT_APP.Model.Search;
using VLC_WINRT_APP.Services.RunTime;
using VLC_WINRT_APP.ViewModels.MusicVM;
using VLC_WINRT_APP.Views.MainPages;
using VLC_WINRT_APP.Views.VariousPages;
#if WINDOWS_PHONE_APP
using AppBar = CustomAppBarDesktop.AppBar;
#else
using AppBar = CustomAppBarDesktop.AppBar;
#endif
using Panel = VLC_WINRT_APP.Model.Panel;
using Windows.UI.Popups;

namespace VLC_WINRT_APP.ViewModels
{
    public class MainVM : BindableBase
    {
        #region private fields

        private ObservableCollection<Panel> _panels = new ObservableCollection<Panel>();
        private ObservableCollection<SearchResult> _searchResults;
        #endregion
        #region private props
        private NetworkListenerService networkListenerService;
        private KeyboardListenerService keyboardListenerService;
        private bool _isInternet;
        private Type _currentPage;
        private GoToPanelCommand _goToPanelCommand;
        private GoToSettingsPageCommand _goToSettingsPageCommand;
        private GoToThanksPageCommand _goToThanksPageCommand;
        private ChangeMainPageMusicViewCommand _changeMainPageMusicViewCommand;
        private AlwaysExecutableCommand _goToSearchPage;
        private ChangeMainPageVideoViewCommand _changeMainPageVideoViewCommand;
        private SearchClickedCommand _searchClickedCommand;
        private string _searchTag = "";
        private bool _preventAppExit = false;
        private string _informationText;

        #endregion
        #region public fields
        public ObservableCollection<SearchResult> SearchResults
        {
            get { return _searchResults; }
            set { SetProperty(ref _searchResults, value); }
        }
        #endregion

        #region public props
        public KeyboardListenerService KeyboardListenerService { get { return keyboardListenerService;} }
        public bool IsInternet
        {
            get { return _isInternet; }
            set
            {
                InformationText = !value ? "No Internet connection" : "";
                SetProperty(ref _isInternet, value);
            }
        }

        public Type CurrentPage
        {
            get { return _currentPage; }
            set { SetProperty(ref _currentPage, value); }
        }

        public GoToPanelCommand GoToPanelCommand
        {
            get { return _goToPanelCommand; }
            set { SetProperty(ref _goToPanelCommand, value); }
        }

        public GoToSettingsPageCommand GoToSettingsPageCommand
        {
            get { return _goToSettingsPageCommand; }
            set { SetProperty(ref _goToSettingsPageCommand, value); }
        }

        public GoToThanksPageCommand GoToThanksPageCommand
        {
            get { return _goToThanksPageCommand; }
            set { SetProperty(ref _goToThanksPageCommand, value); }
        }

        public ChangeMainPageMusicViewCommand ChangeMainPageMusicViewCommand
        {
            get { return _changeMainPageMusicViewCommand; }
            set { SetProperty(ref _changeMainPageMusicViewCommand, value); }
        }
        public AlwaysExecutableCommand GoToSearchPage
        {
            get { return _goToSearchPage; }
            set { _goToSearchPage = value; }
        }
        public ChangeMainPageVideoViewCommand ChangeMainPageVideoViewCommand
        {
            get { return _changeMainPageVideoViewCommand; }
            set { SetProperty(ref _changeMainPageVideoViewCommand, value); }
        }


        public SearchClickedCommand SearchClickedCommand
        {
            get { return _searchClickedCommand; }
            set { SetProperty(ref _searchClickedCommand, value); }
        }
        public string SearchTag
        {
            get { return _searchTag; }
            set
            {
                SetProperty(ref _searchTag, value);
                if (!string.IsNullOrEmpty(value))
                    SearchHelpers.Search();
                else if (SearchResults != null) SearchResults.Clear();
            }
        }

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

        #endregion


        public MainVM()
        {
            keyboardListenerService = App.Container.Resolve<KeyboardListenerService>();
            networkListenerService = App.Container.Resolve<NetworkListenerService>();
            networkListenerService.InternetConnectionChanged += networkListenerService_InternetConnectionChanged;
            _isInternet = NetworkListenerService.IsConnected;

            GoToPanelCommand = new GoToPanelCommand();
            GoToSettingsPageCommand = new GoToSettingsPageCommand();
            GoToThanksPageCommand = new GoToThanksPageCommand();
            ChangeMainPageMusicViewCommand = new ChangeMainPageMusicViewCommand();
            ChangeMainPageVideoViewCommand = new ChangeMainPageVideoViewCommand();
            GoToSearchPage = new ActionCommand(() =>
            {
                App.ApplicationFrame.Navigate(typeof(SearchPage));
            });
            SearchResults = new ObservableCollection<SearchResult>();
            SearchClickedCommand = new SearchClickedCommand();
            // TODO: For Windows 8.1 build, use ResourceLoader.GetForCurrentView(); 
            var resourceLoader = new ResourceLoader();
            Panels.Add(new Panel(resourceLoader.GetString("Home"), 0, App.Current.Resources["HomePath"].ToString(), true));
            Panels.Add(new Panel(resourceLoader.GetString("Videos"), 1, App.Current.Resources["VideoPath"].ToString()));
            Panels.Add(new Panel(resourceLoader.GetString("Music"), 2, App.Current.Resources["MusicPath"].ToString()));
            string removableName = "";
            removableName = resourceLoader.GetString("RemovableStorage");
            Panels.Add(new Panel(removableName, 3, App.Current.Resources["RemovablesPath"].ToString()));
            Initialize();
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
            if (streamFLyout != null)
            {
                streamFLyout.Hide();
            }
        }

        public void OpenStreamFlyout()
        {
            var streamFLyout = App.Current.Resources["PhoneOpenStreamFlyout"] as Flyout;
            if (streamFLyout != null)
            {
                streamFLyout.ShowAt(App.ApplicationFrame);
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
