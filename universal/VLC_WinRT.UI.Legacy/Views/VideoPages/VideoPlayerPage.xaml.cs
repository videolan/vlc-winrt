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
using VLC_WinRT.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using VLC_WinRT.Helpers;
using Windows.Graphics.Display;

namespace VLC_WinRT.Views.VideoPages
{
    public sealed partial class VideoPlayerPage : Page
    {
        private bool isVisible = true;
        private bool isLocked = false;
        public VideoPlayerPage()
        {
            InitializeComponent();
            this.Loaded += VideoPlayerPage_Loaded;
        }

        void VideoPlayerPage_Loaded(object sender, RoutedEventArgs e)
        {
            this.Unloaded += VideoPlayerPage_Unloaded;
            this.SizeChanged += VideoPlayerPage_SizeChanged;
        }

        void VideoPlayerPage_Unloaded(object sender, RoutedEventArgs e)
        {
            this.SizeChanged -= VideoPlayerPage_SizeChanged;
        }

        void VideoPlayerPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Responsive();
        }

        void Responsive()
        {
            var width = Window.Current.Bounds.Width;
            var height = Window.Current.Bounds.Height;
            if (width < 800)
            {
                VisualStateManager.GoToState(this, "Narrow", false);
            }
            else if (width < 1050)
            {
                VisualStateManager.GoToState(this, "Medium", false);
            }
            else
            {
                VisualStateManager.GoToState(this, "Full", false);
            }
            Locator.MediaPlaybackViewModel._mediaService.SetSizeVideoPlayer((uint)Math.Ceiling(width), (uint)Math.Ceiling(height));
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            App.RootPage.SwapChainPanel.Visibility = Visibility.Visible;
            Locator.MediaPlaybackViewModel.MouseService.OnHidden += MouseStateChanged;
            Locator.MediaPlaybackViewModel.MouseService.OnMoved += MouseMoved;
            Locator.VideoVm.OnNavigatedTo();
            Responsive();
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            App.RootPage.SwapChainPanel.Visibility = Visibility.Collapsed;
            Locator.VideoVm.OnNavigatedFrom();
        }

        private void MouseMoved()
        {
            Display();
        }

        private void MouseStateChanged()
        {
            Hide();
        }

        void Display()
        {
            isVisible = true;
            DisplayOrHide();
        }

        void Hide()
        {
            isVisible = false;
            DisplayOrHide();
        }

        void DisplayOrHide()
        {
            if (Locator.VideoVm.IsVideoPlayerAudioTracksSettingsVisible ||
                Locator.VideoVm.IsVideoPlayerSettingsVisible ||
                Locator.VideoVm.IsVideoPlayerSubtitlesSettingsVisible ||
                Locator.VideoVm.IsVideoPlayerVolumeSettingsVisible ||
                Locator.VideoVm.IsVideoPlayerChaptersListVisible)
                return;
            VisualStateManager.GoToState(this, !isVisible ? "ControlsCollapsed" : "ControlsVisible", false);
        }

        private void PlaceholderInteractionGrid_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            if (e.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Touch && !isVisible)
                Locator.MediaPlaybackViewModel.MouseService.Content_Tapped(sender, e);
        }

        private void PlaceholderInteractionGrid_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (e.PointerDeviceType != Windows.Devices.Input.PointerDeviceType.Touch)
            {
                AppViewHelper.SetFullscreen();
            }
        }

        private void LockToggleButton_Click(object sender, RoutedEventArgs e)
        {
            SwitchLock();
        }

        void SwitchLock()
        {
            isLocked = !isLocked;
            LockToggleIcon.Glyph = (isLocked) ? App.Current.Resources["LockedSymbol"].ToString() : App.Current.Resources["UnlockedSymbol"].ToString();
            GoBackButton.Visibility = (isLocked) ? Visibility.Collapsed : Visibility.Visible;
            Slider.IsEnabled = !isLocked;
            SubtitlesButton.IsEnabled = !isLocked;
            PlayButton.IsEnabled = !isLocked;
            PauseButton.IsEnabled = !isLocked;
            VolumeButton.IsEnabled = !isLocked;
            MenuButton.IsEnabled = !isLocked;
            DisplayProperties.AutoRotationPreferences = (isLocked) ? DisplayInformation.GetForCurrentView().CurrentOrientation : DisplayOrientations.None;
        }
    }
}
