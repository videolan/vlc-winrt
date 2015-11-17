using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using VLC_WinRT.ViewModels;

namespace VLC_WinRT.UI.Legacy.Views.MainPages
{
    public sealed partial class MainPageNetwork : Page
    {
        public MainPageNetwork()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            Locator.StreamsVM.OnNavigatedTo();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            Locator.StreamsVM.Dispose();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            Locator.VideoLibraryVM.PlayNetworkMRL.Execute(MrlTextBox.Text);
        }
    }
}