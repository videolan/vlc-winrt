using Windows.UI.Xaml.Controls;
namespace VLC.UI.Views.UserControls.Flyouts
{
    public sealed partial class AlbumMenuFlyout : MenuFlyout
    {
        public AlbumMenuFlyout()
        {
            this.InitializeComponent();
        }

        public AlbumMenuFlyout(object albumItem)
        {
            this.InitializeComponent();
            foreach (var item in MenuFlyout.Items)
                item.DataContext = albumItem;
        }
    }
}
