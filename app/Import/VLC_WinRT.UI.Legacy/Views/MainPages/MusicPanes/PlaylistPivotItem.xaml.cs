using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Microsoft.Xaml.Interactivity;
using Windows.UI.Xaml.Navigation;
using VLC.ViewModels;

namespace VLC_WinRT.Views.MainPages.MusicPanes
{
    public sealed partial class PlaylistPivotItem : Page
    {
        public PlaylistPivotItem()
        {
            this.InitializeComponent();
            this.Loaded += PlaylistPivotItem_Loaded;
        }

        private void PlaylistPivotItem_Loaded(object sender, RoutedEventArgs e)
        {
            Locator.MusicLibraryVM.OnNavigatedTo();
        }
    }
}
