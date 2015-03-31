using Windows.UI.Xaml.Controls;
using VLC_WinRT.ViewModels;
#if WINDOWS_PHONE_APP
using Windows.Phone.UI.Input;
#endif

namespace VLC_WinRT.Views.MainPages.MusicPanes.MusicPanesControls
{
    public sealed partial class MusicPaneComboBox : UserControl
    {
        public MusicPaneComboBox()
        {
            this.InitializeComponent();
#if WINDOWS_PHONE_APP
            this.Loaded += MusicPaneComboBox_Loaded;
#endif
        }

#if WINDOWS_PHONE_APP
        void MusicPaneComboBox_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            HardwareButtons.BackPressed += HardwareButtons_BackPressed;
            this.Unloaded += MusicPaneComboBox_Unloaded;
        }

        void HardwareButtons_BackPressed(object sender, BackPressedEventArgs e)
        {
            if (MusicViewComboBox.IsDropDownOpen)
            {
                e.Handled = true;
                MusicViewComboBox.IsDropDownOpen = false;
            }
        }

        void MusicPaneComboBox_Unloaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            HardwareButtons.BackPressed -= HardwareButtons_BackPressed;
        }
#endif
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
