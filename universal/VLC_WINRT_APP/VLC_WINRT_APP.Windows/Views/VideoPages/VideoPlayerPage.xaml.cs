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
using VLC_WINRT_APP.Helpers;
using VLC_WINRT_APP.ViewModels;
using WinRTXamlToolkit.Controls.Extensions;

namespace VLC_WINRT_APP.Views.VideoPages
{
    /// <summary>
    ///     An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class VideoPlayerPage : Page
    {
        public VideoPlayerPage()
        {
            InitializeComponent();
        }

        private void GoBack(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
            Locator.VideoVm.CleanViewModel();
            Locator.VideoVm.IsRunning = false;
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
    }
}
