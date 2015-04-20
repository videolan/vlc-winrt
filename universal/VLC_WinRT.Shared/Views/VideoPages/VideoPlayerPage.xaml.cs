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
        private bool isVideoSettingsOpen = false;
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
            if (width < 800)
            {
                VisualStateManager.GoToState(this, "Narrow", false);
            }
            else if (width < 848)
            {
                VisualStateManager.GoToState(this, "Medium", false);
            }
            else
            {
                VisualStateManager.GoToState(this, "Full", false);
            }
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            AppViewHelper.SetFullscren(true);
            Locator.MediaPlaybackViewModel.MouseService.OnHidden += MouseStateChanged;
            Locator.MediaPlaybackViewModel.MouseService.OnMoved += MouseStateChanged;
            // If no playback was ever started, ContinueIndexing can be null
            // If we navigate back and forth to the main page, we also don't want to 
            // re-mark the task as completed.
            Locator.MediaPlaybackViewModel.ContinueIndexing = new TaskCompletionSource<bool>();
        }


        private void MouseStateChanged()
        {
            DisplayOrHide();
        }


        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            AppViewHelper.SetFullscren(false);
            if (Locator.MediaPlaybackViewModel.ContinueIndexing != null && !Locator.MediaPlaybackViewModel.ContinueIndexing.Task.IsCompleted)
            {
                Locator.MediaPlaybackViewModel.ContinueIndexing.SetResult(true);
            }
        }

        void DisplayOrHide()
        {
            VisualStateManager.GoToState(this, isVisible ? "ControlsCollapsed" : "ControlsVisible", false);
            isVisible = !isVisible;
        }

        private void PlaceholderInteractionGrid_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            Locator.MediaPlaybackViewModel.MouseService.Content_Tapped(sender, e);
            if (isVideoSettingsOpen)
            {
                VideoPlayerSettings.Visibility = Visibility.Collapsed;
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

        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            isVideoSettingsOpen = true;
            VideoPlayerSettings.Visibility = Visibility.Visible;
        }
    }
}
