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
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
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

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(6);
            timer.Tick += TimerOnTick;
            timer.Start();
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

        async Task DisplayOrHide()
        {
            if (isVisible)
            {
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
                timer.Start();
            }
            isVisible = !isVisible;
        }

        private void ControlsGrid_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (timer.IsEnabled)
                timer.Stop();
        }

        private void EnableDoubleTapToShowCommands_Click(object sender, RoutedEventArgs e)
        {
            needDoubleTapToAct = !needDoubleTapToAct;
            timer.Start();
        }
    }
}
