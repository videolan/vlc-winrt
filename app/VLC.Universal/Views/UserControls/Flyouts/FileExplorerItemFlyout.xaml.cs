using Windows.UI.Xaml.Controls;
namespace VLC.Views.UserControls.Flyouts
{
    public sealed partial class FileExplorerItemFlyout : MenuFlyout
    {
        public FileExplorerItemFlyout()
        {
            this.InitializeComponent();
        }

        public void setCurrentVLCStorageItem(object o)
        {
            CopyToLocalStorageItem.CommandParameter = o;
        }
    }
}
