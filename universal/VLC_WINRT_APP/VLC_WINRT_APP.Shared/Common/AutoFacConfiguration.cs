/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using Autofac;
using VLC_WINRT_APP.Services.Interface;
using VLC_WINRT_APP.Services.RunTime;
using VLC_WINRT_APP.ViewModels;
using VLC_WINRT_APP.ViewModels.MusicVM;
using VLC_WINRT_APP.ViewModels.NetworkVM;
using VLC_WINRT_APP.ViewModels.RemovableDevicesVM;
using VLC_WINRT_APP.ViewModels.Settings;
using VLC_WINRT_APP.ViewModels.VideoVM;
using VLC_WINRT_APP.Views.MainPages;
using DesignTime = VLC_WINRT_APP.Services.DesignTime;

namespace VLC_WINRT_APP.Common
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
            // TODO: These should not be SingleInstance
            builder.RegisterType<MainVM>().SingleInstance();
            builder.RegisterType<MediaPlaybackViewModel>().SingleInstance();
            builder.RegisterType<MusicLibraryVM>().SingleInstance();
            builder.RegisterType<MusicPlayerVM>().SingleInstance();
            builder.RegisterType<VideoLibraryVM>().SingleInstance();
            builder.RegisterType<VideoPlayerVM>().SingleInstance();
            builder.RegisterType<SettingsViewModel>().SingleInstance();

            builder.RegisterType<ExternalStorageViewModel>().SingleInstance();

            builder.RegisterType<DLNAVM>().SingleInstance();

            // Register Services
            builder.RegisterType<MediaService>().As<IMediaService>().SingleInstance();
            builder.RegisterType<MusicMetaService>().SingleInstance();
            builder.RegisterType<KeyboardListenerService>().SingleInstance();
            builder.RegisterType<NetworkListenerService>().SingleInstance();

            #if WINDOWS_APP
            builder.RegisterType<MouseService>().SingleInstance();
            #endif

            builder.RegisterType<ExternalDeviceService>().SingleInstance();
            builder.RegisterType<SpecialThanksViewModel>().SingleInstance();
            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                builder.RegisterType<DesignTime.ThumbnailService>().As<IThumbnailService>().SingleInstance();
            }
            else
            {
                builder.RegisterType<ThumbnailService>().As<IThumbnailService>().SingleInstance();
            }

            // Register Views            

            builder.RegisterType<MainPage>();

            return builder.Build();
        }
    }
}
