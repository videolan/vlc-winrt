using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using VLC_WinRT.Model.Video;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace VLC_WinRT.Views.UserControls
{
    public sealed partial class ShowItemTemplate : UserControl
    {
        public ShowItemTemplate()
        {
            this.InitializeComponent();
        }

        public TvShow TVShow
        {
            get { return (TvShow)GetValue(TVShowProperty); }
            set { SetValue(TVShowProperty, value); }
        }

        public static readonly DependencyProperty TVShowProperty =
            DependencyProperty.Register(nameof(TVShow), typeof(VideoItem), typeof(ShowItemTemplate), new PropertyMetadata(null, PropertyChangedCallback));

        private static void PropertyChangedCallback(DependencyObject dO, DependencyPropertyChangedEventArgs args)
        {
            var that = (ShowItemTemplate)dO;
            that.Init();
        }

        public void Init()
        {
            if (TVShow == null)
                return;

            NameTextBlock.Text = TVShow.ShowTitle;
            ThumbnailImage.Source = TVShow.ShowImage;
            FadeInCover.Begin();
        }

        //private async void Video_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        //{
        //    if (e.PropertyName == nameof(Video.VideoImage))
        //    {
        //        if (Video == null) return;
        //        await DispatchHelper.InvokeAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
        //        {
        //            FadeOutCover.Begin();
        //        });
        //    }
        //    else if (e.PropertyName == nameof(Video.Duration) || e.PropertyName == nameof(Video.TimeWatched))
        //    {
        //        UpdateVideoDurations();
        //    }
        //}

        //async void UpdateVideoDurations()
        //{
        //    await DispatchHelper.InvokeAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
        //    {
        //        TimeWatchedTextBlock.Text = Strings.HumanizedTimeSpan(Video.TimeWatched);
        //        DurationTextBlock.Text = Strings.HumanizedTimeSpan(Video.Duration);

        //        VideoProgressBar.Value = Video.TimeWatched.TotalSeconds;
        //        VideoProgressBar.Maximum = Video.Duration.TotalSeconds;

        //        VideoProgressBar.Visibility = Video.TimeWatched.TotalSeconds > 0 ? Visibility.Visible : Visibility.Collapsed;
        //    });
        //}

        //private void FadeOutCover_Completed(object sender, object e)
        //{
        //    if (Video != null && Video.VideoImage != null)
        //    {
        //        ThumbnailImage.Source = Video.VideoImage;
        //        FadeInCover.Begin();
        //    }
        //}
    }
}
