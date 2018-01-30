/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Foundation;
using VLC.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using VLC.Helpers;
using Windows.Graphics.Display;
using Windows.UI.Input;
using Windows.UI.Popups;
using VLC.Model.Video;
using VLC.Services.RunTime;
using VLC.Utils;
using Windows.UI.Xaml.Controls.Primitives;
using VLC.Commands;
using System.Linq;
using System.Numerics;
using Windows.Devices.Input;
using Windows.Foundation.Metadata;
using Windows.UI;
using Windows.UI.Xaml.Media;
using libVLCX;
using ScrollWatcher;
using WinRTXamlToolkit.AwaitableUI;
using WinRTXamlToolkit.Controls.Extensions;

namespace VLC.UI.Views.VideoPages
{
    public sealed partial class VideoPlayerPage : Page
    {
        private bool isVisible = true;
        private bool isLocked = false;
        private GestureActionType currentGestureActionType;
        private DispatcherTimer controlsTimer = new DispatcherTimer();
        private const float DEFAULT_FOV = 80f;
        private readonly PlaybackService _playbackService = Locator.PlaybackService;
        private readonly float DIVIDER = DeviceHelper.GetDeviceType() == DeviceTypeEnum.Phone ? 70f : 10f;
        private long _borderVisibilityChangedToken;
        readonly SolidColorBrush _white;
        readonly SolidColorBrush _red;
        MediaPlaybackViewModel _viewModel => Locator.MediaPlaybackViewModel;

        public delegate void PlayerControlVisibilityChanged(bool visibility);
        public event PlayerControlVisibilityChanged OnPlayerControlVisibilityChanged;

        public VideoPlayerPage()
        {
            InitializeComponent();
            _white = new SolidColorBrush(Colors.White);
            _red = new SolidColorBrush(Colors.Red);
        }

        void MouseWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerDeviceType != PointerDeviceType.Mouse)
                return;

            e.Handled = true;

            var mouseWheelDelta = e.GetCurrentPoint(this).Properties.MouseWheelDelta;
            if(mouseWheelDelta > 0)
                _viewModel.IncreaseVolumeCommand.Execute(null);
            else
                _viewModel.DecreaseVolumeCommand.Execute(null);
        }

        private void OnBorderVisibilityChanged(DependencyObject sender, DependencyProperty dp)
        {
            if (dp == Border.VisibilityProperty)
            {
                // Since the container is not visible, we can't focus the PlayPauseButton before the fadein
                // animation actually starts. Using opacity only to "hide" the border is not an option, since
                // doing so would keep the button focused while invisible, and pressing A would pause instead of
                // showing the control panel.
                // As a work around, we wait for the container to be visible again.
                if (((Border)sender).Visibility == Visibility.Visible)
                    PlayPauseButton.Focus(FocusState.Programmatic);
            }
        }

        void Responsive()
        {
            if (DeviceHelper.GetDeviceType() == DeviceTypeEnum.Tablet && AppViewHelper.GetFullscreen() == false)
            {
                VisualStateManager.GoToState(this, nameof(WindowState), false);
            }
            else
            {
                VisualStateManager.GoToState(this, nameof(FullscreenState), false);
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            // Set UI Layout
            App.RootPage.SwapChainPanel.Visibility = Visibility.Visible;

            // UI interactions
            Locator.MediaPlaybackViewModel.MouseService.Start();
            Locator.MediaPlaybackViewModel.MouseService.OnMoved += ShowControlPanel;
            RootGrid.Tapped += RootGrid_Tapped;
            controlsTimer.Interval = TimeSpan.FromSeconds(5);
            controlsTimer.Tick += ControlsTimer_Tick;
            controlsTimer.Start();
            Locator.MediaPlaybackViewModel.PropertyChanged += MediaPlaybackViewModelOnPropertyChanged;
            if (DeviceHelper.GetDeviceType() == DeviceTypeEnum.Tablet)
                PointerWheelChanged += MouseWheelChanged;

            // VM initialization
            Locator.VideoPlayerVm.OnNavigatedTo();
            Locator.VideoPlayerVm.PlayerControlVisibilityChangeRequested += VideoPlayerVm_PlayerControlVisibilityChangeRequested;
            OnPlayerControlVisibilityChanged += Locator.VideoPlayerVm.OnPlayerControlVisibilityChanged;

            // Responsive design
            this.SizeChanged += OnSizeChanged;
            Responsive();

            _borderVisibilityChangedToken = ControlsBorder.RegisterPropertyChangedCallback(VisibilityProperty, OnBorderVisibilityChanged);
            Locator.MediaPlaybackViewModel.PlaybackService.Playback_MediaStopped += OnPlaybackStopped;
            FadeOut.Completed += FadeOut_Completed;

            // Swapchain animations
            App.RootPage.StartCompositionAnimationOnSwapChain(false);
        }
        
        void MediaPlaybackViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {    
            if (e.PropertyName == nameof(Locator.MediaPlaybackViewModel.Volume))
            {
                var volume = _viewModel.Volume;
                if (volume > _viewModel.VOLUME_THRESHOLD)
                {
                    if (VolumeSlider.Foreground == _red)
                        return;
                    VolumeSlider.Foreground = _red;
                }
                else
                {
                    if (VolumeSlider.Foreground == _white)
                        return;
                    VolumeSlider.Foreground = _white;
                }
            }
        }

        private void VideoPlayerVm_PlayerControlVisibilityChangeRequested(object sender, bool visibility)
        {
            if (visibility)
                ShowControlPanel();
            else
                HideControlPanel();
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);

            AppViewHelper.SetTitleBarTitle();
            App.RootPage.SwapChainPanel.Visibility = Visibility.Collapsed;
            Locator.NavigationService.CloseVideoFlyouts();

            Locator.VideoPlayerVm.OnNavigatedFrom();

            Locator.VideoPlayerVm.PlayerControlVisibilityChangeRequested -= VideoPlayerVm_PlayerControlVisibilityChangeRequested;
            OnPlayerControlVisibilityChanged -= Locator.VideoPlayerVm.OnPlayerControlVisibilityChanged;

            AppViewHelper.LeaveFullscreen();

            Locator.MediaPlaybackViewModel.PropertyChanged -= MediaPlaybackViewModelOnPropertyChanged;

            _viewModel.MouseService.Stop();
            _viewModel.MouseService.OnMoved -= ShowControlPanel;
            RootGrid.Tapped -= RootGrid_Tapped;
            if (DeviceHelper.GetDeviceType() == DeviceTypeEnum.Tablet)
                PointerWheelChanged -= MouseWheelChanged;

            controlsTimer.Tick -= ControlsTimer_Tick;
            controlsTimer.Stop();
            _viewModel.MouseService.ShowCursor();

            this.SizeChanged -= OnSizeChanged;
            ControlsBorder.UnregisterPropertyChangedCallback(VisibilityProperty, _borderVisibilityChangedToken);
            _viewModel.PlaybackService.Playback_MediaStopped -= OnPlaybackStopped;
            FadeOut.Completed -= FadeOut_Completed;
        }

        async void OnPlaybackStopped()
        {
            await DispatchHelper.InvokeInUIThread(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                if (!Locator.NavigationService.GoBack_Default() && Locator.MainVM.CurrentPanel != null)
                    Locator.NavigationService.Go(Locator.MainVM.CurrentPanel.Target);
            });
        }

        void OnSizeChanged(object sender, SizeChangedEventArgs args)
        {
            Responsive();
        }

        private void ControlsTimer_Tick(object sender, object e)
        {
            HideControlPanel();
        }

        private void RootGrid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (e.PointerDeviceType != Windows.Devices.Input.PointerDeviceType.Touch)
                return;

            if ((e.OriginalSource as FrameworkElement)?.Name == nameof(PlaceholderInteractionGrid))
            {
                ToggleControlPanelVisibility();
            }
            else
            {
                ShowControlPanel();
            }
        }

        private void PlaceholderInteractionGrid_OnTapped(object sender, TappedRoutedEventArgs e)
        {
        }

        void HideControlPanel()
        {
            _viewModel.SliderBindingEnabled = false;
            if (isVisible == false || Locator.VideoPlayerVm.IsVideoPlayerOptionsPanelVisible)
                return;
            isVisible = false;
            ControlsGridFadeOut.Value = ControlsBorder.ActualHeight;
            HeaderGridFadeOut.Value = -HeaderGrid.ActualHeight;
            FadeOut.Begin();
            _viewModel.MouseService.HideCursor();
        }

        void ShowControlPanel()
        {
            VolumeSlider.Foreground = _viewModel.Volume > _viewModel.VOLUME_THRESHOLD ? _red : _white;
            _viewModel.SliderBindingEnabled = true;
            controlsTimer.Stop();
            controlsTimer.Start();
            _viewModel.MouseService.ShowCursor();
            if (isVisible == true)
                return;
            isVisible = true;
            FadeIn.Begin();
            OnPlayerControlVisibilityChanged(true);
        }

        private void FadeOut_Completed(object sender, object e)
        {
            // Toggle the visibility once the animation is complete.
            // Otherwise, on xbox, double tapping B will toggle the boolean, and the 2nd press will 
            // trigger the "Back" action
            OnPlayerControlVisibilityChanged?.Invoke(false);
        }

        void ToggleControlPanelVisibility()
        {
            if (isVisible)
                HideControlPanel();
            else
                ShowControlPanel();
        }

        private void PlaceholderInteractionGrid_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (e.PointerDeviceType != Windows.Devices.Input.PointerDeviceType.Touch)
            {
                AppViewHelper.ToggleFullscreen();
            }
        }

        private void LockToggleButton_Click(object sender, RoutedEventArgs e)
        {
            isLocked = !isLocked;
            LockToggleIcon.Glyph = (isLocked) ? App.Current.Resources["LockedSymbol"].ToString() : App.Current.Resources["UnlockedSymbol"].ToString();
            Slider.IsEnabled = !isLocked;
            SubtitlesButton.IsEnabled = !isLocked;
            PlayPauseButton.IsEnabled = !isLocked;
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
            if (Locator.VideoPlayerVm.Is3DVideo)
            {
                currentGestureActionType = GestureActionType.Exlore3D;
                return;
            }

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
                case GestureActionType.Exlore3D:
                    var scale = Math.Abs(e.Cumulative.Scale);
                    if (scale > 1 || scale < 1)
                    {
                        _playbackService.UpdateViewpoint(new VideoViewpoint(0, 0, 0, scale < 1 ? 0.5f : -0.5f), false);
                        
                    }
                    else
                    {
                        var yaw = (float)(DEFAULT_FOV * -e.Cumulative.Translation.X / App.RootPage.SwapChainPanel.ActualWidth) / DIVIDER;
                        var pitch = (float)(DEFAULT_FOV * -e.Cumulative.Translation.Y / App.RootPage.SwapChainPanel.ActualHeight) / DIVIDER;
                        var viewpoint = new VideoViewpoint(yaw, pitch, 0, 0);
                        _playbackService.UpdateViewpoint(viewpoint, false);
                    }
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
                    _viewModel.Volume = computeVolumeFromGesture(e.Cumulative);
                    break;
                case GestureActionType.Brightness:
                    break;
                case GestureActionType.Seek:
                    var timeInSeconds = e.Cumulative.Translation.X;
                    Locator.MediaPlaybackViewModel.Time += (int)timeInSeconds * 100;
                    break;
                case GestureActionType.Exlore3D:
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
            var currentVol = _viewModel.Volume;
            var volumeDelta = (int)Math.Floor(-cumulative.Translation.Y / 5);
            var newVol = currentVol + volumeDelta;
            if (newVol > _viewModel.VOLUME_MAX)
                newVol = _viewModel.VOLUME_MAX;
            else if (newVol < _viewModel.VOLUME_MIN)
                newVol = 0;
            return newVol;
        }

        private void FullscreenToggle_Click(object sender, RoutedEventArgs e)
        {
            AppViewHelper.ToggleFullscreen();
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.PlayNextCommand.Execute(null);
        }

        private void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.PlayPreviousCommand.Execute(null);
        }

        private void PlaceholderInteractionGrid_OnPointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            var properties = e.GetCurrentPoint(RootGrid).Properties;
            if (properties.IsHorizontalMouseWheel || !Locator.VideoPlayerVm.Is3DVideo) return;

            var fov = (float)-properties.MouseWheelDelta;

            _playbackService.UpdateViewpoint(new VideoViewpoint(0, 0, 0, fov / 10), false );         
        }

        private void FlyoutBase_OnOpened(object sender, object e)
        {
            SubtitlesSubItem.Items.Clear();
            foreach (var sub in Locator.MediaPlaybackViewModel.Subtitles)
            {
                VideoSubItem.Items.Add(new MenuFlyoutItem()
                {
                    Text = sub.Name,
                });
            }
            if (!Locator.MediaPlaybackViewModel.Subtitles.Any())
            {
                SubtitlesSubItem.IsEnabled = false;
            }

            AudioTracksSubItem.Items.Clear();
            foreach (var audTrack in Locator.MediaPlaybackViewModel.AudioTracks)
            {
                AudioTracksSubItem.Items.Add(new MenuFlyoutItem()
                {
                    Text = audTrack.Name,
                    Command = new ActionCommand(() =>
                    {
                        Locator.MediaPlaybackViewModel.CurrentAudioTrack = audTrack;
                    })
                });
            }
        }

        private void PlaceholderInteractionGrid_OnRightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            var grid = sender as UIElement;
            VideoFlyout.ShowAt(grid, e.GetPosition(grid));
        }
    }
}
