using Windows.UI.Xaml.Controls;
using VLC_WINRT_APP.Model.Music;
using VLC_WINRT_APP.ViewModels.MusicVM;

namespace VLC_WINRT_APP.Views.MainPages.MusicPanes.ArtistCollectionPanes
{
    public sealed partial class TracksListView : Grid
    {
        public TracksListView()
        {
            this.InitializeComponent();
        }

        private void ListViewBase_OnItemClick(object sender, ItemClickEventArgs e)
        {
            var vm = this.DataContext as MusicLibraryVM;
            if (vm != null)
            {
                vm.PlayAllSongsCommand.Execute(e);
            }
            else
            {
                var item = this.DataContext as AlbumItem;
                if (item != null)
                {
                    item.TrackClicked.Execute(e);
                }
            }
        }
    }
}
