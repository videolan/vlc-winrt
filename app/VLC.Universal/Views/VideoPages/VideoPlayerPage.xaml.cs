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

namespace VLC.UI.Views.VideoPages
{
    public sealed partial class VideoPlayerPage : Page
    {
        private bool isVisible = true;
        private bool isLocked = false;
        private GestureActionType currentGestureActionType;
        private DispatcherTimer controlsTimer = new DispatcherTimer();

        public delegate void PlayerControlVisibilityChanged(bool visibility);
        public event PlayerControlVisibilityChanged OnPlayerControlVisibilityChanged;

        public VideoPlayerPage()
        {
            InitializeComponent();
            ControlsBorder.RegisterPropertyChangedCallback(Border.VisibilityProperty, OnBorderVisibilityChanged);
            Locator.MediaPlaybackViewModel.PlaybackService.Playback_MediaStopped += OnPlaybackStopped;
            FadeOut.Completed += FadeOut_Completed;
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

            Locator.MediaPlaybackViewModel.PlaybackService.SetSizeVideoPlayer((uint)Math.Ceiling(App.RootPage.SwapChainPanel.ActualWidth), (uint)Math.Ceiling(App.RootPage.SwapChainPanel.ActualHeight));
            Locator.VideoPlayerVm.ChangeSurfaceZoom(Locator.VideoPlayerVm.CurrentSurfaceZoom);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            // Set UI Layout
            App.RootPage.SwapChainPanel.Visibility = Visibility.Visible;
            App.SplitShell.FooterVisibility = AppBarClosedDisplayMode.Hidden;

            // UI interactions
            Locator.MediaPlaybackViewModel.MouseService.Start();
            Locator.MediaPlaybackViewModel.MouseService.OnMoved += ShowControlPanel;
            RootGrid.Tapped += RootGrid_Tapped;
            controlsTimer.Interval = TimeSpan.FromSeconds(5);
            controlsTimer.Tick += ControlsTimer_Tick;
            controlsTimer.Start();

            // VM initialization
            Locator.VideoPlayerVm.OnNavigatedTo();
            Locator.VideoPlayerVm.PlayerControlVisibilityChangeRequested += VideoPlayerVm_PlayerControlVisibilityChangeRequested;
            OnPlayerControlVisibilityChanged += Locator.VideoPlayerVm.OnPlayerControlVisibilityChanged;

            // Responsive design
            this.SizeChanged += OnSizeChanged;
            Responsive();

            // Swapchain animations
            App.RootPage.StartCompositionAnimationOnSwapChain(false);
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
            App.SplitShell.FooterVisibility = AppBarClosedDisplayMode.Minimal;
            Locator.NavigationService.CloseVideoFlyouts();

            Locator.VideoPlayerVm.OnNavigatedFrom();

            Locator.VideoPlayerVm.PlayerControlVisibilityChangeRequested -= VideoPlayerVm_PlayerControlVisibilityChangeRequested;
            OnPlayerControlVisibilityChanged -= Locator.VideoPlayerVm.OnPlayerControlVisibilityChanged;

            AppViewHelper.LeaveFullscreen();

            this.SizeChanged -= OnSizeChanged;

            Locator.MediaPlaybackViewModel.MouseService.Stop();
            Locator.MediaPlaybackViewModel.MouseService.OnMoved -= ShowControlPanel;
            controlsTimer.Tick -= ControlsTimer_Tick;
            controlsTimer.Stop();
            Locator.MediaPlaybackViewModel.MouseService.ShowCursor();
        }

        async void OnPlaybackStopped()
        {
            await DispatchHelper.InvokeAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                if (!Locator.NavigationService.GoBack_Default())
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
            if (isVisible == false || Locator.VideoPlayerVm.IsVideoPlayerOptionsPanelVisible)
                return;
            isVisible = false;
            ControlsGridFadeOut.Value = ControlsBorder.ActualHeight;
            HeaderGridFadeOut.Value = -HeaderGrid.ActualHeight;
            FadeOut.Begin();
            Locator.MediaPlaybackViewModel.MouseService.HideCursor();
        }

        void ShowControlPanel()
        {
            controlsTimer.Stop();
            controlsTimer.Start();
            Locator.MediaPlaybackViewModel.MouseService.ShowCursor();
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
            OnPlayerControlVisibilityChanged(false);
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

        private void PlaceholderInteractionGrid_Loaded(object sender, RoutedEventArgs args)
        {
            var menu = new MenuFlyout();

            menu.Items.Add(new MenuFlyoutItem()
            {
                Name = "PlayPauseItem",
                Command = new ActionCommand(() => Locator.MediaPlaybackViewModel.PlaybackService.Pause())
            });

            menu.Items.Add(new MenuFlyoutItem()
            {
                Text = Strings.Stop,
                Command = new ActionCommand(() => Locator.MediaPlaybackViewModel.GoBack.Execute(null))
            });

            menu.Items.Add(new MenuFlyoutSeparator());

            var audioSubItem = new MenuFlyoutSubItem();
            audioSubItem.Text = Strings.Audio;
            audioSubItem.Name = "AudioSubItem";

            var audioTracksSubItem = new MenuFlyoutSubItem()
            {
                Text = Strings.AudioTracks,
                Name = "AudioTracksSubItem"
            };
            audioSubItem.Items.Add(audioTracksSubItem);

            audioSubItem.Items.Add(new MenuFlyoutSeparator());
            audioSubItem.Items.Add(new MenuFlyoutItem()
            {
                Text = Strings.IncreaseVolume,
                Command = new ActionCommand(() => Locator.MediaPlaybackViewModel.ChangeVolumeCommand.Execute("higher"))
            });
            audioSubItem.Items.Add(new MenuFlyoutItem()
            {
                Text = Strings.DecreaseVolume,
                Command = new ActionCommand(() => Locator.MediaPlaybackViewModel.ChangeVolumeCommand.Execute("lower"))
            });
            audioSubItem.Items.Add(new MenuFlyoutItem()
            {
                Text = Strings.Mute,
                Command = new ActionCommand(() => Locator.MediaPlaybackViewModel.ChangeVolumeCommand.Execute("mute"))
            });

            menu.Items.Add(audioSubItem);

            var videoSubItem = new MenuFlyoutSubItem();
            videoSubItem.Text = Strings.Video;
            videoSubItem.Name = "VideoSubItem";
            videoSubItem.Items.Add(new MenuFlyoutItem()
            {
                Text = "Fullscreen toggle",
                Command = new ActionCommand(() => AppViewHelper.ToggleFullscreen())
            });
            videoSubItem.Items.Add(new MenuFlyoutItem()
            {
                Text = Strings.Zoom,
                Command = new ActionCommand(() => Locator.VideoPlayerVm.ToggleIsVideoPlayerOptionsPanelVisible.Execute(null))
            });
            videoSubItem.Items.Add(new MenuFlyoutSeparator());

            var subtitlesSubItem = new MenuFlyoutSubItem()
            {
                Text = Strings.Subtitles,
                Name = "SubtitlesSubItem"
            };
            videoSubItem.Items.Add(subtitlesSubItem);
            menu.Items.Add(videoSubItem);

            var playbackSubItem = new MenuFlyoutSubItem();
            playbackSubItem.Text = Strings.Playback;
            playbackSubItem.Items.Add(new MenuFlyoutItem()
            {
                Text = Strings.Chapters,
                Command = new ActionCommand(() => Locator.VideoPlayerVm.ToggleIsVideoPlayerOptionsPanelVisible.Execute(null))
            });
            playbackSubItem.Items.Add(new MenuFlyoutSeparator());
            playbackSubItem.Items.Add(new MenuFlyoutItem()
            {
                Text = Strings.IncreaseSpeed,
                Command = new ActionCommand(() => Locator.MediaPlaybackViewModel.ChangePlaybackSpeedRateCommand.Execute("faster"))
            });
            playbackSubItem.Items.Add(new MenuFlyoutItem()
            {
                Text = Strings.DecreaseSpeed,
                Command = new ActionCommand(() => Locator.MediaPlaybackViewModel.ChangePlaybackSpeedRateCommand.Execute("slower"))
            });
            playbackSubItem.Items.Add(new MenuFlyoutItem()
            {
                Text = Strings.ResetSpeed,
                Command = new ActionCommand(() => Locator.MediaPlaybackViewModel.ChangePlaybackSpeedRateCommand.Execute("reset"))
            });
            menu.Items.Add(playbackSubItem);

            menu.Opened += (s, e) =>
            {
                ((MenuFlyoutItem)menu.Items.FirstOrDefault(x => x.Name == "PlayPauseItem")).Text = Strings.Play + " " + Strings.Dash + " " + Strings.Pause;
                var videoSubItemMenu = (MenuFlyoutSubItem)menu.Items.FirstOrDefault(x => x.Name == "VideoSubItem");
                var subtitlesSubItemMenu = (MenuFlyoutSubItem)videoSubItemMenu.Items.FirstOrDefault(x => x.Name == "SubtitlesSubItem");
                subtitlesSubItemMenu.Items.Clear();
                foreach (var sub in Locator.MediaPlaybackViewModel.Subtitles)
                {
                    videoSubItemMenu.Items.Add(new MenuFlyoutItem()
                    {
                        Text = sub.Name,
                    });
                }
                if (!Locator.MediaPlaybackViewModel.Subtitles.Any())
                {
                    subtitlesSubItemMenu.IsEnabled = false;
                }

                var audioSubItemMenu = (MenuFlyoutSubItem)menu.Items.FirstOrDefault(x => x.Name == "AudioSubItem");
                var audioTracksSubItemMenu = (MenuFlyoutSubItem)audioSubItemMenu.Items.FirstOrDefault(x => x.Name == "AudioTracksSubItem");
                audioTracksSubItemMenu.Items.Clear();
                foreach (var audTrack in Locator.MediaPlaybackViewModel.AudioTracks)
                {
                    audioTracksSubItemMenu.Items.Add(new MenuFlyoutItem()
                    {
                        Text = audTrack.Name,
                        Command = new ActionCommand(() =>
                        {
                            Locator.MediaPlaybackViewModel.CurrentAudioTrack = audTrack;
                        })
                    });
                }
            };
            PlaceholderInteractionGrid.ContextFlyout = menu;
        }

        private void FullscreenToggle_Click(object sender, RoutedEventArgs e)
        {
            AppViewHelper.ToggleFullscreen();
        }
    }
}
