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
using System;
using VLC_WINRT.Common;
using VLC_WINRT.Utility.Services.Interface;
using VLC_WINRT.Utility.Services.RunTime;
using VLC_WINRT.ViewModels.MainPage;
using VLC_WINRT.ViewModels.MainPage.PlayMusic;
using VLC_WINRT.ViewModels.PlayVideo;

namespace VLC_WINRT.ViewModels
{
    /// <summary>
    ///     This class contains static references to all the view models in the
    ///     application and provides an entry point for the bindings.
    /// </summary>
    public class Locator
    {
        public static IContainer Container;

        public Locator()
        {
            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                Container = AutoFacConfiguration.Configure();
            }
        }

        /// <summary>
        ///     Initializes a new instance of the Locator class.
        /// </summary>
        public static MusicPlayerViewModel MusicPlayerVM
        {
            get {
                if (Container == null)
                {
                    throw new Exception("Test!");
                }
                return Container.Resolve<MusicPlayerViewModel>(); 
            }
        }
        public static MusicLibraryViewModel MusicLibraryVM
        {
            get { return Container.Resolve<MusicLibraryViewModel>(); }
        }

        public static PlayVideoViewModel PlayVideoVM
        {
            get { return Container.Resolve<PlayVideoViewModel>(); }
        }

        public static MainPageViewModel MainPageVM
        {
            get { return Container.Resolve<MainPageViewModel>(); }
        }
    }
}
