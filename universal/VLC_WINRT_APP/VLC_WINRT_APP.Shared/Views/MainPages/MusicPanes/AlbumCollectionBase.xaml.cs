using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using VLC_WINRT_APP.Helpers;
using VLC_WINRT_APP.Model.Video;
using VLC_WINRT_APP.ViewModels.MusicVM;
using VLC_WINRT_APP.Model.Music;

namespace VLC_WINRT_APP.Views.MainPages.MusicPanes
{
    public sealed partial class AlbumCollectionBase : UserControl
    {
        public AlbumCollectionBase()
        {
            this.InitializeComponent();
        }

        private void AlbumsWrapGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            TemplateSizer.ComputeAlbums(sender as ItemsWrapGrid, TemplateSize.Compact, this.ActualWidth);
        }

        private async void ListViewBase_OnContainerContentChanging(ListViewBase sender,
            ContainerContentChangingEventArgs args)
        {
        }
    }
}
