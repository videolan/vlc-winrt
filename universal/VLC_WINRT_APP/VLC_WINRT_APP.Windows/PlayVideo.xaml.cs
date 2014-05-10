/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System;
using System.Linq;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using VLC_WINRT.Utility.Helpers;
using VLC_WINRT.ViewModels;

namespace VLC_WINRT.Views
{
    /// <summary>
    ///     An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PlayVideo : BasePage
    {
        float rate = 1.0f;
        private bool isCommandShown;
        private bool isFingerOnScreen = false;
        DispatcherTimer _timer = new DispatcherTimer();
        public PlayVideo()
        {
            InitializeComponent();
            this.SizeChanged += OnSizeChanged;

            _timer.Interval = TimeSpan.FromSeconds(5);
            _timer.Tick += TimerOnTick;
            ShowCommands();
        }

        private void TimerOnTick(object sender, object o)
        {
            if (isCommandShown)
                HideCommands();
        }

        private void OnPointerEntered(object sender, PointerRoutedEventArgs pointerRoutedEventArgs)
        {
            ShowCommands();
        }
        private void Grid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ShowCommands();
        }

        private void ShowCommands()
        {
            SpeedRateTextBlock.Visibility = Visibility.Visible;
            _timer.Start();
            UIAnimationHelper.FadeIn(Commands);
            isCommandShown = true;
        }

        void HideCommands()
        {
            _timer.Stop();
            UIAnimationHelper.FadeOut(Commands);
            SpeedRateTextBlock.Visibility = Visibility.Collapsed;
            isCommandShown = false;
        }
        private async void OnSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
        {
            var x = Window.Current.Bounds.Width;
            var y = Window.Current.Bounds.Height;
            await Locator.PlayVideoVM.SetSizeVideoPlayer((uint)x, (uint)y);

            if (x == 320)
            {
                ControlsGrid.Height = 200;
                BackButton.Margin = new Thickness(5, 50, 0, 50);
                MainButtonsNormal.Visibility = Visibility.Collapsed;
                SecondaryButtonsNormalLeft.Visibility = Visibility.Collapsed;
                SecondaryButtonsNormalRight.Visibility = Visibility.Collapsed;

                MainButtonsSnapped.Visibility = Visibility.Visible;
                SecondaryButtonsSnapped.Visibility = Visibility.Visible;
            }
            else
            {
                ControlsGrid.Height = 165;
                BackButton.Margin = new Thickness(50, 50, 0, 50);
                MainButtonsNormal.Visibility = Visibility.Visible;
                SecondaryButtonsNormalLeft.Visibility = Visibility.Visible;
                SecondaryButtonsNormalRight.Visibility = Visibility.Visible;

                MainButtonsSnapped.Visibility = Visibility.Collapsed;
                SecondaryButtonsSnapped.Visibility = Visibility.Collapsed;
            }
        }

        public override void SetDataContext()
        {
            _vm = (NavigateableViewModel)DataContext;
            base.SetDataContext();
        }

        private async void Subtitles_Click(object sender, RoutedEventArgs e)
        {
            PopupMenu popup = new PopupMenu();
            for (int i = 0; i < MathHelper.Clamp(0, 5, Locator.PlayVideoVM.SubtitlesCount); i++)
            {
                popup.Commands.Add(new UICommand()
                {
                    Id = Locator.PlayVideoVM.SubtitlesTracks.ElementAt(i).Key,
                    Label = Locator.PlayVideoVM.SubtitlesTracks.ElementAt(i).Value,
                    Invoked = command => Locator.PlayVideoVM.SetSubtitleTrackCommand.Execute(command.Id),
                });
            }
            popup.Commands.Add(new UICommand()
            {
                Label = "Open subtitle file",
                Invoked = command => Locator.PlayVideoVM.OpenSubtitleCommand.Execute(""),
            });
            await popup.ShowForSelectionAsync(((Button)sender).GetBoundingRect());
        }

        private async void AudioTracks_Click(object sender, RoutedEventArgs e)
        {
            PopupMenu popup = new PopupMenu();
            for (int i = 0; i < MathHelper.Clamp(0, 6, Locator.PlayVideoVM.AudioTracksCount); i++)
            {
                popup.Commands.Add(new UICommand()
                {
                    Id = Locator.PlayVideoVM.AudioTracks.ElementAt(i).Key,
                    Label = Locator.PlayVideoVM.AudioTracks.ElementAt(i).Value,
                    Invoked = command => Locator.PlayVideoVM.SetAudioTrackCommand.Execute(command.Id),
                });
            }
            await popup.ShowForSelectionAsync(((Button)sender).GetBoundingRect());
        }

        private void Grid_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {

        }

        private void Grid_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (e.Cumulative.Translation.X > 50 && !isFingerOnScreen)
            {
                Locator.PlayVideoVM.SkipAhead.Execute("");
                SpeedRateTextBlock.Text = "+ 10s";
                e.Handled = true;
                isFingerOnScreen = true;
            }
            else if (e.Cumulative.Translation.X < -50 && !isFingerOnScreen)
            {
                Locator.PlayVideoVM.SkipBack.Execute("");
                SpeedRateTextBlock.Text = "- 10s";
                e.Handled = true;
                isFingerOnScreen = true;
            }
        }

        void SetRate()
        {
            Locator.PlayVideoVM.SetRate(rate);
            SpeedRateTextBlock.Text = "x " + rate.ToString();
        }

        private void IncreaseRate_Click(object sender, RoutedEventArgs e)
        {
            rate = MathHelper.Clamp(0.5f, 2.0f, rate + 0.5f);
            SetRate();
        }

        private void DecreaseRate_Click(object sender, RoutedEventArgs e)
        {
            rate = MathHelper.Clamp(0.5f, 2.0f, rate - 0.5f);
            SetRate();
        }

        private void Grid_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            isFingerOnScreen = false;
        }
    }
}
