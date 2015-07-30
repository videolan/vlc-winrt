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
        }


        private void ItemsWrapGrid_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            var itemsWrapGrid = sender as ItemsWrapGrid;
            TemplateSizer.ComputeAlbums(itemsWrapGrid, itemsWrapGrid.ActualWidth- itemsWrapGrid.Margin.Left- itemsWrapGrid.Margin.Right);
        }
    }
}
