using System;
using System.ComponentModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using VLC.Model.Music;
using VLC.Utils;
using VLC.ViewModels;
using VLC.ViewModels.MusicVM;
using VLC.UI.Views.UserControls;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI;
using VLC.Model;

namespace VLC.UI.Views.UserControls
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
            Locator.MediaPlaybackViewModel.PlaybackService.Playback_MediaSet -= UpdateTrack;
        }

        public TrackItem Track
        {
            get { return GetValue(TrackProperty) as TrackItem; }
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

            this.Opacity = Track.IsAvailable ? 1 : Numbers.NotAvailableFileItemOpacity;
            NameTextBlock.Text = Track.Name;
            ArtistAlbumNameTextBlock.Text = Track.ArtistName + Strings.Dash + Track.AlbumName;

            Track.PropertyChanged += Track_PropertyChanged;
            var trackItem = Track;
            Task.Run(async () =>
            {
                await trackItem.ResetAlbumArt();
            });

            Locator.MediaPlaybackViewModel.PlaybackService.Playback_MediaSet += UpdateTrack;
            UpdateTrack(Track);
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

        async void UpdateTrack(IMediaItem media)
        {
            await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Low, () =>
            {
                if (Track == null)
                    return;

                if (Locator.MediaPlaybackViewModel.PlaybackService.CurrentPlaylistIndex == -1 || Locator.MediaPlaybackViewModel.PlaybackService.Playlist?.Count == 0)
                    return;

                if (Track.IsCurrentPlaying())
                {
                    RootGrid.Background = (Brush)App.Current.Resources["MainColor"];
                }
                else
                {
                    RootGrid.Background = new SolidColorBrush(Colors.Transparent);
                }
            });
        }
    }
}
