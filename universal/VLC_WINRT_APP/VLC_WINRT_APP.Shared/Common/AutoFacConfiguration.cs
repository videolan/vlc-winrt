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
using VLC_WINRT_APP.Services.Mocks;
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
            builder.RegisterType<VideoPlayerVM>().SingleInstance();
            builder.RegisterType<MainVM>().SingleInstance();
            builder.RegisterType<MusicLibraryVM>().SingleInstance();
            builder.RegisterType<MusicPlayerVM>().SingleInstance();
            builder.RegisterType<VideoLibraryVM>().SingleInstance();
            builder.RegisterType<SettingsViewModel>().SingleInstance();
#if WINDOWS_APP
            builder.RegisterType<ExternalStorageViewModel>().SingleInstance();
#endif
            builder.RegisterType<DLNAVM>().SingleInstance();

            // Register Services
            builder.RegisterType<MediaService>().As<IMediaService>().SingleInstance();
#if WINDOWS_PHONE_APP && USE_DUMMY_PLAYER
            builder.RegisterType<VlcServiceMock>().As<IVlcService>().SingleInstance();
#else
            builder.RegisterType<VlcService>().As<IVlcService>().SingleInstance();
#endif
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
