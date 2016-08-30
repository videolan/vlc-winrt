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

namespace VLC.UI.Legacy.Views.UserControls
{
    public sealed partial class CommandBarBottom : CommandBar
    {
        public CommandBarBottom()
        {
            this.InitializeComponent();
            this.Loaded += CommandBarBottom_Loaded;
            this.Opened += CommandBarBottom_Opened;
        }

        private void CommandBarBottom_Opened(object sender, object e)
        {
            UpdatePlayerVisibility();
        }

        #region init
        private void CommandBarBottom_Loaded(object sender, RoutedEventArgs e)
        {
            UpdatePlayerVisibility();
            Locator.MusicPlayerVM.PropertyChanged += MusicPlayerVM_PropertyChanged;
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

        private void MusicPlayerVM_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Locator.MusicPlayerVM.IsMiniPlayerVisible))
            {
                this.MiniPlayerVisibility = Locator.MusicPlayerVM.IsMiniPlayerVisible;
            }
        }
        #endregion

        #region properties
        public Visibility MiniPlayerVisibility
        {
            get { return (Visibility)GetValue(MiniPlayerVisibilityProperty); }
            set { SetValue(MiniPlayerVisibilityProperty, value); }
        }

        public static readonly DependencyProperty MiniPlayerVisibilityProperty =
            DependencyProperty.Register(nameof(MiniPlayerVisibility), typeof(Visibility), typeof(CommandBarBottom), new PropertyMetadata(Visibility.Collapsed, PlayerVisibilityChanged));

        private static void PlayerVisibilityChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var that = (CommandBarBottom)obj;
            that.UpdatePlayerVisibility();
        }

        public void UpdatePlayerVisibility()
        {
            NowPlayingArtistGrid.Visibility =
                PlayPreviousButton.Visibility =
                PlayNextButton.Visibility =
                MiniPlayerVisibility;

            var shuffleButton = FindName(nameof(ShuffleButton)) as FrameworkElement;
            if (shuffleButton != null)
                shuffleButton.Visibility = MiniPlayerVisibility;

            var repeatButton = FindName(nameof(RepeatButton)) as FrameworkElement;
            if (repeatButton != null)
                repeatButton.Visibility = MiniPlayerVisibility;

            var miniWindowButton = FindName(nameof(MiniWindowButton)) as FrameworkElement;
            if (miniWindowButton != null)
            {
                if (DeviceHelper.GetDeviceType() != DeviceTypeEnum.Tablet || UIViewSettings.GetForCurrentView().UserInteractionMode == UserInteractionMode.Touch)
                {
                    miniWindowButton.Visibility = Visibility.Collapsed;
                }
                else
                    miniWindowButton.Visibility = MiniPlayerVisibility;
            }

            if (App.SplitShell.FooterVisibility != AppBarClosedDisplayMode.Hidden)
                App.SplitShell.FooterVisibility = MiniPlayerVisibility == Visibility.Visible ? AppBarClosedDisplayMode.Compact : AppBarClosedDisplayMode.Minimal;
        }

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

            UpdatePlayerVisibility();
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
            Locator.MediaPlaybackViewModel.PlaybackService.Stop();
            await Locator.MediaPlaybackViewModel.PlaybackService.ResetCollection();
        }
    }
}
