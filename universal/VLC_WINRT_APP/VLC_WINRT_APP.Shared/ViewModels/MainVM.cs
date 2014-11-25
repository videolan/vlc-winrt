/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Foundation.Collections;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using VLC_WINRT.Common;
using VLC_WINRT_APP.Commands.MainPageCommands;
using VLC_WINRT_APP.Common;
using VLC_WINRT_APP.Helpers;
using VLC_WINRT_APP.Commands;
using VLC_WINRT_APP.Views.MainPages;
using Panel = VLC_WINRT_APP.Model.Panel;

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
#if WINDOWS_PHONE_APP
        private ChangeMainPageMusicViewCommand _changeMainPageMusicViewCommand;
#endif
        #endregion
        #region public fields
#if WINDOWS_PHONE_APP
        public IObservableVector<ICommandBarElement> AppBarElements
        {
            get
            {
                if (App.ApplicationFrame != null && App.ApplicationFrame.CurrentSourcePageType == typeof(MainPageHome))
                {
                    return ((App.ApplicationFrame.Content as MainPageHome).BottomAppBar as CommandBar).PrimaryCommands;
                }
                return null;
            }
        }
#endif
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
        public ChangeMainPageMusicViewCommand ChangeMainPageMusicViewCommand
        {
            get { return _changeMainPageMusicViewCommand; }
            set { SetProperty(ref _changeMainPageMusicViewCommand, value); }
        }
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
#if WINDOWS_PHONE_APP
            OpenSidebarCommand = new ActionCommand(() =>
            {
                App.RootPage.PanelsView.ShowSidebar();
            });
            ChangeMainPageMusicViewCommand = new ChangeMainPageMusicViewCommand();
#endif
            // TODO: For Windows 8.1 build, use ResourceLoader.GetForCurrentView(); 

            var resourceLoader = new ResourceLoader();
            Panels.Add(new Panel(resourceLoader.GetString("Home"), 0, 1, App.Current.Resources["HomePath"].ToString(), true));
            Panels.Add(new Panel(resourceLoader.GetString("Videos"), 1, 0.4, App.Current.Resources["VideoPath"].ToString()));
            Panels.Add(new Panel(resourceLoader.GetString("Music"), 2, 0.4, App.Current.Resources["MusicPath"].ToString()));
            string removableName = "";
#if WINDOWS_APP
            removableName = resourceLoader.GetString("RemovableStorage");
            Panels.Add(new Panel(removableName, 3, 0.4, App.Current.Resources["RemovablesPath"].ToString()));
#else
            //removableName = resourceLoader.GetString("SdCard");
            Panels.Add(new Panel("more", 3, 0.4, App.Current.Resources["HamburgerPath"].ToString()));
#endif
#if WINDOWS_APP
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
        public void UpdateAppBar(int index)
        {
            if (AppBarElements == null) return;
            AppBarElements.Clear();
            switch (index)
            {
                case 0:
                    AppBarElements.Add(new AppBarButton()
                    {
                        Label = "search",
                        Icon = new SymbolIcon(Symbol.Find),
                        Command = OpenSidebarCommand
                    });
                    AppBarElements.Add(new AppBarButton()
                    {
                        Label = "open file",
                        Icon = new SymbolIcon(Symbol.OpenFile),
                        Command = Locator.VideoLibraryVM.PickVideo
                    });
                    AppBarElements.Add(new AppBarButton()
                    {
                        Label = "open stream",
                        Icon = new SymbolIcon(Symbol.World),
                        Flyout = App.Current.Resources["PhoneOpenStreamFlyout"] as Flyout,
                    });
                    break;
                case 1:
                    break;
                case 2:
                    AppBarElements.Add(new AppBarButton()
                    {
                        Label = "view all",
                        Icon = new SymbolIcon(Symbol.ViewAll),
                        Flyout = App.Current.Resources["PhoneChangeMusicViewFlyout"] as MenuFlyout,
                    });
                    AppBarElements.Add(new AppBarButton()
                    {
                        Label = "random",
                        Icon = new SymbolIcon(Symbol.Shuffle),
                        Command = Locator.MusicLibraryVM.PlayAllRandomCommand
                    });
                    break;
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
