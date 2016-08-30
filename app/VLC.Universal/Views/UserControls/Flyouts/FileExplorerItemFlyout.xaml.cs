using Windows.UI.Xaml.Controls;
namespace VLC.Views.UserControls.Flyouts
{
    public sealed partial class FileExplorerItemFlyout : MenuFlyout
    {
        public FileExplorerItemFlyout()
        {
            this.InitializeComponent();
        }

        public void setCurrentVLCStorageItem(object storageItem)
        {
            this.InitializeComponent();
            foreach (var item in MenuFlyout.Items)
                item.DataContext = storageItem;
        }
    }
}
