using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace VLC_WINRT_APP.Views.UserControls
{
    public sealed partial class TrackItemTemplate : UserControl
    {
        public TrackItemTemplate()
        {
            this.InitializeComponent();
            this.SizeChanged += (sender, args) => Responsive();
            this.Loaded += (sender, args) => Responsive();
        }

        private void Responsive()
        {
            if (Window.Current.Bounds.Width < 550)
            {
                ArtistName.Visibility = Visibility.Collapsed;
            }
            else if (Window.Current.Bounds.Width < 800)
            {
                AlbumName.Visibility = Visibility.Collapsed;
                ArtistName.Visibility = Visibility.Visible;
            }
            else
            {
                ArtistName.Visibility = Visibility.Visible;
                AlbumName.Visibility = Visibility.Visible;
            }
        }
    }
}
