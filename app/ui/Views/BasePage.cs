/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System;
using Windows.ApplicationModel.Resources;
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
            var resourceLoader = new ResourceLoader();
            var privacyCommand = new SettingsCommand("privacy", resourceLoader.GetString("PrivacyStatement"),
                h => Launcher.LaunchUriAsync(new Uri("http://videolan.org/vlc/privacy.html")));

            args.Request.ApplicationCommands.Clear();
            args.Request.ApplicationCommands.Add(privacyCommand);

            //SettingsCommand about = new SettingsCommand("AboutID", "About", command =>
            //{
            //    AboutTheApp privacyPolicy = new AboutTheApp();
            //    privacyPolicy.Show();
            //});
            //args.Request.ApplicationCommands.Add(about);

            var specialThanks = new SettingsCommand("specialThanks", resourceLoader.GetString("SpecialThanks"), 
                command => NavigationService.NavigateTo(typeof(SpecialThanks)));
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

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            SetDataContext();
            await _vm.OnNavigatedTo();
        }

        protected override async void OnNavigatedFrom(NavigationEventArgs e)
        {
            await _vm.OnNavigatedFrom();
            _vm = null;
        }

        public virtual void SetDataContext()
        {
        }
    }
}
