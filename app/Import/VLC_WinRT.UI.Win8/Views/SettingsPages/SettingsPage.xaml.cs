using Windows.UI.Xaml.Controls;
using VLC.Utils;

namespace VLC_WinRT.Views.VariousPages
{
    public sealed partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            this.InitializeComponent();
            AppVersion.Text = "v" + Strings.AppVersion;
            //Extensions.HideWindowsOnlyElements(RootPanel);
        }
    }
}
