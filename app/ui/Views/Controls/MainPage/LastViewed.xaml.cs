using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236
using VLC_WINRT.Utility.Commands;

namespace VLC_WINRT.Views.Controls.MainPage
{
    public sealed partial class LastViewed : UserControl
    {
        public LastViewed()
        {
            InitializeComponent();
        }

        private void FavoriteAlbumItemClick(object sender, ItemClickEventArgs e)
        {
            new PlayAlbumCommand().Execute(e.ClickedItem);
        }
    }
}