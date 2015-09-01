using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using VLC_WinRT.Model.Video;

namespace VLC_WinRT.Views.MainPages.MusicPanes
{
    public sealed partial class AlbumCollectionBase : UserControl
    {
        public AlbumCollectionBase()
        {
            this.InitializeComponent();
        }

        private void AlbumsWrapGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            TemplateSizer.ComputeAlbums(sender as ItemsWrapGrid, AlbumsZoomedInView.ItemsPanelRoot.ActualWidth - 6);
        }

        private void SemanticZoom_OnViewChangeCompleted(object sender, SemanticZoomViewChangedEventArgs e)
        {
            AlbumsZoomedOutView.ItemsSource = GroupAlbums.View.CollectionGroups;
        }
    }
}
