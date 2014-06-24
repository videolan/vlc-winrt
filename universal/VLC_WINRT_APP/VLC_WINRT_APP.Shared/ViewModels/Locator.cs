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
using VLC_WINRT.ViewModels;
using VLC_WINRT.ViewModels.MainPage;
using VLC_WINRT.ViewModels.PlayVideo;
#if WINDOWS_PHONE_APP
using VLC_WINPRT;
#endif
using VLC_WINRT_APP.Common;
using VLC_WINRT_APP.ViewModels.Settings;
using VLC_WINRT_APP.ViewModels.MusicVM;

namespace VLC_WINRT_APP.ViewModels
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
        public static MusicPlayerVM MusicPlayerVM
        {
            get { return App.Container.Resolve<MusicPlayerVM>(); }
        }
        public static MusicLibraryVM MusicLibraryVM
        {
            get { return App.Container.Resolve<MusicLibraryVM>(); }
        }

        public static VideoLibraryViewModel VideoLibraryVM
        {
            get { return App.Container.Resolve<VideoLibraryViewModel>(); }
        }

        public static PlayVideoViewModel PlayVideoVM
        {
            get { return App.Container.Resolve<PlayVideoViewModel>(); }
        }

        public static MainVM MainVM
        {
            get { return App.Container.Resolve<MainVM>(); }
        }

        public static SettingsViewModel SettingsVM
        {
            get { return App.Container.Resolve<SettingsViewModel>(); }
        }
          public static SpecialThanksViewModel SpecialThanksVM
        {
            get { return App.Container.Resolve<SpecialThanksViewModel>(); }
        }

    }
}
