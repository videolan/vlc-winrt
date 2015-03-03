using Windows.UI.Xaml.Controls;
using VLC_WINRT_APP.ViewModels;
namespace VLC_WINRT_APP.Views.MainPages.MainVideoControls.VideoPanesControls
{
    public sealed partial class VideoPaneComboBox : UserControl
    {
        public VideoPaneComboBox()
        {
            this.InitializeComponent();
        }

        private void ComboBox_OnDropDownOpened(object sender, object e)
        {
            Locator.MainVM.PreventAppExit = true;
        }

        private void ComboBox_OnDropDownClosed(object sender, object e)
        {
            Locator.MainVM.PreventAppExit = false;
        }
    }
}
