using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using VLC_WINRT_APP.Views.MusicPages;

namespace VLC_WINRT_APP.Controls
{
    public sealed partial class SnapMiniPlayer : UserControl
    {
        public SnapMiniPlayer()
        {
            this.InitializeComponent();
        }

        private void UIElement_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            if(App.ApplicationFrame.CurrentSourcePageType != typeof(MusicPlayerPage))
                App.ApplicationFrame.Navigate(typeof (MusicPlayerPage));
        }
    }
}