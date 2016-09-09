using System.Threading.Tasks;
using VLC.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace VLC.UI.Views.UserControls
{
    public sealed partial class CommandBarXbox : UserControl
    {
        public CommandBarXbox()
        {
            this.InitializeComponent();
            this.Loaded += CommandBarXbox_Loaded;
        }

        #region init
        private void CommandBarXbox_Loaded(object sender, RoutedEventArgs e)
        {
            UpdatePlayerVisibility();
            Locator.MusicPlayerVM.PropertyChanged += MusicPlayerVM_PropertyChanged;
            this.SizeChanged += CommandBarBottom_SizeChanged;
            App.SplitShell.ContentSizeChanged += SplitShell_ContentSizeChanged;
            Responsive();
            UpdatePlayerVisibility();
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
            var that = (CommandBarXbox)obj;
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
