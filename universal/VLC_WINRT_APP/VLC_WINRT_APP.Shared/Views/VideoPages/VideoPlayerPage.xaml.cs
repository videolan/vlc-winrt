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
using VLC_WINRT_APP.ViewModels;
using Windows.Devices.Input;
using Windows.Graphics.Display;
using Windows.System;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using VLC_WINRT_APP.Helpers;
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
        private TimeSpan _fadeDuration = TimeSpan.FromMilliseconds(250);
        private DispatcherTimer timer;

        public VideoPlayerPage()
        {
            InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
#if WINDOWS_APP
#if DEBUG
#else
            AppViewHelper.SetFullscren(true);
#endif
#endif
            // If no playback was ever started, ContinueIndexing can be null
            // If we navigate back and forth to the main page, we also don't want to 
            // re-mark the task as completed.
            App.IMediaService.ContinueIndexing = new TaskCompletionSource<bool>();
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(5);
            timer.Tick += TimerOnTick;
            timer.Start();
#if WINDOWS_PHONE_APP
            HardwareButtons.BackPressed += HardwareButtonsOnBackPressed;
            StatusBar sB = StatusBar.GetForCurrentView();
            await sB.HideAsync();
#endif
        }

#if WINDOWS_PHONE_APP
        private async void HardwareButtonsOnBackPressed(object sender, BackPressedEventArgs backPressedEventArgs)
        {
            backPressedEventArgs.Handled = true;
            Locator.MediaPlaybackViewModel.GoBack.Execute("");
            DisplayInformation.AutoRotationPreferences = DisplayOrientations.None;
        }
#endif
        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
#if WINDOWS_APP
            AppViewHelper.SetFullscren(false);
#endif
            if (App.IMediaService.ContinueIndexing != null && !App.IMediaService.ContinueIndexing.Task.IsCompleted)
            {
                App.IMediaService.ContinueIndexing.SetResult(true);
            }
#if WINDOWS_PHONE_APP
            HardwareButtons.BackPressed -= HardwareButtonsOnBackPressed;
#endif
        }

        private async void VideoGrid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (!needDoubleTapToAct)
                await DisplayOrHide();
        }

        private async void VideoGrid_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            await DisplayOrHide();
        }

        private async void TimerOnTick(object sender, object o)
        {
            await DisplayOrHide();
        }

        async Task DisplayOrHide()
        {
            if (timer == null)
                timer = new DispatcherTimer();
            Task t1;
            Task t2;
            if (isVisible)
            {
                t1 = ControlsGrid.FadeOut(_fadeDuration);
                t2 = FooterGrid.FadeOut(_fadeDuration);
                FooterGrid.IsHitTestVisible = false;
                timer.Stop();
            }
            else
            {
                t1 = FooterGrid.FadeIn(_fadeDuration);
                t2 = ControlsGrid.FadeIn(_fadeDuration);
                FooterGrid.IsHitTestVisible = true;
                timer.Start();
            }
            await Task.WhenAll(t1, t2);
            isVisible = !isVisible;
        }

        private void EnableDoubleTapToShowCommands_Click(object sender, RoutedEventArgs e)
        {
            needDoubleTapToAct = !needDoubleTapToAct;
            timer.Start();
        }

        private void ControlsGrid_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (e.PointerDeviceType == PointerDeviceType.Mouse)
                return;
            if (e.Cumulative.Translation.X > 100)
            {
                Locator.MediaPlaybackViewModel.SkipAhead.Execute(null);
                e.Handled = true;
            }
            else if (e.Cumulative.Translation.X < -100)
            {
                Locator.MediaPlaybackViewModel.SkipBack.Execute(null);
                e.Handled = true;
            }
        }
    }
}
