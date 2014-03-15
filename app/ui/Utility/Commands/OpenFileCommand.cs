/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using Autofac;
using System;
using Windows.Media;
using Windows.UI.Xaml.Media.Animation;
using VLC_WINRT.Common;
using VLC_WINRT.Utility.Helpers;
using VLC_WINRT.Utility.Services.RunTime;
using VLC_WINRT.ViewModels;
using VLC_WINRT.ViewModels.MainPage;
using VLC_WINRT.Views;

namespace VLC_WINRT.Utility.Commands
{
    public class OpenVideoCommand : AlwaysExecutableCommand
    {
        public async override void Execute(object parameter)
        {
            if (parameter.GetType() != typeof (MediaViewModel) && parameter.GetType() != typeof (ViewedVideoViewModel))
                throw new ArgumentException("Expecting to see a Media View Model for this command");

            if(MediaControl.IsPlaying)
                Locator.MusicPlayerVM.Stop();

            var historyService = App.Container.Resolve<HistoryService>();
            var vm = (MediaViewModel) parameter;

            string token = historyService.Add(vm.File);

            var frame = App.ApplicationFrame;
            var page = frame.Content as Views.MainPage;
            if (page != null)
            {
                var sB = page.Resources["FadeOutPage"] as Storyboard;
                if (sB != null)
                {
                    await sB.BeginAsync();
                }
            }
            Locator.PlayVideoVM.CurrentVideo = parameter as MediaViewModel;
            NavigationService.NavigateTo(typeof(PlayVideo));
            Locator.PlayVideoVM.SetActiveVideoInfo(token, vm.File.Name);
        }
    }
}
