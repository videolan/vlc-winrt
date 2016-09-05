using System.Linq;
using VLC.Model;
using VLC.Utils;
using VLC.ViewModels;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace VLC.UI.UWP.Views.VariousPages
{
    public sealed partial class ExternalStorageIncludePage
    {
        public ExternalStorageIncludePage()
        {
            this.InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Locator.NavigationService.GoBack_HideFlyout();

            if (Index.IsChecked == true)
                Locator.ExternalDeviceService.AskExternalDeviceIndexing();
            else if (Select.IsChecked == true)
                // TODO: open the explorer window of the added device.
                Locator.MainVM.CurrentPanel = Locator.MainVM.Panels.FirstOrDefault(x => x.Target == VLCPage.MainPageFileExplorer);
        }

        /* We want the flyout to fit the size of its content and not
         * be hidden on background click so we had this member that
         * is detected by the SplitShell. */
        public static object ModalMode;
    }
}
