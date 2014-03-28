/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using Autofac;
using VLC_WINRT.Utility.Services.Interface;
using VLC_WINRT.Utility.Services.RunTime;
using DesignTime = VLC_WINRT.Utility.Services.DesignTime;
using VLC_WINRT.ViewModels;
using VLC_WINRT.ViewModels.MainPage;
using VLC_WINRT.ViewModels.MainPage.PlayMusic;
using VLC_WINRT.ViewModels.PlayVideo;

#if NETFX_CORE
using VLC_WINRT.Views;
using VLC_WINRT.ViewModels.Settings;
#endif
#if WINDOWS_PHONE_APP
using VLC_WINPRT.Views;
#endif

namespace VLC_WINRT.Common
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
            builder.RegisterType<PlayVideoViewModel>().SingleInstance();
            builder.RegisterType<MainPageViewModel>().SingleInstance();
            builder.RegisterType<MusicLibraryViewModel>().SingleInstance();
            builder.RegisterType<MusicPlayerViewModel>().SingleInstance();
            builder.RegisterType<SettingsViewModel>().SingleInstance();

            // Register Services
            builder.RegisterType<HistoryService>().SingleInstance();
            builder.RegisterType<MediaService>().As<IMediaService>().SingleInstance();
            #if NETFX_CORE
            builder.RegisterType<MouseService>().SingleInstance();
            #endif
            builder.RegisterType<VlcService>().SingleInstance();
            builder.RegisterType<ExternalDeviceService>().SingleInstance();
            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                builder.RegisterType<DesignTime.ThumbnailService>().As<IThumbnailService>().SingleInstance();
            }
            else
            {
                builder.RegisterType<ThumbnailService>().As<IThumbnailService>().SingleInstance();
            }

            // Register Views            
#if NETFX_CORE
            builder.RegisterType<RootPage>();
#endif
            return builder.Build();
        }
    }
}
