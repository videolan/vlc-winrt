using Windows.UI.Xaml.Controls;
using VLC_WINRT_APP.ViewModels;

namespace VLC_WINRT_APP.Views.UserControls
{
    public sealed partial class FocusTextBox : TextBox
    {
        public FocusTextBox()
        {
            this.InitializeComponent();
            this.GotFocus += FocusTextBox_GotFocus;
            this.LostFocus += FocusTextBox_LostFocus;
        }

        void FocusTextBox_LostFocus(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Locator.MainVM.KeyboardListenerService.CanListen = true;
        }

        void FocusTextBox_GotFocus(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Locator.MainVM.KeyboardListenerService.CanListen = false;
        }
    }
}
