using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using VLC.Model.Music;
using VLC.Utils;
using VLC.ViewModels;
using VLC.ViewModels.MusicVM;
using VLC.Model;

namespace VLC.Views.UserControls
{
    public sealed partial class TrackItemTemplateDetailed : UserControl
    {
        private Brush previousBrush = null;
        public TrackItemTemplateDetailed()
        {
            this.InitializeComponent();
        }

        public bool IsFlyoutEnabled
        {
            get { return (bool)GetValue(IsFlyoutEnabledProperty); }
            set { SetValue(IsFlyoutEnabledProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsFlyoutEnabled.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsFlyoutEnabledProperty =
            DependencyProperty.Register(nameof(IsFlyoutEnabled), typeof(bool), typeof(TrackItemTemplateDetailed), new PropertyMetadata(true));
        
        public TrackItem Track
        {
            get { return (TrackItem)GetValue(TrackProperty); }
            set { SetValue(TrackProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Track.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TrackProperty =
            DependencyProperty.Register(nameof(Track), typeof(TrackItem), typeof(TrackItemTemplateDetailed), new PropertyMetadata(null, PropertyChangedCallback));

        private static void PropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var that = (TrackItemTemplateDetailed)dependencyObject;
            that.Init();
        }

        public void Init()
        {
            if (Track == null)
                return;

            this.Opacity = Track.IsAvailable ? 1 : Numbers.NotAvailableFileItemOpacity;
            NameTextBlock.Text = Track.Name;
            ArtistNameTextBlock.Text = Strings.HumanizedArtistName(Track.ArtistName);
            AlbumNameTextBlock.Text = Strings.HumanizedAlbumName(Track.AlbumName);
            DurationTextBlock.Text = Strings.HumanizeSeconds(Track.Duration.TotalSeconds);

            Locator.MediaPlaybackViewModel.PlaybackService.Playback_MediaSet += UpdateTrack;
            UpdateTrack(Track);
        }

        private void Grid_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            if (IsFlyoutEnabled)
                Flyout.ShowAttachedFlyout((Grid)sender);
        }

        private void NameTextBlock_OnLoaded(object sender, RoutedEventArgs e)
        {
            this.Unloaded += TrackItemTemplateDetailed_Unloaded;
        }

        private void TrackItemTemplateDetailed_Unloaded(object sender, RoutedEventArgs e)
        {
            if (Track != null)
                Locator.MediaPlaybackViewModel.PlaybackService.Playback_MediaSet -= UpdateTrack;
        }
        

        async void UpdateTrack(IMediaItem media)
        {
            await DispatchHelper.InvokeAsync(Windows.UI.Core.CoreDispatcherPriority.Low, () =>
            {
                if (Track == null)
                    return;
                if (Track.IsCurrentPlaying())
                {
                    previousBrush = NameTextBlock.Foreground;
                    NameTextBlock.Foreground = (Brush)App.Current.Resources["MainColor"];
                    MusicLogo.Visibility = Visibility.Visible;
                }
                else
                {
                    MusicLogo.Visibility = Visibility.Collapsed;
                    if (previousBrush != null)
                        NameTextBlock.Foreground = previousBrush;
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
