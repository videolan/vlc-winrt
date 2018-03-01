/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using Windows.Graphics.Display;
using Windows.UI.Input;
using System.Linq;
using Windows.System;
using VLC;
using VLC.Commands;
using VLC.Commands.Navigation;
using VLC.Helpers;
using VLC.Model.Video;
using VLC.Utils;
using VLC.ViewModels;
using WinRTXamlToolkit.Controls.Extensions;

namespace VLC_WinRT.Views.VideoPages
{
    public sealed partial class VideoPlayerPage : Page
    {
        private bool isVisible = true;
        private bool isLocked = false;
        private GestureActionType currentGestureActionType;
        private DispatcherTimer _controlsTimer = new DispatcherTimer();
        public VideoPlayerPage()
        {
            InitializeComponent();
        }

        void Responsive()
        {
            var width = Window.Current.Bounds.Width;
            if (width < 800)
            {
                VisualStateManager.GoToState(this, nameof(Narrow), false);
            }
            else if (width < 1050)
            {
                VisualStateManager.GoToState(this, nameof(Medium), false);
            }
            else
            {
                VisualStateManager.GoToState(this, nameof(Full), false);
            }

            if (DeviceHelper.GetDeviceType() == DeviceTypeEnum.Tablet && AppViewHelper.GetFullscreen() == false)
            {
                VisualStateManager.GoToState(this, nameof(WindowState), false);
            }
            else
            {
                VisualStateManager.GoToState(this, nameof(FullscreenState), false);
            }

          //  Locator.MediaPlaybackViewModel.PlaybackService.SetSizeVideoPlayer((uint)Math.Ceiling(App.RootPage.SwapChainPanel.ActualWidth), (uint)Math.Ceiling(App.RootPage.SwapChainPanel.ActualHeight));
            Locator.VideoPlayerVm.ChangeSurfaceZoom(Locator.VideoPlayerVm.CurrentSurfaceZoom);
            DisplayOrHide(true);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            // Set UI Layout
            App.RootPage.SwapChainPanel.Visibility = Visibility.Visible;
     //       App.SplitShell.FooterVisibility = //AppBarClosedDisplayMode.Hidden;
            AppViewHelper.SetTitleBarTitle(Locator.VideoPlayerVm.CurrentVideo?.Name);

            // UI interactions
#if WINDOWS_APP
            Locator.MediaPlaybackViewModel.MouseService.Start();
            Locator.MediaPlaybackViewModel.MouseService.OnHidden += MouseCursorHidden;
            Locator.MediaPlaybackViewModel.MouseService.OnMoved += MouseMoved;            
#endif
            RootGrid.Tapped += RootGrid_Tapped;
            _controlsTimer.Interval = TimeSpan.FromSeconds(4);
            _controlsTimer.Tick += ControlsTimer_Tick;
            _controlsTimer.Start();

            // VM initialization
            Locator.VideoPlayerVm.OnNavigatedTo();

            // Responsive design
            this.SizeChanged += OnSizeChanged;
            Responsive();
            
            // Swapchain animations
            //      App.RootPage.SwapChainPanel.StartCompositionAnimationOnSwapChain(false);

            KeyDown += OnKeyDown;
        }

        private void OnKeyDown(object sender, KeyRoutedEventArgs keyRoutedEventArgs)
        {
            if (keyRoutedEventArgs.Key == VirtualKey.Escape)
                GoBack();
        }

        private void GoBack(object sender, RoutedEventArgs e)
        {
            GoBack();
        }

        private void GoBack()
        {
            Locator.NavigationService.GoBack_Default();
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            AppViewHelper.SetTitleBarTitle();
            App.RootPage.SwapChainPanel.Visibility = Visibility.Collapsed;
            //App.SplitShell.FooterVisibility = AppBarClosedDisplayMode.Minimal;

            Locator.VideoPlayerVm.OnNavigatedFrom();
            if (AppViewHelper.GetFullscreen())
                AppViewHelper.EnterFullscreen();
            Locator.PlaybackService.Stop();
            this.SizeChanged -= OnSizeChanged;
            KeyDown -= OnKeyDown;
#if WINDOWS_APP
            Locator.MediaPlaybackViewModel.MouseService.Stop();
            Locator.MediaPlaybackViewModel.MouseService.OnHidden -= MouseCursorHidden;
            Locator.MediaPlaybackViewModel.MouseService.OnMoved -= MouseMoved;
#endif
            _controlsTimer.Tick -= ControlsTimer_Tick;
            _controlsTimer.Stop();
            Locator.MediaPlaybackViewModel.MouseService.ShowCursor();
        }

        void OnSizeChanged(object sender, SizeChangedEventArgs args)
        {
            Responsive();
        }

        private void ControlsTimer_Tick(object sender, object e)
        {
            if (e == null)
                DisplayOrHide(false);
            else
                DisplayOrHide((bool)e);

            _controlsTimer.Stop();
            _controlsTimer.Start();
        }

        private void MouseCursorHidden()
        {
            ControlsTimer_Tick(null, false);
        }

        private void MouseMoved()
        {
            ControlsTimer_Tick(null, true);
        }

        private void RootGrid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (e.PointerDeviceType != Windows.Devices.Input.PointerDeviceType.Touch)
                return;

            if ((e.OriginalSource as FrameworkElement)?.Name == nameof(PlaceholderInteractionGrid))
            {
                ControlsTimer_Tick(null, !isVisible);
            }
            else
            {
                ControlsTimer_Tick(null, true);
            }
        }

        private void PlaceholderInteractionGrid_OnTapped(object sender, TappedRoutedEventArgs e)
        {
        }
        
        async void DisplayOrHide(bool mouseOrTouchPresent)
        {
            isVisible = mouseOrTouchPresent;

            if (/*Locator.VideoPlayerVm.IsVideoPlayerAudioTracksSettingsVisible ||*/
                Locator.VideoPlayerVm.IsVideoPlayerOptionsPanelVisible
                /*Locator.VideoPlayerVm.IsVideoPlayerSubtitlesSettingsVisible ||
                Locator.VideoPlayerVm.IsVideoPlayerVolumeSettingsVisible*/)
                return;
            if (!isVisible)
            {
                ControlsGridFadeOut.Value = ControlsBorder.ActualHeight;
                HeaderGridFadeOut.Value = -HeaderGrid.ActualHeight;
                FadeOut.Begin();
#if WINDOWS_APP
                Locator.MediaPlaybackViewModel.MouseService.HideCursor();
#elif WINDOWS_PHONE_APP
                var statusBar = Windows.UI.ViewManagement.StatusBar.GetForCurrentView();

                // Hide the status bar
                await statusBar.HideAsync();
#endif
                
                await VolumeGrid.FadeOut();
                await BackButton.FadeOut();
            }
            else
            {                
                FadeIn.Begin();
#if WINDOWS_APP
                Locator.MediaPlaybackViewModel.MouseService.ShowCursor();
#elif WINDOWS_PHONE_APP
                var statusBar = Windows.UI.ViewManagement.StatusBar.GetForCurrentView();

                // Hide the status bar
                await statusBar.ShowAsync();
#endif
                await VolumeGrid.FadeIn();
                await BackButton.FadeIn();
            }
        }

        private void PlaceholderInteractionGrid_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (e.PointerDeviceType != Windows.Devices.Input.PointerDeviceType.Touch)
            {
                AppViewHelper.EnterFullscreen();
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
            Slider.IsEnabled = !isLocked;
            SubtitlesButton.IsEnabled = !isLocked;
            PlayButton.IsEnabled = !isLocked;
            PauseButton.IsEnabled = !isLocked;
            VolumeSlider.IsEnabled = !isLocked;
            MenuButton.IsEnabled = !isLocked;

            if (Locator.SettingsVM.ForceLandscape && DeviceHelper.GetDeviceType() != DeviceTypeEnum.Xbox)
            {
                DisplayInformation.AutoRotationPreferences = DisplayOrientations.Landscape;
            }
            else if (isLocked)
            {
                DisplayInformation.AutoRotationPreferences = DisplayInformation.GetForCurrentView().CurrentOrientation;
            }
            else
            {
                DisplayInformation.AutoRotationPreferences = DisplayOrientations.None;
            }
        }

        private void PlaceholderInteractionGrid_OnManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            if (isLocked) return;
            Debug.WriteLine("VideoPlayerPage gesture started");
            if (Math.Abs(e.Cumulative.Translation.Y) > Math.Abs(e.Cumulative.Translation.X))
            {
                if (e.Position.X < (Window.Current.Bounds.Width / 2))
                {
                    //currentGestureActionType = GestureActionType.Brightness; As we don't have brightness API access yet
                    currentGestureActionType = GestureActionType.Volume; // brightness = volume for the moment
                }
                else if (e.Position.X > (Window.Current.Bounds.Width / 2))
                {
                    currentGestureActionType = GestureActionType.Volume;
                }
            }
            else
            {
                currentGestureActionType = GestureActionType.Seek;
            }
            GestureBorder.Visibility = Visibility.Visible;
        }

        private void PlaceholderInteractionGrid_OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (isLocked) return;
            var cumulativeTranslationX = e.Cumulative.Translation.X;
            switch (currentGestureActionType)
            {
                case GestureActionType.Null:
                    break;
                case GestureActionType.Volume:
                    GestureTextBlockDescription.Text = Strings.Volume + " " + computeVolumeFromGesture(e.Cumulative) + "%";
                    break;
                case GestureActionType.Brightness:
                    break;
                case GestureActionType.Seek:
                    var seekInSeconds = Math.Floor(cumulativeTranslationX / 10);
                    GestureTextBlockDescription.Text = StringsHelper.SecondsToString(seekInSeconds) + " (" + StringsHelper.MillisecondsToString(Locator.MediaPlaybackViewModel.Time) + ")";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void PlaceholderInteractionGrid_OnManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            if (isLocked) return;
            switch (currentGestureActionType)
            {
                case GestureActionType.Null:
                    break;
                case GestureActionType.Volume:
                    Locator.MediaPlaybackViewModel.Volume = computeVolumeFromGesture(e.Cumulative);
                    break;
                case GestureActionType.Brightness:
                    break;
                case GestureActionType.Seek:
                    var timeInSeconds = e.Cumulative.Translation.X;
                    Locator.MediaPlaybackViewModel.Time += (int)timeInSeconds * 100;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            Debug.WriteLine("VideoPlayerPage gesture completed");
            GestureTextBlockDescription.Text = "";
            GestureBorder.Visibility = Visibility.Collapsed;
        }

        int computeVolumeFromGesture(ManipulationDelta cumulative)
        {
            var currentVol = Locator.MediaPlaybackViewModel.Volume;
            var volumeDelta = (int)Math.Floor(-cumulative.Translation.Y / 5);
            var newVol = currentVol + volumeDelta;
            if (newVol > 100)
                newVol = 100;
            else if (newVol < 0)
                newVol = 0;
            return newVol;
        }

        private void FullscreenToggle_Click(object sender, RoutedEventArgs e)
        {
            if(AppViewHelper.GetFullscreen())
                AppViewHelper.LeaveFullscreen();
            else AppViewHelper.EnterFullscreen();
        }

        private void ShowAudioTrackMenu(object sender, RoutedEventArgs e)
        {
            if (AudioTracksSettings.Visibility == Visibility.Visible) return;
            AudioTracksSettings.Visibility = Visibility.Visible;
        }

        private void ShowSubtitlesMenu(object sender, RoutedEventArgs e)
        {
            if (SubtitlesSettings.Visibility == Visibility.Visible) return;
            SubtitlesSettings.Visibility = Visibility.Visible;
        }


    }
}
