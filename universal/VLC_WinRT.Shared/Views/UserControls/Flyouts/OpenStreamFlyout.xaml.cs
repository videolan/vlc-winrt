using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using VLC_WinRT.ViewModels;
using WinRTXamlToolkit.Controls.Extensions;
using Windows.UI.Xaml.Controls.Primitives;

namespace VLC_WinRT.Views.UserControls.Flyouts
{
    public sealed partial class OpenStreamFlyout
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
            var urlTextBox = ((sender as Button).Parent as Grid).GetFirstDescendantOfType<FocusTextBox>();
            if (urlTextBox != null) Locator.VideoLibraryVM.PlayNetworkMRL.Execute(urlTextBox.Text);
        }
    }
}
