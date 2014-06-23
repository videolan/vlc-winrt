/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using Windows.Media;
using Windows.Storage.AccessCache;
using Windows.UI.Xaml.Media.Animation;
using Autofac;
using System;
using VLC_WINRT.Common;
using VLC_WINRT_APP.Utility.Helpers;
using VLC_WINRT_APP.Utility.Services.RunTime;
using VLC_WINRT.ViewModels;
using VLC_WINRT.ViewModels.MainPage;
using VLC_WINRT_APP;
using VLC_WINRT_APP.ViewModels;
using VLC_WINRT_APP.Views.MainPages;
#if NETFX_CORE
using VLC_WINRT.Views;
#endif
#if WINDOWS_PHONE_APP
using VLC_WINPRT;
using VLC_WINRT_APP.Utility.Services.Interface;
#endif

namespace VLC_WINRT_APP.Utility.Commands
{
    public class OpenVideoCommand : AlwaysExecutableCommand
    {
        public async override void Execute(object parameter)
        {
            if (parameter.GetType() != typeof (MediaViewModel))
                throw new ArgumentException("Expecting to see a Media View Model for this command");

            if(MediaControl.IsPlaying)
                Locator.MusicPlayerVM.Stop();

            var vm = (MediaViewModel) parameter;

            string token = StorageApplicationPermissions.FutureAccessList.Add(vm.File);

            var frame = App.ApplicationFrame;
#if NETFX_CORE
            var page = frame.Content as MainPage;
            if (page != null)
            {
                var sB = page.Resources["FadeOutPage"] as Storyboard;
                if (sB != null)
                {
                    await sB.BeginAsync();
                }
            }
#endif
            Locator.PlayVideoVM.CurrentVideo = parameter as MediaViewModel;
#if NETFX_CORE
            App.ApplicationFrame.Navigate(typeof(PlayVideo));
#endif
            Locator.PlayVideoVM.SetActiveVideoInfo(token, vm.File.Name);
        }
    }
}
