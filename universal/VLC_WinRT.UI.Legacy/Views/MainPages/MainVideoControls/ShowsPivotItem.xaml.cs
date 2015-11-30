using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using VLC_WinRT.ViewModels;

namespace VLC_WinRT.Views.MainPages.MainVideoControls
{
    public sealed partial class ShowsPivotItem : Page
    {
        public ShowsPivotItem()
        {
            this.InitializeComponent();
            this.Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            Locator.VideoLibraryVM.OnNavigatedToAllVideos();
        }
    }
}
