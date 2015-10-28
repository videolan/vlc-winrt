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
using VLC_WinRT.ViewModels.Others;

namespace VLC_WinRT.ViewModels
{
    /// <summary>
    ///     This class contains static references to all the view models in the
    ///     application and provides an entry point for the bindings.
    /// </summary>
    public class Locator
    {
        private static MainVM _main;
        private static MediaPlaybackViewModel _mediaPlayback;
        private static MusicPlayerVM _musicPlayer;
        private static MusicLibraryVM _musicLibrary;
        private static VideoLibraryVM _videoLibrary;
        private static VideoPlayerVM _videoPlayer;
        private static VLCExplorerViewModel _vlcExplorer;
        private static SettingsViewModel _settings;
        private static SearchViewModel _search;
        private static SpecialThanksViewModel _specialThanks;

        private static NavigationService _navigationService;
        private static VLCService _vlcService;
        private static MFService _mfService;
#if WINDOWS_PHONE_APP
        private static BGPlayerService _bgPlayerService;
#endif
        private static MusicMetaService _musicMetaService;
        private static MetroSlideshow _metroSlideshow;

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
        public static MainVM MainVM => _main ?? (_main = App.Container.Resolve<MainVM>());

        public static MediaPlaybackViewModel MediaPlaybackViewModel=> _mediaPlayback ?? (_mediaPlayback = App.Container.Resolve<MediaPlaybackViewModel>()); 

        public static MusicPlayerVM MusicPlayerVM => _musicPlayer ?? (_musicPlayer = App.Container.Resolve<MusicPlayerVM>()); 

        public static MusicLibraryVM MusicLibraryVM => _musicLibrary ?? (_musicLibrary = App.Container.Resolve<MusicLibraryVM>()); 

        public static VideoLibraryVM VideoLibraryVM => _videoLibrary ?? (_videoLibrary = App.Container.Resolve<VideoLibraryVM>()); 

        public static VideoPlayerVM VideoPlayerVm => _videoPlayer ?? (_videoPlayer = App.Container.Resolve<VideoPlayerVM>()); 

        public static VLCExplorerViewModel FileExplorerVM => _vlcExplorer ?? (_vlcExplorer = App.Container.Resolve<VLCExplorerViewModel>());

        public static SettingsViewModel SettingsVM => _settings ?? (_settings = App.Container.Resolve<SettingsViewModel>());
        public static SearchViewModel SearchVM => _search ?? (_search = App.Container.Resolve<SearchViewModel>());

        public static SpecialThanksViewModel SpecialThanksVM => _specialThanks ?? (_specialThanks = App.Container.Resolve<SpecialThanksViewModel>()); 

        public static NavigationService NavigationService => _navigationService ?? (_navigationService = App.Container.Resolve<NavigationService>());
        public static VLCService VLCService => _vlcService ?? (_vlcService = App.Container.Resolve<VLCService>());
        public static MFService MFService => _mfService ?? (_mfService = App.Container.Resolve<MFService>());
#if WINDOWS_PHONE_APP
        public static BGPlayerService BGPlayerService => _bgPlayerService ?? (_bgPlayerService = App.Container.Resolve<BGPlayerService>());
#endif
        public static MusicMetaService MusicMetaService => _musicMetaService ?? (_musicMetaService = App.Container.Resolve<MusicMetaService>());
        public static MetroSlideshow Slideshow => _metroSlideshow ?? (_metroSlideshow = App.Container.Resolve<MetroSlideshow>());
    }
}