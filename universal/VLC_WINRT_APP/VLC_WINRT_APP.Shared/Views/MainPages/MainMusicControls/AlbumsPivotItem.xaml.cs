using System.Diagnostics;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using VLC_WINRT_APP.Helpers;
using VLC_WINRT_APP.Model.Music;
using VLC_WINRT_APP.Model.Video;
using VLC_WINRT_APP.ViewModels.MusicVM;

namespace VLC_WINRT_APP.Views.MainPages.MainMusicControls
{
    public sealed partial class AlbumsPivotItem : Page
    {
        public AlbumsPivotItem()
        {
            this.InitializeComponent();
        }

        private void Collection_Loaded(object sender, RoutedEventArgs e)
        {
#if WINDOWS_APP
            AlbumCollectionBase.Margin = new Thickness(24,0,0,0);
#endif
        }
    }
}
