/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System;
using Windows.System;
using Windows.UI.ApplicationSettings;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using VLC_WINRT.ViewModels;
using VLC_WINRT.Utility.Services.RunTime;

namespace VLC_WINRT.Views
{
    public class BasePage : Page, IDisposable
    {
        protected NavigateableViewModel _vm;

        protected BasePage()
        {
            SettingsPane.GetForCurrentView().CommandsRequested += SettingsCommandRequested;
        }

        public void Dispose()
        {
            _vm = null;
            SettingsPane.GetForCurrentView().CommandsRequested -= SettingsCommandRequested;
        }

        private void SettingsCommandRequested(SettingsPane sender, SettingsPaneCommandsRequestedEventArgs args)
        {
            var privacyCommand = new SettingsCommand("privacy", "Privacy Statement",
                h => Launcher.LaunchUriAsync(new Uri("http://videolan.org/vlc/privacy.html")));

            args.Request.ApplicationCommands.Clear();
            args.Request.ApplicationCommands.Add(privacyCommand);

            //SettingsCommand about = new SettingsCommand("AboutID", "About", command =>
            //{
            //    AboutTheApp privacyPolicy = new AboutTheApp();
            //    privacyPolicy.Show();
            //});
            //args.Request.ApplicationCommands.Add(about);

            SettingsCommand specialThanks = new SettingsCommand("specialThanks", "Special Thanks", command =>
            {
                NavigationService.NavigateTo(typeof(SpecialThanks));
            });
            args.Request.ApplicationCommands.Add(specialThanks);
        }

        public void NavigateFrom()
        {
            OnNavigatedFrom(null);
        }

        public void NavigateTo()
        {
            OnNavigatedTo(null);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            SetDataContext();
            _vm.OnNavigatedTo();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            _vm.OnNavigatedFrom();
            _vm = null;
        }

        public virtual void SetDataContext()
        {
        }
    }
}
