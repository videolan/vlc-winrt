using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using VLC.Model.Video;
using VLC.ViewModels;

namespace VLC_WinRT.Views.MainPages.MusicPanes
{
    public sealed partial class AlbumCollectionBase : UserControl
    {
        public AlbumCollectionBase()
        {
            this.InitializeComponent();
            this.SizeChanged += AlbumCollectionBase_SizeChanged;
        }

        private void AlbumCollectionBase_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (AlbumsZoomedInView.ItemsPanelRoot == null) return;
            TemplateSizer.ComputeAlbums(AlbumsZoomedInView.ItemsPanelRoot as ItemsWrapGrid, AlbumsZoomedInView.ItemsPanelRoot.ActualWidth - 6);
        }
    }
}
