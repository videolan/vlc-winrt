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
using System.Threading.Tasks;
using Windows.Foundation;
using VLC_WinRT.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using VLC_WinRT.Helpers;
using Windows.Graphics.Display;
using Windows.UI.Input;
using Windows.UI.Popups;
using VLC_WinRT.Model.Video;
using VLC_WinRT.Services.RunTime;
using VLC_WinRT.Utils;

namespace VLC_WinRT.Views.VideoPages
{
    public sealed partial class VideoPlayerPage : Page
    {
        private bool isVisible = true;
        private bool isLocked = false;
        private GestureActionType currentGestureActionType;
        public VideoPlayerPage()
        {
            InitializeComponent();
            this.Loaded += VideoPlayerPage_Loaded;
        }

        void VideoPlayerPage_Loaded(object sender, RoutedEventArgs e)
        {
            this.Unloaded += VideoPlayerPage_Unloaded;
            this.SizeChanged += VideoPlayerPage_SizeChanged;
            Responsive();
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
            else if (width < 1050)
            {
                VisualStateManager.GoToState(this, "Medium", false);
            }
            else
            {
                VisualStateManager.GoToState(this, "Full", false);
            }
            Locator.MediaPlaybackViewModel._mediaService.SetSizeVideoPlayer((uint)Math.Ceiling(App.RootPage.SwapChainPanel.ActualWidth), (uint)Math.Ceiling(App.RootPage.SwapChainPanel.ActualHeight));
            Locator.VideoVm.ChangeSurfaceZoom(Locator.VideoVm.CurrentSurfaceZoom);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            App.RootPage.SwapChainPanel.Visibility = Visibility.Visible;
            Locator.MediaPlaybackViewModel.MouseService.OnHidden += MouseStateChanged;
            Locator.MediaPlaybackViewModel.MouseService.OnMoved += MouseMoved;
            Locator.VideoVm.OnNavigatedTo();
            Responsive();
            AppViewHelper.FullscreenStateChanged += FullScreenStateChanged;
            FullScreenStateChanged();
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            App.RootPage.SwapChainPanel.Visibility = Visibility.Collapsed;
            Locator.VideoVm.OnNavigatedFrom();
            AppViewHelper.FullscreenStateChanged -= FullScreenStateChanged;
            App.SplitShell.TitleBarHeight = AppViewHelper.TitleBarHeight;
        }

        private void FullScreenStateChanged()
        {
            if (AppViewHelper.IsFullScreen())
            {
                App.SplitShell.TitleBarHeight = 0;
            }
            else
            {
                App.SplitShell.TitleBarHeight = AppViewHelper.TitleBarHeight;
            }
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
                Locator.VideoVm.IsVideoPlayerOptionsPanelVisible ||
                Locator.VideoVm.IsVideoPlayerSubtitlesSettingsVisible ||
                Locator.VideoVm.IsVideoPlayerVolumeSettingsVisible)
                return;
            if (!isVisible)
            {
                ControlsGridFadeOut.Value = ControlsGrid.ActualHeight + ControlsGrid.Padding.Top +
                                            ControlsGrid.Padding.Bottom;
                HeaderGridFadeOut.Value = -HeaderGrid.ActualHeight;
                FadeOut.Begin();
            }
            else
            {
                FadeIn.Begin();
            }
        }

        private void PlaceholderInteractionGrid_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            if (e.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Touch)
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
            VolumeSlider.IsEnabled = !isLocked;
            ZoomButton.IsEnabled = !isLocked;
            MenuButton.IsEnabled = !isLocked;
            DisplayProperties.AutoRotationPreferences = (isLocked) ? DisplayInformation.GetForCurrentView().CurrentOrientation : DisplayOrientations.None;

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

        private void PlaceholderInteractionGrid_OnRightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            PopMainMenu();
        }

        private async void PopMainMenu()
        {
            if (isLocked) return;
            var pos = MouseService.GetPointerPosition();
            var menu = new PopupMenu();
            menu.Commands.Add(new UICommand(Locator.MediaPlaybackViewModel.IsPlaying ? Strings.Pause : Strings.Play, command => Locator.MediaPlaybackViewModel.Pause()));
            menu.Commands.Add(new UICommand(Strings.Stop, command => Locator.MediaPlaybackViewModel.GoBack.Execute(null)));
            menu.Commands.Add(new UICommandSeparator());
            menu.Commands.Add(new UICommand(Strings.Audio, PopAudioMenu));
            menu.Commands.Add(new UICommand(Strings.Video, PopVideoMenu));
            menu.Commands.Add(new UICommand(Strings.Playback, PopPlaybackMenu));
            await menu.ShowForSelectionAsync(new Rect(pos, pos), Placement.Right);
        }

        private async void PopVideoMenu(IUICommand c)
        {
            var pos = MouseService.GetPointerPosition();
            var menu = new PopupMenu();
            menu.Commands.Add(new UICommand(Strings.Back, command => PopMainMenu()));
            menu.Commands.Add(new UICommand("Fullscreen toggle", command => AppViewHelper.SetFullscreen()));
            menu.Commands.Add(new UICommand(Strings.Zoom, command => Locator.VideoVm.ToggleIsVideoPlayerOptionsPanelVisible.Execute(null)));
            menu.Commands.Add(new UICommandSeparator());
            menu.Commands.Add(new UICommand(Strings.Subtitles, command => Locator.VideoVm.ToggleIsVideoPlayerSubtitlesSettingsVisible.Execute(null)));
            await menu.ShowForSelectionAsync(new Rect(pos, pos), Placement.Right);
        }

        private async void PopPlaybackMenu(IUICommand c)
        {
            var pos = MouseService.GetPointerPosition();
            var menu = new PopupMenu();
            menu.Commands.Add(new UICommand(Strings.Back, command => PopMainMenu()));
            menu.Commands.Add(new UICommand(Strings.Chapters, command => Locator.VideoVm.ToggleIsVideoPlayerOptionsPanelVisible.Execute(null)));
            menu.Commands.Add(new UICommandSeparator());
            menu.Commands.Add(new UICommand(Strings.IncreaseSpeed, command => Locator.MediaPlaybackViewModel.ChangePlaybackSpeedRateCommand.Execute("faster")));
            menu.Commands.Add(new UICommand(Strings.DecreaseSpeed, command => Locator.MediaPlaybackViewModel.ChangePlaybackSpeedRateCommand.Execute("slower")));
            menu.Commands.Add(new UICommand(Strings.ResetSpeed, command => Locator.MediaPlaybackViewModel.ChangePlaybackSpeedRateCommand.Execute("reset")));
            await menu.ShowForSelectionAsync(new Rect(pos, pos), Placement.Right);

        }

        private async void PopAudioMenu(IUICommand c)
        {
            var pos = MouseService.GetPointerPosition();
            var menu = new PopupMenu();
            menu.Commands.Add(new UICommand(Strings.Back, command => PopMainMenu()));
            menu.Commands.Add(new UICommand(Strings.AudioTracks, command => Locator.VideoVm.ToggleIsVideoPlayerAudioTracksSettingsVisible.Execute(null)));
            menu.Commands.Add(new UICommandSeparator());
            menu.Commands.Add(new UICommand(Strings.IncreaseVolume, command => Locator.MediaPlaybackViewModel.ChangeVolumeCommand.Execute("higher")));
            menu.Commands.Add(new UICommand(Strings.DecreaseVolume, command => Locator.MediaPlaybackViewModel.ChangeVolumeCommand.Execute("lower")));
            menu.Commands.Add(new UICommand(Strings.Mute, command => Locator.MediaPlaybackViewModel.ChangeVolumeCommand.Execute("mute")));
            await menu.ShowForSelectionAsync(new Rect(pos, pos), Placement.Right);
        }
    }
}
