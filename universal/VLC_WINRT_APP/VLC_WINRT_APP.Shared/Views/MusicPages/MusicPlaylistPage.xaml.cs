#if WINDOWS_PHONE_APP
using Windows.Phone.UI.Input;
#endif
using Windows.UI.Xaml.Controls;

namespace VLC_WINRT_APP.Views.MusicPages
{
    public sealed partial class MusicPlaylistPage : Page
    {
        public MusicPlaylistPage()
        {
            this.InitializeComponent();
            this.Loaded += MusicPlaylistPage_Loaded;
        }

        private void MusicPlaylistPage_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
#if WINDOWS_PHONE_APP
            HardwareButtons.BackPressed += HardwareButtons_BackPressed;
#endif
            this.Unloaded += MusicPlaylistPage_Unloaded;
        }

#if WINDOWS_PHONE_APP
        private void HardwareButtons_BackPressed(object sender, BackPressedEventArgs e)
        {
            e.Handled=true;
            App.ApplicationFrame.GoBack();
        }
#endif

        private void MusicPlaylistPage_Unloaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
#if WINDOWS_PHONE_APP
            HardwareButtons.BackPressed -= HardwareButtons_BackPressed;
#endif
        }
    }
}
