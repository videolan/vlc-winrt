using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using VLC.Helpers;
using VLC.Model.Music;
using VLC.Utils;
using VLC.ViewModels;
using VLC.ViewModels.MusicVM;
using VLC.Model;

namespace VLC.UI.Views.UserControls
{
    public sealed partial class TrackItemTemplateDetailed : UserControl
    {
        private Brush _previousBrush;

        public TrackItemTemplateDetailed()
        {
            InitializeComponent();
            Unloaded += TrackItemTemplateDetailed_Unloaded;
        }

        public TrackItem Track
        {
            get { return (TrackItem)GetValue(TrackProperty); }
            set { SetValue(TrackProperty, value); }
        }

        public bool IsFlyoutEnabled
        {
            get { return (bool)GetValue(IsFlyoutEnabledProperty); }
            set { SetValue(IsFlyoutEnabledProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsFlyoutEnabled.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsFlyoutEnabledProperty = DependencyProperty.Register(nameof(IsFlyoutEnabled), 
            typeof(bool), typeof(TrackItemTemplateDetailed), new PropertyMetadata(true));
        
        // Using a DependencyProperty as the backing store for Track.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TrackProperty = DependencyProperty.Register(nameof(Track), 
            typeof(TrackItem), typeof(TrackItemTemplateDetailed), new PropertyMetadata(null, PropertyChangedCallback));

        private static void PropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var that = (TrackItemTemplateDetailed)dependencyObject;
            that.Init();
        }

        public void Init()
        {
            if (Track == null)
                return;

            Opacity = Track.IsAvailable ? 1 : Numbers.NotAvailableFileItemOpacity;
            NameTextBlock.Text = Track.Name;
            ArtistNameTextBlock.Text = Strings.HumanizedArtistName(Track.ArtistName);
            AlbumNameTextBlock.Text = Strings.HumanizedAlbumName(Track.AlbumName);
            DurationTextBlock.Text = Strings.HumanizeSeconds(Track.Duration.TotalSeconds);

            Locator.MediaPlaybackViewModel.PlaybackService.Playback_MediaSet += UpdateTrack;

            var currentMedia = Locator.MediaPlaybackViewModel.PlaybackService.CurrentPlaybackMedia;
            if(currentMedia != null)
                UpdateTrack(currentMedia);
        }
        
        private void Grid_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            if (IsFlyoutEnabled)
                Flyout.ShowAttachedFlyout((Grid)sender);
        }
        
        private void TrackItemTemplateDetailed_Unloaded(object sender, RoutedEventArgs e)
        {
            if (Track == null) return;

            Locator.MediaPlaybackViewModel.PlaybackService.Playback_MediaSet -= UpdateTrack;
        }

        private async void UpdateTrack(IMediaItem media)
        {
            await DispatchHelper.InvokeInUIThread(Windows.UI.Core.CoreDispatcherPriority.Low, () =>
            {
                if (Track == null)
                    return;

                if (media.Id == Track.Id && media.IsCurrentPlaying())
                {
                    _previousBrush = NameTextBlock.Foreground;
                    NameTextBlock.Foreground = (Brush)Application.Current.Resources["MainColor"];
                    MusicLogo.Visibility = Visibility.Visible;
                }
                else
                {
                    MusicLogo.Visibility = Visibility.Collapsed;
                    if (_previousBrush != null)
                        NameTextBlock.Foreground = _previousBrush;
                }
            });
        }

        private void Grid_Holding(object sender, HoldingRoutedEventArgs e)
        {
            if (IsFlyoutEnabled)
                Flyout.ShowAttachedFlyout((Grid)sender);
        }
    }
}