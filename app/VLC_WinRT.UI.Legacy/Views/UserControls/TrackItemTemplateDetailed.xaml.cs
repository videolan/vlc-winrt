using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using VLC_WinRT.Model.Music;
using VLC_WinRT.Utils;
using VLC_WinRT.ViewModels;
using VLC_WinRT.ViewModels.MusicVM;

namespace VLC_WinRT.Views.UserControls
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
            DependencyProperty.Register("IsFlyoutEnabled", typeof(bool), typeof(TrackItemTemplateDetailed), new PropertyMetadata(true));
        
        public TrackItem Track
        {
            get { return (TrackItem)GetValue(TrackProperty); }
            set { SetValue(TrackProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Track.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TrackProperty =
            DependencyProperty.Register("Track", typeof(TrackItem), typeof(TrackItemTemplateDetailed), new PropertyMetadata(null, PropertyChangedCallback));

        private static void PropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var that = (TrackItemTemplateDetailed)dependencyObject;
            that.Init();
        }

        public void Init()
        {
            if (Track == null) return;
            NameTextBlock.Text = Track.Name;
            ArtistNameTextBlock.Text = Strings.HumanizedArtistName(Track.ArtistName);
            AlbumNameTextBlock.Text = Strings.HumanizedAlbumName(Track.AlbumName);
            DurationTextBlock.Text = Strings.HumanizeSeconds(Track.Duration.TotalSeconds);
            Track.PropertyChanged += TrackItemOnPropertyChanged;
            UpdateTrack();
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
            Track.PropertyChanged -= TrackItemOnPropertyChanged;
        }

        private void TrackItemOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == nameof(TrackItem.IsCurrentPlaying))
            {
                UpdateTrack();
            }
        }

        void UpdateTrack()
        {
            if (Track.IsCurrentPlaying)
            {
                previousBrush = NameTextBlock.Foreground;
                NameTextBlock.Foreground = (Brush)App.Current.Resources["MainColor"];
                MusicLogo.Visibility = Visibility.Visible;
            }
            else
            {
                MusicLogo.Visibility = Visibility.Collapsed;
                if (previousBrush != null) NameTextBlock.Foreground = previousBrush;
            }
        }
        private void Grid_Holding(object sender, HoldingRoutedEventArgs e)
        {
            if (IsFlyoutEnabled)
                Flyout.ShowAttachedFlyout((Grid)sender);
        }
    }
}
