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
using Windows.UI.Core;
using Windows.UI;

namespace VLC_WinRT.UI.Legacy.Views.UserControls
{
    public sealed partial class TrackItemTemplateDetailedVertical : UserControl
    {
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
        
        public static readonly DependencyProperty TrackProperty = DependencyProperty.Register(nameof(Track), typeof(TrackItem), typeof(TrackItemTemplateDetailedVertical), new PropertyMetadata(null, PropertyChangedCallback));

        private static void PropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var that = (TrackItemTemplateDetailedVertical)dependencyObject;
            that.Init();
        }

        public void Init()
        {
            if (Track == null)
                return;
            NameTextBlock.Text = Track.Name;
            ArtistAlbumNameTextBlock.Text = Track.ArtistName + Strings.Dash + Track.AlbumName;

            Track.PropertyChanged += Track_PropertyChanged;
            var trackItem = Track;
            Task.Run(async () =>
            {
                await trackItem.ResetAlbumArt();
            });

            Locator.MediaPlaybackViewModel.TrackCollection.PropertyChanged += TrackItemOnPropertyChanged;
            UpdateTrack();
        }

        private async void Track_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Track.AlbumImage))
            {
                if (Track == null)
                    return;
                await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Low, () =>
                {
                    CoverImage.Source = Track.AlbumImage;
                });
            }
        }

        private void TrackItemOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == nameof(PlaylistItem.CurrentMedia))
            {
                UpdateTrack();
            }
        }

        void UpdateTrack()
        {
            if (Track == null)
                return;
            if (Locator.MediaPlaybackViewModel.TrackCollection.CurrentMedia == -1 || Locator.MediaPlaybackViewModel.TrackCollection.Playlist?.Count == 0) return;
            if (Track.Id == Locator.MediaPlaybackViewModel.TrackCollection.Playlist[Locator.MediaPlaybackViewModel.TrackCollection.CurrentMedia].Id)
            {
                RootGrid.Background = (Brush)App.Current.Resources["MainColor"];
            }
            else
            {
                RootGrid.Background = new SolidColorBrush(Colors.Transparent);
            }
        }
    }
}
