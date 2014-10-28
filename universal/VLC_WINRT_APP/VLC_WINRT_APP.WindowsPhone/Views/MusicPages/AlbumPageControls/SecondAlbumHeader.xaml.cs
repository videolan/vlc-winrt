using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace VLC_WINRT_APP.Views.MusicPages.AlbumPageControls
{
    public sealed partial class SecondAlbumHeader : UserControl
    {
        public SecondAlbumHeader()
        {
            this.InitializeComponent();
        }

        private void SwypeLeftToRight_Button_Click(object sender, RoutedEventArgs e)
        {
            var albumPage = App.ApplicationFrame.Content as AlbumPage;
            if (albumPage != null)
                Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => albumPage.HeaderFlipView.SelectedIndex = 0);
        }
    }
}
