using System;
using System.ComponentModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using VLC_WinRT.Model.Music;
using VLC_WinRT.Utils;
using VLC_WinRT.ViewModels;
using VLC_WinRT.ViewModels.MusicVM;
using VLC_WinRT.Views.UserControls;
using System.Diagnostics;
using System.Threading.Tasks;

namespace VLC_WinRT.UI.Legacy.Views.UserControls
{
    public sealed partial class TrackItemTemplateDetailedVertical : UserControl
    {
        private Brush previousBrush = null;
        public TrackItemTemplateDetailedVertical()
        {
            this.InitializeComponent();
            this.Unloaded += TrackItemTemplate_Unloaded;
        }

        private void TrackItemTemplate_Unloaded(object sender, RoutedEventArgs e)
        {
            Locator.MediaPlaybackViewModel.TrackCollection.PropertyChanged -= TrackItemOnPropertyChanged;
        }
        
        public TrackItem Track
        {
            get { return (TrackItem)GetValue(TrackProperty); }
            set { SetValue(TrackProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Track.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TrackProperty =
            DependencyProperty.Register("Track", typeof(TrackItem), typeof(TrackItemTemplateDetailedVertical), new PropertyMetadata(null, PropertyChangedCallback));

        private static void PropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var that = (TrackItemTemplateDetailedVertical)dependencyObject;
            that.Init();
        }

        public void Init()
        {
            if (Track == null) return;
            NameTextBlock.Text = Track.Name;
            ArtistAlbumNameTextBlock.Text = Track.ArtistName + Strings.Dash + Track.AlbumName;
            var trackItem = Track;
            Task.Run(async () =>
            {
                var uri = await trackItem.LoadThumbnail();
                if (!string.IsNullOrEmpty(uri))
                {
                    await DispatchHelper.InvokeAsync(Windows.UI.Core.CoreDispatcherPriority.Low, () => CoverImage.Source = new BitmapImage(new Uri(uri)));
                }
            });

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
            if (Track == null) return;
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
