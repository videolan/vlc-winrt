/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using Autofac;
using Slide2D;
using VLC.Helpers.MusicLibrary;
using VLC.Helpers.VideoLibrary;
using VLC.Model.Library;
using VLC.Services.RunTime;
using VLC.ViewModels;
using VLC.ViewModels.MusicVM;
using VLC.ViewModels.Others;
using VLC.ViewModels.RemovableDevicesVM;
using VLC.ViewModels.Settings;
using VLC.ViewModels.VideoVM;

namespace VLC.Utils
{
    public static class AutoFacConfiguration
    {
        /// <summary>
        /// Configures the AutoFac IoC Container for VLC.
        /// </summary>
        /// <returns>Built container with all the </returns>
        /// TODO: Make a design time and a runtime configure?
        public static IContainer Configure()
        {
            var builder = new ContainerBuilder();

            // Register View Models

            builder.RegisterType<MainVM>();
            builder.RegisterType<MediaPlaybackViewModel>();
            builder.RegisterType<MediaLibrary>().SingleInstance();

            builder.RegisterType<MediaLibraryVM>();
            builder.RegisterType<MusicLibraryVM>();
            builder.RegisterType<MusicPlayerVM>();
            builder.RegisterType<VideoLibraryVM>();
            builder.RegisterType<VideoPlayerVM>();

            builder.RegisterType<StreamsViewModel>();
            builder.RegisterType<SettingsViewModel>();
            builder.RegisterType<SearchViewModel>();

            builder.RegisterType<VLCExplorerViewModel>();

            // Register Services
            builder.RegisterType<MetroSlideshow>();
            builder.RegisterType<PlaybackService>();
            builder.RegisterType<NavigationService>();
            builder.RegisterType<MusicMetaService>();
            builder.RegisterType<VideoMetaService>();

            builder.RegisterType<KeyboardListenerService>();
            builder.RegisterType<NetworkListenerService>();
            builder.RegisterType<MouseService>();
            builder.RegisterType<ExternalDeviceService>().SingleInstance();
            builder.RegisterType<GamepadService>();
            builder.RegisterType<SpecialThanksViewModel>();
            builder.RegisterType<ThumbnailService>().SingleInstance();
            builder.RegisterType<HttpServer>().SingleInstance();
            builder.RegisterType<FileCopyService>().SingleInstance();
            builder.RegisterType<PlaylistService>().SingleInstance();

            return builder.Build();
        }
    }
}
