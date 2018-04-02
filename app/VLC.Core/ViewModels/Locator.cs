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
using VLC.ViewModels.RemovableDevicesVM;
using VLC.ViewModels.Settings;
using VLC.ViewModels.MusicVM;
using VLC.ViewModels.VideoVM;
using VLC.Services.RunTime;
using VLC.Utils;
using Slide2D;
using VLC.ViewModels.Others;
using VLC.Model.Library;

namespace VLC.ViewModels
{
    /// <summary>
    ///     This class contains static references to all the view models in the
    ///     application and provides an entry point for the bindings.
    /// </summary>
    public class Locator
    {
        private static MediaLibrary _mediaLibrary;


        private static MainVM _main;
        private static MediaPlaybackViewModel _mediaPlayback;
        private static MusicPlayerVM _musicPlayer;
        private static MediaLibraryVM _mediaLibraryVM;
        private static MusicLibraryVM _musicLibraryVM;
        private static VideoLibraryVM _videoLibraryVM;
        private static VideoPlayerVM _videoPlayer;
        private static VLCExplorerViewModel _vlcExplorer;
        private static StreamsViewModel _streams;
        private static SettingsViewModel _settings;
        private static SearchViewModel _search;
        private static SpecialThanksViewModel _specialThanks;
        private static ExternalDeviceService _externalDeviceService;
        private static GamepadService _gamePadService;
        private static NavigationService _navigationService;
        private static PlaybackService _playbackService;
        private static MusicMetaService _musicMetaService;
        private static VideoMetaService _videoMetaService;
        private static MetroSlideshow _metroSlideshow;
        private static HttpServer _httpServer;
        private static FileCopyService _fileCopyService;
        static RendererService _rendererService;

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

        public static MediaLibrary MediaLibrary => _mediaLibrary ?? (_mediaLibrary = App.Container.Resolve<MediaLibrary>());

        public static MusicPlayerVM MusicPlayerVM => _musicPlayer ?? (_musicPlayer = App.Container.Resolve<MusicPlayerVM>());

        public static MediaLibraryVM MediaLibraryVM => _mediaLibraryVM ?? (_mediaLibraryVM = App.Container.Resolve<MediaLibraryVM>());

        public static MusicLibraryVM MusicLibraryVM => _musicLibraryVM ?? (_musicLibraryVM = App.Container.Resolve<MusicLibraryVM>());

        public static VideoLibraryVM VideoLibraryVM => _videoLibraryVM ?? (_videoLibraryVM = App.Container.Resolve<VideoLibraryVM>());
        
        public static VideoPlayerVM VideoPlayerVm => _videoPlayer ?? (_videoPlayer = App.Container.Resolve<VideoPlayerVM>()); 

        public static VLCExplorerViewModel FileExplorerVM => _vlcExplorer ?? (_vlcExplorer = App.Container.Resolve<VLCExplorerViewModel>());

        public static SettingsViewModel SettingsVM => _settings ?? (_settings = App.Container.Resolve<SettingsViewModel>());
        public static SearchViewModel SearchVM => _search ?? (_search = App.Container.Resolve<SearchViewModel>());
        public static StreamsViewModel StreamsVM => _streams ?? (_streams = App.Container.Resolve<StreamsViewModel>());
        public static SpecialThanksViewModel SpecialThanksVM => _specialThanks ?? (_specialThanks = App.Container.Resolve<SpecialThanksViewModel>()); 

        public static NavigationService NavigationService => _navigationService ?? (_navigationService = App.Container.Resolve<NavigationService>());
        public static PlaybackService PlaybackService => _playbackService ?? (_playbackService = App.Container.Resolve<PlaybackService>());
        public static ExternalDeviceService ExternalDeviceService => _externalDeviceService ?? (_externalDeviceService = App.Container.Resolve<ExternalDeviceService>());
        public static GamepadService GamepadService => _gamePadService ?? (_gamePadService = App.Container.Resolve<GamepadService>());
        public static MusicMetaService MusicMetaService => _musicMetaService ?? (_musicMetaService = App.Container.Resolve<MusicMetaService>());
        public static VideoMetaService VideoMetaService => _videoMetaService ?? (_videoMetaService = App.Container.Resolve<VideoMetaService>());
        public static MetroSlideshow Slideshow => _metroSlideshow ?? (_metroSlideshow = App.Container.Resolve<MetroSlideshow>());
        public static HttpServer HttpServer => _httpServer ?? (_httpServer = App.Container.Resolve<HttpServer>());
        public static FileCopyService FileCopyService => _fileCopyService ?? (_fileCopyService = App.Container.Resolve<FileCopyService>());
        public static RendererService RendererService => _rendererService ?? (_rendererService = App.Container.Resolve<RendererService>());
    }
}