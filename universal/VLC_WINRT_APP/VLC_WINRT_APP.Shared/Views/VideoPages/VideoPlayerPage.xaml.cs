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
        private DispatcherTimer timer;

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
#if WINDOWS_APP
#if DEBUG
#else
            AppViewHelper.SetFullscren(true);
#endif
#endif
            // If no playback was ever started, ContinueIndexing can be null
            // If we navigate back and forth to the main page, we also don't want to 
            // re-mark the task as completed.
            Locator.MediaPlaybackViewModel.ContinueIndexing = new TaskCompletionSource<bool>();
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(5);
            timer.Tick += TimerOnTick;
            timer.Start();
#if WINDOWS_PHONE_APP
            HardwareButtons.BackPressed += HardwareButtonsOnBackPressed;
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
            if (Locator.MediaPlaybackViewModel.ContinueIndexing != null && !Locator.MediaPlaybackViewModel.ContinueIndexing.Task.IsCompleted)
            {
                Locator.MediaPlaybackViewModel.ContinueIndexing.SetResult(true);
            }
#if WINDOWS_PHONE_APP
            HardwareButtons.BackPressed -= HardwareButtonsOnBackPressed;
#endif
        }

        private async void TimerOnTick(object sender, object o)
        {
            await DisplayOrHide();
        }

        async Task DisplayOrHide()
        {
            if (timer == null)
                timer = new DispatcherTimer();
            isVisible = !isVisible;
        }
    }
}
