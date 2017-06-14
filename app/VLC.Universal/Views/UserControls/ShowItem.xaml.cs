using VLC.Model.Video;
using VLC.Utils;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace VLC.UI.Views.UserControls
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
            TVShow.PropertyChanged += TVShow_PropertyChanged;
            if(TVShow.ShowImage != null)
                FadeOutCover.Begin();
        }

        private async void TVShow_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TVShow.ShowImage))
            {
                await DispatchHelper.InvokeInUIThread(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    FadeOutCover.Begin();
                });
            }
        }

        private void FadeOutCover_Completed(object sender, object e)
        {
            if (TVShow != null && TVShow.ShowImage != null)
            {
                ThumbnailImage.Source = TVShow.ShowImage;
                FadeInCover.Begin();
            }
        }
    }
}
