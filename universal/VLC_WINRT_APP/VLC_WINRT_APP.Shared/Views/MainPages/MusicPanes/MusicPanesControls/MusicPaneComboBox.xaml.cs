using Windows.UI.Xaml.Controls;
using VLC_WINRT_APP.ViewModels;

namespace VLC_WINRT_APP.Views.MainPages.MusicPanes.MusicPanesControls
{
    public sealed partial class MusicPaneComboBox : UserControl
    {
        public MusicPaneComboBox()
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
