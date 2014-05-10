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
                async h => await Launcher.LaunchUriAsync(new Uri("http://videolan.org/vlc/privacy.html")));
            
            var specialThanks = new SettingsCommand("specialThanks", resourceLoader.GetString("SpecialThanks"),
                command =>
                {
                    NavigationService.NavigateTo(typeof(SpecialThanks));
                });

            var settings = new SettingsCommand("settings", resourceLoader.GetString("Settings"),
                command =>
                {
                    NavigationService.NavigateTo(typeof(SettingsPage));
                });

            var about = new SettingsCommand("about", "About The App",
                command =>
                {
                    NavigationService.NavigateTo(typeof(AboutPage));
                });

            args.Request.ApplicationCommands.Clear();
            args.Request.ApplicationCommands.Add(privacyCommand);
            args.Request.ApplicationCommands.Add(specialThanks);
            args.Request.ApplicationCommands.Add(settings);
            args.Request.ApplicationCommands.Add(about);
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
            await _vm.OnNavigatedTo(e);
        }

        protected override async void OnNavigatedFrom(NavigationEventArgs e)
        {
            await _vm.OnNavigatedFrom(e);
            _vm = null;
        }

        public virtual void SetDataContext()
        {
        }
    }
}
