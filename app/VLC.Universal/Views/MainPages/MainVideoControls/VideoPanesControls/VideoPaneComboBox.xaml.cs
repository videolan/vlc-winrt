using Windows.UI.Xaml.Controls;
using VLC.ViewModels;

namespace VLC.Views.MainPages.MainVideoControls.VideoPanesControls
{
    public sealed partial class VideoPaneComboBox : UserControl
    {
        public VideoPaneComboBox()
        {
            this.InitializeComponent();
        }

        private void ComboBox_OnDropDownOpened(object sender, object e)
        {
            Locator.NavigationService.PreventAppExit = true;
        }

        private void ComboBox_OnDropDownClosed(object sender, object e)
        {
            Locator.NavigationService.PreventAppExit = false;
        }
    }
}
