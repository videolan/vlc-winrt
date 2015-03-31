using Windows.UI.Xaml.Controls;
using VLC_WinRT.Model.Music;
using VLC_WinRT.ViewModels.MusicVM;

namespace VLC_WinRT.Views.MainPages.MusicPanes.ArtistCollectionPanes
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
                item?.TrackClicked.Execute(e);
            }
        }
    }
}
