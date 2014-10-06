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
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.Devices.Input;
using Windows.System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using VLC_WINRT_APP.Helpers;
using VLC_WINRT_APP.ViewModels;
using WinRTXamlToolkit.Controls.Extensions;

namespace VLC_WINRT_APP.Views.VideoPages
{
    public sealed partial class VideoPlayerPage : Page
    {
        private bool isVisible = true;
        private bool needDoubleTapToAct = false;
        private TimeSpan _fadeDuration = TimeSpan.FromMilliseconds(350);
        DispatcherTimer timer;

        public VideoPlayerPage()
        {
            InitializeComponent();
            this.Loaded += OnLoaded;
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
        {
            if (sizeChangedEventArgs.NewSize.Width < 550)
            {
                LockToggleButton.Visibility = Visibility.Collapsed;
                MenuButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                LockToggleButton.Visibility = Visibility.Visible;
                MenuButton.Visibility = Visibility.Visible;
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(6);
            timer.Tick += TimerOnTick;
            timer.Start();
            this.SizeChanged += OnSizeChanged;
            this.Unloaded += OnUnloaded;
            Window.Current.Content.AddHandler(KeyDownEvent, new KeyEventHandler(KeyPressedDown), true);
        }

        private void KeyPressedDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Space)
            {
                Locator.VideoVm.PlayOrPauseCommand.Execute("");
            }
        }

        private void OnUnloaded(object sender, RoutedEventArgs routedEventArgs)
        {
            Window.Current.Content.RemoveHandler(KeyDownEvent, new KeyEventHandler(KeyPressedDown));
            this.SizeChanged -= OnSizeChanged;
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
            timer.Stop();
        }

        async void DisplayOrHide()
        {
            if (timer == null)
                timer = new DispatcherTimer();
            if (isVisible)
            {
                await ControlsGrid.FadeOut(_fadeDuration);
                await HeaderGrid.FadeOut(_fadeDuration);
                HeaderGrid.IsHitTestVisible = false;
                await FooterGrid.FadeOut(_fadeDuration);
                FooterGrid.IsHitTestVisible = false;
            }
            else
            {
                await HeaderGrid.FadeIn(_fadeDuration);
                HeaderGrid.IsHitTestVisible = true;
                await FooterGrid.FadeIn(_fadeDuration);
                FooterGrid.IsHitTestVisible = true;
                await ControlsGrid.FadeIn(_fadeDuration);
                timer.Start();
            }
            isVisible = !isVisible;
        }


        private void ControlsGrid_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (timer == null)
                timer = new DispatcherTimer();

            if (timer.IsEnabled)
                timer.Stop();
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
