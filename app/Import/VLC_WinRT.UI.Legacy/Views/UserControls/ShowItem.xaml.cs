using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using VLC.Model.Video;

namespace VLC_WinRT.Views.UserControls
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
