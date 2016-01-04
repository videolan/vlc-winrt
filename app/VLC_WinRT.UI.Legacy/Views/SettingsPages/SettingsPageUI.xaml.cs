using Windows.UI.Xaml.Controls;
using VLC_WinRT.Utils;

namespace VLC_WinRT.UI.Legacy.Views.SettingsPages
{
    public sealed partial class SettingsPageUI : UserControl
    {
        public SettingsPageUI()
        {
            this.InitializeComponent();
            Extensions.HideWindowsOnlyElements(RootPanel);
        }
    }
}
