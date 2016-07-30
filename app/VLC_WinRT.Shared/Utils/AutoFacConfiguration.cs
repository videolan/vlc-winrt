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
using VLC_WinRT.Helpers.MusicLibrary;
using VLC_WinRT.Helpers.VideoLibrary;
using VLC_WinRT.Model.Library;
using VLC_WinRT.Services.Interface;
using VLC_WinRT.Services.RunTime;
using VLC_WinRT.ViewModels;
using VLC_WinRT.ViewModels.MusicVM;
using VLC_WinRT.ViewModels.Others;
using VLC_WinRT.ViewModels.RemovableDevicesVM;
using VLC_WinRT.ViewModels.Settings;
using VLC_WinRT.ViewModels.VideoVM;

namespace VLC_WinRT.Utils
{
    public static class AutoFacConfiguration
    {
        /// <summary>
        /// Configures the AutoFac IoC Container for VLC_WINRT.
        /// </summary>
        /// <returns>Built container with all the </returns>
        /// TODO: Make a design time and a runtime configure?
        public static IContainer Configure()
        {
            var builder = new ContainerBuilder();

            // Register View Models

            builder.RegisterType<MainVM>();
            builder.RegisterType<MediaPlaybackViewModel>();
            builder.RegisterType<MediaLibrary>();

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
            builder.RegisterType<VLCService>();
            builder.RegisterType<NavigationService>();
            builder.RegisterType<MusicMetaService>();
            builder.RegisterType<VideoMetaService>();

            builder.RegisterType<KeyboardListenerService>();
            builder.RegisterType<NetworkListenerService>();
            builder.RegisterType<MouseService>();
            builder.RegisterType<ExternalDeviceService>().SingleInstance();
            builder.RegisterType<SpecialThanksViewModel>();
            builder.RegisterType<ThumbnailService>().SingleInstance();

            return builder.Build();
        }
    }
}
