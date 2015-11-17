using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using VLC_WinRT.ViewModels;

namespace VLC_WinRT.UI.Legacy.Views.MainPages
{
    public sealed partial class MainPageNetwork : Page
    {
        public MainPageNetwork()
        {
            this.InitializeComponent();
        }
        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            Locator.VideoLibraryVM.PlayNetworkMRL.Execute(MrlTextBox.Text);
        }
    }
}
