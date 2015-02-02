using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
namespace VLC_WINRT_APP.Views.MusicPages.AlbumPageControls
{
    public sealed partial class AlbumPageBase : UserControl
    {
        public AlbumPageBase()
        {
            this.InitializeComponent();
        }

        private void AlbumPageBase_OnLoaded(object sender, RoutedEventArgs e)
        {
#if WINDOWS_APP
            this.Width = 350;
#endif
        }
    }
}
