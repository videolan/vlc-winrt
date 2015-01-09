using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using VLC_WINRT_APP.Model.Music;

namespace VLC_WINRT_APP.Views.UserControls
{
    public sealed partial class ArtistItemTemplate : UserControl
    {
        public ArtistItemTemplate()
        {
            this.InitializeComponent();
        }

        private void Image_OnImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            var albumItem = (this.DataContext as ArtistItem);
            Debug.WriteLine("Cover " + albumItem.Name + " failed");
        }

        private void Image_OnImageOpened(object sender, RoutedEventArgs e)
        {
            var albumItem = (this.DataContext as ArtistItem);
            Debug.WriteLine("Cover " + albumItem.Name + " opened");
        }
    }
}
