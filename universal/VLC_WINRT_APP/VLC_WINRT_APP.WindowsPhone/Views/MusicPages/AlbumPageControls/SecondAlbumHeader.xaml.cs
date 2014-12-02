using System;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

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
            SwypeToPanelOne();
        }

        private void SwypeLeftToRight_Button_Tap(object sender, TappedRoutedEventArgs e)
        {
            SwypeToPanelOne();
        }

        async Task SwypeToPanelOne()
        {
            var albumPage = App.ApplicationFrame.Content as AlbumPage;
            if (albumPage != null)
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => albumPage.HeaderFlipView.SelectedIndex = 0);
        
        }
    }
}
