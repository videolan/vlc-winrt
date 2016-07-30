using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using VLC.Model.Video;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace VLC.Views.UserControls
{
    public sealed partial class ShowItem : UserControl
    {
        public ShowItem()
        {
            this.InitializeComponent();
        }

        public TvShow TVShow
        {
            get { return (TvShow)GetValue(TVShowProperty); }
            set { SetValue(TVShowProperty, value); }
        }

        public static readonly DependencyProperty TVShowProperty =
            DependencyProperty.Register(nameof(TVShow), typeof(VideoItem), typeof(ShowItem), new PropertyMetadata(null, PropertyChangedCallback));

        private static void PropertyChangedCallback(DependencyObject dO, DependencyPropertyChangedEventArgs args)
        {
            var that = (ShowItem)dO;
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
    }
}
