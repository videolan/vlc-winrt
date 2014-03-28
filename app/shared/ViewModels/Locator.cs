/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

/*
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"
*/

using Autofac;
using VLC_WINRT.Common;
using VLC_WINRT.ViewModels.MainPage;
using VLC_WINRT.ViewModels.MainPage.PlayMusic;
using VLC_WINRT.ViewModels.PlayVideo;
#if WINDOWS_PHONE_APP
using VLC_WINPRT;
#endif

namespace VLC_WINRT.ViewModels
{
    /// <summary>
    ///     This class contains static references to all the view models in the
    ///     application and provides an entry point for the bindings.
    /// </summary>
    public class Locator
    {
        public Locator()
        {
            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                App.Container = AutoFacConfiguration.Configure();
            }
        }

        /// <summary>
        ///     Initializes a new instance of the Locator class.
        /// </summary>
        public static MusicPlayerViewModel MusicPlayerVM
        {
            get { return App.Container.Resolve<MusicPlayerViewModel>(); }
        }
        public static MusicLibraryViewModel MusicLibraryVM
        {
            get { return App.Container.Resolve<MusicLibraryViewModel>(); }
        }

        public static PlayVideoViewModel PlayVideoVM
        {
            get { return App.Container.Resolve<PlayVideoViewModel>(); }
        }

        public static MainPageViewModel MainPageVM
        {
            get { return App.Container.Resolve<MainPageViewModel>(); }
        }

        public static SettingsViewModel SettingsVM
        {
            get { return App.Container.Resolve<SettingsViewModel>(); }
        }
    }
}
