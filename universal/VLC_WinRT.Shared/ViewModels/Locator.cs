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
using VLC_WinRT.ViewModels.RemovableDevicesVM;
using VLC_WinRT.ViewModels.Settings;
using VLC_WinRT.ViewModels.MusicVM;
using VLC_WinRT.ViewModels.VideoVM;
using VLC_WinRT.Services.RunTime;
using VLC_WinRT.Utils;
using Slide2D;

namespace VLC_WinRT.ViewModels
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
        public static MainVM MainVM => App.Container?.Resolve<MainVM>();

        public static MediaPlaybackViewModel MediaPlaybackViewModel=> App.Container?.Resolve<MediaPlaybackViewModel>(); 

        public static MusicPlayerVM MusicPlayerVM => App.Container?.Resolve<MusicPlayerVM>(); 

        public static MusicLibraryVM MusicLibraryVM => App.Container?.Resolve<MusicLibraryVM>(); 

        public static VideoLibraryVM VideoLibraryVM => App.Container?.Resolve<VideoLibraryVM>(); 

        public static VideoPlayerVM VideoVm => App.Container?.Resolve<VideoPlayerVM>(); 

        public static VLCExplorerViewModel FileExplorerVM => App.Container?.Resolve<VLCExplorerViewModel>(); 

        public static SettingsViewModel SettingsVM => App.Container?.Resolve<SettingsViewModel>(); 

        public static SpecialThanksViewModel SpecialThanksVM => App.Container?.Resolve<SpecialThanksViewModel>(); 

        public static NavigationService NavigationService => App.Container?.Resolve<NavigationService>();

        public static VLCService VLCService => App.Container?.Resolve<VLCService>();

        public static MetroSlideshow Slideshow => App.Container?.Resolve<MetroSlideshow>();
    }
}