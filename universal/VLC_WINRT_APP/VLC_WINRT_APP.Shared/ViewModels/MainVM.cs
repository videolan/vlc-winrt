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
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.Resources;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using VLC_WINRT.Common;
using VLC_WINRT_APP.Commands.MainPageCommands;
using VLC_WINRT_APP.Common;
using VLC_WINRT_APP.Helpers;
using VLC_WINRT_APP.Commands;
using VLC_WINRT_APP.Model.Search;
using VLC_WINRT_APP.Views.MainPages;
using VLC_WINRT_APP.Views.VariousPages;
#if WINDOWS_PHONE_APP
using AppBar = CustomAppBar.AppBar;
#endif
using Panel = VLC_WINRT_APP.Model.Panel;

namespace VLC_WINRT_APP.ViewModels
{
    public class MainVM : BindableBase
    {
        #region private fields
        private ObservableCollection<Panel> _panels = new ObservableCollection<Panel>();
#if WINDOWS_PHONE_APP
        private ObservableCollection<SearchResult> _searchResults;
#endif
        #endregion
        #region private props
        private bool _isInternet;
        private Type _currentPage;
        private GoToPanelCommand _goToPanelCommand;
        private GoToSettingsPageCommand _goToSettingsPageCommand;
        private GoToThanksPageCommand _goToThanksPageCommand;
        private GoToAboutPageCommand _goToAboutPageCommand;
#if WINDOWS_PHONE_APP
        private ChangeMainPageMusicViewCommand _changeMainPageMusicViewCommand;
        private ChangeMainPageVideoViewCommand _changeMainPageVideoViewCommand;
        private AlwaysExecutableCommand _goToSearchPage;
        private SearchClickedCommand _searchClickedCommand;
        private string _searchTag;
#endif
        #endregion
        #region public fields
#if WINDOWS_PHONE_APP
        public AppBar CommandBar
        {
            get
            {
                if (App.RootPage != null && App.RootPage.CommandBar != null)
                    return App.RootPage.CommandBar;
                return null;
            }
        }
        public List<ICommandBarElement> AppBarElements
        {
            get { if (CommandBar != null) return CommandBar.PrimaryCommands as List<ICommandBarElement>; return null; }
            set { if (CommandBar != null) CommandBar.PrimaryCommands = value; }
        }
        public IList<AppBarButton> SecondaryAppBarElements
        {
            get { if (CommandBar != null) return CommandBar.SecondaryCommands as List<AppBarButton>; return null; }
            set { if (CommandBar != null) CommandBar.SecondaryCommands = value; }
        }

        public ObservableCollection<SearchResult> SearchResults
        {
            get { return _searchResults; }
            set { SetProperty(ref _searchResults, value); }
        }
#endif
        #endregion
        #region public props

        public bool IsInternet
        {
            get { return _isInternet; }
            set { SetProperty(ref _isInternet, value); }
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

        public GoToAboutPageCommand GoToAboutPageCommand
        {
            get { return _goToAboutPageCommand; }
            set { SetProperty(ref _goToAboutPageCommand, value); }
        }
#if WINDOWS_PHONE_APP
        public ChangeMainPageMusicViewCommand ChangeMainPageMusicViewCommand
        {
            get { return _changeMainPageMusicViewCommand; }
            set { SetProperty(ref _changeMainPageMusicViewCommand, value); }
        }
        public ChangeMainPageVideoViewCommand ChangeMainPageVideoViewCommand
        {
            get { return _changeMainPageVideoViewCommand; }
            set { SetProperty(ref _changeMainPageVideoViewCommand, value); }
        }

        public AlwaysExecutableCommand GoToSearchPage
        {
            get { return _goToSearchPage; }
            set { _goToSearchPage = value; }
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
#endif
        #endregion


        public MainVM()
        {
            GoToPanelCommand = new GoToPanelCommand();
            GoToSettingsPageCommand = new GoToSettingsPageCommand();
            GoToThanksPageCommand = new GoToThanksPageCommand();
            GoToAboutPageCommand = new GoToAboutPageCommand();
#if WINDOWS_PHONE_APP
            GoToSearchPage = new ActionCommand(() =>
            {
                App.ApplicationFrame.Navigate(typeof(SearchPage));
            });
            ChangeMainPageMusicViewCommand = new ChangeMainPageMusicViewCommand();
            ChangeMainPageVideoViewCommand = new ChangeMainPageVideoViewCommand();
            SearchResults = new ObservableCollection<SearchResult>();
            SearchClickedCommand = new SearchClickedCommand();
#endif
            // TODO: For Windows 8.1 build, use ResourceLoader.GetForCurrentView(); 
#if WINDOWS_APP
            var resourceLoader = new ResourceLoader();
            Panels.Add(new Panel(resourceLoader.GetString("Home"), 0, 1, App.Current.Resources["HomePath"].ToString(), true));
            Panels.Add(new Panel(resourceLoader.GetString("Videos"), 1, 0.4, App.Current.Resources["VideoPath"].ToString()));
            Panels.Add(new Panel(resourceLoader.GetString("Music"), 2, 0.4, App.Current.Resources["MusicPath"].ToString()));
            string removableName = "";
            removableName = resourceLoader.GetString("RemovableStorage");
            Panels.Add(new Panel(removableName, 3, 0.4, App.Current.Resources["RemovablesPath"].ToString()));
            //removableName = resourceLoader.GetString("SdCard");
            Panels.Add(new Panel(resourceLoader.GetString("More"), 3, 0.4, App.Current.Resources["HamburgerPath"].ToString()));
            Panels.Add(new Panel(resourceLoader.GetString("MediaServers"), 4, 0.4, App.Current.Resources["ServerPath"].ToString()));
#endif
            Initialize();
        }

        void Initialize()
        {
            if (ApplicationSettingsHelper.ReadSettingsValue("ContinueVideoPlaybackInBackground") == null)
                ApplicationSettingsHelper.SaveSettingsValue("ContinueVideoPlaybackInBackground", true);
        }

#if WINDOWS_PHONE_APP
        public void CloseStreamFlyout()
        {
            var streamFLyout = App.Current.Resources["PhoneOpenStreamFlyout"] as Flyout;
            if (streamFLyout != null)
            {
                streamFLyout.Hide();
            }
        }
#endif
        public ObservableCollection<Panel> Panels
        {
            get { return _panels; }
            set
            {
                SetProperty(ref _panels, value);
            }
        }
    }
}
