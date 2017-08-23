using Windows.UI.Xaml.Controls;
namespace VLC.Universal8._1.Views.UserControls.Flyouts
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
            //foreach (var item in AlbumFlyoutMenu.Items)
            //    item.DataContext = albumItem;
        }
    }
}
