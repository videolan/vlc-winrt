using VLC_WinRT.Model.Video;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace VLC_WinRT.Views.MusicPages.ArtistPageControls
{
    public sealed partial class ArtistAlbumsList : UserControl
    {
        public ArtistAlbumsList()
        {
            this.InitializeComponent();
            this.SizeChanged += ArtistAlbumsList_SizeChanged;
        }

        private void ArtistAlbumsList_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var itemsWrapGrid = AlbumsListView.ItemsPanelRoot as ItemsWrapGrid;
            TemplateSizer.ComputeAlbums(itemsWrapGrid, AlbumsListView.ActualWidth - itemsWrapGrid.Margin.Left - itemsWrapGrid.Margin.Right);
        }
    }
}
