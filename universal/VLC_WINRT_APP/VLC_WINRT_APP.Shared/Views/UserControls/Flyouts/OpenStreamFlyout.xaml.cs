using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

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
    }
}
