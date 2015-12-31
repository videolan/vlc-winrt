using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using VLC_WinRT.Model.Music;
using Windows.UI.Xaml.Media;
using VLC_WinRT.Utils;
using System.ComponentModel;
using VLC_WinRT.ViewModels;
using VLC_WinRT.ViewModels.MusicVM;

namespace VLC_WinRT.Views.UserControls
{
    public sealed partial class TrackItemTemplate : UserControl
    {
        private Brush previousBrush = null;
        public TrackItemTemplate()
        {
            this.InitializeComponent();
            this.Unloaded += TrackItemTemplate_Unloaded;
        }

        private void TrackItemTemplate_Unloaded(object sender, RoutedEventArgs e)
        {
            Locator.MediaPlaybackViewModel.TrackCollection.PropertyChanged -= TrackItemOnPropertyChanged;
        }

        private void Grid_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            Flyout.ShowAttachedFlyout((Grid)sender);
        }


        public TrackItem Track
        {
            get { return (TrackItem)GetValue(TrackProperty); }
            set { SetValue(TrackProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Track.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TrackProperty =
            DependencyProperty.Register("Track", typeof(TrackItem), typeof(TrackItemTemplate), new PropertyMetadata(null, PropertyChangedCallback));

        private static void PropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var that = (TrackItemTemplate)dependencyObject;
            that.Init();
        }

        public void Init()
        {
            if (Track == null) return;
            NameTextBlock.Text = Track.Name;
            DurationTextBlock.Text = Strings.HumanizeSeconds(Track.Duration.TotalSeconds);

            Locator.MediaPlaybackViewModel.TrackCollection.PropertyChanged += TrackItemOnPropertyChanged;
            UpdateTrack();
        }

        private void TrackItemOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == nameof(TrackCollection.CurrentTrack))
            {
                UpdateTrack();
            }
        }

        void UpdateTrack()
        {
            if (Locator.MediaPlaybackViewModel.TrackCollection.CurrentTrack == -1 || Locator.MediaPlaybackViewModel.TrackCollection.Playlist?.Count == 0) return;
            if (Track.Id == Locator.MediaPlaybackViewModel.TrackCollection.Playlist[Locator.MediaPlaybackViewModel.TrackCollection.CurrentTrack].Id)
            {
                previousBrush = NameTextBlock.Foreground;
                NameTextBlock.Foreground = (Brush)App.Current.Resources["MainColor"];
            }
            else
            {
                if (previousBrush != null) NameTextBlock.Foreground = previousBrush;
            }
        }
    }
}
