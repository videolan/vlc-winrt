/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System;
using Windows.Devices.Input;
using Windows.Graphics.Display;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using VLC_WINRT_APP.Helpers;
using VLC_WINRT_APP.ViewModels;
using WinRTXamlToolkit.Controls.Extensions;
#if WINDOWS_PHONE_APP
using Windows.Phone.UI.Input;
#endif
namespace VLC_WINRT_APP.Views.VideoPages
{
    public sealed partial class VideoPlayerPage : Page
    {
        private bool isVisible = true;
        private bool needDoubleTapToAct = false;
        private TimeSpan _fadeDuration = TimeSpan.FromMilliseconds(350);
        private DispatcherTimer timer;

        public VideoPlayerPage()
        {
            InitializeComponent();
        }

        private void Responsive()
        {
#if WINDOWS_APP
            if (Window.Current.Bounds.Width < 550)
            {
                LeftButtons.Visibility = Visibility.Collapsed;
                RightButtons.Visibility = Visibility.Collapsed;
            }
            else
            {
                LeftButtons.Visibility = Visibility.Visible;
                RightButtons.Visibility = Visibility.Visible;
            }
#else
            VolumeSlider.Visibility = Visibility.Collapsed;
#endif
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
        {
            Responsive();
        }

        protected override void OnNavigatedTo(Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(5);
            timer.Tick += TimerOnTick;
            timer.Start();
            this.SizeChanged += OnSizeChanged;
            Window.Current.Content.AddHandler(KeyDownEvent, new KeyEventHandler(KeyPressedDown), true);
            Responsive();
#if WINDOWS_PHONE_APP
            HardwareButtons.BackPressed += HardwareButtonsOnBackPressed;
            DisplayInformation.AutoRotationPreferences = DisplayOrientations.Landscape;
#endif
        }

#if WINDOWS_PHONE_APP
        private void HardwareButtonsOnBackPressed(object sender, BackPressedEventArgs backPressedEventArgs)
        {
            backPressedEventArgs.Handled = true;
            if (isVisible)
            {
                DisplayOrHide();
            }
            else
            {
                Locator.VideoVm.GoBack.Execute("");
                DisplayInformation.AutoRotationPreferences = DisplayOrientations.None;
            }
        }
#endif
        protected override void OnNavigatingFrom(Windows.UI.Xaml.Navigation.NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            Window.Current.Content.RemoveHandler(KeyDownEvent, new KeyEventHandler(KeyPressedDown));
            this.SizeChanged -= OnSizeChanged;
#if WINDOWS_PHONE_APP
            HardwareButtons.BackPressed -= HardwareButtonsOnBackPressed;
#endif
        }


        private void KeyPressedDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Space)
            {
                Locator.VideoVm.PlayOrPauseCommand.Execute("");
            }
        }



        private void VideoGrid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (!needDoubleTapToAct)
                DisplayOrHide();
        }

        private void VideoGrid_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            DisplayOrHide();
        }

        private void TimerOnTick(object sender, object o)
        {
            DisplayOrHide();
        }

        async void DisplayOrHide()
        {
            if (timer == null)
                timer = new DispatcherTimer();
            if (isVisible)
            {
                ControlsGrid.FadeOut(_fadeDuration);
                HeaderGrid.FadeOut(_fadeDuration);
                HeaderGrid.IsHitTestVisible = false;
                FooterGrid.FadeOut(_fadeDuration);
                FooterGrid.IsHitTestVisible = false;
                timer.Stop();
            }
            else
            {
                HeaderGrid.FadeIn(_fadeDuration);
                HeaderGrid.IsHitTestVisible = true;
                FooterGrid.FadeIn(_fadeDuration);
                FooterGrid.IsHitTestVisible = true;
                ControlsGrid.FadeIn(_fadeDuration);
                timer.Start();
            }
            isVisible = !isVisible;
        }

        private void ControlsGrid_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
#if WINDOWS_APP
            if (timer == null)
                timer = new DispatcherTimer();

            if (timer.IsEnabled)
            {
                timer.Stop();
                timer.Start();
            }
            else
            {
                DisplayOrHide();
            }   
#endif
        }

        private void EnableDoubleTapToShowCommands_Click(object sender, RoutedEventArgs e)
        {
            needDoubleTapToAct = !needDoubleTapToAct;
            timer.Start();
        }

        private void Flyout_Opening(object sender, object e)
        {
#if WINDOWS_PHONE_APP
            (sender as Flyout).Placement = FlyoutPlacementMode.Full;
            ((sender as Flyout).Content as Grid).Margin = new Thickness(0, 0, 36, 0);
#endif
        }

        private void ControlsGrid_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (e.PointerDeviceType == PointerDeviceType.Mouse)
                return;
            if (e.Cumulative.Translation.X > 100)
            {
                Locator.VideoVm.SkipAhead.Execute(null);
                e.Handled = true;
            }
            else if (e.Cumulative.Translation.X < -100)
            {
                Locator.VideoVm.SkipBack.Execute(null);
                e.Handled = true;
            }
        }

        private void ControlsGrid_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {

        }

        private void LockToggleButton_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (!CapabilitiesHelper.IsTouchCapable)
            {
                LockToggleButton.Visibility = Visibility.Collapsed;
            }
        }
    }
}
