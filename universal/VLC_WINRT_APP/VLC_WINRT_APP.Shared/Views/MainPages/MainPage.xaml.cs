/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Graphics.Display;
using Windows.Media;
using Windows.System;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Autofac;
using VLC_WINRT_APP.Helpers;
using VLC_WINRT_APP.Model;
using VLC_WINRT_APP.Services.RunTime;
using VLC_WINRT_APP.Services.Interface;
using VLC_WINRT_APP.ViewModels;
using VLC_WINRT_APP.Views.UserControls;
using VLC_WINRT_APP.Views.VariousPages;
#if WINDOWS_APP
using Windows.UI.ApplicationSettings;
using SettingsFlyout = VLC_WINRT_APP.Views.UserControls.SettingsFlyout;
#endif
namespace VLC_WINRT_APP.Views.MainPages
{
    public sealed partial class MainPage : SwapChainPanel
    {
        public MainPage()
        {
            InitializeComponent();
            Loaded += SwapPanelLoaded;
            Locator.MediaPlaybackViewModel.SetMediaTransportControls(SystemMediaTransportControls.GetForCurrentView());
        }

        async void Responsive()
        {
#if WINDOWS_PHONE_APP
            if (DisplayHelper.IsPortrait())
            {
                await StatusBarHelper.Show();
                var rect = StatusBarHelper.OccludedRect;
                SplitShell.Margin = new Thickness(0, rect.Height, 0, 0);
            }
            else
            {
                SplitShell.Margin = new Thickness(0);
                await StatusBarHelper.Hide();
            }
#endif
        }

        private void SwapPanelLoaded(object sender, RoutedEventArgs e)
        {
            App.Container.Resolve<VLCService>().Initialize(SwapChainPanel);
            Window.Current.SizeChanged += Current_SizeChanged;
            DisplayInformation.GetForCurrentView().OrientationChanged += MainPage_OrientationChanged;
            Unloaded += MainPage_Unloaded;
            Responsive();
        }

        async void MainPage_OrientationChanged(DisplayInformation sender, object args)
        {
            Responsive();
        }

        void Current_SizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            Responsive();
            Locator.MediaPlaybackViewModel._mediaService.SetSizeVideoPlayer((uint)e.Size.Width, (uint)e.Size.Height);
        }

        private void MfMediaElement_OnLoaded(object sender, RoutedEventArgs e)
        {
            App.Container.Resolve<MFService>().Initialize(MfMediaElement);
        }

        private void MainPage_Unloaded(object sender, RoutedEventArgs e)
        {
            Window.Current.SizeChanged -= Current_SizeChanged;
        }
    }
}