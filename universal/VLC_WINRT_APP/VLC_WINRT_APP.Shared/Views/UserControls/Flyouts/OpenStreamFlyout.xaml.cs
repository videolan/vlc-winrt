using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using VLC_WINRT_APP.ViewModels;
using WinRTXamlToolkit.Controls.Extensions;

namespace VLC_WINRT_APP.Views.UserControls.Flyouts
{
    public sealed partial class OpenStreamFlyout : Flyout
    {
        public OpenStreamFlyout()
        {
            this.InitializeComponent();
#if WINDOWS_PHONE_APP
            this.Placement = FlyoutPlacementMode.Full;
#endif
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var urlTextBox = ((sender as Button).Parent as Grid).GetFirstDescendantOfType<TextBox>();
            if (urlTextBox != null) Locator.VideoLibraryVM.PlayNetworkMRL.Execute(urlTextBox.Text);
        }
    }
}
