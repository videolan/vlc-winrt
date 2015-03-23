using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
namespace VLC_WINRT_APP.Views.MainPages.MusicPanes.MusicPanesControls
{
    public sealed partial class MusicPaneButtons : Grid
    {
        public MusicPaneButtons()
        {
            this.InitializeComponent();
        }

        private void RefreshButton_Loaded(object sender, RoutedEventArgs e)
        {
#if WINDOWS_PHONE_APP
            (sender as AppBarButton).Visibility = Visibility.Collapsed;
#endif
        }
    }
}
