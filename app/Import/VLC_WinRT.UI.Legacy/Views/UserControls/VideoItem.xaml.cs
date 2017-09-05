using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using VLC.Utils;

using VLC.Model;
using VLC.Model.Music;

namespace VLC_WinRT.Views.UserControls
{
    public sealed partial class VideoItem : UserControl
    {
        public VideoItem()
        {
            this.InitializeComponent();
        }

        private void RootAlbumItem_Holding(object sender, HoldingRoutedEventArgs e)
        {
            Flyout.ShowAttachedFlyout((Grid)sender);
        }

        private void Grid_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            Flyout.ShowAttachedFlyout((Grid)sender);
        }

        public VLC.Model.Video.VideoItem Video
        {
            get { return (VLC.Model.Video.VideoItem)GetValue(VideoProperty); }
            set { SetValue(VideoProperty, value); }
        }

        public static readonly DependencyProperty VideoProperty =
            DependencyProperty.Register(nameof(Video), typeof(VLC.Model.Video.VideoItem), typeof(VideoItem), new PropertyMetadata(null, PropertyChangedCallback));

        private static void PropertyChangedCallback(DependencyObject dO, DependencyPropertyChangedEventArgs args)
        {
            var that = (VideoItem)dO;
            that.Init();
        }

        public void Init()
        {
            if (Video == null)
                return;

            NameTextBlock.Text = Video.Name;
            UpdateVideoDurations();
            Video.PropertyChanged += Video_PropertyChanged;

            var video = Video;
            Task.Run( () =>
            {
                 video.InitializeVideoImage();
            });
        }

        private async void Video_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Video.VideoImage))
            {
                if (Video == null) return;
                await DispatchHelper.InvokeInUIThreadAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    FadeOutCover.Begin();
                });
            }
            else if (e.PropertyName == nameof(Video.Duration) || e.PropertyName == nameof(Video.TimeWatched))
            {
                UpdateVideoDurations();
            }
        }

        async void UpdateVideoDurations()
        {
            await DispatchHelper.InvokeInUIThreadAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                TimeWatchedTextBlock.Text = Strings.HumanizedTimeSpan(Video.TimeWatched);
                DurationTextBlock.Text = Strings.HumanizedTimeSpan(Video.Duration);

                VideoProgressBar.Value = Video.TimeWatched.TotalSeconds;
                VideoProgressBar.Maximum = Video.Duration.TotalSeconds;

                VideoProgressBar.Visibility = Video.TimeWatched.TotalSeconds > 0 ? Visibility.Visible : Visibility.Collapsed;
            });
        }

        private void FadeOutCover_Completed(object sender, object e)
        {
            if (Video != null && Video.VideoImage != null)
            {
                ThumbnailImage.Source = Video.VideoImage;
                FadeInCover.Begin();
            }
        }
    }
}
