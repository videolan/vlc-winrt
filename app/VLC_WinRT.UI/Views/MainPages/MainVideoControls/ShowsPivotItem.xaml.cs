using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using VLC.ViewModels;

namespace VLC.Views.MainPages.MainVideoControls
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
            this.Unloaded += ShowsPivotItem_Unloaded;
            Locator.VideoLibraryVM.OnNavigatedToShows();
        }

        private async void ShowsPivotItem_Unloaded(object sender, RoutedEventArgs e)
        {
            await Locator.VideoLibraryVM.OnNavigatedFromCamera();
        }
    }
}
