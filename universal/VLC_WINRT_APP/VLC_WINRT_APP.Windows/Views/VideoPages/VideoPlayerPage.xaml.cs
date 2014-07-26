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
        }

        private async void Subtitles_Click(object sender, RoutedEventArgs e)
        {
            PopupMenu popup = new PopupMenu();
            for (int i = 0; i < MathHelper.Clamp(0, 5, Locator.VideoVm.SubtitlesCount); i++)
            {
                popup.Commands.Add(new UICommand()
                {
                    Id = Locator.VideoVm.SubtitlesTracks.ElementAt(i).Key,
                    Label = Locator.VideoVm.SubtitlesTracks.ElementAt(i).Value,
                    Invoked = command => Locator.VideoVm.SetSubtitleTrackCommand.Execute(command.Id),
                });
            }
            popup.Commands.Add(new UICommand()
            {
                Label = "Open subtitle file",
                Invoked = command => Locator.VideoVm.OpenSubtitleCommand.Execute(""),
            });
            await popup.ShowForSelectionAsync(((Button)sender).GetBoundingRect());
        }

        private async void AudioTracks_Click(object sender, RoutedEventArgs e)
        {
            PopupMenu popup = new PopupMenu();
            for (int i = 0; i < MathHelper.Clamp(0, 6, Locator.VideoVm.AudioTracksCount); i++)
            {
                popup.Commands.Add(new UICommand()
                {
                    Id = Locator.VideoVm.AudioTracks.ElementAt(i).Key,
                    Label = Locator.VideoVm.AudioTracks.ElementAt(i).Value,
                    Invoked = command => Locator.VideoVm.SetAudioTrackCommand.Execute(command.Id),
                });
            }
            await popup.ShowForSelectionAsync(((Button)sender).GetBoundingRect());
        }

        private void Grid_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {

        }

        void SetRate()
        {
            //Locator.PlayVideoVM.SetRate(rate);
            //SpeedRateTextBlock.Text = "x " + rate.ToString();
        }

        //private void IncreaseRate_Click(object sender, RoutedEventArgs e)
        //{
        //    rate = MathHelper.Clamp(0.5f, 2.0f, rate + 0.5f);
        //    SetRate();
        //}

        //private void DecreaseRate_Click(object sender, RoutedEventArgs e)
        //{
        //    rate = MathHelper.Clamp(0.5f, 2.0f, rate - 0.5f);
        //    SetRate();
        //}

        //private void Grid_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        //{
        //    isFingerOnScreen = false;
        //}

        private void VideoGrid_Tapped(object sender, TappedRoutedEventArgs e)
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
            if(timer.IsEnabled)
                timer.Stop();
        }
    }
}
