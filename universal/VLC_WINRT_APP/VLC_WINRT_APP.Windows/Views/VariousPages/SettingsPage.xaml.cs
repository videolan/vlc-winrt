using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using VLC_WINRT_APP.ViewModels;

namespace VLC_WINRT_APP.Views.VariousPages
{
    public sealed partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            Locator.SettingsVM.Initialize();
        }

        private void VideoFolder_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Flyout.ShowAttachedFlyout(sender as FrameworkElement);
        }
    }
}