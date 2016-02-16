/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/
using Windows.Graphics.Display;
using Windows.Media;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Autofac;
using VLC_WinRT.Helpers;
using VLC_WinRT.Services.RunTime;
using VLC_WinRT.ViewModels;
using Windows.UI.ViewManagement;
using Slide2D;
using Windows.UI.Xaml.Navigation;
using VLC_WinRT.Utils;
using VLC_WinRT.Model;
using System;
using Windows.System;
#if WINDOWS_APP
using Windows.UI.ApplicationSettings;
#endif

namespace VLC_WinRT.Views.MainPages
{
    public sealed partial class MainPage
    {
        public MainPage()
        {
            InitializeComponent();
            var smtc = SystemMediaTransportControls.GetForCurrentView();
            Locator.MediaPlaybackViewModel.SetMediaTransportControls(smtc);
        }

        public void SetBackground(bool force = false, bool dark = false)
        {
            if (force)
            {
                if (dark)
                    Dark.Begin();
                else
                    Light.Begin();
            }
            else
            {
                if (Locator.SettingsVM.ApplicationTheme == ApplicationTheme.Dark)
                    Dark.Begin();
                else
                    Light.Begin();
            }
        }

        private void Slideshower_Loaded_1(object sender, RoutedEventArgs e)
        {
            Locator.Slideshow.Initialize(ref Slideshower);
        }
        
        private void SplitShell_FlyoutCloseRequested(object sender, System.EventArgs e)
        {
            Locator.NavigationService.GoBack_HideFlyout();
        }
        
        private void MainFrame_OnNavigated(object sender, NavigationEventArgs e)
        {
#if WINDOWS_APP
            SettingsPane pane = SettingsPane.GetForCurrentView();
            pane.CommandsRequested += SettingsCommandRequested;
#endif
        }

#if WINDOWS_APP
        private void SettingsCommandRequested(SettingsPane sender, SettingsPaneCommandsRequestedEventArgs args)
        {
            var privacyCommand = new SettingsCommand("privacy", Strings.PrivacyStatement,
                async h => await Launcher.LaunchUriAsync(new Uri("http://videolan.org/vlc/privacy.html")));

            var specialThanks = new SettingsCommand("specialThanks", Strings.SpecialThanks,
                command =>
                {
                    Locator.NavigationService.Go(VLCPage.SpecialThanksPage);
                });

            var settings = new SettingsCommand("settings", Strings.Settings,
                command =>
                {
                    Locator.NavigationService.Go(VLCPage.SettingsPage);
                });
            var license = new SettingsCommand("license", Strings.License, command =>
            {
                Locator.NavigationService.Go(VLCPage.LicensePage);
            });
            args.Request.ApplicationCommands.Clear();
            args.Request.ApplicationCommands.Add(privacyCommand);
            args.Request.ApplicationCommands.Add(specialThanks);
            args.Request.ApplicationCommands.Add(settings);
            args.Request.ApplicationCommands.Add(license);
        }
#endif
    }
}