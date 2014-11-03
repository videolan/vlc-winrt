/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Media;
using VLC_WINRT.Common;
using VLC_WINRT_APP.Commands.MainPageCommands;
using VLC_WINRT_APP.Common;
using VLC_WINRT_APP.Helpers;
using VLC_WINRT_APP.Model;
using VLC_WINRT_APP.Commands;

namespace VLC_WINRT_APP.ViewModels
{
    public class MainVM : BindableBase
    {
        #region private fields
        private ObservableCollection<Panel> _panels = new ObservableCollection<Panel>();
        #endregion
        #region private props
        private GoToPanelCommand _goToPanelCommand;
        private GoToSettingsPageCommand _goToSettingsPageCommand;
        private GoToThanksPageCommand _goToThanksPageCommand;
        private GoToAboutPageCommand _goToAboutPageCommand;
        private AlwaysExecutableCommand _openSidebarCommand;

        #endregion
        #region public fields
        #endregion
        #region public props

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
        public AlwaysExecutableCommand OpenSidebarCommand
        {
            get { return _openSidebarCommand; }
            set { SetProperty(ref _openSidebarCommand, value); }
        }
#endif
        #endregion


        public MainVM()
        {
            GoToPanelCommand = new GoToPanelCommand();
            GoToSettingsPageCommand = new GoToSettingsPageCommand();
            GoToThanksPageCommand = new GoToThanksPageCommand();
            GoToAboutPageCommand = new GoToAboutPageCommand();
            OpenSidebarCommand = new ActionCommand(() =>
            {
                App.RootPage.PanelsView.ShowSidebar();
            });
            // TODO: For Windows 8.1 build, use ResourceLoader.GetForCurrentView(); 

            var resourceLoader = new ResourceLoader();
            Panels.Add(new Panel(resourceLoader.GetString("Home"), 0, 1, App.Current.Resources["HomePath"].ToString(), true));
            Panels.Add(new Panel(resourceLoader.GetString("Videos"), 1, 0.4, App.Current.Resources["VideoPath"].ToString()));
            Panels.Add(new Panel(resourceLoader.GetString("Music"), 2, 0.4, App.Current.Resources["MusicPath"].ToString()));
            string removableName = "";
#if WINDOWS_APP
            removableName = resourceLoader.GetString("RemovableStorage");
#else
            removableName = resourceLoader.GetString("SdCard");
#endif
            Panels.Add(new Panel(removableName, 3, 0.4, App.Current.Resources["RemovablesPath"].ToString()));
#if WINDOWS_APP
            Panels.Add(new Panel(resourceLoader.GetString("MediaServers"), 4, 0.4, App.Current.Resources["ServerPath"].ToString()));
#endif
            Initialize();
        }

        public async Task Initialize()
        {
            if (ApplicationSettingsHelper.ReadSettingsValue("ContinueVideoPlaybackInBackground") == null)
                ApplicationSettingsHelper.SaveSettingsValue("ContinueVideoPlaybackInBackground", true);
        }

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
