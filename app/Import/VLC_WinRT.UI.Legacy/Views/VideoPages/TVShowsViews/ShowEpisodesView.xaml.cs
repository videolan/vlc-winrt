using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using VLC.Model.Video;

namespace VLC_WinRT.UI.Legacy.Views.VideoPages.TVShowsViews
{
    public sealed partial class ShowEpisodesView : Page
    {
        public ShowEpisodesView()
        {
            this.InitializeComponent();
        }

        private void VideosWrapGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            TemplateSizer.ComputeCompactVideo(sender as ItemsWrapGrid, this.ActualWidth);
        }
    }
}
