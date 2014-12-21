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
using Windows.Phone.UI.Input;
using Windows.System;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using WinRTXamlToolkit.Controls.Extensions;

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

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(5);
            timer.Tick += TimerOnTick;
            timer.Start();
            HardwareButtons.BackPressed += HardwareButtonsOnBackPressed;
            StatusBar sB = StatusBar.GetForCurrentView();
            await sB.HideAsync();
        }

        private async void HardwareButtonsOnBackPressed(object sender, BackPressedEventArgs backPressedEventArgs)
        {
            backPressedEventArgs.Handled = true;
            if (isVisible)
            {
                await DisplayOrHide();
            }
            else
            {
                Locator.VideoVm.GoBack.Execute("");
                DisplayInformation.AutoRotationPreferences = DisplayOrientations.None;
            }
        }

        protected override async void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            HardwareButtons.BackPressed -= HardwareButtonsOnBackPressed;
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
                Locator.VideoVm.SkipAhead.Execute(null);
                e.Handled = true;
            }
            else if (e.Cumulative.Translation.X < -100)
            {
                Locator.VideoVm.SkipBack.Execute(null);
                e.Handled = true;
            }
        }
    }
}
