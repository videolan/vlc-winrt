using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using VLC.Helpers;
using VLC.ViewModels;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace VLC.UI.Views.UserControls
{
    public sealed partial class CommandBarBottom : CommandBar
    {
        public CommandBarBottom()
        {
            this.InitializeComponent();
            this.Loaded += CommandBarBottom_Loaded;
        }

        #region init
        private void CommandBarBottom_Loaded(object sender, RoutedEventArgs e)
        {
            this.SizeChanged += CommandBarBottom_SizeChanged;
            App.SplitShell.ContentSizeChanged += SplitShell_ContentSizeChanged;
            Responsive();
        }

        private void CommandBarBottom_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Responsive();
        }

        private void SplitShell_ContentSizeChanged(double newWidth)
        {
            Responsive();
        }

        #endregion

        #region properties

        #endregion

        #region interactions
        private void RootMiniPlayer_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Locator.MusicPlayerVM.GoToMusicPlayerPage.Execute(null);
        }
        #endregion

        void Responsive()
        {
            if (this.ActualWidth < 420)
            {
                TrackNameTextBlock.Visibility = ArtistNameTextBlock.Visibility = Visibility.Collapsed;
            }
            else
            {
                TrackNameTextBlock.Visibility = ArtistNameTextBlock.Visibility = Visibility.Visible;
            }
        }

        private async void PlayButton_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            await StopPlayback();
        }

        private async void PauseButton_Holding(object sender, HoldingRoutedEventArgs e)
        {
            await StopPlayback();
        }

        public async Task StopPlayback()
        {
            Locator.PlaybackService.Stop();
            await Locator.PlaybackService.ClearPlaylist();
        }
    }
}
